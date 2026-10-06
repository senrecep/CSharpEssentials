using System.Text.Json;
using CSharpEssentials.Enums;

namespace CSharpEssentials.Json;

/// <summary>
/// The 4.x naming helper, now a facade over <see cref="EnumMetadata"/>: names are the generated wire names, so JSON, EF Core
/// storage, OpenAPI schemas and binding spell every member the same way. Enums without generated metadata are not supported.
/// </summary>
/// <remarks>
/// Use <see cref="EnumMetadata"/>, <see cref="EnumValueParser"/>, <see cref="EnumValueFormatter"/> or the generated
/// <c>ToWireName()</c>/<c>TryParseWire</c> helpers in new code. Runtime naming policies other than
/// <see cref="JsonNamingPolicy.SnakeCaseLower"/> are not supported; the naming is set at build time.
/// </remarks>
[Obsolete("Use EnumMetadata and the generated ToWireName() and TryParseWire helpers instead. Wire names are set at build time.")]
public static class StringEnumNaming
{
    private static readonly EnumConventions ReadConventions = EnumConventions.Default with { UnknownValue = UnknownEnumValueHandling.Reject };

    /// <summary>
    /// The only supported policy (<see cref="JsonNamingPolicy.SnakeCaseLower"/>); the wire names themselves come from enum metadata.
    /// </summary>
    public static JsonNamingPolicy DefaultPolicy { get; } = JsonNamingPolicy.SnakeCaseLower;

    /// <summary>
    /// The 4.x "is this a string enum" predicate: an enum type marked with <see cref="StringEnumAttribute"/>.
    /// </summary>
    public static bool IsStringEnum(Type type) =>
        type is not null && type.IsEnum && type.IsDefined(typeof(StringEnumAttribute), inherit: false);

    /// <summary>
    /// Gets the wire name of an enum value. Flag combinations are their single flags joined with <c>", "</c>; undefined values are
    /// written as their number.
    /// </summary>
    /// <exception cref="NotSupportedException"><paramref name="namingPolicy"/> is not <see langword="null"/> or <see cref="DefaultPolicy"/>.</exception>
    public static string GetName<TEnum>(TEnum value, JsonNamingPolicy? namingPolicy = null)
        where TEnum : struct, Enum => FormatName(GetInfo<TEnum>(namingPolicy), value);

    /// <summary>
    /// Gets the wire name of an enum value or of a number of the underlying type.
    /// </summary>
    /// <exception cref="NotSupportedException"><paramref name="namingPolicy"/> is not <see langword="null"/> or <see cref="DefaultPolicy"/>.</exception>
    public static string GetName(Type enumType, object value, JsonNamingPolicy? namingPolicy = null)
    {
        _ = value ?? throw new ArgumentNullException(nameof(value));
        return GetInfo(enumType, namingPolicy).Accept(new NameVisitor(value));
    }

    /// <summary>
    /// Gets the wire names of all members, in declaration order.
    /// </summary>
    /// <exception cref="NotSupportedException"><paramref name="namingPolicy"/> is not <see langword="null"/> or <see cref="DefaultPolicy"/>.</exception>
    public static IReadOnlyList<string> GetNames(Type enumType, JsonNamingPolicy? namingPolicy = null) =>
        GetInfo(enumType, namingPolicy).WireNames;

    /// <summary>
    /// Gets the wire names of all members, in declaration order.
    /// </summary>
    /// <exception cref="NotSupportedException"><paramref name="namingPolicy"/> is not <see langword="null"/> or <see cref="DefaultPolicy"/>.</exception>
    public static IReadOnlyList<string> GetNames<TEnum>(JsonNamingPolicy? namingPolicy = null)
        where TEnum : struct, Enum => GetInfo<TEnum>(namingPolicy).WireNames;

    /// <summary>
    /// Whether <paramref name="value"/> is a defined member or, for <see cref="FlagsAttribute"/> enums,
    /// a combination of defined flags.
    /// </summary>
    public static bool IsDefined<TEnum>(TEnum value)
        where TEnum : struct, Enum => GetInfo<TEnum>(null).IsDefined(value);

    /// <summary>
    /// Parses a value written by <see cref="GetName{TEnum}"/>. Matching is case-insensitive and accepts the wire name, the C# member
    /// name, aliases and (optionally) the number of a defined value; surrounding whitespace is ignored. For
    /// <see cref="FlagsAttribute"/> enums a comma separated list is accepted.
    /// </summary>
    /// <exception cref="NotSupportedException"><paramref name="namingPolicy"/> is not <see langword="null"/> or <see cref="DefaultPolicy"/>.</exception>
    public static bool TryParse<TEnum>(string? value, out TEnum result, JsonNamingPolicy? namingPolicy = null, bool allowIntegerValues = true)
        where TEnum : struct, Enum
    {
        if (TryParse(typeof(TEnum), value, out object? boxed, namingPolicy, allowIntegerValues))
        {
            result = (TEnum)boxed!;
            return true;
        }
        result = default;
        return false;
    }

    /// <summary>
    /// Non-generic variant of <see cref="TryParse{TEnum}"/>.
    /// </summary>
    /// <exception cref="NotSupportedException"><paramref name="namingPolicy"/> is not <see langword="null"/> or <see cref="DefaultPolicy"/>.</exception>
    public static bool TryParse(Type enumType, string? value, out object? result, JsonNamingPolicy? namingPolicy = null, bool allowIntegerValues = true)
    {
        IEnumInfo info = GetInfo(enumType, namingPolicy);
        result = string.IsNullOrWhiteSpace(value) ? null : info.Accept(new ParseVisitor(value, allowIntegerValues));
        return result is not null;
    }

    private static EnumInfo<TEnum> GetInfo<TEnum>(JsonNamingPolicy? namingPolicy) where TEnum : struct, Enum =>
        (EnumInfo<TEnum>)GetInfo(typeof(TEnum), namingPolicy);

    private static IEnumInfo GetInfo(Type enumType, JsonNamingPolicy? namingPolicy)
    {
        if (enumType?.IsEnum != true)
            throw new ArgumentException($"Type '{enumType}' is not an enum.", nameof(enumType));
        if (namingPolicy is not null && !ReferenceEquals(namingPolicy, DefaultPolicy))
        {
            throw new NotSupportedException(
                "Runtime enum naming policies are not supported. Set the naming at build time with the CSharpEssentialsEnumNaming " +
                "MSBuild property, [StringEnum(Naming = ...)] or [JsonStringEnumMemberName] (enum conventions design, section 4.1).");
        }

        return EnumMetadata.TryGet(enumType, out IEnumInfo? info)
            ? info
            : throw new InvalidOperationException(
                $"Enum '{enumType.FullName}' has no generated metadata. Mark it [StringEnum], keep it and its containing types public " +
                "or internal (not private, protected, file-local or nested in a generic type), and build with C# 9 or newer.");
    }

    private static string FormatName<TEnum>(EnumInfo<TEnum> info, TEnum value) where TEnum : struct, Enum
    {
        if (info.TryGetMember(value, out EnumMemberInfo<TEnum>? member))
            return member.WireName;
        if (info.IsFlags && info.ToRawValue(value) != 0 && info.IsDefined(value))
        {
            List<string> names = [];
            info.FormatFlags(value, names);
            return string.Join(", ", names);
        }

        return value.ToString("D");
    }

    private sealed class NameVisitor(object value) : IEnumInfoVisitor<string>
    {
        public string Visit<TEnum>(EnumInfo<TEnum> info) where TEnum : struct, Enum =>
            FormatName(info, value is TEnum typed ? typed : info.FromRawValue(ToRaw(value)));

        private static ulong ToRaw(object number) => number switch
        {
            sbyte v => unchecked((ulong)v),
            short v => unchecked((ulong)v),
            int v => unchecked((ulong)v),
            long v => unchecked((ulong)v),
            byte v => v,
            ushort v => v,
            uint v => v,
            ulong v => v,
            _ => throw new ArgumentException($"Unsupported enum value type '{number.GetType()}'.", nameof(number)),
        };
    }

    private sealed class ParseVisitor(string value, bool allowIntegerValues) : IEnumInfoVisitor<object?>
    {
        public object? Visit<TEnum>(EnumInfo<TEnum> info) where TEnum : struct, Enum
        {
            ulong raw = 0;
            foreach (string part in info.IsFlags ? value.Split(',') : [value])
            {
                string token = part.Trim();
                if (token.Length == 0 || !allowIntegerValues && IsNumber(token) ||
                    !info.TryParse(token, EnumReadMode.Data, ReadConventions, out TEnum parsed, out _))
                    return null;
                raw |= info.ToRawValue(parsed);
            }

            return info.FromRawValue(raw);
        }

        private static bool IsNumber(string token) =>
            char.IsDigit(token[0]) || token[0] == '-' || token[0] == '+';
    }
}
