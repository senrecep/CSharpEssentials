namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Result of <see cref="IIdempotencyStore.TryReserveAsync"/>. <see cref="Token"/> is set only for
/// <see cref="IdempotencyReservationStatus.Reserved"/> and <see cref="Response"/> only for
/// <see cref="IdempotencyReservationStatus.Completed"/>.
/// </summary>
public sealed class IdempotencyReservation
{
    public static IdempotencyReservation InFlight { get; } = new(IdempotencyReservationStatus.InFlight, null, null);

    public static IdempotencyReservation FingerprintMismatch { get; } = new(IdempotencyReservationStatus.FingerprintMismatch, null, null);

    private IdempotencyReservation(IdempotencyReservationStatus status, string? token, IdempotentResponse? response)
    {
        Status = status;
        Token = token;
        Response = response;
    }

    public IdempotencyReservationStatus Status { get; }

    /// <summary>
    /// Identifies this reservation. <see cref="IIdempotencyStore.CompleteAsync"/> and
    /// <see cref="IIdempotencyStore.ReleaseAsync"/> only change the entry while it still carries this token.
    /// </summary>
    public string? Token { get; }

    public IdempotentResponse? Response { get; }

    /// <param name="token">A value unique to this reservation, for example <c>Guid.NewGuid().ToString("N")</c>.</param>
    public static IdempotencyReservation Reserved(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);
        return new(IdempotencyReservationStatus.Reserved, token, null);
    }

    public static IdempotencyReservation Completed(IdempotentResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);
        return new(IdempotencyReservationStatus.Completed, null, response);
    }
}
