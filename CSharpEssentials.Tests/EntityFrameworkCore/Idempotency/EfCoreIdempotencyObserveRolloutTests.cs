using System.Transactions;

using CSharpEssentials.EntityFrameworkCore.Idempotency;
using CSharpEssentials.EntityFrameworkCore.Transactions;
using CSharpEssentials.Idempotency;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Tests.Idempotency;

using FluentAssertions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Idempotency;

/// <summary>
/// A rollout from <see cref="IdempotencyMode.Observe"/> to <see cref="IdempotencyMode.Enforce"/> with the EF Core store
/// and <see cref="IdempotencyPlacement.InsideTransaction"/>: what Observe records must be exactly what committed.
/// </summary>
[Collection(IdempotencyPostgresCollection.Name)]
public sealed class EfCoreIdempotencyObserveRolloutTests(IdempotencyPostgresFixture postgres) : IDisposable
{
    private readonly ServiceProvider _services = postgres.CreateServices();
    private readonly string _account = "acct-" + Guid.NewGuid().ToString("N");
    private readonly TestOptionsMonitor<IdempotencyExecutorOptions> _monitor = new(new IdempotencyExecutorOptions { Mode = IdempotencyMode.Observe });
    private int _handled;

    public void Dispose() => _services.Dispose();

    [Fact]
    public async Task A_Retry_After_The_Switch_To_Enforce_Should_Run_Again_When_The_Observed_Commit_Failed()
    {
        PaymentWebhook webhook = new(_account + ":evt-1", _account, 25, Decline: false);

        Func<Task> observed = () => DeliverAsync(webhook, new FailingCommitInterceptor());
        await observed.Should().ThrowAsync<InvalidOperationException>().WithMessage("The commit failed.");
        int paymentsAfterFailure = await CountPaymentsAsync(webhook.EventId);
        int recordsAfterFailure = await CountRecordsAsync(webhook);
        _monitor.CurrentValue = new IdempotencyExecutorOptions { Mode = IdempotencyMode.Enforce };
        Result<int> retried = await DeliverAsync(webhook);

        _handled.Should().Be(2);
        retried.IsSuccess.Should().BeTrue();
        paymentsAfterFailure.Should().Be(0);
        recordsAfterFailure.Should().Be(0);
        (await CountPaymentsAsync(webhook.EventId)).Should().Be(1);
        (await CountRecordsAsync(webhook)).Should().Be(1);
    }

    [Fact]
    public async Task A_Retry_After_The_Switch_To_Enforce_Should_Replay_What_Observe_Committed()
    {
        PaymentWebhook webhook = new(_account + ":evt-1", _account, 25, Decline: false);

        Result<int> observed = await DeliverAsync(webhook);
        _monitor.CurrentValue = new IdempotencyExecutorOptions { Mode = IdempotencyMode.Enforce };
        Result<int> retried = await DeliverAsync(webhook);

        observed.IsSuccess.Should().BeTrue();
        retried.IsSuccess.Should().BeTrue();
        retried.Value.Should().Be(observed.Value);
        _handled.Should().Be(1);
        (await CountPaymentsAsync(webhook.EventId)).Should().Be(1);
        (await CountRecordsAsync(webhook)).Should().Be(1);
    }

    [Fact]
    public async Task Observe_Should_Commit_The_Business_When_The_Result_Write_Fails()
    {
        PaymentWebhook webhook = new(_account + ":evt-1", _account, 25, Decline: false);

        Result<int> observed = await DeliverAsync(webhook, new FailingRecordWriteInterceptor());

        observed.IsSuccess.Should().BeTrue();
        _handled.Should().Be(1);
        (await CountPaymentsAsync(webhook.EventId)).Should().Be(1);
        (await CountRecordsAsync(webhook)).Should().Be(0);
    }

    [Fact]
    public async Task Observe_Should_Store_The_Result_In_A_Read_Committed_Ambient_Transaction()
    {
        PaymentWebhook webhook = new(_account + ":evt-1", _account, 25, Decline: false);

        Result<int> observed = await DeliverAsync(webhook, ambient: IsolationLevel.ReadCommitted);

        observed.IsSuccess.Should().BeTrue();
        (await CountPaymentsAsync(webhook.EventId)).Should().Be(1);
        (await CountRecordsAsync(webhook)).Should().Be(1);
    }

    [Fact]
    public async Task Observe_Should_Commit_The_Ambient_Transaction_When_The_Result_Write_Fails()
    {
        PaymentWebhook webhook = new(_account + ":evt-1", _account, 25, Decline: false);

        Result<int> observed = await DeliverAsync(webhook, new FailingRecordWriteInterceptor(), IsolationLevel.ReadCommitted);

        observed.IsSuccess.Should().BeTrue();
        _handled.Should().Be(1);
        (await CountPaymentsAsync(webhook.EventId)).Should().Be(1);
        (await CountRecordsAsync(webhook)).Should().Be(0);
    }

    [Fact]
    public async Task Observe_Should_Commit_A_Serializable_Ambient_Transaction_Without_The_Result()
    {
        PaymentWebhook webhook = new(_account + ":evt-1", _account, 25, Decline: false);

        Result<int> observed = await DeliverAsync(webhook, ambient: IsolationLevel.Serializable);

        observed.IsSuccess.Should().BeTrue();
        (await CountPaymentsAsync(webhook.EventId)).Should().Be(1);
        (await CountRecordsAsync(webhook)).Should().Be(0);
    }

    private async Task<Result<int>> DeliverAsync(PaymentWebhook webhook, IInterceptor? interceptor = null, IsolationLevel? ambient = null)
    {
        using TransactionScope? scope = ambient is { } isolation
            ? new(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = isolation }, TransactionScopeAsyncFlowOption.Enabled)
            : null;
        DbContextOptionsBuilder<IdempotencyTestDbContext> options = new DbContextOptionsBuilder<IdempotencyTestDbContext>().UseNpgsql(postgres.ConnectionString);
        if (interceptor is not null)
            options.AddInterceptors(interceptor);
        await using IdempotencyTestDbContext context = new(options.Options);
        IdempotencyExecutor executor = new(
            new EfCoreIdempotencyRecordStore<IdempotencyTestDbContext>(context, _services.GetRequiredService<IServiceScopeFactory>()),
            _monitor);
        TransactionBehavior<PaymentWebhook, Result<int>> transaction = new(new EfCoreTransactionRunner<IdempotencyTestDbContext>(context));
        IdempotencyBehavior<PaymentWebhook, Result<int>> idempotency = new(
            executor,
            _monitor,
            new IdempotencyBehaviorOptions { Placement = IdempotencyPlacement.InsideTransaction },
            NullLogger<IdempotencyBehavior<PaymentWebhook, Result<int>>>.Instance);

        Result<int> result = scope is not null
            ? await idempotency.Handle(webhook, (request, ct) => HandleAsync(context, request, ct), CancellationToken.None)
            : await transaction.Handle(
                webhook,
                (message, token) => idempotency.Handle(message, (request, ct) => HandleAsync(context, request, ct), token),
                CancellationToken.None);
        scope?.Complete();
        return result;
    }

    private async ValueTask<Result<int>> HandleAsync(IdempotencyTestDbContext context, PaymentWebhook webhook, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _handled);
        IdempotencyPayment payment = new() { EventId = webhook.EventId, Amount = webhook.Amount };
        context.Payments.Add(payment);
        await context.SaveChangesAsync(cancellationToken);
        return payment.Id;
    }

    private async Task<int> CountPaymentsAsync(string eventId)
    {
        await using IdempotencyTestDbContext context = postgres.CreateContext();
        return await context.Payments.CountAsync(payment => payment.EventId == eventId);
    }

    private async Task<int> CountRecordsAsync(PaymentWebhook webhook)
    {
        string key = ((IIdempotentRequest)webhook).IdempotencyKey;
        await using IdempotencyTestDbContext context = postgres.CreateContext();
        return await context.IdempotencyRecords.CountAsync(record => record.Key == key);
    }
}
