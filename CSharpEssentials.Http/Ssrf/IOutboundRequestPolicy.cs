#if NET9_0_OR_GREATER
namespace CSharpEssentials.Http;

public interface IOutboundRequestPolicy
{
    bool IsAllowed(Uri requestUri);
}
#endif
