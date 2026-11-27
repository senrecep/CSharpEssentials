namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>Options of <see cref="OutboxProcessor{TContext}"/> and of the hosted service that drives it.</summary>
public sealed class OutboxProcessorOptions
{
    /// <summary>The most rows claimed by one <see cref="IOutboxProcessor.ProcessBatchAsync"/>. Default: 50.</summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>
    /// The attempts before a message is dead-lettered. A message whose handler throws on this attempt, or whose lease
    /// ends after it, gets <see cref="OutboxMessage.DeadLetteredAt"/> and is not claimed again. Default: 10.
    /// </summary>
    public int MaxAttempts { get; set; } = 10;

    /// <summary>
    /// How long a claim holds a row. A processor that stops mid-batch leaves its rows claimed until the lease ends,
    /// then another processor claims them again. Keep it longer than a batch takes. Default: 5 minutes.
    /// </summary>
    public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>The retry delay after the first failure; see <see cref="OutboxRetryBackoff.Compute"/>. Default: 5 seconds.</summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>The largest retry delay. Default: 10 minutes.</summary>
    public TimeSpan RetryMaxDelay { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>Whether retry delays are spread randomly. Default: <see langword="true"/>.</summary>
    public bool UseJitter { get; set; } = true;

    /// <summary>
    /// Replaces the retry delay: gets the attempt that failed (from 1) and returns the delay before the next one.
    /// <see langword="null"/> (the default) uses <see cref="OutboxRetryBackoff.Compute"/> with the options above.
    /// </summary>
    public Func<int, TimeSpan>? RetryDelay { get; set; }

    /// <summary>The longest error text stored in <see cref="OutboxMessage.Error"/>. Default: 2000.</summary>
    public int MaxErrorLength { get; set; } = 2000;

    /// <summary>How long the hosted service waits after a batch that was not full. Must be positive. Default: 5 seconds.</summary>
    public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Claims the rows. Default: <see cref="ConditionalUpdateOutboxMessageClaimer"/>, which works on every relational
    /// provider. Set a <see cref="SqlOutboxMessageClaimer"/> for provider-specific row locking.
    /// </summary>
    public IOutboxMessageClaimer Claimer { get; set; } = ConditionalUpdateOutboxMessageClaimer.Instance;

    internal void Validate()
    {
        Require(BatchSize >= 1, nameof(BatchSize), "must be at least 1");
        Require(MaxAttempts >= 1, nameof(MaxAttempts), "must be at least 1");
        Require(LeaseDuration > TimeSpan.Zero, nameof(LeaseDuration), "must be positive");
        Require(RetryBaseDelay >= TimeSpan.Zero, nameof(RetryBaseDelay), "must not be negative");
        Require(RetryMaxDelay >= RetryBaseDelay, nameof(RetryMaxDelay), $"must not be less than {nameof(RetryBaseDelay)}");
        Require(MaxErrorLength >= 1, nameof(MaxErrorLength), "must be at least 1");
        Require(PollingInterval > TimeSpan.Zero, nameof(PollingInterval), "must be positive");
        if (Claimer is null)
            throw new InvalidOperationException($"{nameof(OutboxProcessorOptions)}.{nameof(Claimer)} must not be null.");
    }

    private static void Require(bool condition, string option, string rule)
    {
        if (!condition)
            throw new ArgumentOutOfRangeException(option, $"{nameof(OutboxProcessorOptions)}.{option} {rule}.");
    }

    internal TimeSpan GetRetryDelay(int attempt) =>
        RetryDelay?.Invoke(attempt) ?? OutboxRetryBackoff.Compute(attempt, RetryBaseDelay, RetryMaxDelay, UseJitter);
}
