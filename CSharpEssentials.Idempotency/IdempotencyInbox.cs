namespace CSharpEssentials.Idempotency;

/// <summary>
/// Processes each message at most once per consumer, over <see cref="IIdempotencyExecutor"/>. The record namespace is
/// <c>messaging:{consumer}</c>, the key is <see cref="IdempotencyKeys.FromMessage"/> and the fingerprint is constant,
/// because the message id is authoritative: a redelivery that serializes the same message differently is still a
/// duplicate. Operations use the <see cref="IdempotencyExecutorOptions.MessagingProfile"/> profile unless
/// <see cref="IdempotencyExecutionOptions.Profile"/> names another.
/// </summary>
/// <param name="executor">Runs the handler.</param>
public sealed class IdempotencyInbox(IIdempotencyExecutor executor)
{
    private const string Fingerprint = "message";

    private static readonly IIdempotencyResultCodec<bool> CompletionCodec = IdempotencyResultCodec.Create(
        static _ => ReadOnlyMemory<byte>.Empty,
        static _ => true);

    /// <summary>
    /// Runs <paramref name="handler"/> unless <paramref name="consumer"/> already processed <paramref name="messageId"/>
    /// in <paramref name="scope"/>, and returns its result or the stored one.
    /// <see cref="IdempotencyExecutionStatus.Replayed"/> means the message is a duplicate: acknowledge it.
    /// <see cref="IdempotencyExecutionStatus.InFlight"/> means another delivery is processing it: redeliver it later.
    /// A handler that throws keeps the message in flight until the lease expires; a redelivery after that runs it again.
    /// </summary>
    /// <param name="consumer">Names the handler, so two consumers of the same message do not deduplicate each other.</param>
    /// <param name="messageId">The id the broker or the sender gives the message.</param>
    /// <param name="scope">The owner of the message, for example the tenant or the sending account; <see cref="IdempotencyScope.Global"/> when ids are unique across owners.</param>
    /// <param name="handler">Processes the message.</param>
    /// <param name="codec">Serializes, reads and classifies the handler's result.</param>
    /// <param name="options">Settings for this consumer; <see langword="null"/> uses the messaging profile.</param>
    /// <param name="cancellationToken">Cancels the reservation and the handler.</param>
    /// <exception cref="ArgumentException"><paramref name="consumer"/> or <paramref name="messageId"/> is empty.</exception>
    public ValueTask<IdempotencyExecution<TResult>> ProcessAsync<TResult>(
        string consumer,
        string messageId,
        IdempotencyScope scope,
        Func<CancellationToken, ValueTask<TResult>> handler,
        IIdempotencyResultCodec<TResult> codec,
        IdempotencyExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNullOrEmpty(consumer);
        Guard.NotNullOrEmpty(messageId);
        IdempotencyRecordKey key = new("messaging:" + consumer, scope, IdempotencyKeys.FromMessage(messageId, consumer));
        return executor.ExecuteAsync(key, Fingerprint, WithMessagingProfile(options), handler, codec, cancellationToken);
    }

    /// <summary>
    /// Same as <see cref="ProcessAsync{TResult}"/> for a handler without a result; every completed run is stored.
    /// </summary>
    /// <returns><see cref="IdempotencyExecutionStatus.Executed"/>, <see cref="IdempotencyExecutionStatus.Replayed"/> or <see cref="IdempotencyExecutionStatus.InFlight"/>.</returns>
    public async ValueTask<IdempotencyExecutionStatus> ProcessAsync(
        string consumer,
        string messageId,
        IdempotencyScope scope,
        Func<CancellationToken, ValueTask> handler,
        IdempotencyExecutionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(handler);
        IdempotencyExecution<bool> execution = await ProcessAsync(
            consumer,
            messageId,
            scope,
            async token =>
            {
                await handler(token).ConfigureAwait(false);
                return true;
            },
            CompletionCodec,
            options,
            cancellationToken).ConfigureAwait(false);
        return execution.Status;
    }

    private static IdempotencyExecutionOptions WithMessagingProfile(IdempotencyExecutionOptions? options)
    {
        if (options?.Profile is not null)
            return options;

        return new()
        {
            Profile = IdempotencyExecutorOptions.MessagingProfile,
            Mode = options?.Mode,
            Retention = options?.Retention,
            Lease = options?.Lease,
            FailureRetention = options?.FailureRetention,
            StoreKey = options?.StoreKey,
        };
    }
}
