#if NET9_0_OR_GREATER
namespace CSharpEssentials.Http;

public sealed class DefaultOutboundRequestPolicy : IOutboundRequestPolicy
{
    private const int HttpsPort = 443;
    private const int HttpPort = 80;

    private readonly bool _allowHttp;
    private readonly HashSet<int> _allowedPorts;

    public DefaultOutboundRequestPolicy(SsrfGuardOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _allowHttp = options.AllowHttp;
        _allowedPorts = [.. options.AllowedPorts];
        if (_allowedPorts.Count > 0)
            return;

        _allowedPorts.Add(HttpsPort);
        if (options.AllowHttp)
            _allowedPorts.Add(HttpPort);
    }

    public bool IsAllowed(Uri requestUri)
    {
        ArgumentNullException.ThrowIfNull(requestUri);

        if (!requestUri.IsAbsoluteUri)
            return false;

        bool schemeAllowed = requestUri.Scheme == Uri.UriSchemeHttps
            || _allowHttp && requestUri.Scheme == Uri.UriSchemeHttp;

        return schemeAllowed && _allowedPorts.Contains(requestUri.Port);
    }
}
#endif
