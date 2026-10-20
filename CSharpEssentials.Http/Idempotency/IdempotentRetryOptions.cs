namespace CSharpEssentials.Http;

/// <summary>Settings of <see cref="IdempotentRetry"/> and <c>RetryUnsafeMethodsOnlyWithIdempotencyKey</c>.</summary>
public sealed class IdempotentRetryOptions
{
    /// <summary>
    /// Longest wait taken from the <c>Retry-After</c> header of a <c>409 Idempotency.InFlight</c> response. A longer
    /// value is clamped to this one. Default: 30 seconds.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
    public TimeSpan MaxRetryAfter
    {
        get;
        set => field = value >= TimeSpan.Zero
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "MaxRetryAfter cannot be negative.");
    } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Time allowed to read the problem body of a <c>409</c> response to look for the <c>Idempotency.InFlight</c> code.
    /// A read that does not finish in time counts as unreadable and the response is not retried. Default: 2 seconds.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is zero or negative.</exception>
    public TimeSpan ProblemReadTimeout
    {
        get;
        set => field = value > TimeSpan.Zero
            ? value
            : throw new ArgumentOutOfRangeException(nameof(value), value, "ProblemReadTimeout must be positive.");
    } = TimeSpan.FromSeconds(2);
}
