namespace Examples.Idempotency.EndToEnd;

/// <summary>
/// Loses the response of the first request after the server has processed it, the way a timeout or a reset
/// connection does: the client cannot tell whether the payment happened.
/// </summary>
public sealed class DroppedResponseHandler(HttpMessageHandler innerHandler) : DelegatingHandler(innerHandler)
{
    private int _attempts;

    public int Attempts => _attempts;

    public string? DroppedBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        if (Interlocked.Increment(ref _attempts) > 1)
            return response;

        DroppedBody = await response.Content.ReadAsStringAsync(cancellationToken);
        response.Dispose();
        throw new HttpRequestException("The connection was reset before the response arrived.");
    }
}
