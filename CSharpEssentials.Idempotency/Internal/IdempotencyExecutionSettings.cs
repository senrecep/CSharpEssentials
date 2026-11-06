namespace CSharpEssentials.Idempotency;

/// <summary>The effective settings of an operation: each value from the operation, else its profile, else the executor options.</summary>
internal readonly struct IdempotencyExecutionSettings
{
    private IdempotencyExecutionSettings(
        IdempotencyMode mode,
        TimeSpan retention,
        TimeSpan lease,
        TimeSpan failureRetention,
        string? storeKey,
        string source)
    {
        Mode = mode;
        Retention = retention;
        Lease = lease;
        FailureRetention = failureRetention;
        StoreKey = storeKey;
        Source = source;
    }

    public IdempotencyMode Mode { get; }

    public TimeSpan Retention { get; }

    public TimeSpan Lease { get; }

    public TimeSpan FailureRetention { get; }

    public string? StoreKey { get; }

    public string Source { get; }

    /// <param name="options">The executor options.</param>
    /// <param name="operation">The settings of the operation, or <see langword="null"/>.</param>
    /// <param name="owner">Names what the settings belong to in an error message.</param>
    /// <param name="ownerNamespace">
    /// A record namespace appended to <paramref name="owner"/> in an error message, so a caller on the hot path never
    /// formats a message that is only read when the settings are invalid.
    /// </param>
    /// <exception cref="InvalidOperationException">The profile is unknown or the merged settings are invalid.</exception>
    internal static IdempotencyExecutionSettings Resolve(
        IdempotencyExecutorOptions options,
        IdempotencyExecutionOptions? operation,
        string owner,
        string? ownerNamespace = null)
    {
        IdempotencyExecutionOptions? profile = FindProfile(options, operation, owner, ownerNamespace);
        var settings = new IdempotencyExecutionSettings(
            operation?.Mode ?? profile?.Mode ?? options.Mode,
            operation?.Retention ?? profile?.Retention ?? options.Retention,
            operation?.Lease ?? profile?.Lease ?? options.Lease,
            operation?.FailureRetention ?? profile?.FailureRetention ?? options.FailureRetention,
            operation?.StoreKey ?? profile?.StoreKey,
            operation?.Source ?? IdempotencyMetrics.ExecutorSource);
        settings.Validate(options.MaxLease, owner, ownerNamespace);
        return settings;
    }

    /// <exception cref="InvalidOperationException">The operation names a profile that is not registered.</exception>
    internal static IdempotencyExecutionOptions? FindProfile(
        IdempotencyExecutorOptions options,
        IdempotencyExecutionOptions? operation,
        string owner,
        string? ownerNamespace = null)
    {
        IdempotencyExecutionOptions? profile = null;
        if (operation?.Profile is { } profileName && !options.Profiles.TryGetValue(profileName, out profile))
        {
            string known = string.Join(", ", options.Profiles.Keys.Select(static name => $"'{name}'"));
            throw new InvalidOperationException(
                $"{Owner(owner, ownerNamespace)} uses the idempotency profile '{profileName}', which is not registered. Register it with AddIdempotencyExecutor(o => o.AddProfile(\"{profileName}\", ...)). Registered profiles: {known}.");
        }

        return profile;
    }

    private void Validate(TimeSpan maxLease, string owner, string? ownerNamespace)
    {
        if ((uint)Mode > (uint)IdempotencyMode.Enforce)
            throw Invalid(owner, ownerNamespace, $"Mode ({Mode}) is not a defined value.");
        if (Retention <= TimeSpan.Zero)
            throw Invalid(owner, ownerNamespace, $"Retention ({Retention}) must be positive.");
        if (Lease <= TimeSpan.Zero || Lease > Retention || Lease > maxLease)
            throw Invalid(owner, ownerNamespace, $"Lease ({Lease}) must be positive and at most Retention ({Retention}) and MaxLease ({maxLease}).");
        if (FailureRetention <= TimeSpan.Zero || FailureRetention > Retention)
            throw Invalid(owner, ownerNamespace, $"FailureRetention ({FailureRetention}) must be positive and at most Retention ({Retention}).");
        if (StoreKey is { Length: 0 })
            throw Invalid(owner, ownerNamespace, "StoreKey must not be empty.");
    }

    private static string Owner(string owner, string? ownerNamespace) =>
        ownerNamespace is null ? owner : $"{owner} in '{ownerNamespace}'";

    private static InvalidOperationException Invalid(string owner, string? ownerNamespace, string message) =>
        new($"The idempotency settings of {Owner(owner, ownerNamespace)} are invalid: {message}");
}
