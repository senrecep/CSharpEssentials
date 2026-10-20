namespace CSharpEssentials.Idempotency;

/// <summary>Outcome of <see cref="IIdempotencyRecordStore.TryReserveAsync"/>.</summary>
public enum IdempotencyReservationStatus
{
    /// <summary>The key was free (or its entry had expired) and is now reserved for the caller.</summary>
    Reserved = 0,

    /// <summary>Another reservation with the same fingerprint holds the key and has not completed yet.</summary>
    InFlight = 1,

    /// <summary>The key completed with the same fingerprint; the stored payload can be replayed.</summary>
    Completed = 2,

    /// <summary>The key is in flight or completed for a different fingerprint, so it was reused for another request.</summary>
    FingerprintMismatch = 3,
}
