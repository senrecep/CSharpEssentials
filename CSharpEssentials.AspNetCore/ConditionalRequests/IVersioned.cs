namespace CSharpEssentials.AspNetCore;

/// <summary>
/// A resource with a version that changes whenever the resource changes, such as a row version or a revision number.
/// The default <see cref="IETagGenerator"/> turns it into the strong ETag <c>"{Version}"</c>. A version that is not a valid
/// entity tag (characters outside <c>!</c> and <c>#</c> to <c>~</c>) is sent as the base64url SHA-256 of its UTF-8 bytes
/// instead; an empty version gives no ETag. Numbers (a <c>uint xmin</c>, a <c>long</c> revision) are always valid, so
/// <see cref="Preconditions.TryGetIfMatchVersion"/> returns the version itself; for other versions compare with
/// <see cref="Preconditions.Matches(IVersioned?)"/> or <see cref="ConditionalRequestExtensions.ToETag"/>.
/// </summary>
public interface IVersioned
{
    /// <summary>The current version of the resource.</summary>
    string Version { get; }
}
