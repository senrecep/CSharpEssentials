namespace CSharpEssentials.Idempotency;

/// <summary>
/// Writes <see cref="IdempotencyRecordKey.ToLogHash"/> into a log message, computed only when the message is written.
/// </summary>
internal readonly struct IdempotencyKeyHash(IdempotencyRecordKey key)
{
    public override string ToString() => key.ToLogHash();
}
