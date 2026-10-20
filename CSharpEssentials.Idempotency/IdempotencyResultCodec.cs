namespace CSharpEssentials.Idempotency;

/// <summary>Builds <see cref="IIdempotencyResultCodec{TResult}"/> instances.</summary>
public static class IdempotencyResultCodec
{
    /// <summary>A codec that uses the given delegates.</summary>
    /// <param name="serialize">Writes a result as the stored payload.</param>
    /// <param name="deserialize">Reads a payload written by <paramref name="serialize"/>.</param>
    /// <param name="classify">
    /// Decides whether a result is stored, stored as a failure or released. Default: every result is
    /// <see cref="IdempotencyResultHandling.Store"/>.
    /// </param>
    public static IIdempotencyResultCodec<TResult> Create<TResult>(
        Func<TResult, ReadOnlyMemory<byte>> serialize,
        Func<ReadOnlyMemory<byte>, TResult> deserialize,
        Func<TResult, IdempotencyResultHandling>? classify = null)
    {
        Guard.NotNull(serialize);
        Guard.NotNull(deserialize);
        return new DelegateResultCodec<TResult>(serialize, deserialize, classify);
    }
}
