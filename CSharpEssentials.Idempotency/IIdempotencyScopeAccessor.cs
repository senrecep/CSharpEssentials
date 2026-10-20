using System.Security.Claims;

namespace CSharpEssentials.Idempotency;

/// <summary>
/// Gives a pipeline that has no request context of its own the principal of the current caller, so an
/// <see cref="IIdempotencyScopeResolver"/> can resolve its scope. <c>CSharpEssentials.AspNetCore</c> registers one
/// that reads <c>HttpContext.User</c>.
/// </summary>
public interface IIdempotencyScopeAccessor
{
    /// <summary>The current caller, or <see langword="null"/> when there is none.</summary>
    ClaimsPrincipal? Principal { get; }
}
