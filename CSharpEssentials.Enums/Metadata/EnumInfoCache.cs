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
            if (value is null && EnumMetadata.TryGet(typeof(TEnum), out IEnumInfo? info))
            {
                value = (EnumInfo<TEnum>)info;
                Registered = value;
            }

            return value;
        }
    }

    public static void Set(EnumInfo<TEnum> info) => Registered = info;
}
