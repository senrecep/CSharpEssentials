namespace CSharpEssentials.Mediator.Benchmarks;

/// <summary>A record of <see cref="HandWrittenIdempotencyStore"/>.</summary>
/// <param name="Fingerprint">The fingerprint of the request that reserved the key.</param>
/// <param name="Payload">The stored response as JSON, or <see langword="null"/> while the request runs.</param>
public sealed record IdempotencyRecord(string Fingerprint, byte[]? Payload);
