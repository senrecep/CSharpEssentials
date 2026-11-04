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

    private static readonly Dictionary<TEnum, string> Names = BuildNames();
    private static readonly Dictionary<string, TEnum> Values = BuildValues();

    private static string Write(TEnum value) => Names.TryGetValue(value, out string? name) ? name : Format(value);

    private static TEnum Read(string value) => Values.TryGetValue(value, out TEnum result) ? result : Parse(value);

    private static string Format(TEnum value) => value.ToString().ToSnakeCase();

    private static TEnum Parse(string value) => Enum.Parse<TEnum>(value.ToPascalCase(), true);

    private static Dictionary<TEnum, string> BuildNames()
    {
        Dictionary<TEnum, string> names = [];
        foreach (TEnum member in Enum.GetValues<TEnum>())
            names.TryAdd(member, Format(member));

        return names;
    }

    private static Dictionary<string, TEnum> BuildValues()
    {
        Dictionary<string, TEnum> values = [with(StringComparer.Ordinal)];
        foreach (string name in Names.Values)
        {
            if (Enum.TryParse(name.ToPascalCase(), true, out TEnum value))
                values.TryAdd(name, value);
        }

        return values;
    }
}
