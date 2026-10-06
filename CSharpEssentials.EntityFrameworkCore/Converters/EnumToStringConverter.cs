using CSharpEssentials.Core;
using CSharpEssentials.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CSharpEssentials.EntityFrameworkCore.Converters;

/// <summary>
/// Stores an enum as the name produced by <see cref="EnumValueFormatter"/>, the same string used by JSON
/// (snake_case by default, <c>[JsonStringEnumMemberName]</c> wins).
/// </summary>
/// <remarks>
/// Reading is tolerant: values written by <see cref="LegacySnakeCaseEnumConverter{TEnum}"/> (3.x storage format)
/// are still accepted, so existing rows stay readable after upgrading. Numbers and unknown names throw.
/// </remarks>
public sealed class EnumToFormattedStringConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    public EnumToFormattedStringConverter()
        : base(
            v => Write(v),
            v => Read(v)
        )
    {
    }

    private static readonly EnumConventions ReadConventions = EnumConventions.Default with { UnknownValue = UnknownEnumValueHandling.Reject };

    private static string Write(TEnum value) => EnumValueFormatter.Format(value, EnumWireFormat.String);

    private static TEnum Read(string value)
    {
        if (!IsNumber(value) && EnumMetadata.Get<TEnum>().TryParse(value, EnumReadMode.Data, ReadConventions, out TEnum result, out _))
            return result;

        foreach (TEnum member in Enum.GetValues<TEnum>())
            if (string.Equals(member.ToString().ToSnakeCase(), value, StringComparison.OrdinalIgnoreCase))
                return member;

        throw new ArgumentException($"'{value}' is not a stored name of {typeof(TEnum).Name}.", nameof(value));
    }

    private static bool IsNumber(string value) =>
        value.Length > 0 && (char.IsDigit(value[0]) || value[0] == '-' || value[0] == '+');
}
