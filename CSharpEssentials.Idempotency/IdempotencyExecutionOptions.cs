namespace CSharpEssentials.Idempotency;

/// <summary>
/// Settings for one idempotent operation or for a profile registered with
/// <see cref="IdempotencyExecutorOptions.AddProfile"/>. A setting left <see langword="null"/> comes from the profile
/// named by <see cref="Profile"/>, else from <see cref="IdempotencyExecutorOptions"/>.
/// </summary>
public sealed class IdempotencyExecutionOptions
{
    /// <summary>The profile that supplies the settings left <see langword="null"/>. Names are compared case-insensitively.</summary>
    public string? Profile { get; set; }

    /// <summary>Overrides <see cref="IdempotencyExecutorOptions.Mode"/>.</summary>
    public IdempotencyMode? Mode { get; set; }

    /// <summary>Overrides <see cref="IdempotencyExecutorOptions.Retention"/>.</summary>
    public TimeSpan? Retention { get; set; }

    /// <summary>Overrides <see cref="IdempotencyExecutorOptions.Lease"/>.</summary>
    public TimeSpan? Lease { get; set; }

    /// <summary>Overrides <see cref="IdempotencyExecutorOptions.FailureRetention"/>.</summary>
    public TimeSpan? FailureRetention { get; set; }

    /// <summary>
    /// The key of a keyed <see cref="IIdempotencyRecordStore"/> registration to use instead of the default store, for
    /// example a transactional store for payments next to a cache store for everything else.
    /// </summary>
    public string? StoreKey { get; set; }

    /// <summary>The <c>source</c> tag of the decisions the executor records for this operation; <see langword="null"/> gives <c>executor</c>.</summary>
    internal string? Source { get; set; }
}
