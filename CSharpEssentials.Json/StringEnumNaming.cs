using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;

namespace CSharpEssentials.Json;

/// <summary>
/// Single source of truth for the string form of enum members.
/// JSON (<see cref="ConditionalStringEnumConverter"/>), EF Core storage, OpenAPI schemas and
/// query/route binding all use this naming so the same member is spelled the same way everywhere.
/// </summary>
/// <remarks>
/// A member name is resolved as <c>[JsonStringEnumMemberName]</c> when present, otherwise
/// <c>namingPolicy.ConvertName(memberName)</c>. This mirrors <see cref="JsonStringEnumConverter"/>.
/// </remarks>
public static class StringEnumNaming
{
    /// <summary>
    /// The default naming policy (<see cref="JsonNamingPolicy.SnakeCaseLower"/>).
    /// </summary>
    public static JsonNamingPolicy DefaultPolicy { get; } = JsonNamingPolicy.SnakeCaseLower;

    /// <summary>
    /// The default "is this a string enum" predicate: an enum type marked with <see cref="StringEnumAttribute"/>.
    /// </summary>
    public static bool IsStringEnum(Type type) =>
        type is not null && type.IsEnum && type.IsDefined(typeof(StringEnumAttribute), inherit: false);

    private static readonly ConcurrentDictionary<(Type EnumType, JsonNamingPolicy? Policy), EnumNameTable> _tables = new();

    /// <summary>
    /// Gets the string form of an enum value.
    /// Flag combinations are joined with <c>", "</c>; undefined values are written as their number.
    /// </summary>
    public static string GetName<TEnum>(TEnum value, JsonNamingPolicy? namingPolicy = null)
        where TEnum : struct, Enum => GetTable(typeof(TEnum), namingPolicy).Format(value);

    /// <summary>
    /// Gets the string form of an enum value.
    /// </summary>
    public static string GetName(Type enumType, object value, JsonNamingPolicy? namingPolicy = null) =>
        GetTable(enumType, namingPolicy).Format(value);

    /// <summary>
    /// Gets the string forms of all members, in declaration order.
    /// </summary>
    public static IReadOnlyList<string> GetNames(Type enumType, JsonNamingPolicy? namingPolicy = null) =>
        GetTable(enumType, namingPolicy).Names;

    /// <summary>
    /// Gets the string forms of all members, in declaration order.
    /// </summary>
    public static IReadOnlyList<string> GetNames<TEnum>(JsonNamingPolicy? namingPolicy = null)
        where TEnum : struct, Enum => GetTable(typeof(TEnum), namingPolicy).Names;

    /// <summary>
    /// Parses a value written by <see cref="GetName{TEnum}"/>. Matching is case-insensitive and accepts
    /// the policy name, the C# member name and (optionally) the numeric value of a defined member.
    /// For <see cref="FlagsAttribute"/> enums a comma separated list is accepted.
    /// </summary>
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
    public static bool TryParse(Type enumType, string? value, out object? result, JsonNamingPolicy? namingPolicy = null, bool allowIntegerValues = true) =>
        GetTable(enumType, namingPolicy).TryParseValue(value, allowIntegerValues, out result);

    private static EnumNameTable GetTable(Type enumType, JsonNamingPolicy? namingPolicy)
    {
        if (enumType?.IsEnum != true)
            throw new ArgumentException($"Type '{enumType}' is not an enum.", nameof(enumType));
        return _tables.GetOrAdd((enumType, namingPolicy ?? DefaultPolicy), static key => new EnumNameTable(key.EnumType, key.Policy!));
    }

    private sealed class EnumNameTable
    {
        private readonly Type _enumType;
        private readonly bool _isFlags;
        private readonly ulong[] _values;
        private readonly string[] _names;
        private readonly Dictionary<string, ulong> _lookup;

        public EnumNameTable(Type enumType, JsonNamingPolicy policy)
        {
            _enumType = enumType;
            _isFlags = enumType.IsDefined(typeof(FlagsAttribute), inherit: false);
            _lookup = [with(StringComparer.OrdinalIgnoreCase)];
            FieldInfo[] fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
            _values = new ulong[fields.Length];
            _names = new string[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                _values[i] = ToUInt64(Enum.Parse(enumType, field.Name));
                _names[i] = GetCustomName(field) ?? policy.ConvertName(field.Name);
                _lookup.TryAdd(_names[i], _values[i]);
            }
            // C# member names are accepted as a fallback, after every policy name is registered.
            for (int i = 0; i < fields.Length; i++)
                _lookup.TryAdd(fields[i].Name, _values[i]);
        }

        public IReadOnlyList<string> Names => _names;

        public string Format(object value)
        {
            ulong raw = ToUInt64(value);
            int index = Array.IndexOf(_values, raw);
            if (index >= 0)
                return _names[index];

            if (_isFlags && raw != 0)
            {
                List<string> parts = [];
                ulong remaining = raw;
                for (int i = 0; i < _values.Length; i++)
                {
                    ulong flag = _values[i];
                    if (flag != 0 && (raw & flag) == flag && (remaining & flag) != 0)
                    {
                        parts.Add(_names[i]);
                        remaining &= ~flag;
                    }
                }
                if (remaining == 0)
                    return string.Join(", ", parts);
            }

            return ((Enum)Enum.ToObject(_enumType, value)).ToString("D");
        }

        public bool TryParseValue(string? value, bool allowIntegerValues, out object? result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (!_isFlags)
            {
                if (!TryParseSingle(value.Trim(), allowIntegerValues, out ulong single))
                    return false;
                result = Enum.ToObject(_enumType, single);
                return true;
            }

            ulong combined = 0;
            foreach (string part in value.Split(','))
            {
                string token = part.Trim();
                if (token.Length == 0 || !TryParseSingle(token, allowIntegerValues, out ulong flag))
                    return false;
                combined |= flag;
            }
            result = Enum.ToObject(_enumType, combined);
            return true;
        }

        private bool TryParseSingle(string token, bool allowIntegerValues, out ulong value)
        {
            if (_lookup.TryGetValue(token, out value))
                return true;

            if (allowIntegerValues && IsNumeric(token) &&
                ulong.TryParse(token, NumberStyles.None, CultureInfo.InvariantCulture, out value))
                return IsDefinedValue(value);

            value = 0;
            return false;
        }

        private bool IsDefinedValue(ulong value)
        {
            if (Array.IndexOf(_values, value) >= 0)
                return true;
            if (!_isFlags)
                return false;
            ulong remaining = value;
            foreach (ulong flag in _values)
                remaining &= ~flag;
            return remaining == 0;
        }

        private static bool IsNumeric(string token)
        {
            foreach (char c in token)
                if (c is < '0' or > '9')
                    return false;
            return true;
        }

        private static string? GetCustomName(FieldInfo field)
        {
#if NET9_0_OR_GREATER
            return field.GetCustomAttribute<JsonStringEnumMemberNameAttribute>()?.Name;
#else
            // JsonStringEnumMemberNameAttribute ships with System.Text.Json 9+, read it by name
            // so older package versions still compile.
            foreach (CustomAttributeData attribute in field.CustomAttributes)
                if (attribute.AttributeType.FullName == "System.Text.Json.Serialization.JsonStringEnumMemberNameAttribute" &&
                    attribute.ConstructorArguments.Count == 1)
                    return attribute.ConstructorArguments[0].Value as string;
            return null;
#endif
        }

        private static ulong ToUInt64(object value) => value switch
        {
            Enum e => ToUInt64(Convert.ChangeType(e, Enum.GetUnderlyingType(e.GetType()), CultureInfo.InvariantCulture)),
            sbyte v => unchecked((ulong)v),
            short v => unchecked((ulong)v),
            int v => unchecked((ulong)v),
            long v => unchecked((ulong)v),
            byte v => v,
            ushort v => v,
            uint v => v,
            ulong v => v,
            _ => throw new ArgumentException($"Unsupported enum value type '{value.GetType()}'.", nameof(value)),
        };
    }
}
