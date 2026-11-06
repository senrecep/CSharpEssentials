using System.Collections.Concurrent;
using System.Globalization;

namespace CSharpEssentials.Idempotency;

/// <summary>
/// <see cref="IIdempotencyRecordStore"/> kept in process memory. Reservation is atomic, so it is correct for a single
/// instance; with several instances each one has its own entries. Expired entries are removed on access and by a sweep
/// that runs at most once a minute during reservations.
/// </summary>
public sealed class InMemoryIdempotencyRecordStore : IIdempotencyRecordStore
{
    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(1);
    private static long s_lastToken;

    private readonly ConcurrentDictionary<IdempotencyRecordKey, Entry> _entries = new();
    private readonly TimeProvider _timeProvider;
    private long _nextSweepTicks;

    /// <summary>Uses <see cref="TimeProvider.System"/>.</summary>
    public InMemoryIdempotencyRecordStore()
        : this(TimeProvider.System)
    {
    }

    /// <param name="timeProvider">Clock for leases and retention.</param>
    public InMemoryIdempotencyRecordStore(TimeProvider timeProvider)
    {
        Guard.NotNull(timeProvider);
        _timeProvider = timeProvider;
    }

    internal int Count => _entries.Count;

    public ValueTask<IdempotencyRecordReservation> TryReserveAsync(
        IdempotencyRecordKey key,
        string fingerprint,
        TimeSpan lease,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(key);
        Guard.NotNullOrEmpty(fingerprint);
        Guard.Positive(lease);
        cancellationToken.ThrowIfCancellationRequested();

        DateTimeOffset now = _timeProvider.GetUtcNow();
        SweepIfDue(now);

        Entry? reservation = null;
        while (true)
        {
            if (!_entries.TryGetValue(key, out Entry? existing))
            {
                reservation ??= new(NextToken(), fingerprint, null, now + lease);
                if (_entries.TryAdd(key, reservation))
                    return new(IdempotencyRecordReservation.Reserved(reservation.Token));
                continue;
            }

            if (existing.ExpiresAt <= now)
            {
                reservation ??= new(NextToken(), fingerprint, null, now + lease);
                if (_entries.TryUpdate(key, reservation, existing))
                    return new(IdempotencyRecordReservation.Reserved(reservation.Token));
                continue;
            }

            if (!string.Equals(existing.Fingerprint, fingerprint, StringComparison.Ordinal))
                return new(IdempotencyRecordReservation.FingerprintMismatch);
            return new(existing.Completed ?? IdempotencyRecordReservation.InFlight);
        }
    }

    public ValueTask<bool> CompleteAsync(
        IdempotencyRecordKey key,
        string token,
        string fingerprint,
        ReadOnlyMemory<byte> payload,
        TimeSpan retention,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(key);
        Guard.NotNullOrEmpty(token);
        Guard.NotNullOrEmpty(fingerprint);
        Guard.Positive(retention);

        Entry completed = new(token, fingerprint, IdempotencyRecordReservation.Completed(payload.ToArray()), _timeProvider.GetUtcNow() + retention);
        while (true)
        {
            if (_entries.TryGetValue(key, out Entry? existing))
            {
                if (existing.Completed is not null || !string.Equals(existing.Token, token, StringComparison.Ordinal))
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

    public ValueTask<bool> ReleaseAsync(IdempotencyRecordKey key, string token, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(key);
        Guard.NotNullOrEmpty(token);

        return new(_entries.TryGetValue(key, out Entry? existing)
            && existing.Completed is null
            && string.Equals(existing.Token, token, StringComparison.Ordinal)
            && TryRemove(new KeyValuePair<IdempotencyRecordKey, Entry>(key, existing)));
    }

    /// <summary>
    /// A token unique to one reservation among every in-memory store of the process. Tokens never leave the process, so
    /// a counter fences a stale reservation as well as a random value and costs no call into the system random source.
    /// </summary>
    private static string NextToken() =>
        Interlocked.Increment(ref s_lastToken).ToString(CultureInfo.InvariantCulture);

    private void SweepIfDue(DateTimeOffset now)
    {
        long next = Interlocked.Read(ref _nextSweepTicks);
        if (now.UtcTicks < next
            || Interlocked.CompareExchange(ref _nextSweepTicks, now.UtcTicks + SweepInterval.Ticks, next) != next)
            return;

        foreach (KeyValuePair<IdempotencyRecordKey, Entry> pair in _entries)
        {
            if (pair.Value.ExpiresAt <= now)
                TryRemove(pair);
        }
    }

    /// <summary>
    /// Removes the pair only while the key still maps to that exact entry. <c>TryRemove(KeyValuePair)</c> is not part of
    /// <c>netstandard2.1</c>; the collection interface does the same atomic compare-and-remove on every target.
    /// </summary>
    private bool TryRemove(KeyValuePair<IdempotencyRecordKey, Entry> pair) =>
        ((ICollection<KeyValuePair<IdempotencyRecordKey, Entry>>)_entries).Remove(pair);

    /// <summary>A class, not a record: <c>TryUpdate</c> and the compare-and-remove compare entries by reference.</summary>
    private sealed class Entry(string token, string fingerprint, IdempotencyRecordReservation? completed, DateTimeOffset expiresAt)
    {
        public string Token { get; } = token;

        public string Fingerprint { get; } = fingerprint;

        /// <summary>The reservation every replay of a completed entry returns; <see langword="null"/> while in flight.</summary>
        public IdempotencyRecordReservation? Completed { get; } = completed;

        public DateTimeOffset ExpiresAt { get; } = expiresAt;
    }
}
