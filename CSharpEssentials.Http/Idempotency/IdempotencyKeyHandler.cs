namespace CSharpEssentials.Http;

/// <summary>
/// Adds a generated <c>Idempotency-Key</c> to POST and PATCH requests that have none. Optional: setting the key on the
/// request with <see cref="HttpRequestMessageIdempotencyExtensions.SetIdempotencyKey"/> or
/// <see cref="HttpRequestBuilder.WithIdempotencyKey"/> gives the same result without a handler.
/// Register it before (outside) the resilience handler so that the key is set once per logical call.
/// </summary>
public sealed class IdempotencyKeyHandler : DelegatingHandler
{
    /// <summary>Creates the handler; the inner handler is set by the <c>IHttpClientFactory</c> pipeline.</summary>
    public IdempotencyKeyHandler()
    {
    }

    /// <summary>Creates the handler around <paramref name="innerHandler"/>.</summary>
    /// <param name="innerHandler">The handler that sends the request.</param>
    public IdempotencyKeyHandler(HttpMessageHandler innerHandler)
        : base(innerHandler)
    {
    }

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        EnsureKey(request);
        return base.SendAsync(request, cancellationToken);
    }

#if NET9_0_OR_GREATER
    /// <inheritdoc />
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        EnsureKey(request);
        return base.Send(request, cancellationToken);
    }
#endif

    private static void EnsureKey(HttpRequestMessage request)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(request);
#else
        if (request is null)
            throw new ArgumentNullException(nameof(request));
#endif

        if ((request.Method == HttpMethod.Post || request.Method == HttpMethod.Patch) && !IdempotencyHttpProtocol.HasKey(request))
            request.SetIdempotencyKey();
    }
}
