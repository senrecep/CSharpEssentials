using CSharpEssentials.Idempotency;

using Examples.Idempotency.EndToEnd.App;

using Microsoft.EntityFrameworkCore;

namespace Examples.Idempotency.EndToEnd;

/// <summary>The broker redelivers a message the consumer already processed.</summary>
[Collection(PostgresCollection.Name)]
public sealed class MessageConsumerTests(PostgresFixture postgres)
{
    [Fact]
    public async Task A_Redelivered_Message_Should_Issue_One_Receipt()
    {
        await using PaymentsHost host = await PaymentsHost.StartAsync(postgres.ConnectionString);
        Guid paymentId = Guid.NewGuid();
        PaymentCaptured message = new(paymentId.ToString("N"), "merchant-" + Guid.NewGuid().ToString("N"), paymentId, 18m);

        host.Bus.Publish(message);
        MessageDelivery first = await host.NextAcknowledgementAsync();
        host.Bus.Publish(message);
        MessageDelivery redelivery = await host.NextAcknowledgementAsync();

        Assert.Equal(IdempotencyExecutionStatus.Executed, first.Status);
        Assert.Equal(IdempotencyExecutionStatus.Replayed, redelivery.Status);
        await using PaymentsDbContext db = postgres.CreateContext();
        Assert.Equal(1, await db.Receipts.CountAsync(receipt => receipt.PaymentId == paymentId));
    }
}
