using CSharpEssentials.Entity.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>
/// <see cref="IOutboxProcessor"/> over the <see cref="OutboxMessage"/> table of <typeparamref name="TContext"/>. Each
/// batch runs in a new service scope: the claimer claims the due rows, then every message is deserialized with
/// <see cref="IOutboxMessageSerializer"/> and passed to an <see cref="IOutboxMessageHandler"/> resolved from a scope of
/// its own, so one handler's tracked changes never leak into the next. Completion, retry and dead-letter updates are
/// conditional on the claim token, so a processor whose lease ended cannot overwrite the result of the processor that
/// claimed the row after it. Register it with <see cref="OutboxServiceCollectionExtensions.AddOutboxProcessor{TContext}"/>.
/// </summary>
/// <typeparam name="TContext">The context that maps <see cref="OutboxMessage"/>.</typeparam>
public sealed partial class OutboxProcessor<TContext> : IOutboxProcessor
    where TContext : DbContext
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OutboxProcessorOptions _options;
    private readonly ILogger<OutboxProcessor<TContext>> _logger;
    private readonly TimeProvider _timeProvider;

    /// <summary>Creates the processor.</summary>
    /// <param name="scopeFactory">Creates the scope of each batch and each message.</param>
    /// <param name="options">The processor options; validated here.</param>
    /// <param name="logger">Logs failures and dead letters.</param>
    /// <param name="timeProvider">The clock; <see cref="TimeProvider.System"/> when <see langword="null"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">An option is out of range.</exception>
    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        OutboxProcessorOptions options,
        ILogger<OutboxProcessor<TContext>> logger,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        options.Validate();
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default)
    {
        AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            TContext context = scope.ServiceProvider.GetRequiredService<TContext>();
            IOutboxMessageSerializer serializer = scope.ServiceProvider.GetRequiredService<IOutboxMessageSerializer>();
            DateTime now = UtcNow();
            OutboxClaimRequest request = new(Guid.NewGuid(), _options.BatchSize, now, now + _options.LeaseDuration);
            IReadOnlyList<OutboxMessage> claimed = await _options.Claimer.ClaimAsync(context, request, cancellationToken).ConfigureAwait(false);

            foreach (OutboxMessage message in claimed.OrderBy(static m => m.OccurredAt).ThenBy(static m => m.Id))
                await ProcessAsync(context, serializer, message, request.ClaimToken, cancellationToken).ConfigureAwait(false);

            return claimed.Count;
        }
    }

    private async Task ProcessAsync(
        TContext context, IOutboxMessageSerializer serializer, OutboxMessage message, Guid token, CancellationToken cancellationToken)
    {
        if (message.Attempts > _options.MaxAttempts)
        {
            LogDeadLettered(message.Id, message.Type, message.Attempts);
            await FinishAsync(context, message.Id, token, processed: false,
                "The lease of the last attempt ended without a result.", null, cancellationToken).ConfigureAwait(false);
            return;
        }

        string? error = null;
        try
        {
            IDomainEvent domainEvent = serializer.Deserialize(message.Type, message.Content);
            AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            await using (scope.ConfigureAwait(false))
            {
                IOutboxMessageHandler handler = scope.ServiceProvider.GetRequiredService<IOutboxMessageHandler>();
                await handler.HandleAsync(message, domainEvent, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            LogAttemptFailed(exception, message.Id, message.Type, message.Attempts);
            error = Describe(exception);
        }

        if (error is null)
        {
            await FinishAsync(context, message.Id, token, processed: true, null, null, cancellationToken).ConfigureAwait(false);
        }
        else if (message.Attempts >= _options.MaxAttempts)
        {
            LogDeadLettered(message.Id, message.Type, message.Attempts);
            await FinishAsync(context, message.Id, token, processed: false, error, null, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            DateTime retryAt = UtcNow() + _options.GetRetryDelay(message.Attempts);
            await FinishAsync(context, message.Id, token, processed: false, error, retryAt, cancellationToken).ConfigureAwait(false);
        }
    }

    private Task<int> FinishAsync(
        TContext context, Guid id, Guid token, bool processed, string? error, DateTime? retryAt, CancellationToken cancellationToken)
    {
        DateTime now = UtcNow();
        IQueryable<OutboxMessage> row = context.Set<OutboxMessage>().Where(m => m.Id == id && m.ClaimToken == token);
        if (processed)
        {
            return row.ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(m => m.ProcessedAt, now)
                    .SetProperty(m => m.ClaimToken, (Guid?)null)
                    .SetProperty(m => m.Error, (string?)null),
                cancellationToken);
        }

        if (retryAt is { } next)
        {
            return row.ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(m => m.NextAttemptAt, next)
                    .SetProperty(m => m.ClaimToken, (Guid?)null)
                    .SetProperty(m => m.Error, error),
                cancellationToken);
        }

        return row.ExecuteUpdateAsync(
            setters => setters
                .SetProperty(m => m.DeadLetteredAt, now)
                .SetProperty(m => m.ClaimToken, (Guid?)null)
                .SetProperty(m => m.Error, error),
            cancellationToken);
    }

    private string Describe(Exception exception)
    {
        string text = $"{exception.GetType().FullName}: {exception.Message}";
        return text.Length <= _options.MaxErrorLength ? text : text[.._options.MaxErrorLength];
    }

    private DateTime UtcNow() => _timeProvider.GetUtcNow().UtcDateTime;

    [LoggerMessage(Level = LogLevel.Warning, Message = "Outbox message {MessageId} ({MessageType}) failed on attempt {Attempt}")]
    private partial void LogAttemptFailed(Exception exception, Guid messageId, string messageType, int attempt);

    [LoggerMessage(Level = LogLevel.Error, Message = "Outbox message {MessageId} ({MessageType}) was dead-lettered after {Attempts} attempts")]
    private partial void LogDeadLettered(Guid messageId, string messageType, int attempts);
}
