#if NET9_0_OR_GREATER
using System.Net;

namespace CSharpEssentials.Http;

internal sealed class SsrfGuardHandler(SsrfGuardOptions options, IOutboundRequestPolicy requestPolicy) : DelegatingHandler
{
    private const string CookieHeader = "Cookie";

    private volatile bool _primaryHandlerVerified;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsurePrimaryHandlerIsGuarded();

        CancellationTokenSource? timeout = null;
        if (options.Timeout is { } limit)
        {
            timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(limit);
        }

        try
        {
            HttpResponseMessage response = await SendWithRedirectsAsync(request, timeout?.Token ?? cancellationToken).ConfigureAwait(false);
            return Guard(response, request.RequestUri, ref timeout);
        }
        catch (OperationCanceledException ex) when (timeout is { IsCancellationRequested: true } && !cancellationToken.IsCancellationRequested)
        {
            throw CreateTimeoutException(request.RequestUri, ex);
        }
        catch (HttpRequestException ex) when (ex is not SsrfBlockedException && FindBlockedException(ex) is { } blocked)
        {
            throw new SsrfBlockedException(blocked.Reason, blocked.Message, blocked.RequestUri, ex, blocked.BlockedAddress);
        }
        finally
        {
            timeout?.Dispose();
        }
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken) =>
        throw new NotSupportedException("The SSRF guard supports only asynchronous requests. Use SendAsync.");

    internal static SsrfBlockedException CreateTimeoutException(Uri? requestUri, Exception innerException) =>
        new(SsrfBlockReason.Timeout, "The outbound request exceeded the SSRF guard timeout.", requestUri, new TimeoutException(innerException.Message, innerException));

    private async Task<HttpResponseMessage> SendWithRedirectsAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        int redirects = 0;
        while (true)
        {
            Uri requestUri = EnsureRequestAllowed(request, requestPolicy);
            PreventHttp3(request);
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            Uri? location = GetRedirectLocation(response, requestUri);
            if (location is null)
                return response;

            response.Dispose();

            if (redirects >= options.MaxRedirects)
            {
                throw new SsrfBlockedException(
                    SsrfBlockReason.RedirectLimitExceeded,
                    $"The redirect limit of {options.MaxRedirects} was exceeded. Last URI: {DescribeUri(requestUri)}",
                    requestUri);
            }

            redirects++;
            PrepareRedirect(request, response.StatusCode, requestUri, location);
        }
    }

    internal static Uri EnsureRequestAllowed(HttpRequestMessage request, IOutboundRequestPolicy requestPolicy)
    {
        Uri requestUri = request.RequestUri
            ?? throw new SsrfBlockedException(SsrfBlockReason.RequestNotAllowed, "The request has no URI.");

        if (!requestUri.IsAbsoluteUri || !requestPolicy.IsAllowed(requestUri))
        {
            throw new SsrfBlockedException(
                SsrfBlockReason.RequestNotAllowed,
                $"The request to '{DescribeUri(requestUri)}' is not allowed by the outbound request policy.",
                requestUri);
        }

        return requestUri;
    }

    // HTTP/3 runs over QUIC, which never calls ConnectCallback, so it would skip the address check.
    // Capping every hop at HTTP/2 with RequestVersionOrLower also rules out an Alt-Svc upgrade to HTTP/3.
    internal static void PreventHttp3(HttpRequestMessage request)
    {
        if (request.Version > HttpVersion.Version20)
        {
            request.Version = HttpVersion.Version20;
            request.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        }

        if (request.VersionPolicy == HttpVersionPolicy.RequestVersionOrHigher)
            request.VersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
    }

    // Query strings and user info often carry tokens, so messages show only scheme, host, port and path.
    private static string DescribeUri(Uri uri) =>
        uri.IsAbsoluteUri
            ? uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.UriEscaped)
            : uri.OriginalString[..GetPathLength(uri.OriginalString)];

    private static int GetPathLength(string uri)
    {
        int end = uri.AsSpan().IndexOfAny('?', '#');
        return end < 0 ? uri.Length : end;
    }

    private static Uri? GetRedirectLocation(HttpResponseMessage response, Uri requestUri)
    {
        if (response.StatusCode is not (HttpStatusCode.MovedPermanently or HttpStatusCode.Found or HttpStatusCode.SeeOther
            or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect))
            return null;

        Uri? location = response.Headers.Location;
        if (location is null)
            return null;

        return location.IsAbsoluteUri ? location : new Uri(requestUri, location);
    }

    private static void PrepareRedirect(HttpRequestMessage request, HttpStatusCode statusCode, Uri previousUri, Uri location)
    {
        bool switchToGet = statusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Found
            ? request.Method == HttpMethod.Post
            : statusCode == HttpStatusCode.SeeOther && request.Method != HttpMethod.Get && request.Method != HttpMethod.Head;

        if (switchToGet)
        {
            request.Method = HttpMethod.Get;
            request.Content = null;
            request.Headers.TransferEncodingChunked = false;
        }

        if (!IsSameOrigin(previousUri, location))
        {
            request.Headers.Authorization = null;
            request.Headers.ProxyAuthorization = null;
            request.Headers.Remove(CookieHeader);
        }

        request.RequestUri = location;
    }

    private static bool IsSameOrigin(Uri first, Uri second) =>
        Uri.Compare(first, second, UriComponents.SchemeAndServer, UriFormat.UriEscaped, StringComparison.OrdinalIgnoreCase) == 0;

    private HttpResponseMessage Guard(HttpResponseMessage response, Uri? requestUri, ref CancellationTokenSource? timeout)
    {
        long? maxLength = options.MaxResponseContentLength;
        if (maxLength is null && timeout is null)
            return response;

        if (maxLength is { } max && response.Content.Headers.ContentLength is { } declared && declared > max)
        {
            response.Dispose();
            throw new SsrfBlockedException(
                SsrfBlockReason.ResponseTooLarge,
                $"The response declared {declared} bytes, which exceeds the limit of {max} bytes.",
                requestUri);
        }

        response.Content = new SsrfGuardedContent(response.Content, maxLength, timeout, requestUri);
        timeout = null;
        return response;
    }

    private static SsrfBlockedException? FindBlockedException(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SsrfBlockedException blocked)
                return blocked;
        }

        return null;
    }

    private void EnsurePrimaryHandlerIsGuarded()
    {
        if (_primaryHandlerVerified)
            return;

        HttpMessageHandler? handler = InnerHandler;
        while (handler is not SsrfPrimaryHandler and DelegatingHandler delegating)
            handler = delegating.InnerHandler;

        if (handler is not SsrfPrimaryHandler
            {
                InnerHandler: SocketsHttpHandler { UseProxy: false, AllowAutoRedirect: false, UseCookies: false, ConnectCallback.Target: SsrfConnector },
            })
        {
            throw new InvalidOperationException(
                "The SSRF guard requires the primary handler configured by AddSsrfGuard: a guarded SocketsHttpHandler with the SSRF ConnectCallback, " +
                "no proxy, no automatic redirects and no cookie container. Do not replace or reconfigure the primary handler after AddSsrfGuard.");
        }

        _primaryHandlerVerified = true;
    }
}
#endif
