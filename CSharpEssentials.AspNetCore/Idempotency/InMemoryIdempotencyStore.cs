using System.Collections.Concurrent;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// <see cref="IIdempotencyStore"/> kept in process memory. Reservation is atomic, so it is correct for a single
/// instance; with several instances each one has its own entries. Expired entries are removed on access and by a
/// sweep that runs at most once a minute during reservations.
/// </summary>
public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(1);

    private readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.Ordinal);
    private readonly TimeProvider _timeProvider;
    private long _nextSweepTicks;

    public InMemoryIdempotencyStore()
        : this(TimeProvider.System)
    {
    }

    public InMemoryIdempotencyStore(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
    }

    internal int Count => _entries.Count;

    public ValueTask<IdempotencyReservation> TryReserveAsync(
        string key,
        string fingerprint,
        TimeSpan inFlightTimeout,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(fingerprint);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(inFlightTimeout, TimeSpan.Zero);
        cancellationToken.ThrowIfCancellationRequested();

        DateTimeOffset now = _timeProvider.GetUtcNow();
        SweepIfDue(now);

        Entry reservation = new(Guid.NewGuid().ToString("N"), fingerprint, null, now + inFlightTimeout);
        while (true)
        {
            if (_entries.TryAdd(key, reservation))
                return new(IdempotencyReservation.Reserved(reservation.Token));
            if (!_entries.TryGetValue(key, out Entry? existing))
                continue;
            if (existing.ExpiresAt <= now)
            {
                if (_entries.TryUpdate(key, reservation, existing))
                    return new(IdempotencyReservation.Reserved(reservation.Token));
                continue;
            }

            if (!string.Equals(existing.Fingerprint, fingerprint, StringComparison.Ordinal))
                return new(IdempotencyReservation.FingerprintMismatch);
            return new(existing.Response is null
                ? IdempotencyReservation.InFlight
                : IdempotencyReservation.Completed(existing.Response));
        }
    }

    public ValueTask<bool> CompleteAsync(
        string key,
        string token,
        string fingerprint,
        IdempotentResponse response,
        TimeSpan retention,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(token);
        ArgumentException.ThrowIfNullOrEmpty(fingerprint);
        ArgumentNullException.ThrowIfNull(response);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(retention, TimeSpan.Zero);

        Entry completed = new(token, fingerprint, response, _timeProvider.GetUtcNow() + retention);
        while (true)
        {
            // An expired reservation that nobody took over may still be completed: the work is done.
            if (_entries.TryGetValue(key, out Entry? existing))
            {
                if (existing.Response is not null || !string.Equals(existing.Token, token, StringComparison.Ordinal))
                    return new(false);
                if (_entries.TryUpdate(key, completed, existing))
                    return new(true);
            }
            else if (_entries.TryAdd(key, completed))
            {
                return new(true);
            }
        }
    }

    public ValueTask<bool> ReleaseAsync(string key, string token, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(token);

        return new(_entries.TryGetValue(key, out Entry? existing)
            && existing.Response is null
            && string.Equals(existing.Token, token, StringComparison.Ordinal)
            && _entries.TryRemove(new KeyValuePair<string, Entry>(key, existing)));
    }

    private void SweepIfDue(DateTimeOffset now)
    {
        long next = Interlocked.Read(ref _nextSweepTicks);
        if (now.UtcTicks < next
            || Interlocked.CompareExchange(ref _nextSweepTicks, now.UtcTicks + SweepInterval.Ticks, next) != next)
            return;

        foreach (KeyValuePair<string, Entry> pair in _entries)
        {
            if (pair.Value.ExpiresAt <= now)
                _entries.TryRemove(pair);
        }
    }

    // A class, not a record: TryUpdate and TryRemove must compare entries by reference.
    private sealed class Entry(string token, string fingerprint, IdempotentResponse? response, DateTimeOffset expiresAt)
    {
        public string Token { get; } = token;

        public string Fingerprint { get; } = fingerprint;

        public IdempotentResponse? Response { get; } = response;

        public DateTimeOffset ExpiresAt { get; } = expiresAt;
    }
}
