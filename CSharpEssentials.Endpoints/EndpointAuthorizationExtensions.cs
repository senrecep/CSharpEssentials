using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;

namespace CSharpEssentials.Endpoints;

/// <summary>
/// Shortcuts over <see cref="AuthorizationEndpointConventionBuilderExtensions.RequireAuthorization{TBuilder}(TBuilder, IAuthorizeData[])"/>
/// for roles, policies and authentication schemes.
/// </summary>
public static class EndpointAuthorizationExtensions
{
    /// <summary>
    /// Requires an authenticated user in at least one of <paramref name="roles"/>.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint, group or route handler builder.</param>
    /// <param name="roles">The accepted roles. One role is enough.</param>
    /// <returns>The same <paramref name="builder"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="roles"/> is empty or contains a null, blank or comma-separated entry.</exception>
    public static TBuilder RequireRoles<TBuilder>(this TBuilder builder, params string[] roles)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(roles);
        EnsureValues(roles, nameof(roles));
        return builder.RequireAuthorization(new AuthorizeAttribute { Roles = string.Join(',', roles) });
    }

    /// <summary>
    /// Requires an authenticated user that satisfies every policy in <paramref name="policies"/>.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint, group or route handler builder.</param>
    /// <param name="policies">The policy names. Each policy must succeed.</param>
    /// <returns>The same <paramref name="builder"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="policies"/> is empty or contains a null, blank or comma-separated entry.</exception>
    public static TBuilder RequirePolicies<TBuilder>(this TBuilder builder, params string[] policies)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(policies);
        EnsureValues(policies, nameof(policies));
        return builder.RequireAuthorization([.. policies.Select(static policy => new AuthorizeAttribute(policy))]);
    }

    /// <summary>
    /// Requires a user authenticated by at least one of <paramref name="schemes"/>.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint, group or route handler builder.</param>
    /// <param name="schemes">The authentication schemes to run. One successful scheme is enough.</param>
    /// <returns>The same <paramref name="builder"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="schemes"/> is empty or contains a null, blank or comma-separated entry.</exception>
    public static TBuilder RequireAuthSchemes<TBuilder>(this TBuilder builder, params string[] schemes)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(schemes);
        EnsureValues(schemes, nameof(schemes));
        return builder.RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = string.Join(',', schemes) });
    }

    private static void EnsureValues(string[] values, string parameterName)
    {
        if (values.Length == 0)
        {
            throw new ArgumentException("At least one value is required.", parameterName);
        }

        if (values.Any(static value => string.IsNullOrWhiteSpace(value) || value.Contains(',', StringComparison.Ordinal)))
        {
            throw new ArgumentException("Values must not be null, blank or contain a comma.", parameterName);
        }
    }
}
