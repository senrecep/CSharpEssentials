using CSharpEssentials.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSharpEssentials.EntityFrameworkCore.Converters;

/// <summary>
/// Stores an enum as its wire name (design section 11.3). Reads accept every spelling in <see cref="EnumReadMode.Data"/> mode and map
/// unknown values to the <see cref="EnumFallbackAttribute"/> member; without one they throw <see cref="InvalidOperationException"/>
/// (inner <see cref="EnumValueException"/>) naming the column. Writing an undefined value throws <see cref="EnumValueException"/>.
/// </summary>
/// <remarks>
/// With <see cref="UnknownEnumValueHandling.UseFallback"/>, an entity loaded with the fallback member and saved again writes the
/// fallback member over the unknown stored value; set <see cref="EnumConventions.UnknownValue"/> to
/// <see cref="UnknownEnumValueHandling.Reject"/> to fail instead.
/// </remarks>
/// <typeparam name="TEnum">The enum type.</typeparam>
public sealed class EnumWireNameConverter<TEnum> : ValueConverter<TEnum, string> where TEnum : struct, Enum
{
    /// <summary>Creates the converter with the generated metadata of <typeparamref name="TEnum"/> and <see cref="EnumConventions.Default"/>.</summary>
    public EnumWireNameConverter()
        : this(EnumMetadata.Get<TEnum>(), EnumConventions.Default)
    {
    }

    /// <summary>Creates the converter.</summary>
    /// <param name="info">The enum metadata.</param>
    /// <param name="conventions">The conventions used for reads.</param>
    /// <param name="column">The column named in errors, for example <c>orders.status</c>.</param>
    public EnumWireNameConverter(EnumInfo<TEnum> info, EnumConventions conventions, string? column = null)
        : this(new EnumColumnCodec<TEnum>(info, conventions, legacyFormat: null, column))
    {
    }

    private EnumWireNameConverter(EnumColumnCodec<TEnum> codec)
        : base(value => codec.ToText(value), text => codec.FromText(text))
    {
    }
}
