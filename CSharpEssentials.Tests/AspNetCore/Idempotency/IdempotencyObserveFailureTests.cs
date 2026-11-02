using CSharpEssentials.AspNetCore;
using CSharpEssentials.Idempotency;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

public sealed class IdempotencyObserveFailureTests
{
    [Fact]
    public async Task Observe_Should_Run_Every_Request_And_Warn_When_The_Keyed_Store_Is_Not_Registered()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            configureEndpoints: app => app.MapPost("/observe-missing-store", async (HttpContext context, IdempotencyProbe probe) =>
            {
                using var reader = new StreamReader(context.Request.Body);
                string body = await reader.ReadToEndAsync();
                return HttpResults.Ok(new { execution = probe.Execute(), body });
            }).WithIdempotency(endpoint =>
            {
                endpoint.Mode = IdempotencyMode.Observe;
                endpoint.StoreKey = "missing-store";
            }));

        IdempotencyResponse first = await host.PostAsync("/observe-missing-store", body: "first-body");
        IdempotencyResponse second = await host.PostAsync("/observe-missing-store", body: "first-body");

        new[] { first, second }.Select(response => response.StatusCode).Should().Equal(200, 200);
        first.Body.Should().Contain("first-body");
        second.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(2);
        host.Logs.Named("Idempotency.Observe.ResolutionFailed").Should().HaveCount(2)
            .And.OnlyContain(entry => entry.Level == LogLevel.Warning && entry.Message.Contains("/observe-missing-store"));
    }

    [Fact]
    public async Task Enforce_Should_Fail_The_Request_When_The_Keyed_Store_Is_Not_Registered()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            configureEndpoints: app => app.MapPost("/enforce-missing-store", (IdempotencyProbe probe) => HttpResults.Ok(new { execution = probe.Execute() }))
                .WithIdempotency(endpoint => endpoint.StoreKey = "missing-store"));

        IdempotencyResponse response = await host.PostAsync("/enforce-missing-store");

        response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        host.Probe.Executions.Should().Be(0);
        host.Logs.Named("Idempotency.Observe.ResolutionFailed").Should().BeEmpty();
    }

    [Fact]
    public async Task Observe_Should_Run_The_Request_And_Warn_When_Reserving_Throws()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options =>
        {
            options.Mode = IdempotencyMode.Observe;
            options.UseStore<UnusedIdempotencyStore>(ServiceLifetime.Singleton);
        });

        IdempotencyResponse first = await host.PostAsync("/orders");
        IdempotencyResponse second = await host.PostAsync("/orders");

        new[] { first, second }.Select(response => response.StatusCode).Should().Equal(201, 201);
        second.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(2);
        host.Logs.Named("Idempotency.Observe.ReserveFailed").Should().HaveCount(2)
            .And.OnlyContain(entry => entry.Level == LogLevel.Warning && entry.EventId.Id == 7015);
    }

    [Fact]
    public async Task Enforce_Should_Fail_The_Request_When_Reserving_Throws()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options =>
            options.UseStore<UnusedIdempotencyStore>(ServiceLifetime.Singleton));

        Func<Task> act = () => host.PostAsync("/orders");

        await act.Should().ThrowAsync<NotSupportedException>();
        host.Probe.Executions.Should().Be(0);
        host.Logs.Named("Idempotency.Observe.ReserveFailed").Should().BeEmpty();
    }

    [Fact]
    public async Task Request_Timeout_Attribute_Longer_Than_The_Lease_Should_Warn()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            configureServices: services => services.AddRequestTimeouts(options =>
                options.AddPolicy("long", TimeSpan.FromMinutes(2))),
            configureEndpoints: app =>
            {
                app.MapPost("/timeout-ms", Execute).WithIdempotency().WithMetadata(new RequestTimeoutAttribute(120_000));
                app.MapPost("/timeout-named", Execute).WithIdempotency().WithMetadata(new RequestTimeoutAttribute("long"));
                app.MapPost("/timeout-short", Execute).WithIdempotency().WithMetadata(new RequestTimeoutAttribute(5_000));
                app.MapPost("/timeout-unknown", Execute).WithIdempotency().WithMetadata(new RequestTimeoutAttribute("unknown"));
            });

        foreach (string path in new[] { "/timeout-ms", "/timeout-named", "/timeout-short", "/timeout-unknown" })
            (await host.PostAsync(path, key: path)).StatusCode.Should().Be(StatusCodes.Status200OK);

        host.Logs.Named("Idempotency.RequestTimeoutExceedsLease").Select(entry => entry.Message).Should().HaveCount(2)
            .And.Contain(message => message.Contains("/timeout-ms"))
            .And.Contain(message => message.Contains("/timeout-named"));
    }

    private static IResult Execute(IdempotencyProbe probe) => HttpResults.Ok(new { execution = probe.Execute() });
}
