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

    /// <summary>The user: <see cref="ClaimTypes.NameIdentifier"/>, then <c>sub</c>.</summary>
    public static IIdempotencyScopeResolver User { get; } =
        new DelegateScopeResolver(static principal => Of(FindUser(principal)));

    /// <summary>
    /// The calling client: <c>client_id</c>, then <c>azp</c>. Meant for machine principals (client credentials), where
    /// there is no user. Choose it explicitly: it lets every user of the same client share a key space.
    /// </summary>
    public static IIdempotencyScopeResolver Client { get; } =
        new DelegateScopeResolver(static principal =>
            Of(FindClaim(principal, ClientIdClaimType) ?? FindClaim(principal, AuthorizedPartyClaimType)));

    /// <summary>
    /// <see cref="IdempotencyScope.Global"/> for every caller, anonymous ones included: any caller that sends the same
    /// key gets the same result. Use it only for keys that are already unique across callers, such as message ids.
    /// </summary>
    public static IIdempotencyScopeResolver Global { get; } =
        new DelegateScopeResolver(static _ => IdempotencyScope.Global);

    /// <summary>The tenant read from <paramref name="claimType"/>.</summary>
    public static IIdempotencyScopeResolver Tenant(string claimType)
    {
        Guard.NotNullOrEmpty(claimType);
        return new DelegateScopeResolver(principal => Of(FindClaim(principal, claimType)));
    }

    /// <summary>
    /// The user (as <see cref="User"/>) within the tenant read from <paramref name="tenantClaimType"/>. Both must be
    /// present. The scope is <c>{n}:{user}|{n}:{tenant}</c> with UTF-8 byte lengths, so no user and tenant pair can
    /// produce the scope of another pair.
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

            StringBuilder builder = new(user.Length + tenant.Length + 8);
            LengthPrefixed.Append(builder, user).Append('|');
            return IdempotencyScope.Of(LengthPrefixed.Append(builder, tenant).ToString());
        });
    }

    /// <summary>
    /// A scope computed by <paramref name="resolve"/>. A <see langword="null"/> or empty result means unresolved; implement
    /// <see cref="IIdempotencyScopeResolver"/> to return <see cref="IdempotencyScope.Global"/> as well.
    /// </summary>
    public static IIdempotencyScopeResolver Custom(Func<ClaimsPrincipal?, string?> resolve)
    {
        Guard.NotNull(resolve);
        return new DelegateScopeResolver(principal => Of(resolve(principal)));
    }

    private static IdempotencyScope? Of(string? value) =>
        string.IsNullOrEmpty(value) ? null : IdempotencyScope.Of(value);

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
