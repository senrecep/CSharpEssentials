#if NET9_0_OR_GREATER
namespace CSharpEssentials.Http;

public enum SsrfBlockReason
{
    RequestNotAllowed = 1,
    HostNotAllowed = 2,
    AddressNotAllowed = 3,
    RedirectLimitExceeded = 4,
    ResponseTooLarge = 5,
    Timeout = 6,
}
#endif
