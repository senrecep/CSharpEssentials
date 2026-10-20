using CSharpEssentials.Idempotency;
using CSharpEssentials.ResultPattern;

using Examples.Idempotency.EndToEnd.App;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Examples.Idempotency.EndToEnd;

/// <summary>
/// Rolling the database layer out with <c>Idempotency:PaymentsMode = Observe</c>: decisions are recorded but nothing
/// is blocked, so a concurrent duplicate runs while the first payment's transaction is still open. The mode is read
/// from configuration on every call, so switching it to <c>Enforce</c> needs a configuration reload, not a restart.
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class ObserveModeTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Observe_Should_Not_Block_A_Duplicate_While_The_First_Transaction_Is_Open()
    {
        string merchant = "merchant-" + Guid.NewGuid().ToString("N");
        TestPaymentProvider provider = new(holdFirstCharge: true);
        await using PaymentsHost host = await PaymentsHost.StartAsync(postgres.ConnectionString, IdempotencyMode.Observe, provider);
        CapturePayment command = new(merchant, "order-7-capture", "order-7", 30m);

        Task<Result<CapturedPayment>> first = SendAsync(host, command);
        Result<CapturedPayment> duplicate;
        try
        {
            await provider.FirstChargeEntered.WaitAsync(TimeSpan.FromSeconds(10));
            duplicate = await SendAsync(host, command).WaitAsync(TimeSpan.FromSeconds(10));
        }
        finally
        {
            provider.Release();
        }

        Result<CapturedPayment> original = await first.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.True(duplicate.IsSuccess);
        Assert.True(original.IsSuccess);
        await using PaymentsDbContext db = postgres.CreateContext();
        Assert.Equal(2, await db.Payments.CountAsync(payment => payment.MerchantId == merchant));
    }

    [Fact]
    public async Task Switching_The_Configuration_To_Enforce_Should_Take_Effect_Without_A_Restart()
    {
        string merchant = "merchant-" + Guid.NewGuid().ToString("N");
        await using PaymentsHost host = await PaymentsHost.StartAsync(postgres.ConnectionString, IdempotencyMode.Observe);
        CapturePayment observed = new(merchant, "order-8-capture", "order-8", 20m);
        CapturePayment enforced = new(merchant, "order-9-capture", "order-9", 20m);

        await SendAsync(host, observed);
        await SendAsync(host, observed);
        host.SwitchPaymentsMode(IdempotencyMode.Enforce);
        Result<CapturedPayment> original = await SendAsync(host, enforced);
        Result<CapturedPayment> retry = await SendAsync(host, enforced);

        Assert.Equal(original.Value.PaymentId, retry.Value.PaymentId);
        await using PaymentsDbContext db = postgres.CreateContext();
        Assert.Equal(2, await db.Payments.CountAsync(payment => payment.Reference == "order-8" && payment.MerchantId == merchant));
        Assert.Equal(1, await db.Payments.CountAsync(payment => payment.Reference == "order-9" && payment.MerchantId == merchant));
    }

    private static async Task<Result<CapturedPayment>> SendAsync(PaymentsHost host, CapturePayment command)
    {
        await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
        CommandSender sender = scope.ServiceProvider.GetRequiredService<CommandSender>();
        return await sender.SendAsync<CapturePayment, Result<CapturedPayment>>(command, CancellationToken.None);
    }
}
