using CSharpEssentials.Enums;

namespace CSharpEssentials.Tests.Http;

[StringEnum]
public enum HttpOrderStatus
{
    Pending = 0,

    [EnumAlias("Approval")]
    PendingApproval = 1,

    [EnumFallback]
    Unknown = 99_999,
}

[StringEnum, Flags]
public enum HttpPermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    ReadWrite = Read | Write,
}

public enum HttpPlainStatus
{
    Pending = 0,
    Approved = 1,
}

public sealed record HttpOrderPayload(HttpOrderStatus Status, HttpPermissions Permissions, List<HttpOrderStatus> History);

public sealed record HttpPlainPayload(HttpPlainStatus Status);
