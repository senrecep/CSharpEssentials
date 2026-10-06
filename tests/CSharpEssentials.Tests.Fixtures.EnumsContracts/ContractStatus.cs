using CSharpEssentials.Enums;

namespace CSharpEssentials.Tests.Fixtures.EnumsContracts;

/// <summary>An order state shared through a contracts assembly.</summary>
[StringEnum]
public enum ContractStatus
{
    /// <summary>Waiting for approval.</summary>
    PendingApproval,

    /// <summary>Shipped to the customer.</summary>
    HTTPShipped,

    /// <summary>A value sent by a newer producer.</summary>
    [EnumFallback]
    Unknown = 99,
}
