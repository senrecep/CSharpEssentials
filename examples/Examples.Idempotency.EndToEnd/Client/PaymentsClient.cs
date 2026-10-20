using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;

using Examples.Idempotency.EndToEnd.App;

namespace Examples.Idempotency.EndToEnd.Client;

/// <summary>
/// The merchant's client of <c>POST /payments</c>. The caller picks the key once per logical payment, for example
/// from its own order id, and every retry of that payment sends the same key; <see cref="IdempotentRetryHandler"/>
/// retries the request after a lost response, and the server answers the retry from its idempotency record.
/// </summary>
public sealed class PaymentsClient(HttpClient http, string merchantId)
{
    public async Task<HttpResponseMessage> CaptureAsync(CapturePaymentRequest payment, string idempotencyKey, CancellationToken cancellationToken = default)
    {
        Result<HttpRequestMessage> request = HttpRequestBuilder.Post("payments")
            .WithJsonContent(payment)
            .WithIdempotencyKey(idempotencyKey)
            .WithHeader(MerchantAuthenticationHandler.HeaderName, merchantId)
            .Build();
        using HttpRequestMessage message = request.Value;
        return await http.SendAsync(message, cancellationToken);
    }
}
