using System.ComponentModel;
using CSharpEssentials.Enums;

namespace Examples.AspNetCore.Swashbuckle;

[StringEnum]
[Description("Order lifecycle state.")]
public enum OrderStatus
{
    [Description("Created, waiting for payment.")]
    Pending,

    [Description("Paid, waiting for approval.")]
    PendingApproval,

    [EnumFallback]
    [Description("A value this API version does not know.")]
    Unknown = 99,
}

public sealed record Order(int Id, OrderStatus Status);

public sealed record CreateOrder(string Customer, OrderStatus? Status);

public sealed record UpdateOrder(OrderStatus Status);
