using CSharpEssentials.Enums;

namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

[StringEnum]
public enum StoredOrderStatus
{
    Pending,
    PendingApproval,
    Shipped,
}

[StringEnum]
public enum StoredOrderStatusV2
{
    Pending,
    PendingApproval,
    Shipped,
    Returned,
}

[StringEnum]
public enum StoredShipment
{
    Packed,
    InTransit,
    [EnumFallback] Unknown,
}

[StringEnum(Storage = EnumStorage.Integer)]
public enum StoredPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
}

[Flags]
[StringEnum]
public enum StoredPermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
}

[StringEnum]
public enum QuotedOrderStatus
{
    [EnumAlias("o'pen")] Open,
    Closed,
}

public enum PlainColor
{
    Red,
    Green,
}

public sealed class StoredOrder
{
    public int Id { get; set; }
    public StoredOrderStatus Status { get; set; }
    public StoredOrderStatus? PreviousStatus { get; set; }
    public StoredShipment Shipment { get; set; }
    public StoredPriority Priority { get; set; }
    public StoredPermissions Permissions { get; set; }
    public StoredPermissions SharedPermissions { get; set; }
    public PlainColor Color { get; set; }
    public StoredOrderStatus ManualStatus { get; set; }
    public List<StoredOrderStatus> History { get; set; } = [];
    public List<StoredPriority> Priorities { get; set; } = [];
    public StoredOrderDetails Details { get; set; } = new();
}

public sealed class StoredOrderDetails
{
    public StoredOrderStatus Status { get; set; }
    public StoredPriority Priority { get; set; }
    public PlainColor Color { get; set; }
}

public sealed class StoredOrderV1
{
    public int Id { get; set; }
    public StoredOrderStatus Status { get; set; }
}

public sealed class StoredOrderV2
{
    public int Id { get; set; }
    public StoredOrderStatusV2 Status { get; set; }
}

public sealed class PlainOrder
{
    public int Id { get; set; }
    public PlainColor Color { get; set; }
    public PlainColor? PreviousColor { get; set; }
    public List<PlainColor> Colors { get; set; } = [];
}

[StringEnum]
public enum StoredEmptyStatus
{
}

public sealed class StoredNumericOrder
{
    public int Id { get; set; }
    public StoredSByteLevel SByteLevel { get; set; }
    public StoredByteLevel ByteLevel { get; set; }
    public StoredShortLevel ShortLevel { get; set; }
    public StoredUShortLevel UShortLevel { get; set; }
    public StoredUIntLevel UIntLevel { get; set; }
    public StoredLongLevel LongLevel { get; set; }
    public StoredUIntMask UIntFlags { get; set; }
    public StoredWideMask WideFlags { get; set; }
    public StoredEmptyStatus Empty { get; set; }
    public StoredAccess Access { get; set; }
}

public sealed class StoredAddress
{
    public StoredOrderStatus Status { get; set; }
}

public sealed class StoredComplexOrder
{
    public int Id { get; set; }
    public StoredAddress Address { get; set; } = new();
}

public sealed class StoredUnreachableOrder
{
    public int Id { get; set; }
    public CSharpEssentials.Tests.Fixtures.EnumsContracts.UnreachableHolder<int>.Status Status { get; set; }
}

public abstract class StoredParcel
{
    public int Id { get; set; }
}

public sealed class StoredLetter : StoredParcel
{
    public StoredOrderStatus Status { get; set; }
}

public sealed class StoredBox : StoredParcel
{
    public StoredOrderStatus Status { get; set; }
}

public sealed class StoredCrate : StoredParcel
{
    public StoredShipment Status { get; set; }
}
