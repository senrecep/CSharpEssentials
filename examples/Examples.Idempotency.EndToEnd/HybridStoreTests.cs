using System.Net;
using System.Net.Http.Json;

using CSharpEssentials.Idempotency;

using Examples.Idempotency.EndToEnd.App;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Examples.Idempotency.EndToEnd;

/// <summary>
/// The <c>payments</c> profile writes to the database; the HTTP layer and the <c>messaging</c> profile write to the
/// default in-memory store.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class HybridStoreTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Each_Layer_Should_Write_To_The_Store_Of_Its_Profile()
    {
        string merchant = "merchant-" + Guid.NewGuid().ToString("N");
        await using PaymentsHost host = await PaymentsHost.StartAsync(postgres.ConnectionString);

        using HttpResponseMessage response = await host.CreatePaymentsClient(merchant).CaptureAsync(new CapturePaymentRequest("order-9", 60m), "order-9-capture");
        CapturedPayment payment = (await response.Content.ReadFromJsonAsync<CapturedPayment>())!;
        MessageDelivery receipt = await host.NextAcknowledgementAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(IdempotencyExecutionStatus.Executed, receipt.Status);
        Assert.Equal([typeof(CapturePayment).FullName!], await DatabaseNamespacesAsync(IdempotencyScope.Client(merchant)));
        IIdempotencyRecordStore defaultStore = host.Services.GetRequiredService<IIdempotencyRecordStore>();
        Assert.IsType<InMemoryIdempotencyRecordStore>(defaultStore);
        IdempotencyRecordReservation inbox = await defaultStore.TryReserveAsync(
            new IdempotencyRecordKey(
                "messaging:" + ReceiptConsumer.Consumer,
                IdempotencyScope.Client(merchant),
                IdempotencyKeys.FromMessage(payment.PaymentId.ToString("N"), ReceiptConsumer.Consumer)),
            "message",
            TimeSpan.FromMinutes(1));
        Assert.Equal(IdempotencyReservationStatus.Completed, inbox.Status);
    }

    private async Task<string[]> DatabaseNamespacesAsync(IdempotencyScope scope)
    {
        await using PaymentsDbContext db = postgres.CreateContext();
        return await db.IdempotencyRecords
            .Where(record => record.Scope == scope.Value)
            .Select(record => record.Namespace)
            .ToArrayAsync();
    }
}
