using System.Net;
using System.Text.Json;

using Examples.Idempotency.EndToEnd.App;

using Microsoft.EntityFrameworkCore;

namespace Examples.Idempotency.EndToEnd;

/// <summary>The payment provider delivers its webhooks at least once.</summary>
[Collection(PostgresCollection.Name)]
public sealed class WebhookTests(PostgresFixture postgres)
{
    private readonly string _account = "acct-" + Guid.NewGuid().ToString("N");

    [Fact]
    public async Task The_Same_Event_Delivered_Twice_Should_Record_One_Settlement_And_Acknowledge_Both()
    {
        await using PaymentsHost host = await PaymentsHost.StartAsync(postgres.ConnectionString);
        ProviderEvent settlement = new("evt_" + Guid.NewGuid().ToString("N"), PaymentsApi.SettlementEventType, _account, 120m);

        using HttpResponseMessage first = await host.DeliverWebhookAsync(settlement);
        using HttpResponseMessage resent = await host.DeliverWebhookAsync(settlement);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, resent.StatusCode);
        Assert.Equal(await SettlementIdAsync(first), await SettlementIdAsync(resent));
        Assert.Equal(1, await CountSettlementsAsync());
    }

    [Fact]
    public async Task An_Unsigned_Event_Should_Be_Rejected_Without_Claiming_The_Event_Id()
    {
        await using PaymentsHost host = await PaymentsHost.StartAsync(postgres.ConnectionString);
        ProviderEvent settlement = new("evt_" + Guid.NewGuid().ToString("N"), PaymentsApi.SettlementEventType, _account, 75m);

        using HttpResponseMessage forged = await host.DeliverWebhookAsync(settlement, signed: false);
        using HttpResponseMessage genuine = await host.DeliverWebhookAsync(settlement);

        Assert.Equal(HttpStatusCode.Unauthorized, forged.StatusCode);
        Assert.Equal(HttpStatusCode.OK, genuine.StatusCode);
        Assert.Equal(1, await CountSettlementsAsync());
    }

    [Fact]
    public void A_Signature_Should_Be_Compared_As_Bytes_Whatever_The_Hex_Case()
    {
        byte[] secret = [1, 2, 3, 4];
        byte[] body = "{}"u8.ToArray();
        string signature = WebhookSignature.Compute(secret, body);

        Assert.True(WebhookSignature.IsValid(secret, body, signature));
        Assert.True(WebhookSignature.IsValid(secret, body, signature.ToUpperInvariant()));
        Assert.False(WebhookSignature.IsValid(secret, body, "not-hex"));
        Assert.False(WebhookSignature.IsValid(secret, body, signature[..^2]));
        Assert.False(WebhookSignature.IsValid(secret, body, null));
    }

    private static async Task<Guid> SettlementIdAsync(HttpResponseMessage response)
    {
        using JsonDocument body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("settlementId").GetGuid();
    }

    private async Task<int> CountSettlementsAsync()
    {
        await using PaymentsDbContext db = postgres.CreateContext();
        return await db.Settlements.CountAsync(settlement => settlement.AccountId == _account);
    }
}
