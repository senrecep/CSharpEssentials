using CSharpEssentials.Idempotency;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Idempotency settings of one endpoint, route group or named profile. Every setting left <see langword="null"/>
/// falls back to the next level: endpoint (<c>.WithIdempotency(...)</c> or <see cref="IdempotentAttribute"/>), then
/// the profile named by <see cref="Profile"/> (<see cref="IdempotencyOptions.AddProfile"/>), then
/// <see cref="IdempotencyOptions"/>. When an endpoint and its route group (or an MVC action and its controller) both
/// set a value, the more specific one wins. The merged settings must satisfy the same rules as
/// <see cref="IdempotencyOptions"/>: profiles are checked in <c>AddIdempotency</c>, endpoints on their first request
/// and again after the options reload.
/// </summary>
public sealed class IdempotencyEndpointOptions
{
    /// <summary>
    /// The profile registered with <see cref="IdempotencyOptions.AddProfile"/> whose settings apply where this
    /// endpoint sets none. Names are compared case-insensitively; an unknown name throws
    /// <see cref="InvalidOperationException"/> on the endpoint's requests. Not allowed on a profile itself.
    /// </summary>
    public string? Profile { get; set; }

    /// <summary>Overrides <see cref="IdempotencyOptions.Mode"/>.</summary>
    public IdempotencyMode? Mode { get; set; }

    /// <summary>Overrides <see cref="IdempotencyOptions.RetentionPeriod"/>.</summary>
    public TimeSpan? RetentionPeriod { get; set; }

    /// <summary>Overrides <see cref="IdempotencyOptions.InFlightTimeout"/> (the lease).</summary>
    public TimeSpan? InFlightTimeout { get; set; }

    /// <summary>Overrides <see cref="IdempotencyOptions.ServerErrorRetention"/>.</summary>
    public TimeSpan? ServerErrorRetention { get; set; }

    /// <summary>Overrides <see cref="IdempotencyOptions.RequireKey"/>.</summary>
    public bool? RequireKey { get; set; }

    /// <summary>Overrides <see cref="IdempotencyOptions.ShouldStore"/>.</summary>
    public Func<IdempotencyOutcome, bool>? ShouldStore { get; set; }

    /// <summary>Overrides <see cref="IdempotencyOptions.UnscopedKeyBehavior"/>.</summary>
    public UnscopedKeyBehavior? UnscopedKeyBehavior { get; set; }

    /// <summary>
    /// Service key of a keyed <see cref="IIdempotencyRecordStore"/> used instead of the default store, for example
    /// a transactional store for payments registered with
    /// <c>services.AddKeyedScoped&lt;IIdempotencyRecordStore, PaymentsStore&gt;("payments")</c>. A key without a
    /// registration throws on the endpoint's requests.
    /// </summary>
    public string? StoreKey { get; set; }

    internal void MergeFrom(IdempotencyEndpointOptions other)
    {
        Profile = other.Profile ?? Profile;
        Mode = other.Mode ?? Mode;
        RetentionPeriod = other.RetentionPeriod ?? RetentionPeriod;
        InFlightTimeout = other.InFlightTimeout ?? InFlightTimeout;
        ServerErrorRetention = other.ServerErrorRetention ?? ServerErrorRetention;
        RequireKey = other.RequireKey ?? RequireKey;
        ShouldStore = other.ShouldStore ?? ShouldStore;
        UnscopedKeyBehavior = other.UnscopedKeyBehavior ?? UnscopedKeyBehavior;
        StoreKey = other.StoreKey ?? StoreKey;
    }
}
