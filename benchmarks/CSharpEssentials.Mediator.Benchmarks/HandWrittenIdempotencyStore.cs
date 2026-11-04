using System.Collections.Concurrent;

namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>
/// The in-memory record store of the hand-written idempotency behaviors: no lease or retention expiry, no scope, no
/// reservation token. A lower bound, not a feature match.
/// </summary>
public sealed class HandWrittenIdempotencyStore
{
    /// <summary>The records by request type and key.</summary>
    public ConcurrentDictionary<(string Type, string Key), IdempotencyRecord> Records { get; } = new();
}
