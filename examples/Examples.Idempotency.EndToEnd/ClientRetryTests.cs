using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using CSharpEssentials.Idempotency;

using Examples.Idempotency.EndToEnd.App;
using Examples.Idempotency.EndToEnd.Client;

using Microsoft.EntityFrameworkCore;

namespace Examples.Idempotency.EndToEnd;

/// <summary>A client retries a payment whose response was lost.</summary>
[Collection(PostgresCollection.Name)]
public sealed class ClientRetryTests(PostgresFixture postgres)
{
    private readonly string _merchant = "merchant-" + Guid.NewGuid().ToString("N");

    [Fact]
    public async Task A_Retried_Payment_Should_Create_One_Payment_And_Replay_The_First_Response()
    {
        TestPaymentProvider provider = new();
        await using PaymentsHost host = await PaymentsHost.StartAsync(postgres.ConnectionString, provider: provider);
        DroppedResponseHandler network = null!;
        PaymentsClient client = host.CreatePaymentsClient(_merchant, server => network = new DroppedResponseHandler(server));

        using HttpResponseMessage response = await client.CaptureAsync(new CapturePaymentRequest("order-1", 25m), "order-1-capture");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, network.Attempts);
        Assert.Equal("true", response.Headers.GetValues("Idempotency-Replayed").Single());
        Assert.Equal(network.DroppedBody, await response.Content.ReadAsStringAsync());
        Assert.Equal(1, provider.Charges);
        Assert.Equal(1, provider.Calls);
        Assert.Equal(1, await CountPaymentsAsync());
    }

    [Fact]
    public async Task A_Retry_That_Reaches_Another_Instance_Should_Get_The_Stored_Result_From_The_Database()
    {
        TestPaymentProvider provider = new();
        await using PaymentsHost first = await PaymentsHost.StartAsync(postgres.ConnectionString, provider: provider);
        await using PaymentsHost second = await PaymentsHost.StartAsync(postgres.ConnectionString, provider: provider);
        CapturePaymentRequest payment = new("order-2", 40m);

        using HttpResponseMessage original = await first.CreatePaymentsClient(_merchant).CaptureAsync(payment, "order-2-capture");
        using HttpResponseMessage retry = await second.CreatePaymentsClient(_merchant).CaptureAsync(payment, "order-2-capture");

        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        Assert.False(retry.Headers.Contains("Idempotency-Replayed"));
        Assert.Equal(
            (await original.Content.ReadFromJsonAsync<CapturedPayment>())!.PaymentId,
            (await retry.Content.ReadFromJsonAsync<CapturedPayment>())!.PaymentId);
        Assert.Equal(1, provider.Charges);
        Assert.Equal(1, provider.Calls);
        Assert.Equal(1, await CountPaymentsAsync());
    }

    [Fact]
    public async Task The_Same_Key_With_Another_Amount_Should_Be_Rejected_Without_A_Second_Payment()
    {
        await using PaymentsHost host = await PaymentsHost.StartAsync(postgres.ConnectionString);
        PaymentsClient client = host.CreatePaymentsClient(_merchant);

        using HttpResponseMessage original = await client.CaptureAsync(new CapturePaymentRequest("order-3", 10m), "order-3-capture");
        using HttpResponseMessage changed = await client.CaptureAsync(new CapturePaymentRequest("order-3", 99m), "order-3-capture");

        Assert.Equal(HttpStatusCode.OK, original.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, changed.StatusCode);
        Assert.Equal(1, await CountPaymentsAsync());
    }

    /// <summary>
    /// The <c>422</c> comes from the database and Mediator layer, not the HTTP layer: each instance keeps its own
    /// in-memory HTTP store, so the second instance has no stored fingerprint to compare and the conflict is only
    /// found once the command reaches the shared database.
    /// </summary>
    [Fact]
    public async Task The_Same_Key_With_Another_Amount_On_Another_Instance_Should_Be_Rejected_By_The_Database_Layer()
    {
        await using PaymentsHost first = await PaymentsHost.StartAsync(postgres.ConnectionString);
        await using PaymentsHost second = await PaymentsHost.StartAsync(postgres.ConnectionString);

        using HttpResponseMessage original = await first.CreatePaymentsClient(_merchant).CaptureAsync(new CapturePaymentRequest("order-4", 10m), "order-4-capture");
        using HttpResponseMessage changed = await second.CreatePaymentsClient(_merchant).CaptureAsync(new CapturePaymentRequest("order-4", 99m), "order-4-capture");

        Assert.Equal(HttpStatusCode.OK, original.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, changed.StatusCode);
        using JsonDocument problem = JsonDocument.Parse(await changed.Content.ReadAsStringAsync());
        Assert.Equal(IdempotencyErrorCodes.KeyReused, problem.RootElement.GetProperty("code").GetString());
        Assert.Equal(1, await CountPaymentsAsync());
    }

    private async Task<int> CountPaymentsAsync()
    {
        await using PaymentsDbContext db = postgres.CreateContext();
        return await db.Payments.CountAsync(payment => payment.MerchantId == _merchant);
    }
}
