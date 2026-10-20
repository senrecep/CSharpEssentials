using CSharpEssentials.EntityFrameworkCore.Idempotency;
using CSharpEssentials.EntityFrameworkCore.Transactions;
using CSharpEssentials.Errors;
using CSharpEssentials.Idempotency;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Tests.Idempotency;

using FluentAssertions;

using Mediator;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace CSharpEssentials.Tests.EntityFrameworkCore.Idempotency;

/// <summary>
/// A provider webhook deduplicated by its event id: the transaction behavior wraps the idempotency behavior
/// (<see cref="IdempotencyPlacement.InsideTransaction"/>), so the claim commits or rolls back with the business row.
/// </summary>
[Collection(IdempotencyPostgresCollection.Name)]
public sealed class EfCoreIdempotencyWebhookTests(IdempotencyPostgresFixture postgres) : IDisposable
{
    private readonly ServiceProvider _services = postgres.CreateServices();
    private readonly string _account = "acct-" + Guid.NewGuid().ToString("N");

    public void Dispose() => _services.Dispose();

    [Fact]
    public async Task The_Same_Event_Delivered_Twice_Should_Create_One_Payment_And_Succeed_Both_Times()
    {
        PaymentWebhook webhook = new(EventId("evt-1"), _account, 25, Decline: false);

        Result<int> first = await DeliverAsync(webhook);
        Result<int> duplicate = await DeliverAsync(webhook);

        first.IsSuccess.Should().BeTrue();
        duplicate.IsSuccess.Should().BeTrue();
        duplicate.Value.Should().Be(first.Value);
        (await CountPaymentsAsync(webhook.EventId)).Should().Be(1);
        (await CountRecordsAsync(webhook)).Should().Be(1);
    }

    [Fact]
    public async Task A_Failed_Delivery_Should_Leave_No_Record_So_The_Resend_Is_Processed()
    {
        PaymentWebhook declined = new(EventId("evt-1"), _account, 25, Decline: true);

        Result<int> failed = await DeliverAsync(declined);
        int recordsAfterFailure = await CountRecordsAsync(declined);
        int paymentsAfterFailure = await CountPaymentsAsync(declined.EventId);
        Result<int> resent = await DeliverAsync(declined with { Decline = false });

        failed.IsFailure.Should().BeTrue();
        recordsAfterFailure.Should().Be(0);
        paymentsAfterFailure.Should().Be(0);
        resent.IsSuccess.Should().BeTrue();
        (await CountPaymentsAsync(declined.EventId)).Should().Be(1);
        (await CountRecordsAsync(declined)).Should().Be(1);
    }

    [Fact]
    public async Task A_Concurrent_Delivery_Should_Wait_For_The_First_And_Replay_Its_Response()
    {
        PaymentWebhook webhook = new(EventId("evt-1"), _account, 25, Decline: false);
        TaskCompletionSource entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource gate = new(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<Result<int>> first = DeliverAsync(webhook, async () =>
        {
            entered.SetResult();
            await gate.Task;
        });
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Task<Result<int>> second = DeliverAsync(webhook);
        await Task.Delay(TimeSpan.FromMilliseconds(500));
        bool secondFinishedFirst = second.IsCompleted;
        gate.SetResult();
        Result<int> firstResult = await first.WaitAsync(TimeSpan.FromSeconds(10));
        Result<int> secondResult = await second.WaitAsync(TimeSpan.FromSeconds(10));

        secondFinishedFirst.Should().BeFalse();
        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeTrue();
        secondResult.Value.Should().Be(firstResult.Value);
        (await CountPaymentsAsync(webhook.EventId)).Should().Be(1);
    }

    private string EventId(string id) => _account + ":" + id;

    private async Task<Result<int>> DeliverAsync(PaymentWebhook webhook, Func<Task>? afterSave = null)
    {
        await using AsyncServiceScope scope = _services.CreateAsyncScope();
        IdempotencyTestDbContext context = scope.ServiceProvider.GetRequiredService<IdempotencyTestDbContext>();
        TestOptionsMonitor<IdempotencyExecutorOptions> monitor = new(new IdempotencyExecutorOptions());
        IdempotencyExecutor executor = new(
            new EfCoreIdempotencyRecordStore<IdempotencyTestDbContext>(context, scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>()),
            monitor);
        TransactionBehavior<PaymentWebhook, Result<int>> transaction = new(new EfCoreTransactionRunner<IdempotencyTestDbContext>(context));
        IdempotencyBehavior<PaymentWebhook, Result<int>> idempotency = new(
            executor,
            monitor,
            new IdempotencyBehaviorOptions { Placement = IdempotencyPlacement.InsideTransaction },
            NullLogger<IdempotencyBehavior<PaymentWebhook, Result<int>>>.Instance);

        return await transaction.Handle(
            webhook,
            (message, token) => idempotency.Handle(message, (request, ct) => HandleAsync(context, request, afterSave, ct), token),
            CancellationToken.None);
    }

    private static async ValueTask<Result<int>> HandleAsync(
        IdempotencyTestDbContext context, PaymentWebhook webhook, Func<Task>? afterSave, CancellationToken cancellationToken)
    {
        IdempotencyPayment payment = new() { EventId = webhook.EventId, Amount = webhook.Amount };
        context.Payments.Add(payment);
        await context.SaveChangesAsync(cancellationToken);
        if (afterSave is not null)
            await afterSave();
        if (webhook.Decline)
            return Error.Failure("Payment.Declined", "The payment was declined.");
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

/// <summary>A provider event: deduplicated by its event id per provider account, with a fixed fingerprint.</summary>
internal sealed record PaymentWebhook(string EventId, string AccountId, decimal Amount, bool Decline)
    : ICommand<Result<int>>, IIdempotentRequest, ITransactionalRequest
{
    string IIdempotentRequest.IdempotencyKey => IdempotencyKeys.FromMessage(EventId, "payment-webhook");

    IdempotencyScope? IIdempotentRequest.IdempotencyScope => IdempotencyScope.Client(AccountId);

    string? IIdempotentRequest.IdempotencyFingerprint => "payment-webhook";
}
