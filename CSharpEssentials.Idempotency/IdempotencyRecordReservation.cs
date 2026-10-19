namespace CSharpEssentials.Idempotency;

/// <summary>
/// Result of <see cref="IIdempotencyRecordStore.TryReserveAsync"/>. <see cref="Token"/> is set only for
/// <see cref="IdempotencyReservationStatus.Reserved"/> and <see cref="Payload"/> only for
/// <see cref="IdempotencyReservationStatus.Completed"/>.
/// </summary>
public sealed class IdempotencyRecordReservation
{
    private IdempotencyRecordReservation(IdempotencyReservationStatus status, string? token, ReadOnlyMemory<byte>? payload)
    {
        Status = status;
        Token = token;
        Payload = payload;
    }

    /// <summary>The key is held by another reservation with the same fingerprint.</summary>
    public static IdempotencyRecordReservation InFlight { get; } = new(IdempotencyReservationStatus.InFlight, null, null);

    /// <summary>The key is in flight or completed for a different fingerprint.</summary>
    public static IdempotencyRecordReservation FingerprintMismatch { get; } =
        new(IdempotencyReservationStatus.FingerprintMismatch, null, null);

    public IdempotencyReservationStatus Status { get; }

    /// <summary>
    /// Identifies this reservation. <see cref="IIdempotencyRecordStore.CompleteAsync"/> and
    /// <see cref="IIdempotencyRecordStore.ReleaseAsync"/> only change the entry while it still carries this token.
    /// </summary>
    public string? Token { get; }

    /// <summary>The payload stored by <see cref="IIdempotencyRecordStore.CompleteAsync"/>; it may be empty.</summary>
    public ReadOnlyMemory<byte>? Payload { get; }

    /// <param name="token">A value unique to this reservation, for example <c>Guid.NewGuid().ToString("N")</c>.</param>
    public static IdempotencyRecordReservation Reserved(string token)
    {
        Guard.NotNullOrEmpty(token);
        return new(IdempotencyReservationStatus.Reserved, token, null);
    }

    /// <param name="payload">The opaque payload stored when the reservation completed.</param>
    public static IdempotencyRecordReservation Completed(ReadOnlyMemory<byte> payload) =>
        new(IdempotencyReservationStatus.Completed, null, payload);
}
