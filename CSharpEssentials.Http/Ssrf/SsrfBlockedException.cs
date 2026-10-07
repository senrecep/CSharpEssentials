#if NET9_0_OR_GREATER
using System.Net;

namespace CSharpEssentials.Http;

public sealed class SsrfBlockedException : HttpRequestException
{
    public SsrfBlockedException()
        : this(SsrfBlockReason.RequestNotAllowed, "The outbound request was blocked by the SSRF guard.")
    {
    }

    public SsrfBlockedException(string message)
        : this(SsrfBlockReason.RequestNotAllowed, message)
    {
    }

    public SsrfBlockedException(string message, Exception innerException)
        : this(SsrfBlockReason.RequestNotAllowed, message, null, innerException)
    {
    }

    public SsrfBlockedException(
        SsrfBlockReason reason,
        string message,
        Uri? requestUri = null,
        Exception? innerException = null,
        IPAddress? blockedAddress = null)
        : base(message, innerException)
    {
        Reason = reason;
        RequestUri = requestUri;
        BlockedAddress = blockedAddress;
    }

    public SsrfBlockReason Reason { get; }

    public Uri? RequestUri { get; }

    public IPAddress? BlockedAddress { get; }
}
#endif
