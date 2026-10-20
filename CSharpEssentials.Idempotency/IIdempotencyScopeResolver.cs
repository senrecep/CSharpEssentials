using System.Security.Claims;

namespace CSharpEssentials.Idempotency;

/// <summary>
/// Finds the owner of an idempotency key from the caller's principal, so callers cannot replay each other's results.
/// Ready-made resolvers are on <see cref="IdempotencyScopes"/>.
/// </summary>
public interface IIdempotencyScopeResolver
{
    /// <summary>
    /// The caller's scope, <see cref="IdempotencyScope.Global"/> when the key space is deliberately shared, or
    /// <see langword="null"/> when the scope cannot be resolved (for example an anonymous caller). The caller decides
    /// what an unresolved scope means; it is never treated as global silently.
    /// </summary>
    IdempotencyScope? Resolve(ClaimsPrincipal? principal);
}
