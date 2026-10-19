namespace CSharpEssentials.AspNetCore;

/// <summary>Ready-made <see cref="IdempotencyOptions.ShouldStore"/> policies.</summary>
public static class IdempotencyStorePolicies
{
    /// <summary>
    /// The default. Stores 2xx, and 3xx and 5xx when the handler started; releases 4xx and everything else. A stored
    /// 5xx is kept for <see cref="IdempotencyOptions.ServerErrorRetention"/>, so a handler that failed after a side
    /// effect is not run again by an immediate retry. Statuses in <see cref="IdempotencyOptions.NeverStoreStatusCodes"/>
    /// never reach the policy.
    /// </summary>
    public static Func<IdempotencyOutcome, bool> Default { get; } = static outcome => outcome.StatusCode switch
    {
        >= 200 and < 300 => true,
        >= 300 and < 400 or >= 500 and < 600 => outcome.HandlerStarted,
        _ => false,
    };

    /// <summary>
    /// Stores 2xx only, the behaviour before version 7. It deliberately releases 3xx and 5xx as well as 4xx, so a
    /// retry after a handler that failed (or redirected) runs the handler again, even when the first run already
    /// caused a side effect.
    /// </summary>
    public static Func<IdempotencyOutcome, bool> SuccessOnly { get; } =
        static outcome => outcome.StatusCode is >= 200 and < 300;
}
