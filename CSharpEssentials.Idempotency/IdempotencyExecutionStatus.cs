namespace CSharpEssentials.Idempotency;

/// <summary>How <see cref="IIdempotencyExecutor"/> answered an idempotent operation.</summary>
public enum IdempotencyExecutionStatus
{
    /// <summary>The operation ran; <see cref="IdempotencyExecution{TResult}.Value"/> is its result.</summary>
    Executed = 0,

    /// <summary>
    /// The operation did not run; <see cref="IdempotencyExecution{TResult}.Value"/> is the result stored by an earlier
    /// run with the same key and fingerprint.
    /// </summary>
    Replayed = 1,

    /// <summary>
    /// The operation did not run because another run holds the key (<see cref="IdempotencyErrorCodes.InFlight"/>). Retry
    /// later with the same key.
    /// </summary>
    InFlight = 2,

    /// <summary>
    /// The operation did not run because the key was used with another fingerprint
    /// (<see cref="IdempotencyErrorCodes.KeyReused"/>).
    /// </summary>
    KeyReused = 3,
}
