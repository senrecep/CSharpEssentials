namespace CSharpEssentials.Idempotency;

internal sealed class DelegateResultCodec<TResult>(
    Func<TResult, ReadOnlyMemory<byte>> serialize,
    Func<ReadOnlyMemory<byte>, TResult> deserialize,
    Func<TResult, IdempotencyResultHandling>? classify) : IIdempotencyResultCodec<TResult>
{
    public ReadOnlyMemory<byte> Serialize(TResult result) => serialize(result);

    public TResult Deserialize(ReadOnlyMemory<byte> payload) => deserialize(payload);

    public IdempotencyResultHandling Classify(TResult result) => classify?.Invoke(result) ?? IdempotencyResultHandling.Store;
}
