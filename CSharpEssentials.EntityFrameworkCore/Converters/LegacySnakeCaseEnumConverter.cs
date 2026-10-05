using CSharpEssentials.Core;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSharpEssentials.EntityFrameworkCore.Converters;

/// <summary>
/// The 3.x storage format: <c>value.ToString().ToSnakeCase()</c> from CSharpEssentials.Core.
/// It differs from the JSON name for acronyms and digits (<c>HTTPStatus</c> → <c>httpstatus</c>, <c>Value1</c> → <c>value_1</c>).
/// Use it only to keep writing the old format; see <see cref="EnumConventionOptions.UseLegacySnakeCase"/>.
/// </summary>
public sealed class LegacySnakeCaseEnumConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    public LegacySnakeCaseEnumConverter()
        : base(
            v => Write(v),
            v => Read(v)
        )
    {
    }

    private static string Write(TEnum value) => value.ToString().ToSnakeCase();

    private static TEnum Read(string value) => Enum.Parse<TEnum>(value.ToPascalCase(), true);
}
