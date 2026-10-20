using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;

using CSharpEssentials.Idempotency;

using Examples.Idempotency.EndToEnd.App;
using Examples.Idempotency.EndToEnd.Client;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Examples.Idempotency.EndToEnd;

/// <summary>
/// One running instance of <see cref="PaymentsApi"/> on an in-memory test server. Two hosts on the same connection
/// string are two instances of the service behind a load balancer: they share the database and nothing else.
/// </summary>
public sealed class PaymentsHost : IAsyncDisposable
{
    private readonly WebApplication _app;
    private readonly byte[] _webhookSecret;
    private readonly ConcurrentBag<HttpClient> _clients = [];

    private PaymentsHost(WebApplication app, byte[] webhookSecret)
    {
        _app = app;
        _webhookSecret = webhookSecret;
    }

    public IServiceProvider Services => _app.Services;

    public MessageBus Bus => _app.Services.GetRequiredService<MessageBus>();

    private TestServer Server => (TestServer)_app.Services.GetRequiredService<IServer>();

    public static async Task<PaymentsHost> StartAsync(
        string connectionString,
        IdempotencyMode paymentsMode = IdempotencyMode.Enforce,
        IPaymentProvider? provider = null)
    {
        byte[] webhookSecret = RandomNumberGenerator.GetBytes(32);
        WebApplication app = PaymentsApi.Create(builder =>
        {
            builder.WebHost.UseTestServer();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:" + PaymentsApi.ConnectionStringName] = connectionString,
                [PaymentsApi.PaymentsModeKey] = paymentsMode.ToString(),
                [PaymentsApi.WebhookSecretKey] = Convert.ToBase64String(webhookSecret),
            });
            if (provider is not null)
                builder.Services.AddSingleton(provider);
        });
        await app.StartAsync();
        return new PaymentsHost(app, webhookSecret);
    }

    /// <summary>
    /// A merchant's client of this instance. <paramref name="network"/> sits between the retry handler and the server,
    /// where a real network would lose responses.
    /// </summary>
    public PaymentsClient CreatePaymentsClient(string merchantId, Func<HttpMessageHandler, HttpMessageHandler>? network = null)
    {
        HttpMessageHandler server = Server.CreateHandler();
        HttpClient http = new(new IdempotentRetryHandler(network is null ? server : network(server)))
        {
            BaseAddress = Server.BaseAddress,
        };
        _clients.Add(http);
        return new PaymentsClient(http, merchantId);
    }

    /// <summary>
    /// Sends <c>POST /payments</c> with <paramref name="keyHeader"/> as the raw <c>Idempotency-Key</c> value, for
    /// clients that write the key as a structured-field string (<c>"abc"</c>), which <see cref="PaymentsClient"/> does
    /// not.
    /// </summary>
    public async Task<HttpResponseMessage> CaptureWithKeyHeaderAsync(string merchantId, CapturePaymentRequest payment, string keyHeader)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, "payments")
        {
            Content = JsonContent.Create(payment),
        };
        request.Headers.TryAddWithoutValidation("Idempotency-Key", keyHeader);
        request.Headers.Add(MerchantAuthenticationHandler.HeaderName, merchantId);
        using HttpClient http = Server.CreateClient();
        return await http.SendAsync(request);
    }

    /// <summary>
    /// Changes <see cref="PaymentsApi.PaymentsModeKey"/> and reloads the configuration, the way an edited
    /// configuration source does, without restarting the instance.
    /// </summary>
    public void SwitchPaymentsMode(IdempotencyMode mode)
    {
        _app.Configuration[PaymentsApi.PaymentsModeKey] = mode.ToString();
        ((IConfigurationRoot)_app.Configuration).Reload();
    }

    /// <summary>Delivers a webhook the way the payment provider does: a JSON body signed with the shared secret.</summary>
    public async Task<HttpResponseMessage> DeliverWebhookAsync(ProviderEvent providerEvent, bool signed = true)
    {
        byte[] body = JsonSerializer.SerializeToUtf8Bytes(providerEvent, JsonSerializerOptions.Web);
        using HttpRequestMessage request = new(HttpMethod.Post, "webhooks/payment-provider")
        {
            Content = new ByteArrayContent(body),
        };
        request.Content.Headers.ContentType = new("application/json");
        request.Headers.Add(WebhookSignature.HeaderName, signed ? WebhookSignature.Compute(_webhookSecret, body) : "0000");
        using HttpClient http = Server.CreateClient();
        return await http.SendAsync(request);
    }

    /// <summary>Waits for the consumer to acknowledge the next delivery.</summary>
    public async Task<MessageDelivery> NextAcknowledgementAsync()
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(10));
        return await Bus.Acknowledgements.ReadAsync(timeout.Token);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (HttpClient client in _clients)
            client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }
}
