using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace CSharpEssentials.Enums;

/// <summary>
/// The one formatter of the enum contract, over generated metadata (<see cref="EnumMetadata.Get{TEnum}"/>).
/// </summary>
public static class EnumValueFormatter
{
    /// <summary>
    /// The wire name (or the number) of <paramref name="value"/>. <see cref="FlagsAttribute"/> values are their single flags joined
    /// with <c>,</c>. Undefined values throw <see cref="EnumValueException"/>.
    /// </summary>
    public static string Format<TEnum>(TEnum value, EnumWireFormat format) where TEnum : struct, Enum =>
        EnumMetadata.Get<TEnum>().Format(value, format);

    /// <summary>Adds the wire names of the single flags of <paramref name="value"/>; zero adds nothing.</summary>
    public static void FormatFlags<TEnum>(TEnum value, IList<string> names) where TEnum : struct, Enum =>
        EnumMetadata.Get<TEnum>().FormatFlags(value, names);

    /// <summary>
    /// Formats a boxed enum value like <see cref="Format{TEnum}(TEnum, EnumWireFormat)"/>, for code that has only an
    /// <see cref="object"/> (HTTP client adapters).
    /// </summary>
    /// <param name="value">A boxed enum value whose type passes <see cref="EnumConventions.CanHandle"/> and has generated metadata.</param>
    /// <param name="conventions">The conventions; <see cref="EnumConventions.WriteAs"/> is used when <paramref name="format"/> is null.</param>
    /// <param name="format">The output format, overriding <see cref="EnumConventions.WriteAs"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not a handled enum.</exception>
    /// <exception cref="EnumValueException"><paramref name="value"/> is not a defined value.</exception>
    public static string Format(object value, EnumConventions conventions, EnumWireFormat? format = null)
    {
        _ = value ?? throw new ArgumentNullException(nameof(value));

        return TryFormat(value, conventions, out string? text, format)
            ? text
            : throw new ArgumentException($"'{value.GetType().FullName}' is not an enum handled by the enum conventions.", nameof(value));
    }

    /// <summary>
    /// Formats a boxed enum value like <see cref="Format{TEnum}(TEnum, EnumWireFormat)"/>. Returns <see langword="false"/> when
    /// <paramref name="value"/> is null, not an enum, rejected by <see cref="EnumConventions.CanHandle"/>, so the caller can
    /// fall back to its own formatting.
    /// </summary>
    /// <exception cref="EnumValueException"><paramref name="value"/> is a handled enum but not a defined value.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="value"/> is a handled enum without generated metadata.</exception>
    public static bool TryFormat(object? value, EnumConventions conventions, [NotNullWhen(true)] out string? text, EnumWireFormat? format = null)
    {
        _ = conventions ?? throw new ArgumentNullException(nameof(conventions));

        if (!TryGetInfo(value, conventions, out IEnumInfo? info))
        {
            text = null;
            return false;
        }

        text = info.Accept(new FormatVisitor(value, format ?? conventions.WriteAs));
        return true;
    }

    /// <summary>
    /// Adds the formatted values of a handled enum value or of an <see cref="IEnumerable"/> of handled enum values to
    /// <paramref name="values"/>, one entry per repeated query key. <see cref="FlagsAttribute"/> values add one wire name per single
    /// flag (zero adds nothing) or, with <see cref="EnumWireFormat.Number"/>, one number. Null items are skipped.
    /// </summary>
    /// <returns>
    /// <see langword="false"/>, with nothing added, when <paramref name="value"/> is not a handled enum and not a collection
    /// whose non-null items are all handled enums (an empty collection included).
    /// </returns>
    /// <exception cref="EnumValueException">A handled enum value is not defined.</exception>
    public static bool TryFormatMany(object? value, EnumConventions conventions, IList<string> values, EnumWireFormat? format = null)
    {
        _ = conventions ?? throw new ArgumentNullException(nameof(conventions));
        _ = values ?? throw new ArgumentNullException(nameof(values));

        EnumWireFormat wireFormat = format ?? conventions.WriteAs;
        if (TryGetInfo(value, conventions, out IEnumInfo? info))
            return info.Accept(new FormatManyVisitor(value, wireFormat, values));

        if (value is string || value is not IEnumerable items)
            return false;

        List<string> formatted = [];
        bool hasItems = false;
        foreach (object? item in items)
        {
            if (item is null)
                continue;
            if (!TryGetInfo(item, conventions, out IEnumInfo? itemInfo))
                return false;

            hasItems = itemInfo.Accept(new FormatManyVisitor(item, wireFormat, formatted));
        }

        if (!hasItems)
            return false;

        foreach (string text in formatted)
            values.Add(text);
        return true;
    }

    private static bool TryGetInfo([NotNullWhen(true)] object? value, EnumConventions conventions, [NotNullWhen(true)] out IEnumInfo? info)
    {
        info = null;
        if (value is not Enum)
            return false;

        Type type = value.GetType();
        if (!conventions.CanHandle(type))
            return false;
        if (EnumMetadata.TryGet(type, out info))
            return true;

        throw new InvalidOperationException(
            $"Enum '{type.FullName}' is handled by the enum conventions but has no generated metadata. Rebuild its project with the " +
            "CSharpEssentials.Enums 5.0 generator (C# 9 or newer) and make the enum and its containing types public or internal " +
            "(not private, protected, file-local or nested in a generic type).");
    }

    private sealed class FormatVisitor(object value, EnumWireFormat format) : IEnumInfoVisitor<string>
    {
        public string Visit<TEnum>(EnumInfo<TEnum> info) where TEnum : struct, Enum =>
            info.Format((TEnum)value, format);
    }

    private sealed class FormatManyVisitor(object value, EnumWireFormat format, IList<string> values) : IEnumInfoVisitor<bool>
    {
        public bool Visit<TEnum>(EnumInfo<TEnum> info) where TEnum : struct, Enum
        {
            if (info.IsFlags && format == EnumWireFormat.String)
                info.FormatFlags((TEnum)value, values);
            else
                values.Add(info.Format((TEnum)value, format));
            return true;
        }
    }
}
