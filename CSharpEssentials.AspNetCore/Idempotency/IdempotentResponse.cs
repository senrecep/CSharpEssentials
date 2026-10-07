namespace CSharpEssentials.AspNetCore;

/// <summary>
/// A stored response replayed for a repeated <c>Idempotency-Key</c>: status code, the allowlisted headers
/// (<see cref="IdempotencyOptions.ReplayedHeaders"/>) and the body.
/// </summary>
public sealed record IdempotentResponse(
    int StatusCode,
    IReadOnlyDictionary<string, string[]> Headers,
    ReadOnlyMemory<byte> Body);
