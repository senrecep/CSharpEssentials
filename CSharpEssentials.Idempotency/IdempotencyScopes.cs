using System.Security.Claims;

namespace CSharpEssentials.Idempotency;

/// <summary>
/// Ready-made <see cref="IIdempotencyScopeResolver"/>s. The claim-based resolvers need an authenticated principal and
/// return <see langword="null"/> (unresolved) for an anonymous one or when the claim is missing or empty.
/// </summary>
public static class IdempotencyScopes
{
    /// <summary>JWT subject claim, read when <see cref="ClaimTypes.NameIdentifier"/> is missing.</summary>
    public const string SubjectClaimType = "sub";

    /// <summary>OAuth client id claim of a client-credentials token.</summary>
    public const string ClientIdClaimType = "client_id";

    /// <summary>OpenID Connect authorized party claim, read when <see cref="ClientIdClaimType"/> is missing.</summary>
    public const string AuthorizedPartyClaimType = "azp";

    /// <summary>The user, <c>user:{id}</c>: <see cref="ClaimTypes.NameIdentifier"/>, then <c>sub</c>.</summary>
    public static IIdempotencyScopeResolver User { get; } =
        new DelegateScopeResolver(static principal => FindUser(principal) is { } id ? IdempotencyScope.User(id) : null);

    /// <summary>
    /// The calling client, <c>client:{id}</c>: <c>client_id</c>, then <c>azp</c>. Meant for machine principals (client credentials), where
    /// there is no user. Choose it explicitly: it lets every user of the same client share a key space.
    /// </summary>
    public static IIdempotencyScopeResolver Client { get; } =
        new DelegateScopeResolver(static principal =>
            (FindClaim(principal, ClientIdClaimType) ?? FindClaim(principal, AuthorizedPartyClaimType)) is { } id
                ? IdempotencyScope.Client(id)
                : null);

    /// <summary>
    /// <see cref="IdempotencyScope.Global"/> (stored as <c>global</c>) for every caller, anonymous ones included: any caller that sends the same
    /// key gets the same result. Use it only for keys that are already unique across callers, such as message ids.
    /// </summary>
    public static IIdempotencyScopeResolver Global { get; } =
        new DelegateScopeResolver(static _ => IdempotencyScope.Global);

    /// <summary>The tenant read from <paramref name="claimType"/>, <c>tenant:{id}</c>.</summary>
    public static IIdempotencyScopeResolver Tenant(string claimType)
    {
        Guard.NotNullOrEmpty(claimType);
        return new DelegateScopeResolver(principal => FindClaim(principal, claimType) is { } id ? IdempotencyScope.Tenant(id) : null);
    }

    /// <summary>
    /// The user (as <see cref="User"/>) within the tenant read from <paramref name="tenantClaimType"/>. Both must be
    /// present. The scope is <see cref="IdempotencyScope.UserAndTenant"/>: <c>{n}:tenant:{tenantId}/user:{userId}</c>,
    /// where <c>{n}</c> is the UTF-8 byte length of the tenant part, so no user and tenant pair can produce the scope of
    /// another pair.
    /// </summary>
    public static IIdempotencyScopeResolver UserAndTenant(string tenantClaimType)
    {
        Guard.NotNullOrEmpty(tenantClaimType);
        return new DelegateScopeResolver(principal =>
        {
            string? user = FindUser(principal);
            string? tenant = FindClaim(principal, tenantClaimType);
            return user is null || tenant is null ? null : IdempotencyScope.UserAndTenant(user, tenant);
        });
    }

    /// <summary>
    /// The scope <see cref="IdempotencyScope.Custom"/>, <c>{kind}:{value}</c>, where <paramref name="resolve"/> computes the value. A <see langword="null"/> or
    /// empty value means unresolved; implement <see cref="IIdempotencyScopeResolver"/> to return
    /// <see cref="IdempotencyScope.Global"/> as well.
    /// </summary>
    /// <param name="kind">
    /// Names the scope, for example <c>account</c>. The same rules as <see cref="IdempotencyScope.Custom"/>: not empty,
    /// no <c>:</c> or <c>/</c>, not only digits, no unpaired surrogate, and not a reserved built-in kind (<c>user</c>,
    /// <c>tenant</c>, <c>client</c>, <c>global</c>, in any casing).
    /// </param>
    /// <param name="resolve">Returns the value for the caller.</param>
    /// <exception cref="ArgumentException"><paramref name="kind"/> is not valid.</exception>
    public static IIdempotencyScopeResolver Custom(string kind, Func<ClaimsPrincipal?, string?> resolve)
    {
        IdempotencyScope.ValidateKind(kind);
        Guard.NotNull(resolve);
        return new DelegateScopeResolver(principal =>
            resolve(principal) is { Length: > 0 } value ? IdempotencyScope.Custom(kind, value) : null);
    }

    private static string? FindUser(ClaimsPrincipal? principal) =>
        FindClaim(principal, ClaimTypes.NameIdentifier) ?? FindClaim(principal, SubjectClaimType);

    private static string? FindClaim(ClaimsPrincipal? principal, string claimType)
    {
        if (principal?.Identity?.IsAuthenticated != true)
            return null;
        string? value = principal.FindFirst(claimType)?.Value;
        return string.IsNullOrEmpty(value) ? null : value;
    }
}
