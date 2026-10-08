using CSharpEssentials.Maybe;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSharpEssentials.EntityFrameworkCore.Converters;

/// <summary>
/// Stores a <c>Maybe&lt;T&gt;?</c> property of a reference type in a nullable column of <typeparamref name="T"/>.
/// <see langword="null"/> and <see cref="Maybe{T}.None"/> are both written as <c>NULL</c>, and <c>NULL</c> is read back as
/// <see langword="null"/>; use <c>Flatten()</c> to turn it into <see cref="Maybe{T}.None"/>.
/// </summary>
/// <typeparam name="T">The reference type.</typeparam>
public sealed class NullableMaybeReferenceConverter<T> : ValueConverter<Maybe<T>, T?>
    where T : class
{
    /// <summary>Creates the converter.</summary>
    public NullableMaybeReferenceConverter()
        : base(
            maybe => maybe.GetValueOrDefault(),
            value => Maybe<T>.From(value))
    {
    }
}
