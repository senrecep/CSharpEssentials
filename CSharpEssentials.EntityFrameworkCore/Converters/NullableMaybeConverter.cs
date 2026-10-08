using CSharpEssentials.Maybe;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSharpEssentials.EntityFrameworkCore.Converters;

/// <summary>
/// Stores a <c>Maybe&lt;T&gt;?</c> property of a value type in a nullable column of <typeparamref name="T"/>.
/// <see langword="null"/> and <see cref="Maybe{T}.None"/> are both written as <c>NULL</c>, and <c>NULL</c> is read back as
/// <see langword="null"/>; use <c>Flatten()</c> to turn it into <see cref="Maybe{T}.None"/>.
/// </summary>
/// <typeparam name="T">The value type.</typeparam>
public sealed class NullableMaybeConverter<T> : ValueConverter<Maybe<T>, T?>
    where T : struct
{
    /// <summary>Creates the converter.</summary>
    public NullableMaybeConverter()
        : base(
            maybe => maybe.HasValue ? maybe.Value : null,
            value => value.HasValue ? Maybe<T>.From(value.Value) : Maybe<T>.None)
    {
    }
}
