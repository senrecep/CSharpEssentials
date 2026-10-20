namespace CSharpEssentials.Idempotency;

/// <summary>
/// What happens to a request that carries an idempotency key when its scope cannot be resolved, for example an anonymous
/// caller with <see cref="IdempotencyScopes.User"/>.
/// </summary>
public enum UnscopedKeyBehavior
{
    /// <summary>
    /// The request runs without idempotency and a warning with the event name
    /// <see cref="IdempotencyErrorCodes.UnscopedKeySkipped"/> is logged.
    /// </summary>
    Skip = 0,

    /// <summary>The request is rejected with <see cref="IdempotencyErrorCodes.ScopeRequired"/>.</summary>
    Reject = 1,

    /// <summary>
    /// The key is stored under <see cref="IdempotencyScope.Global"/>: any caller that sends the same key gets the same
    /// result. Use it only for keys that are already unique across callers.
    /// </summary>
    Global = 2,
}
