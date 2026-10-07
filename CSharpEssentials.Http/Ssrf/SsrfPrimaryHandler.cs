#if NET9_0_OR_GREATER
namespace CSharpEssentials.Http;

// Handlers registered after AddSsrfGuard run between the guard and the primary handler and may rewrite the
// request, so the version cap and the request policy are enforced again right before the socket handler.
internal sealed class SsrfPrimaryHandler(SocketsHttpHandler innerHandler, IOutboundRequestPolicy requestPolicy)
    : DelegatingHandler(innerHandler)
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        SsrfGuardHandler.EnsureRequestAllowed(request, requestPolicy);
        SsrfGuardHandler.PreventHttp3(request);
        return base.SendAsync(request, cancellationToken);
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) =>
        throw new NotSupportedException("The SSRF guard supports only asynchronous requests. Use SendAsync.");
}
#endif
