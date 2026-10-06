namespace CSharpEssentials.Enums;

/// <summary>
/// Per enum static slot of the registered metadata; a generic read is one static property load.
/// </summary>
internal static class EnumInfoCache<TEnum> where TEnum : struct, Enum
{
    private static EnumInfo<TEnum>? Registered { get; set; }

    public static EnumInfo<TEnum>? Value
    {
        get
        {
            EnumInfo<TEnum>? value = Registered;
            // A type-only reference does not run the module initializer of the declaring assembly; run it once and retry.
            return value is null && EnumMetadata.TryRunModuleInitializer(typeof(TEnum).Module) ? Registered : value;
        }
    }

    public static void Set(EnumInfo<TEnum> info) => Registered = info;
}
