using CSharpEssentials.ResultPattern;

using Examples.Idempotency.EndToEnd.App;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Examples.Idempotency.EndToEnd;

/// <summary>
/// The provider call is outside the payment's transaction. When the provider charges but its response is lost, the
/// transaction rolls back and releases the key; the retry charges again with the same derived key, and the provider
/// returns the first charge. The command is sent directly, because over HTTP the exception keeps the HTTP key in
/// flight until its lease ends.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class PaymentProviderTests(PostgresFixture postgres)
{
    [Fact]
    public async Task A_Retry_After_A_Lost_Provider_Response_Should_Not_Charge_Twice()
    {
        string merchant = "merchant-" + Guid.NewGuid().ToString("N");
        TestPaymentProvider provider = new(loseFirstResponse: true);
        await using PaymentsHost host = await PaymentsHost.StartAsync(postgres.ConnectionString, provider: provider);
        CapturePayment command = new(merchant, "order-30-capture", "order-30", 45m);

        await Assert.ThrowsAsync<HttpRequestException>(() => SendAsync(host, command));
        Result<CapturedPayment> retry = await SendAsync(host, command);

        Assert.True(retry.IsSuccess);
        Assert.Equal(2, provider.Calls);
        Assert.Equal(1, provider.Charges);
        Assert.Equal("ch_test_1", retry.Value.ProviderChargeId);
        await using PaymentsDbContext db = postgres.CreateContext();
        Assert.Equal(1, await db.Payments.CountAsync(payment => payment.MerchantId == merchant));
    }

    private static async Task<Result<CapturedPayment>> SendAsync(PaymentsHost host, CapturePayment command)
    {
        await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
        CommandSender sender = scope.ServiceProvider.GetRequiredService<CommandSender>();
        return await sender.SendAsync<CapturePayment, Result<CapturedPayment>>(command, CancellationToken.None);
    }
}
