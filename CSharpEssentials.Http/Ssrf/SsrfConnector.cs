#if NET9_0_OR_GREATER
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace CSharpEssentials.Http;

internal sealed class SsrfConnector(SsrfGuardOptions options, IOutboundAddressPolicy addressPolicy)
{
    private const string WildcardPrefix = "*.";

    private readonly string[] _allowedHosts = NormalizePatterns(options.AllowedHosts);
    private readonly string[] _blockedHosts = NormalizePatterns(options.BlockedHosts);
    private readonly IPNetwork[] _allowedNetworks = [.. options.AllowedNetworks];
    private readonly IPNetwork[] _blockedNetworks = [.. options.BlockedNetworks];

    public async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        string host = context.DnsEndPoint.Host;
        int port = context.DnsEndPoint.Port;
        Uri requestUri = context.InitialRequestMessage.RequestUri ?? new UriBuilder(Uri.UriSchemeHttps, host, port).Uri;
        string normalizedHost = NormalizeHost(host);

        if (MatchesHost(_blockedHosts, normalizedHost))
            throw new SsrfBlockedException(SsrfBlockReason.HostNotAllowed, $"The host '{host}' is blocked.", requestUri);

        IPAddress[] addresses = IPAddress.TryParse(host.Trim('[', ']'), out IPAddress? literal)
            ? [literal]
            : await options.ResolveHostAsync(host, cancellationToken).ConfigureAwait(false);

        if (addresses.Length == 0)
            throw new SocketException((int)SocketError.HostNotFound);

        bool hostAllowed = MatchesHost(_allowedHosts, normalizedHost);
        foreach (IPAddress address in addresses)
        {
            if (!IsAllowed(address, requestUri, hostAllowed))
            {
                throw new SsrfBlockedException(
                    SsrfBlockReason.AddressNotAllowed,
                    $"The host '{host}' resolved to an address that is not allowed.",
                    requestUri,
                    blockedAddress: address);
            }
        }

        return await ConnectToAnyAsync(addresses, port, cancellationToken).ConfigureAwait(false);
    }

    private bool IsAllowed(IPAddress address, Uri requestUri, bool hostAllowed)
    {
        IPAddress normalized = DefaultOutboundAddressPolicy.Normalize(address);

        if (ContainsAny(_blockedNetworks, address) || ContainsAny(_blockedNetworks, normalized))
            return false;

        if (hostAllowed || ContainsAny(_allowedNetworks, address))
            return true;

        // Both forms must pass so a policy that inspects only one of them cannot be bypassed by the other.
        return addressPolicy.IsAllowed(normalized, requestUri)
            && (normalized.Equals(address) || addressPolicy.IsAllowed(address, requestUri));
    }

    private static async ValueTask<Stream> ConnectToAnyAsync(IPAddress[] addresses, int port, CancellationToken cancellationToken)
    {
        SocketException? lastError = null;
        foreach (IPAddress address in addresses)
        {
            Socket? socket = null;
            try
            {
                socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
                await socket.ConnectAsync(new IPEndPoint(address, port), cancellationToken).ConfigureAwait(false);
                var stream = new NetworkStream(socket, ownsSocket: true);
                socket = null;
                return stream;
            }
            catch (SocketException ex)
            {
                lastError = ex;
            }
            finally
            {
                socket?.Dispose();
            }
        }

        throw lastError ?? new SocketException((int)SocketError.HostNotFound);
    }

    private static bool ContainsAny(IPNetwork[] networks, IPAddress address)
    {
        foreach (IPNetwork network in networks)
        {
            if (network.Contains(address))
                return true;
        }

        return false;
    }


    private static bool MatchesHost(string[] patterns, string host)
    {
        foreach (string pattern in patterns)
        {
            if (string.Equals(pattern, host, StringComparison.Ordinal))
                return true;

            if (pattern.StartsWith(WildcardPrefix, StringComparison.Ordinal)
                && host.AsSpan().EndsWith(pattern.AsSpan(1), StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static string[] NormalizePatterns(ICollection<string> patterns) =>
    [
        .. patterns.Select(pattern => pattern.StartsWith(WildcardPrefix, StringComparison.Ordinal)
            ? WildcardPrefix + NormalizeHost(pattern[WildcardPrefix.Length..])
            : NormalizeHost(pattern)),
    ];

    private static string NormalizeHost(string host)
    {
        string trimmed = host.TrimEnd('.');
        try
        {
            return new IdnMapping().GetAscii(trimmed).ToUpperInvariant();
        }
        catch (ArgumentException)
        {
            return trimmed.ToUpperInvariant();
        }
    }
}
#endif
