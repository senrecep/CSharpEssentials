using System.Text.Json.Serialization;
using CSharpEssentials.Enums;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

/// <summary>No <see cref="StringEnumAttribute"/> and no metadata: every layer keeps its framework default.</summary>
public enum GoldenPlain
{
    Active = 0,
    InProgress = 1,
}

/// <summary>The enum of the normative matrix (design section 5): default naming, an alias and a fallback member.</summary>
[StringEnum]
public enum GoldenOrderStatus
{
    Pending = 0,

    [EnumAlias("Approval")]
    PendingApproval = 1,

    [EnumFallback]
    Unknown = 99_999,
}

[StringEnum]
public enum GoldenNaming
{
    [JsonStringEnumMemberName("custom-name")]
    Original = 0,

    PlainValue = 1,
}

/// <summary>The flags enum of design section 5.1, with a named composite member.</summary>
[StringEnum]
[Flags]
public enum GoldenPermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    ReadWrite = Read | Write,
}

/// <summary>Written as numbers (<see cref="EnumWireFormat.Number"/>) and stored as integers.</summary>
[StringEnum(Storage = EnumStorage.Integer)]
public enum GoldenPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
}
