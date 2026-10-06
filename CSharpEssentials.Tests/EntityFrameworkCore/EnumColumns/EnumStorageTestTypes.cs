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
    Low,
    Medium,
    High,
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
