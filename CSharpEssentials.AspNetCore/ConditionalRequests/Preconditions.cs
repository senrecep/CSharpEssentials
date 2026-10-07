using System.Diagnostics.CodeAnalysis;
using Microsoft.Net.Http.Headers;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The <c>If-Match</c> precondition of a request, parsed by <see cref="ConditionalRequestExtensions.WithIfMatch{TBuilder}(TBuilder, bool)"/>
/// or <see cref="IfMatchAttribute"/>. Read it with <see cref="ConditionalRequestExtensions.GetPreconditions"/>.
/// Comparisons are strong (RFC 9110 section 13.1.1): weak tags never match.
/// </summary>
public sealed class Preconditions
{
    internal static readonly Preconditions None = new([], isWildcard: false);

    internal Preconditions(IReadOnlyList<EntityTagHeaderValue> ifMatch, bool isWildcard)
    {
        IfMatch = ifMatch;
        IsWildcard = isWildcard;
    }

    /// <summary>Whether the request has an <c>If-Match</c> header.</summary>
    public bool HasIfMatch => IsWildcard || IfMatch.Count > 0;

    /// <summary>Whether the header is <c>If-Match: *</c> (matches any existing resource).</summary>
    public bool IsWildcard { get; }

    /// <summary>The entity tags of the header; empty without a header and for <c>*</c>.</summary>
    public IReadOnlyList<EntityTagHeaderValue> IfMatch { get; }

    /// <summary>
    /// Gets the unquoted value of the single strong entity tag of the header, to pass as the original concurrency token of an
    /// update (for example EF Core <c>Entry(entity).Property(e =&gt; e.Version).OriginalValue = version</c>), which makes the
    /// check atomic with the write. For an <see cref="IVersioned"/> resource the tag equals <see cref="IVersioned.Version"/> only
    /// when the version is a valid entity tag (numbers such as a <c>uint xmin</c> or a <c>long</c> revision are); other versions
    /// are sent hashed, so compare them with <see cref="Matches(IVersioned?)"/> instead.
    /// </summary>
    /// <param name="version">The tag without quotes.</param>
    /// <returns><see langword="true"/> only for exactly one strong tag (not <c>*</c>, not weak).</returns>
    public bool TryGetIfMatchVersion([MaybeNullWhen(false)] out string version)
    {
        if (IfMatch is [{ IsWeak: false } tag] && tag.Tag.Length >= 2)
        {
            version = tag.Tag.Subsegment(1, tag.Tag.Length - 2).ToString();
            return true;
        }

        version = null;
        return false;
    }

    /// <summary>
    /// Compares the current ETag with the header using strong comparison. Without a header it matches; <c>*</c> matches
    /// when <paramref name="current"/> is not <see langword="null"/>; weak tags never match.
    /// </summary>
    /// <param name="current">The current ETag of the resource; <see langword="null"/> when it has none or does not exist.</param>
    /// <returns>Whether the request may proceed.</returns>
    public bool Matches(EntityTagHeaderValue? current)
    {
        if (!HasIfMatch)
            return true;
        if (current is null)
            return false;
        if (IsWildcard)
            return true;
        foreach (EntityTagHeaderValue tag in IfMatch)
        {
            if (tag.Compare(current, useStrongComparison: true))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Like <see cref="Matches(EntityTagHeaderValue?)"/> with the ETag of <paramref name="current"/>; <c>*</c> matches any
    /// non-null <paramref name="current"/>, even without an ETag.
    /// </summary>
    /// <param name="current">The validators of the current resource; <see langword="null"/> when it does not exist.</param>
    /// <returns>Whether the request may proceed.</returns>
    public bool Matches(ResourceValidators? current) =>
        IsWildcard ? current is not null : Matches(current?.ETag);

    /// <summary>
    /// Like <see cref="Matches(EntityTagHeaderValue?)"/> with the ETag the default <see cref="IETagGenerator"/> sends for
    /// <paramref name="current"/> (<see cref="ConditionalRequestExtensions.ToETag"/>), so hashed versions match too; <c>*</c>
    /// matches any non-null <paramref name="current"/>.
    /// </summary>
    /// <param name="current">The current resource; <see langword="null"/> when it does not exist.</param>
    /// <returns>Whether the request may proceed.</returns>
    public bool Matches(IVersioned? current) =>
        IsWildcard ? current is not null : Matches(current?.ToETag());
}
