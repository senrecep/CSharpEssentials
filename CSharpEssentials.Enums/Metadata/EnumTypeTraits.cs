namespace CSharpEssentials.Enums;

/// <summary>
/// Range and sign of the underlying type of <typeparamref name="TEnum"/>, read once without reflection.
/// </summary>
internal static class EnumTypeTraits<TEnum> where TEnum : struct, Enum
{
    public static bool IsSigned { get; } = Type.GetTypeCode(typeof(TEnum)) is TypeCode.SByte or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64;

    public static long MinSigned { get; } = GetMinSigned(Type.GetTypeCode(typeof(TEnum)));

    public static ulong MaxValue { get; } = GetMaxValue(Type.GetTypeCode(typeof(TEnum)));

    public static string FormatRaw(ulong raw) =>
        IsSigned
            ? unchecked((long)raw).ToString(System.Globalization.CultureInfo.InvariantCulture)
            : raw.ToString(System.Globalization.CultureInfo.InvariantCulture);

    private static long GetMinSigned(TypeCode typeCode)
    {
        if (typeCode == TypeCode.SByte)
            return sbyte.MinValue;
        if (typeCode == TypeCode.Int16)
            return short.MinValue;
        return typeCode == TypeCode.Int32 ? int.MinValue : long.MinValue;
    }

    private static ulong GetMaxValue(TypeCode typeCode)
    {
        if (typeCode == TypeCode.SByte)
            return (ulong)sbyte.MaxValue;
        if (typeCode == TypeCode.Byte)
            return byte.MaxValue;
        if (typeCode == TypeCode.Int16)
            return (ulong)short.MaxValue;
        if (typeCode == TypeCode.UInt16)
            return ushort.MaxValue;
        if (typeCode == TypeCode.Int32)
            return int.MaxValue;
        if (typeCode == TypeCode.UInt32)
            return uint.MaxValue;
        return typeCode == TypeCode.Int64 ? long.MaxValue : ulong.MaxValue;
    }
}
