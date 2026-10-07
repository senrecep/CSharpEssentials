namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Outcome of <see cref="IIdempotencyStore.TryReserveAsync"/>.
/// </summary>
public enum IdempotencyReservationStatus
{
    /// <summary>The key was free and is now reserved for this request.</summary>
    Reserved = 0,

    /// <summary>Another request with the same key and fingerprint is still running.</summary>
    InFlight = 1,

    /// <summary>A request with the same key and fingerprint has completed; its response is replayed.</summary>
    Completed = 2,

    /// <summary>The key was used for a different request (method, path, query or body).</summary>
    FingerprintMismatch = 3,
}
