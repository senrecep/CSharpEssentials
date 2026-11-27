namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>
/// Relays stored outbox messages to the <see cref="IOutboxMessageHandler"/>, one batch per call. Call it from your
/// own loop, a scheduled job or the hosted service of <c>CSharpEssentials.EntityFrameworkCore.Hosting</c>.
/// </summary>
public interface IOutboxProcessor
{
    /// <summary>
    /// Claims up to <see cref="OutboxProcessorOptions.BatchSize"/> due messages and dispatches them in
    /// <see cref="OutboxMessage.OccurredAt"/> order. Each message ends processed, scheduled for a retry or dead-lettered.
    /// A handler failure is recorded and never thrown; a database failure or cancellation is thrown, and the claimed
    /// rows become due again when their lease ends.
    /// </summary>
    /// <param name="cancellationToken">Cancels the batch.</param>
    /// <returns>The number of messages claimed; less than the batch size means the outbox had no more due messages.</returns>
    Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default);
}
