using System.Net;

namespace CSharpEssentials.Http;

internal static class HttpRedirectRules
{
    private const string CookieHeader = "Cookie";

    internal static bool IsRedirect(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Found or HttpStatusCode.SeeOther
            or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;

    internal static bool TrySwitchToGet(HttpRequestMessage request, HttpStatusCode statusCode)
    {
        bool switchToGet = statusCode is HttpStatusCode.MovedPermanently or HttpStatusCode.Found
            ? request.Method == HttpMethod.Post
            : statusCode == HttpStatusCode.SeeOther && request.Method != HttpMethod.Get && request.Method != HttpMethod.Head;

        if (!switchToGet)
            return false;

        request.Method = HttpMethod.Get;
        request.Content = null;
        request.Headers.TransferEncodingChunked = false;
        return true;
    }

    internal static void StripCredentialsIfCrossOrigin(HttpRequestMessage request, Uri previousUri, Uri location)
    {
        if (IsSameOrigin(previousUri, location))
            return;

        request.Headers.Authorization = null;
        request.Headers.ProxyAuthorization = null;
        request.Headers.Remove(CookieHeader);
    }

    private static bool IsSameOrigin(Uri first, Uri second) =>
        Uri.Compare(first, second, UriComponents.SchemeAndServer, UriFormat.UriEscaped, StringComparison.OrdinalIgnoreCase) == 0;
}
