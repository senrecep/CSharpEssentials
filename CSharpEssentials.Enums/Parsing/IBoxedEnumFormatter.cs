namespace CSharpEssentials.Enums;

/// <summary>
/// Formats a boxed value of the enum the metadata describes, without a visitor or a generic virtual call.
/// </summary>
internal interface IBoxedEnumFormatter
{
    /// <summary>Formats <paramref name="value"/> like <see cref="EnumInfo{TEnum}.Format"/>.</summary>
    string FormatBoxed(object value, EnumWireFormat format);

    /// <summary>
    /// Adds the single flags of <paramref name="value"/> for <see cref="EnumWireFormat.String"/> flags, otherwise one formatted entry.
    /// </summary>
    void AddFormatted(object value, EnumWireFormat format, IList<string> values);
}
