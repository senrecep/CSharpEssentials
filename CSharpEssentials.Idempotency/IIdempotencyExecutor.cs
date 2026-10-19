namespace CSharpEssentials.Idempotency;

/// <summary>
/// Runs an operation at most once per <see cref="IdempotencyRecordKey"/>, whatever calls it: a Mediator pipeline, a
/// message consumer or a background job. <see cref="IdempotencyExecutor"/> is the implementation.
/// </summary>
public interface IIdempotencyExecutor
{
    /// <summary>
    /// Reserves <paramref name="key"/> and runs <paramref name="operation"/>, or answers without running it.
    /// <list type="bullet">
    /// <item><description>A result stored for the key and <paramref name="fingerprint"/> is returned as <see cref="IdempotencyExecutionStatus.Replayed"/>.</description></item>
    /// <item><description>A key held by another run returns <see cref="IdempotencyExecutionStatus.InFlight"/>; a key stored or held for another fingerprint returns <see cref="IdempotencyExecutionStatus.KeyReused"/>.</description></item>
    /// <item><description>Otherwise the operation runs and <paramref name="codec"/> decides whether its result is stored, stored as a failure or released.</description></item>
    /// <item><description>A cancellation observed before the operation starts releases the key. An exception thrown by the operation, including <see cref="OperationCanceledException"/>, is rethrown and keeps the key in flight until its lease expires, because its side effects are unknown.</description></item>
    /// <item><description>A result that cannot be stored keeps the key in flight until its lease expires; the result is still returned.</description></item>
    /// </list>
    /// </summary>
    /// <param name="key">The record key: namespace, scope and key.</param>
    /// <param name="fingerprint">Identifies the content of the request; the same key with another fingerprint is <see cref="IdempotencyExecutionStatus.KeyReused"/>.</param>
    /// <param name="options">Settings for this operation, or <see langword="null"/> for the executor defaults.</param>
    /// <param name="operation">The work to run at most once.</param>
    /// <param name="codec">Serializes, reads and classifies the result.</param>
    /// <param name="cancellationToken">Cancels the reservation and the operation. Storing or releasing after the operation ran ignores it.</param>
    /// <exception cref="InvalidOperationException">The settings are invalid, the store key is not registered or a stored result cannot be read.</exception>
    ValueTask<IdempotencyExecution<TResult>> ExecuteAsync<TResult>(
        IdempotencyRecordKey key,
        string fingerprint,
        IdempotencyExecutionOptions? options,
        Func<CancellationToken, ValueTask<TResult>> operation,
        IIdempotencyResultCodec<TResult> codec,
        CancellationToken cancellationToken = default);
}
