using System.Numerics;
using CSharpEssentials.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSharpEssentials.EntityFrameworkCore.Converters;

/// <summary>
/// Stores an enum as its underlying number (design section 11.3). Reads map undefined numbers to the
/// <see cref="EnumFallbackAttribute"/> member; without one they throw <see cref="InvalidOperationException"/> (inner
/// <see cref="EnumValueException"/>) naming the column. Writing an undefined value throws <see cref="EnumValueException"/>.
/// </summary>
/// <typeparam name="TEnum">The enum type.</typeparam>
/// <typeparam name="TNumber">The column type; the convention uses the underlying type of <typeparamref name="TEnum"/>, so the column type does not change.</typeparam>
public sealed class EnumIntegerConverter<TEnum, TNumber> : ValueConverter<TEnum, TNumber>
    where TEnum : struct, Enum
    where TNumber : struct, IBinaryInteger<TNumber>
{
    /// <summary>Creates the converter with the generated metadata of <typeparamref name="TEnum"/> and <see cref="EnumConventions.Default"/>.</summary>
    public EnumIntegerConverter()
        : this(EnumMetadata.Get<TEnum>(), EnumConventions.Default)
    {
    }

    /// <summary>Creates the converter.</summary>
    /// <param name="info">The enum metadata.</param>
    /// <param name="conventions">The conventions used for reads.</param>
    /// <param name="column">The column named in errors, for example <c>orders.status</c>.</param>
    public EnumIntegerConverter(EnumInfo<TEnum> info, EnumConventions conventions, string? column = null)
        : this(new EnumColumnCodec<TEnum>(info, conventions, legacyFormat: null, column))
    {
    }

    private EnumIntegerConverter(EnumColumnCodec<TEnum> codec)
        : base(value => Write(codec, value), number => Read(codec, number))
    {
    }

    private static TNumber Write(EnumColumnCodec<TEnum> codec, TEnum value) => TNumber.CreateTruncating(codec.ToRaw(value));

    private static TEnum Read(EnumColumnCodec<TEnum> codec, TNumber number) =>
        TNumber.IsNegative(number)
            ? codec.FromNumber(long.CreateTruncating(number))
            : codec.FromNumber(ulong.CreateTruncating(number));
}
