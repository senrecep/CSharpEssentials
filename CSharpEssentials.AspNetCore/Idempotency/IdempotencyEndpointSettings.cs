using CSharpEssentials.Idempotency;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The effective settings of an idempotent endpoint: each value from the endpoint, else its profile, else
/// <see cref="IdempotencyOptions"/>.
/// </summary>
internal sealed record IdempotencyEndpointSettings(
    IdempotencyMode Mode,
    TimeSpan RetentionPeriod,
    TimeSpan InFlightTimeout,
    TimeSpan ServerErrorRetention,
    bool RequireKey,
    Func<IdempotencyOutcome, bool> ShouldStore,
    UnscopedKeyBehavior UnscopedKeyBehavior,
    string? StoreKey)
{
    /// <summary>
    /// Merges <paramref name="endpoint"/> over its profile over <paramref name="options"/> and validates the result.
    /// </summary>
    /// <param name="options">The global options and the registered profiles.</param>
    /// <param name="endpoint">The endpoint's own settings, or a profile's when validating that profile.</param>
    /// <param name="owner">Names the endpoint or profile in exception messages.</param>
    /// <exception cref="InvalidOperationException">The profile is unknown or the merged settings are invalid.</exception>
    internal static IdempotencyEndpointSettings Resolve(IdempotencyOptions options, IdempotencyEndpointOptions endpoint, string owner)
    {
        IdempotencyEndpointOptions? profile = null;
        if (endpoint.Profile is { } profileName && !options.Profiles.TryGetValue(profileName, out profile))
        {
            string known = options.Profiles.Count == 0 ? "none" : string.Join(", ", options.Profiles.Keys.Select(static name => $"'{name}'"));
            throw new InvalidOperationException(
                $"{owner} uses the idempotency profile '{profileName}', which is not registered. Register it with AddIdempotency(o => o.AddProfile(\"{profileName}\", ...)). Registered profiles: {known}.");
        }

        IdempotencyEndpointSettings settings = new(
            endpoint.Mode ?? profile?.Mode ?? options.Mode,
            endpoint.RetentionPeriod ?? profile?.RetentionPeriod ?? options.RetentionPeriod,
            endpoint.InFlightTimeout ?? profile?.InFlightTimeout ?? options.InFlightTimeout,
            endpoint.ServerErrorRetention ?? profile?.ServerErrorRetention ?? options.ServerErrorRetention,
            endpoint.RequireKey ?? profile?.RequireKey ?? options.RequireKey,
            endpoint.ShouldStore ?? profile?.ShouldStore ?? options.ShouldStore,
            endpoint.UnscopedKeyBehavior ?? profile?.UnscopedKeyBehavior ?? options.UnscopedKeyBehavior,
            endpoint.StoreKey ?? profile?.StoreKey);
        settings.Validate(options.MaxInFlightTimeout, owner);
        return settings;
    }

    private void Validate(TimeSpan maxInFlightTimeout, string owner)
    {
        if ((uint)Mode > (uint)IdempotencyMode.Enforce)
            throw Invalid(owner, $"Mode ({Mode}) is not a defined value.");
        if (RetentionPeriod <= TimeSpan.Zero)
            throw Invalid(owner, $"RetentionPeriod ({RetentionPeriod}) must be positive.");
        if (InFlightTimeout <= TimeSpan.Zero || InFlightTimeout > RetentionPeriod || InFlightTimeout > maxInFlightTimeout)
            throw Invalid(owner, $"InFlightTimeout ({InFlightTimeout}) must be positive and at most RetentionPeriod ({RetentionPeriod}) and MaxInFlightTimeout ({maxInFlightTimeout}).");
        if (ServerErrorRetention <= TimeSpan.Zero || ServerErrorRetention > RetentionPeriod)
            throw Invalid(owner, $"ServerErrorRetention ({ServerErrorRetention}) must be positive and at most RetentionPeriod ({RetentionPeriod}).");
        if ((uint)UnscopedKeyBehavior > (uint)UnscopedKeyBehavior.Global)
            throw Invalid(owner, $"UnscopedKeyBehavior ({UnscopedKeyBehavior}) is not a defined value.");
        if (StoreKey is { Length: 0 })
            throw Invalid(owner, "StoreKey must not be empty.");
    }

    private static InvalidOperationException Invalid(string owner, string message) =>
        new($"The idempotency settings of {owner} are invalid: {message}");
}
