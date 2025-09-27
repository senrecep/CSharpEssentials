using CSharpEssentials.Core;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSharpEssentials.EntityFrameworkCore.Converters;

public sealed class EnumToFormattedStringConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    public EnumToFormattedStringConverter()
        : base(
            v => v.ToString().ToSnakeCase(),
            v => Enum.Parse<TEnum>(v.ToPascalCase(), true)
        )
    {
    }
}