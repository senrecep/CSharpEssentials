namespace CSharpEssentials.Tests.EntityFrameworkCore.EnumColumns;

/// <summary>The row the enum conversion tests migrate: one plain enum column and one flags column.</summary>
public sealed class ConvertedOrder
{
    public int Id { get; set; }
    public StoredOrderStatus Status { get; set; }
    public StoredPermissions Permissions { get; set; }
}
