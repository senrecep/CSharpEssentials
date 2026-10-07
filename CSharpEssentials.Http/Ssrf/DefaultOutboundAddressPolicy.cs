#if NET9_0_OR_GREATER
using System.Net;
using System.Net.Sockets;

namespace CSharpEssentials.Http;

public sealed class DefaultOutboundAddressPolicy : IOutboundAddressPolicy
{
    private static readonly IPNetwork[] BlockedIPv4Networks =
    [
        IPNetwork.Parse("0.0.0.0/8"),
        IPNetwork.Parse("10.0.0.0/8"),
        IPNetwork.Parse("100.64.0.0/10"),
        IPNetwork.Parse("127.0.0.0/8"),
        IPNetwork.Parse("169.254.0.0/16"),
        IPNetwork.Parse("172.16.0.0/12"),
        IPNetwork.Parse("192.0.0.0/24"),
        IPNetwork.Parse("192.0.2.0/24"),
        IPNetwork.Parse("192.168.0.0/16"),
        IPNetwork.Parse("198.18.0.0/15"),
        IPNetwork.Parse("198.51.100.0/24"),
        IPNetwork.Parse("203.0.113.0/24"),
        IPNetwork.Parse("224.0.0.0/4"),
        IPNetwork.Parse("240.0.0.0/4"),
    ];

    private static readonly IPNetwork[] BlockedIPv6Networks =
    [
        IPNetwork.Parse("::/128"),
        IPNetwork.Parse("::1/128"),
        IPNetwork.Parse("fc00::/7"),
        IPNetwork.Parse("fe80::/10"),
        IPNetwork.Parse("fec0::/10"),
        IPNetwork.Parse("ff00::/8"),
        IPNetwork.Parse("2001::/32"),
        IPNetwork.Parse("2001:db8::/32"),
        IPNetwork.Parse("64:ff9b:1::/48"),
    ];

    private static readonly IPNetwork IPv4CompatibleNetwork = IPNetwork.Parse("::/96");
    private static readonly IPNetwork UnspecifiedAndLoopbackNetwork = IPNetwork.Parse("::/127");
    private static readonly IPNetwork Nat64Network = IPNetwork.Parse("64:ff9b::/96");
    private static readonly IPNetwork SixToFourNetwork = IPNetwork.Parse("2002::/16");

    public static DefaultOutboundAddressPolicy Instance { get; } = new();

    public bool IsAllowed(IPAddress address, Uri requestUri)
    {
        ArgumentNullException.ThrowIfNull(address);
        return IsPublic(Normalize(address));
    }

    internal static IPAddress Normalize(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetworkV6)
            return address;

        if (address.IsIPv4MappedToIPv6)
            return address.MapToIPv4();

        Span<byte> bytes = stackalloc byte[16];
        address.TryWriteBytes(bytes, out _);

        // :: and ::1 are the IPv6 unspecified and loopback addresses, not IPv4-compatible forms of 0.0.0.0 and 0.0.0.1.
        bool ipv4Compatible = IPv4CompatibleNetwork.Contains(address) && !UnspecifiedAndLoopbackNetwork.Contains(address);
        if (ipv4Compatible || Nat64Network.Contains(address))
            return new IPAddress(bytes[12..16]);

        if (SixToFourNetwork.Contains(address))
            return new IPAddress(bytes[2..6]);

        return address;
    }

    private static bool IsPublic(IPAddress address)
    {
        if (address.AddressFamily == AddressFamily.InterNetwork)
            return !ContainsAny(BlockedIPv4Networks, address);

        return address.AddressFamily == AddressFamily.InterNetworkV6 && !ContainsAny(BlockedIPv6Networks, address);
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
}
#endif
