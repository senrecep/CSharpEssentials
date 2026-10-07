namespace CSharpEssentials.AspNetCore;

/// <summary>
/// Options of <see cref="ConditionalRequestExtensions.AddConditionalRequests"/>.
/// </summary>
public sealed class ConditionalRequestOptions
{
    /// <summary>
    /// When <see langword="true"/>, a value that is not <see cref="IVersioned"/> and has no <see cref="IETagSource{T}"/> gets the
    /// weak ETag <c>W/"base64url(SHA-256(JSON))"</c>, serialized with the host's Minimal API JSON options. The body is
    /// serialized once more for the hash, so this costs CPU on every request; it saves bandwidth only. Default <see langword="false"/>.
    /// </summary>
    public bool UseBodyHashFallback { get; set; }
}
