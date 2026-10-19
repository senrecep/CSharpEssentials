using System.Security.Claims;
using System.Text;

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
        new DelegateScopeResolver(static principal => Of("user", FindUser(principal)));

    /// <summary>
    /// The calling client, <c>client:{id}</c>: <c>client_id</c>, then <c>azp</c>. Meant for machine principals (client credentials), where
    /// there is no user. Choose it explicitly: it lets every user of the same client share a key space.
    /// </summary>
    public static IIdempotencyScopeResolver Client { get; } =
        new DelegateScopeResolver(static principal =>
            Of("client", FindClaim(principal, ClientIdClaimType) ?? FindClaim(principal, AuthorizedPartyClaimType)));

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
        return new DelegateScopeResolver(principal => Of("tenant", FindClaim(principal, claimType)));
    }

    /// <summary>
    /// The user (as <see cref="User"/>) within the tenant read from <paramref name="tenantClaimType"/>. Both must be
    /// present. With <c>tenant</c> = <c>tenant:{tenantId}</c> and <c>user</c> = <c>user:{userId}</c>, the scope is
    /// <c>{n}:{tenant}/{user}</c>, where <c>{n}</c> is the UTF-8 byte length of the tenant part, so no user and tenant pair
    /// can produce the scope of another pair.
    /// </summary>
    public static IIdempotencyScopeResolver UserAndTenant(string tenantClaimType)
    {
        Guard.NotNullOrEmpty(tenantClaimType);
        return new DelegateScopeResolver(principal =>
        {
            string? user = FindUser(principal);
            string? tenant = FindClaim(principal, tenantClaimType);
            if (user is null || tenant is null)
                return null;

            string tenantPart = "tenant:" + tenant;
            return IdempotencyScope.Of(
                LengthPrefixed.Append(new StringBuilder(tenantPart.Length + user.Length + 16), tenantPart)
                    .Append("/user:")
                    .Append(user)
                    .ToString());
        });
    }

    /// <summary>
    /// The scope <c>{kind}:{value}</c>, where <paramref name="resolve"/> computes the value. A <see langword="null"/> or
    /// empty value means unresolved; implement <see cref="IIdempotencyScopeResolver"/> to return
    /// <see cref="IdempotencyScope.Global"/> as well.
    /// </summary>
    /// <param name="kind">Names the scope, for example <c>account</c>. Must not be empty.</param>
    /// <param name="resolve">Returns the value for the caller.</param>
    /// <exception cref="ArgumentException"><paramref name="kind"/> is <see langword="null"/> or empty.</exception>
    public static IIdempotencyScopeResolver Custom(string kind, Func<ClaimsPrincipal?, string?> resolve)
    {
        Guard.NotNullOrEmpty(kind);
        Guard.NotNull(resolve);
        return new DelegateScopeResolver(principal => Of(kind, resolve(principal)));
    }

    private static IdempotencyScope? Of(string kind, string? value) =>
        string.IsNullOrEmpty(value) ? null : IdempotencyScope.Of(kind + ":" + value);

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
