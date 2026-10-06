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
}
