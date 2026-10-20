namespace CSharpEssentials.Idempotency;

/// <summary>
/// Turns the result of an idempotent operation into the payload <see cref="IIdempotencyRecordStore"/> keeps and back,
/// and decides whether it is stored. <see cref="IdempotencyResultCodec.Create{TResult}"/> builds one from delegates.
/// </summary>
/// <typeparam name="TResult">The result of the operation.</typeparam>
public interface IIdempotencyResultCodec<TResult>
{
    /// <summary>Writes <paramref name="result"/> as the stored payload.</summary>
    ReadOnlyMemory<byte> Serialize(TResult result);

    /// <summary>Reads a payload written by <see cref="Serialize"/>.</summary>
    TResult Deserialize(ReadOnlyMemory<byte> payload);

    /// <summary>Decides whether <paramref name="result"/> is stored, stored as a failure or released.</summary>
    IdempotencyResultHandling Classify(TResult result);
}
