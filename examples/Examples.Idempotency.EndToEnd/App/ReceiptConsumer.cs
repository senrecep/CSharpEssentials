using CSharpEssentials.Idempotency;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Examples.Idempotency.EndToEnd.App;

/// <summary>
/// Issues a receipt for every captured payment. The inbox (<see cref="IdempotencyInbox"/>) keys each delivery with
/// <see cref="IdempotencyKeys.FromMessage"/>, so a redelivered message is acknowledged without issuing a second
/// receipt. The inbox runs on the <c>messaging</c> profile, which uses the default store, not the payments database.
/// </summary>
public sealed partial class ReceiptConsumer(MessageBus bus, IServiceScopeFactory scopes, ILogger<ReceiptConsumer> logger) : BackgroundService
{
    public const string Consumer = "receipts";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (PaymentCaptured message in bus.Messages.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using AsyncServiceScope scope = scopes.CreateAsyncScope();
                IdempotencyInbox inbox = scope.ServiceProvider.GetRequiredService<IdempotencyInbox>();
                PaymentsDbContext db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
                IdempotencyExecutionStatus status = await inbox.ProcessAsync(
                    Consumer,
                    message.MessageId,
                    IdempotencyScope.Client(message.MerchantId),
                    token => IssueReceiptAsync(db, message, token),
                    cancellationToken: stoppingToken);

                if (status == IdempotencyExecutionStatus.InFlight)
                    bus.Publish(message);
                else
                    bus.Acknowledge(new MessageDelivery(message.MessageId, status));
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                LogReceiptFailed(logger, exception, message.MessageId);
            }
        }
    }

    private static async ValueTask IssueReceiptAsync(PaymentsDbContext db, PaymentCaptured message, CancellationToken cancellationToken)
    {
        db.Receipts.Add(new Receipt
        {
            Id = Guid.NewGuid(),
            PaymentId = message.PaymentId,
            MerchantId = message.MerchantId,
            Amount = message.Amount,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Issuing the receipt of message {MessageId} failed; it stays unacknowledged.")]
    private static partial void LogReceiptFailed(ILogger logger, Exception exception, string messageId);
}
