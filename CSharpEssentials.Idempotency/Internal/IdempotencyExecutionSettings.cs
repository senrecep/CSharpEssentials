namespace CSharpEssentials.Idempotency;

/// <summary>The effective settings of an operation: each value from the operation, else its profile, else the executor options.</summary>
internal readonly struct IdempotencyExecutionSettings
{
    private IdempotencyExecutionSettings(IdempotencyMode mode, TimeSpan retention, TimeSpan lease, TimeSpan failureRetention, string? storeKey)
    {
        Mode = mode;
        Retention = retention;
        Lease = lease;
        FailureRetention = failureRetention;
        StoreKey = storeKey;
    }

    public IdempotencyMode Mode { get; }

    public TimeSpan Retention { get; }

    public TimeSpan Lease { get; }

    public TimeSpan FailureRetention { get; }

    public string? StoreKey { get; }

    /// <exception cref="InvalidOperationException">The profile is unknown or the merged settings are invalid.</exception>
    internal static IdempotencyExecutionSettings Resolve(IdempotencyExecutorOptions options, IdempotencyExecutionOptions? operation, string owner)
    {
        IdempotencyExecutionOptions? profile = null;
        if (operation?.Profile is { } profileName && !options.Profiles.TryGetValue(profileName, out profile))
        {
            string known = string.Join(", ", options.Profiles.Keys.Select(static name => $"'{name}'"));
            throw new InvalidOperationException(
                $"{owner} uses the idempotency profile '{profileName}', which is not registered. Register it with AddIdempotencyExecutor(o => o.AddProfile(\"{profileName}\", ...)). Registered profiles: {known}.");
        }

        var settings = new IdempotencyExecutionSettings(
            operation?.Mode ?? profile?.Mode ?? options.Mode,
            operation?.Retention ?? profile?.Retention ?? options.Retention,
            operation?.Lease ?? profile?.Lease ?? options.Lease,
            operation?.FailureRetention ?? profile?.FailureRetention ?? options.FailureRetention,
            operation?.StoreKey ?? profile?.StoreKey);
        settings.Validate(options.MaxLease, owner);
        return settings;
    }

    private void Validate(TimeSpan maxLease, string owner)
    {
        if ((uint)Mode > (uint)IdempotencyMode.Enforce)
            throw Invalid(owner, $"Mode ({Mode}) is not a defined value.");
        if (Retention <= TimeSpan.Zero)
            throw Invalid(owner, $"Retention ({Retention}) must be positive.");
        if (Lease <= TimeSpan.Zero || Lease > Retention || Lease > maxLease)
            throw Invalid(owner, $"Lease ({Lease}) must be positive and at most Retention ({Retention}) and MaxLease ({maxLease}).");
        if (FailureRetention <= TimeSpan.Zero || FailureRetention > Retention)
            throw Invalid(owner, $"FailureRetention ({FailureRetention}) must be positive and at most Retention ({Retention}).");
        if (StoreKey is { Length: 0 })
            throw Invalid(owner, "StoreKey must not be empty.");
    }

    private static InvalidOperationException Invalid(string owner, string message) =>
        new($"The idempotency settings of {owner} are invalid: {message}");
}
