using System.Net;

namespace CSharpEssentials.Http;

internal static class HttpRedirectRules
{
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
}
