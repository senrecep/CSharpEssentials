using System.Security.Claims;
using CSharpEssentials.Idempotency;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>A scope resolver whose dependency is down.</summary>
internal sealed class ThrowingScopeResolver : IIdempotencyScopeResolver
{
    public IdempotencyScope? Resolve(ClaimsPrincipal? principal) =>
        throw new InvalidOperationException("The tenant directory is unavailable.");
}
