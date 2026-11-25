using System.Security.Cryptography;

namespace CSharpEssentials.EntityFrameworkCore.Outbox;

/// <summary>The default retry delay of the outbox processor: exponential, capped, with optional jitter.</summary>
public static class OutboxRetryBackoff
{
    private const int MaxExponent = 30;

    /// <summary>
    /// The delay before attempt <paramref name="attempt"/> + 1: <paramref name="baseDelay"/> doubled per earlier
    /// attempt, capped at <paramref name="maxDelay"/>. With <paramref name="useJitter"/> the result is drawn uniformly
    /// from the upper half of that delay, so messages that failed together do not retry in lockstep.
    /// </summary>
    /// <param name="attempt">The attempt that failed, starting at 1.</param>
    /// <param name="baseDelay">The delay after the first failure.</param>
    /// <param name="maxDelay">The largest delay.</param>
    /// <param name="useJitter">Whether to spread the delay randomly.</param>
    /// <returns>The delay.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="attempt"/> is below 1, a delay is negative, or <paramref name="maxDelay"/> is below <paramref name="baseDelay"/>.</exception>
    public static TimeSpan Compute(int attempt, TimeSpan baseDelay, TimeSpan maxDelay, bool useJitter)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(attempt, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(baseDelay, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDelay, baseDelay);

        double ticks = Math.Min(baseDelay.Ticks * Math.Pow(2, Math.Min(attempt - 1, MaxExponent)), maxDelay.Ticks);
        if (useJitter)
            ticks = ticks / 2 + ticks / 2 * RandomNumberGenerator.GetInt32(int.MaxValue) / int.MaxValue;
        return TimeSpan.FromTicks((long)ticks);
    }
}
