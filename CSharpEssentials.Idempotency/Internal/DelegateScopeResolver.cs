using System.Security.Claims;

namespace CSharpEssentials.Idempotency;

internal sealed class DelegateScopeResolver(Func<ClaimsPrincipal?, IdempotencyScope?> resolve) : IIdempotencyScopeResolver
{
    public IdempotencyScope? Resolve(ClaimsPrincipal? principal) => resolve(principal);
}
