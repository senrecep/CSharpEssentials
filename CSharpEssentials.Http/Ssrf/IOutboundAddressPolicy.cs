#if NET9_0_OR_GREATER
using System.Net;

namespace CSharpEssentials.Http;

public interface IOutboundAddressPolicy
{
    bool IsAllowed(IPAddress address, Uri requestUri);
}
#endif
