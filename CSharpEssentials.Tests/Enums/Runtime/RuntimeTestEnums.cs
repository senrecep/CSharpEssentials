using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;

namespace CSharpEssentials.Tests.Enums.Runtime;

public enum ParserStatus
{
    [Description("Waiting for work")]
    Pending,

    [EnumAlias("Started", "Running")]
    InProgress,

    [JsonStringEnumMemberName("done")]
    Completed,

    [EnumMember(Value = "cancelled_by_user")]
    Cancelled,

    HTTPStatus,

    [Obsolete("Use Cancelled")]
    Aborted,

    [EnumFallback]
    Unknown = 100,
}

public enum StrictStatus
{
    Open,
    Closed,
}

[Flags]
public enum ParserPermissions : byte
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    ReadWrite = Read | Write,
}

public enum HugeValue : ulong
{
    Zero = 0,
    Max = ulong.MaxValue,
}

public enum SignedValue : long
{
    Negative = -5,
    Min = long.MinValue,
    Positive = 7,
}

public enum TinyValue : sbyte
{
    Low = -128,
    High = 127,
}

public enum ManuallyRegistered
{
    First,
    SecondValue,
}

public enum LazilyRegistered
{
    Only,
}
