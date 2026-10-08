using CSharpEssentials.EntityFrameworkCore.Converters;
using CSharpEssentials.Maybe;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CSharpEssentials.EntityFrameworkCore;

/// <summary>
/// The reference-type overload of <see cref="PropertyBuilderExtensions.HasNullableMaybeConversion{T}"/>. It lives in its
/// own class because two methods that differ only in their <c>struct</c> / <c>class</c> constraint cannot share one;
/// across classes the compiler drops the candidate whose constraint fails, so both are called the same way.
/// </summary>
public static class NullableMaybeReferencePropertyBuilderExtensions
{
    /// <summary>
    /// Stores a <c>Maybe&lt;T&gt;?</c> property of a reference type in a nullable column of <typeparamref name="T"/> with
    /// <see cref="NullableMaybeReferenceConverter{T}"/>. <see langword="null"/> and <see cref="Maybe{T}.None"/> are both
    /// written as <c>NULL</c>, and <c>NULL</c> is read back as <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The reference type.</typeparam>
    /// <param name="builder">The property builder.</param>
    /// <returns>The same builder.</returns>
    public static PropertyBuilder<Maybe<T>?> HasNullableMaybeConversion<T>(this PropertyBuilder<Maybe<T>?> builder)
        where T : class
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        return builder.HasConversion(new NullableMaybeReferenceConverter<T>());
    }
}
