using CSharpEssentials.Idempotency;
using CSharpEssentials.ResultPattern;

using Examples.Idempotency.EndToEnd.App;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Examples.Idempotency.EndToEnd;

/// <summary>
/// Rolling the database layer out with <c>Idempotency:PaymentsMode = Observe</c>: decisions are recorded but nothing
/// is blocked, so a concurrent duplicate runs while the first payment's transaction is still open.
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
        await provider.FirstChargeEntered.WaitAsync(TimeSpan.FromSeconds(10));
        Result<CapturedPayment> duplicate = await SendAsync(host, command).WaitAsync(TimeSpan.FromSeconds(10));
        provider.Release();
        Result<CapturedPayment> original = await first.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.True(duplicate.IsSuccess);
        Assert.True(original.IsSuccess);
        await using PaymentsDbContext db = postgres.CreateContext();
        Assert.Equal(2, await db.Payments.CountAsync(payment => payment.MerchantId == merchant));
    }

    private static async Task<Result<CapturedPayment>> SendAsync(PaymentsHost host, CapturePayment command)
    {
        await using AsyncServiceScope scope = host.Services.CreateAsyncScope();
        CommandSender sender = scope.ServiceProvider.GetRequiredService<CommandSender>();
        return await sender.SendAsync<CapturePayment, Result<CapturedPayment>>(command, CancellationToken.None);
    }
}
