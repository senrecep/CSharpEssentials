using CSharpEssentials.Maybe;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSharpEssentials.EntityFrameworkCore.Converters;

/// <summary>
/// Stores a <c>Maybe&lt;T&gt;?</c> property of a reference type in a nullable column of <typeparamref name="T"/>.
/// <see langword="null"/> and <see cref="Maybe{T}.None"/> are both written as <c>NULL</c>, and <c>NULL</c> is read back as
/// <see langword="null"/>; use <c>Flatten()</c> to turn it into <see cref="Maybe{T}.None"/>.
/// </summary>
/// <remarks>
/// Change tracking compares <see cref="Maybe{T}"/> values with <see cref="Maybe{T}"/> equality. For a mutable
/// <typeparamref name="T"/> such as <c>byte[]</c>, changing the inner value in place is not detected; assign a new value.
/// </remarks>
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
