using CSharpEssentials.AspNetCore;
using CSharpEssentials.EntityFrameworkCore.Idempotency;
using CSharpEssentials.Idempotency;
using CSharpEssentials.Tests.AspNetCore.Idempotency;

using FluentAssertions;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Idempotency;

/// <summary>
/// A rollout from <see cref="IdempotencyMode.Observe"/> to <see cref="IdempotencyMode.Enforce"/> of <c>UseIdempotency</c>
/// on the EF Core store, inside a unit of work that begins a transaction before <c>UseIdempotency</c> and commits it after.
/// </summary>
[Collection(IdempotencyPostgresCollection.Name)]
public sealed class EfCoreIdempotencyHttpObserveRolloutTests(IdempotencyPostgresFixture postgres)
{
    private readonly string _eventId = "http-" + Guid.NewGuid().ToString("N");
    private readonly string _key = "key-" + Guid.NewGuid().ToString("N");
    private bool _failCommit;

    [Fact]
    public async Task A_Retry_After_The_Switch_To_Enforce_Should_Run_Again_When_The_Observed_Commit_Failed()
    {
        IConfigurationRoot configuration = Configuration(IdempotencyMode.Observe);
        await using IdempotencyTestHost host = await StartAsync(configuration);

        _failCommit = true;
        IdempotencyResponse? observed = await SendAsync(host);
        int paymentsAfterFailure = await CountPaymentsAsync();
        _failCommit = false;
        SetMode(configuration, IdempotencyMode.Enforce);
        IdempotencyResponse? retried = await SendAsync(host);

        host.Probe.Executions.Should().Be(2);
        retried!.Replayed.Should().BeFalse();
        retried.StatusCode.Should().Be(200);
        observed?.StatusCode.Should().NotBe(200);
        paymentsAfterFailure.Should().Be(0);
        (await CountPaymentsAsync()).Should().Be(1);
    }

    [Fact]
    public async Task A_Retry_After_The_Switch_To_Enforce_Should_Replay_What_Observe_Committed()
    {
        IConfigurationRoot configuration = Configuration(IdempotencyMode.Observe);
        await using IdempotencyTestHost host = await StartAsync(configuration);

        IdempotencyResponse? observed = await SendAsync(host);
        SetMode(configuration, IdempotencyMode.Enforce);
        IdempotencyResponse? retried = await SendAsync(host);

        observed!.StatusCode.Should().Be(200);
        retried!.Replayed.Should().BeTrue();
        retried.Body.Should().Be(observed.Body);
        host.Probe.Executions.Should().Be(1);
        (await CountPaymentsAsync()).Should().Be(1);
    }

    private static IConfigurationRoot Configuration(IdempotencyMode mode) => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?> { ["Mode"] = mode.ToString() })
        .Build();

    private static void SetMode(IConfigurationRoot configuration, IdempotencyMode mode)
    {
        configuration["Mode"] = mode.ToString();
        configuration.Reload();
    }

    private Task<IdempotencyTestHost> StartAsync(IConfigurationRoot configuration) => IdempotencyTestHost.StartAsync(
        configureServices: services =>
        {
            services.Configure<IdempotencyOptions>(configuration);
            services.AddDbContext<IdempotencyTestDbContext>(options => options.UseNpgsql(postgres.ConnectionString));
            services.AddEfCoreIdempotencyRecordStore<IdempotencyTestDbContext>();
        },
        configurePipeline: app => app.Use(async (context, next) =>
        {
            IdempotencyTestDbContext db = context.RequestServices.GetRequiredService<IdempotencyTestDbContext>();
            await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync();
            await next(context);
            if (_failCommit)
                throw new InvalidOperationException("The commit failed.");
            await transaction.CommitAsync();
        }),
        configureEndpoints: app => app.MapPost("/payments", async (IdempotencyTestDbContext db, IdempotencyProbe probe) =>
        {
            int execution = probe.Execute();
            db.Payments.Add(new IdempotencyPayment { EventId = _eventId, Amount = 10 });
            await db.SaveChangesAsync();
            return HttpResults.Ok(new { execution });
        }).WithIdempotency());

    private async Task<IdempotencyResponse?> SendAsync(IdempotencyTestHost host)
    {
        using HttpRequestMessage request = IdempotencyTestHost.CreateRequest("/payments", _key);
        return await host.SendSettledAsync(request);
    }

    private async Task<int> CountPaymentsAsync()
    {
        await using IdempotencyTestDbContext context = postgres.CreateContext();
        return await context.Payments.CountAsync(payment => payment.EventId == _eventId);
    }
}
