using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CSharpEssentials.Core;

namespace CSharpEssentials.EntityFrameworkCore.Converters;

public sealed class EnumToFormattedStringConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    private static string ToSnake(TEnum v) => v.ToString().ToSnakeCase();
    private static TEnum ToPascal(string v) => Enum.Parse<TEnum>(v.ToPascalCase(), true);

    public EnumToFormattedStringConverter()
        : base(
            v => ToSnake(v),
            v => ToPascal(v)
        )
    {
    }
}