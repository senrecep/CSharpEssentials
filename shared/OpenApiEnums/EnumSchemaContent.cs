using System.ComponentModel;
using System.Reflection;
using System.Text;
using CSharpEssentials.Enums;

namespace CSharpEssentials.AspNetCore;

/// <summary>
/// The model-independent content of an enum component schema (design section 10.1), shared by the Swashbuckle and the
/// Microsoft.AspNetCore.OpenApi packages so both describe an enum the same way.
/// </summary>
internal sealed class EnumSchemaContent
{
    internal const string TableHeader = "| value | number | description |";
    internal const string ResponseOnly = "Response only:";
    internal const string Deprecated = "(deprecated)";

    private EnumSchemaContent(IEnumInfo info, EnumWireFormat format)
    {
        Info = info;
        IsNumber = format == EnumWireFormat.Number;
        IntegerFormat = info.UnderlyingType == typeof(long) || info.UnderlyingType == typeof(ulong) || info.UnderlyingType == typeof(uint)
            ? "int64"
            : "int32";
        VarNames = [.. info.Members.Select(static member => member.MemberName)];
        WireNames = [.. info.Members.Select(static member => member.WireName)];
        NumericTexts = [.. info.Members.Select(static member => member.NumericText)];
        Descriptions = [.. info.Members.Select(DescribeMember)];
    }

    public IEnumInfo Info { get; }

    /// <summary><see langword="true"/> for <c>type: integer</c>, <see langword="false"/> for <c>type: string</c>.</summary>
    public bool IsNumber { get; }

    /// <summary><c>int32</c> or <c>int64</c>, by the underlying type; used when <see cref="IsNumber"/>.</summary>
    public string IntegerFormat { get; }

    /// <summary>
    /// Whether the schema lists its values in <c>enum</c>. A number schema of a flags enum does not, because a combination
    /// is a valid value that no list can enumerate.
    /// </summary>
    public bool HasEnumValues => !(IsNumber && Info.IsFlags);

    /// <summary><c>x-enum-numeric-values</c> is written for string schemas and for flags number schemas (no <c>enum</c>).</summary>
    public bool HasNumericValuesExtension => !IsNumber || Info.IsFlags;

    public IReadOnlyList<string> VarNames { get; }

    public IReadOnlyList<string> WireNames { get; }

    /// <summary>The numbers as JSON number literals (sign-extended for signed underlying types).</summary>
    public IReadOnlyList<string> NumericTexts { get; }

    /// <summary>The <c>x-enum-descriptions</c> entries; empty when a member has no description.</summary>
    public IReadOnlyList<string> Descriptions { get; }

    public static EnumSchemaContent Create(IEnumInfo info, EnumWireFormat format) => new(info, format);

    /// <summary>
    /// Keeps the user's text (XML <c>summary</c>, or the <see cref="DescriptionAttribute"/> of the enum when there is none)
    /// and appends the value table. A table appended earlier is replaced, so applying it twice changes nothing.
    /// </summary>
    public string AppendTable(string? existing)
    {
        string text = StripTable(existing);
        if (text.Length == 0)
            text = Info.EnumType.GetCustomAttribute<DescriptionAttribute>(inherit: false)?.Description?.Trim() ?? string.Empty;

        var builder = new StringBuilder(text);
        if (builder.Length > 0)
            builder.Append("\n\n");
        builder.Append(TableHeader).Append("\n|---|---|---|");
        for (int i = 0; i < Info.Members.Count; i++)
        {
            builder.Append("\n| `").Append(Escape(WireNames[i])).Append("` | ").Append(NumericTexts[i]).Append(" | ")
                .Append(Escape(Descriptions[i])).Append(" |");
        }

        return builder.ToString();
    }

    /// <summary>The value written for <paramref name="member"/> in this format: its wire name or its number.</summary>
    public string ValueOf(IEnumMemberInfo member) => IsNumber ? member.NumericText : member.WireName;

    private static string StripTable(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;
        int table = description.IndexOf(TableHeader, StringComparison.Ordinal);
        return (table < 0 ? description : description[..table]).TrimEnd();
    }

    private static string DescribeMember(IEnumMemberInfo member)
    {
        string text = member.Description?.Trim() ?? string.Empty;
        if (member.IsFallback)
            text = text.Length == 0 ? ResponseOnly : $"{ResponseOnly} {text}";
        if (member.IsObsolete)
            text = text.Length == 0 ? Deprecated : $"{text} {Deprecated}";
        return text;
    }

    private static string Escape(string text) =>
        text.Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("\r\n", " ", StringComparison.Ordinal)
            .Replace('\n', ' ')
            .Replace('\r', ' ');
}
