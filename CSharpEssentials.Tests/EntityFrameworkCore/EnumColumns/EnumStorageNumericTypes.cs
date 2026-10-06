using CSharpEssentials.Enums;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

[Flags]
[StringEnum]
public enum StoredWideMask : ulong
{
    None = 0,
    Low = 1,
    High = 1UL << 63,
}

[StringEnum(Storage = EnumStorage.Integer)]
public enum StoredSByteLevel : sbyte
{
    Below = -1,
    Above = 1,
}

[StringEnum(Storage = EnumStorage.Integer)]
public enum StoredByteLevel : byte
{
    Low = 1,
    High = 200,
}

[StringEnum(Storage = EnumStorage.Integer)]
public enum StoredShortLevel : short
{
    Below = -300,
    Above = 300,
}

[StringEnum(Storage = EnumStorage.Integer)]
public enum StoredUShortLevel : ushort
{
    Low = 1,
    High = 60_000,
}

[StringEnum(Storage = EnumStorage.Integer)]
public enum StoredUIntLevel : uint
{
    Low = 1,
    High = 4_000_000_000,
}

[StringEnum(Storage = EnumStorage.Integer)]
public enum StoredLongLevel : long
{
    Below = -5_000_000_000,
    Above = 5_000_000_000,
}

[Flags]
[StringEnum]
public enum StoredUIntMask : uint
{
    None = 0,
    Low = 1,
    High = 1u << 31,
}

/// <summary>A flags enum without a zero member, so legacy text storage has no name for 0.</summary>
[Flags]
[StringEnum]
public enum StoredAccess
{
    Read = 1,
    Write = 2,
}
