using CSharpEssentials.Idempotency;

namespace CSharpEssentials.Mediator;

/// <summary>
/// Marks a request that <see cref="IdempotencyBehavior{TRequest, TResponse}"/> runs at most once per
/// <see cref="IdempotencyKey"/> and caller: a retry with the same key gets the stored response instead of running the
/// handler again. Only the key is required; every other member has a default.
/// </summary>
public interface IIdempotentRequest
{
    /// <summary>
    /// The idempotency key: 1 to <see cref="IdempotencyKeys.MaxKeyLength"/> printable ASCII characters other than
    /// <c>"</c> and <c>\</c>. Take it from the client (an <c>Idempotency-Key</c> header) or build it from business ids
    /// with <see cref="IdempotencyKeys.For"/> or <see cref="IdempotencyKeys.FromMessage"/>.
    /// </summary>
    string IdempotencyKey { get; }

    /// <summary>
    /// The owner of the key. <see langword="null"/> (the default) resolves it with
    /// <see cref="IdempotencyBehaviorOptions.ScopeResolver"/> from the current caller.
    /// </summary>
    IdempotencyScope? IdempotencyScope => null;

    /// <summary>
    /// Identifies the content of the request; the same key with another fingerprint is rejected with
    /// <see cref="IdempotencyErrorCodes.KeyReused"/>. <see langword="null"/> (the default) uses the SHA-256 of the
    /// request serialized as JSON.
    /// </summary>
    string? IdempotencyFingerprint => null;

    /// <summary>How long the response is replayed; <see langword="null"/> (the default) uses the request type and profile settings.</summary>
    TimeSpan? IdempotencyRetention => null;

    /// <summary>How long a running request blocks its key; <see langword="null"/> (the default) uses the request type and profile settings.</summary>
    TimeSpan? IdempotencyLease => null;
}
