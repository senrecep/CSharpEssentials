#if NET9_0_OR_GREATER
using System.Net;

namespace CSharpEssentials.Http;

public sealed class SsrfGuardOptions
{
    public bool AllowHttp { get; set; }

    public ISet<int> AllowedPorts { get; } = new HashSet<int>();

    public int MaxRedirects
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            field = value;
        }
    } = 3;

    public long? MaxResponseContentLength
    {
        get;
        set
        {
            if (value is { } length)
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
            field = value;
        }
    }

    public TimeSpan? Timeout
    {
        get;
        set
        {
            if (value is { } timeout)
                ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(timeout, TimeSpan.Zero);
            field = value;
        }
    }

    public ICollection<IPNetwork> AllowedNetworks { get; } = [];

    public ICollection<IPNetwork> BlockedNetworks { get; } = [];

    public ICollection<string> AllowedHosts { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public ICollection<string> BlockedHosts { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public IOutboundAddressPolicy? AddressPolicy { get; set; }

    public IOutboundRequestPolicy? RequestPolicy { get; set; }

    internal Func<string, CancellationToken, Task<IPAddress[]>> ResolveHostAsync { get; set; } = Dns.GetHostAddressesAsync;
}
#endif
