namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>What one parity request observed: status, media type, ETag, body bytes, response header names and handler calls.</summary>
public sealed record Probe(HttpStatusCode Status, string? MediaType, string? ETag, byte[] Body, string HeaderNames, int HandlerCalls);
