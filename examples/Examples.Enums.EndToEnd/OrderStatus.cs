using System.ComponentModel;
using CSharpEssentials.Enums;

namespace Examples.Enums.EndToEnd;

[StringEnum]
[Description("Order lifecycle state.")]
public enum OrderStatus
{
    [Description("Created, waiting for payment.")]
    Pending = 0,

    [EnumAlias("Approval")]
    [Description("Paid, waiting for manual approval.")]
    PendingApproval = 1,

    [Description("Shipped to the customer.")]
    Shipped = 2,

    [EnumFallback]
    [Description("a value this API version does not know.")]
    Unknown = 99,
}
