using CSharpEssentials.AspNetCore;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>Reserves in memory and fails every <see cref="CompleteAsync"/> and <see cref="ReleaseAsync"/>.</summary>
internal sealed class ThrowingIdempotencyStore(TimeProvider timeProvider) : IIdempotencyStore
{
    private readonly InMemoryIdempotencyStore _inner = new(timeProvider);

    public ValueTask<IdempotencyReservation> TryReserveAsync(string key, string fingerprint, TimeSpan inFlightTimeout, CancellationToken cancellationToken = default) =>
        _inner.TryReserveAsync(key, fingerprint, inFlightTimeout, cancellationToken);

    public ValueTask<bool> CompleteAsync(string key, string token, string fingerprint, IdempotentResponse response, TimeSpan retention, CancellationToken cancellationToken = default) =>
        throw new IOException("store unavailable");

    public ValueTask<bool> ReleaseAsync(string key, string token, CancellationToken cancellationToken = default) =>
        throw new IOException("store unavailable");
}
