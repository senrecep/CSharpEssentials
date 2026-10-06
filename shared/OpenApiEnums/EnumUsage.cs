using System.Globalization;
using CSharpEssentials.Enums;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// How an enum is used by a property, parameter or body: the enum itself, <see cref="Nullable{T}"/>, or a collection of
/// either. The shared component describes the enum; the usage shape (nullable, array, default) is written where it is used.
/// </summary>
internal sealed class EnumUsage
{
    private EnumUsage(IEnumInfo info, bool isNullable, bool isCollection, bool itemsNullable)
    {
        Info = info;
        IsNullable = isNullable;
        IsCollection = isCollection;
        ItemsNullable = itemsNullable;
    }

    public IEnumInfo Info { get; }

    public Type EnumType => Info.EnumType;

    /// <summary>A <see cref="Nullable{T}"/> enum (not a collection).</summary>
    public bool IsNullable { get; }

    /// <summary>An array or <see cref="IEnumerable{T}"/> of the enum.</summary>
    public bool IsCollection { get; }

    /// <summary>The items of the collection are <see cref="Nullable{T}"/>.</summary>
    public bool ItemsNullable { get; }

    /// <summary>
    /// Classifies <paramref name="type"/>; <see langword="null"/> when it is not a handled enum, a nullable handled enum or a
    /// collection of them (plain enums keep the framework schema).
    /// </summary>
    public static EnumUsage? Classify(Type? type, OpenApiEnumConventions conventions)
    {
        if (type is null || type == typeof(string))
            return null;

        if (Unwrap(type, out bool nullable) is { } enumType)
            return conventions.Resolve(enumType) is { } info ? new EnumUsage(info, nullable, isCollection: false, itemsNullable: false) : null;

        if (GetItemType(type) is { } itemType && Unwrap(itemType, out bool itemsNullable) is { } itemEnumType)
            return conventions.Resolve(itemEnumType) is { } info ? new EnumUsage(info, isNullable: false, isCollection: true, itemsNullable) : null;

        return null;
    }

    /// <summary>
    /// The <c>default</c> of a scalar usage in <paramref name="format"/>: one wire name or number, or the wire names of the single
    /// flags of a <see cref="FlagsAttribute"/> value in the string format. <see langword="null"/> when there is no default or it
    /// is not a defined value.
    /// </summary>
    public IReadOnlyList<string>? FormatDefault(object? value, EnumWireFormat format)
    {
        if (value is null || value is DBNull || IsCollection)
            return null;

        object enumValue;
        if (value.GetType() == EnumType)
            enumValue = value;
        else if (value is IConvertible && value.GetType().IsPrimitive)
            enumValue = Enum.ToObject(EnumType, value);
        else
            return null;

        try
        {
            return Info.Accept(new DefaultVisitor(enumValue, format));
        }
        catch (EnumValueException)
        {
            return null;
        }
    }

    /// <summary>Whether the usage is written as an array in <paramref name="format"/> (a collection, or flags as strings).</summary>
    public bool IsArray(EnumWireFormat format) => IsCollection || IsFlagsArray(format);

    /// <summary>A <see cref="FlagsAttribute"/> enum is an array of wire names in the string format and one integer as a number.</summary>
    public bool IsFlagsArray(EnumWireFormat format) => Info.IsFlags && format == EnumWireFormat.String;

    /// <summary>Whether <paramref name="text"/>, a member number, fits <see cref="long"/>.</summary>
    public static bool TryParseNumber(string text, out long number) =>
        long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out number);

    private static Type? Unwrap(Type type, out bool nullable)
    {
        Type? underlying = Nullable.GetUnderlyingType(type);
        nullable = underlying is not null;
        Type candidate = underlying ?? type;
        return candidate.IsEnum ? candidate : null;
    }

    private static Type? GetItemType(Type type)
    {
        if (type.IsArray)
            return type.GetArrayRank() == 1 ? type.GetElementType() : null;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            return type.GetGenericArguments()[0];

        Type? enumerable = null;
        foreach (Type candidate in type.GetInterfaces())
        {
            if (!candidate.IsGenericType || candidate.GetGenericTypeDefinition() != typeof(IEnumerable<>))
                continue;
            if (enumerable is not null)
                return null;
            enumerable = candidate;
        }

        return enumerable?.GetGenericArguments()[0];
    }

    private sealed class DefaultVisitor(object value, EnumWireFormat format) : IEnumInfoVisitor<IReadOnlyList<string>?>
    {
        public IReadOnlyList<string>? Visit<TEnum>(EnumInfo<TEnum> info) where TEnum : struct, Enum
        {
            var typed = (TEnum)value;
            if (info.IsFlags && format == EnumWireFormat.String)
            {
                List<string> names = [];
                info.FormatFlags(typed, names);
                return names;
            }

            return info.IsDefined(typed) || info.IsFlags ? [info.Format(typed, format)] : null;
        }
    }
}
