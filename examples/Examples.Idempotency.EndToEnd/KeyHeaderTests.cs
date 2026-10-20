using System.Net;
using System.Net.Http.Json;

using Examples.Idempotency.EndToEnd.App;

using Microsoft.EntityFrameworkCore;

namespace Examples.Idempotency.EndToEnd;

/// <summary>
/// A client that sends its key as a structured-field string (<c>"abc"</c>). The HTTP layer unquotes it, and the
/// endpoint parses the header the same way before it builds the command, so both layers dedupe on <c>abc</c>.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class KeyHeaderTests(PostgresFixture postgres)
{
    private readonly string _merchant = "merchant-" + Guid.NewGuid().ToString("N");

    [Fact]
    public async Task A_Quoted_Key_Should_Charge_Once_And_Replay_The_Retry()
    {
        TestPaymentProvider provider = new();
        await using PaymentsHost host = await PaymentsHost.StartAsync(postgres.ConnectionString, provider: provider);
        CapturePaymentRequest payment = new("order-20", 15m);

        using HttpResponseMessage original = await host.CaptureWithKeyHeaderAsync(_merchant, payment, "\"order-20-capture\"");
        using HttpResponseMessage retry = await host.CaptureWithKeyHeaderAsync(_merchant, payment, "\"order-20-capture\"");

        Assert.Equal(HttpStatusCode.OK, original.StatusCode);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        Assert.Equal("true", retry.Headers.GetValues("Idempotency-Replayed").Single());
        Assert.Equal(1, provider.Charges);
        Assert.Equal(1, provider.Calls);
        Assert.Equal(1, await CountPaymentsAsync());
    }

    [Fact]
    public async Task A_Quoted_Key_And_Its_Bare_Form_Should_Be_One_Payment_Across_Instances()
    {
        TestPaymentProvider provider = new();
        await using PaymentsHost first = await PaymentsHost.StartAsync(postgres.ConnectionString, provider: provider);
        await using PaymentsHost second = await PaymentsHost.StartAsync(postgres.ConnectionString, provider: provider);
        CapturePaymentRequest payment = new("order-21", 35m);

        using HttpResponseMessage original = await first.CaptureWithKeyHeaderAsync(_merchant, payment, "\"order-21-capture\"");
        using HttpResponseMessage retry = await second.CaptureWithKeyHeaderAsync(_merchant, payment, "order-21-capture");

        Assert.Equal(HttpStatusCode.OK, original.StatusCode);
        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        Assert.False(retry.Headers.Contains("Idempotency-Replayed"));
        Assert.Equal(
            (await original.Content.ReadFromJsonAsync<CapturedPayment>())!.PaymentId,
            (await retry.Content.ReadFromJsonAsync<CapturedPayment>())!.PaymentId);
        Assert.Equal(1, provider.Charges);
        Assert.Equal(1, provider.Calls);
        Assert.Equal(1, await CountPaymentsAsync());
    }

    private async Task<int> CountPaymentsAsync()
    {
        await using PaymentsDbContext db = postgres.CreateContext();
        return await db.Payments.CountAsync(payment => payment.MerchantId == _merchant);
    }
}
