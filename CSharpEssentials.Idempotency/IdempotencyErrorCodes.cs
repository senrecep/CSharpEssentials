namespace CSharpEssentials.Idempotency;

/// <summary>Stable error codes and log event names shared by every idempotency integration.</summary>
public static class IdempotencyErrorCodes
{
    /// <summary>The operation requires an idempotency key and none was sent.</summary>
    public const string KeyMissing = "Idempotency.KeyMissing";

    /// <summary>The idempotency key is empty, too long or contains characters that are not allowed.</summary>
    public const string KeyInvalid = "Idempotency.KeyInvalid";

    /// <summary>Another request with the same key is still running.</summary>
    public const string InFlight = "Idempotency.InFlight";

    /// <summary>The key was already used for a different request (the fingerprints differ).</summary>
    public const string KeyReused = "Idempotency.KeyReused";

    /// <summary>The request carries a key but its scope could not be resolved, and unscoped keys are rejected.</summary>
    public const string ScopeRequired = "Idempotency.ScopeRequired";

    /// <summary>The request carries a key but its scope could not be resolved, so it ran without idempotency.</summary>
    public const string UnscopedKeySkipped = "Idempotency.UnscopedKeySkipped";
}
