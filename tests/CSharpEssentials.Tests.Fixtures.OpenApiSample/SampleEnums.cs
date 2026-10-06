using System.ComponentModel;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

[StringEnum]
[Description("Order lifecycle state.")]
public enum SampleStatus
{
    [Description("Just created | not paid")]
    Pending = 0,

    PendingApproval = 1,

    [Obsolete("Use PendingApproval.")]
    [Description("Waiting for a reviewer")]
    Waiting = 2,

    [EnumFallback]
    Unknown = 99,
}

[StringEnum]
[Flags]
public enum SamplePermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
}

[StringEnum]
public enum SampleNaming
{
    [JsonStringEnumMemberName("custom-name")]
    Original = 0,

    PlainValue = 1,
}

[StringEnum]
public enum SampleSize : long
{
    Small = 1,
    Huge = 5_000_000_000,
}

[StringEnum]
public enum SampleQuota : ulong
{
    Low = 1,
    Unlimited = ulong.MaxValue,
}

/// <summary>No <see cref="StringEnumAttribute"/> and no metadata: the documents keep the framework schema.</summary>
public enum SamplePlain
{
    First = 0,
    Second = 1,
}
