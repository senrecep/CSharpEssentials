using CSharpEssentials.Enums;

namespace CSharpEssentials.Tests.AspNetCore.EnumIntegration;

[StringEnum]
internal enum EcStatus
{
    Pending = 0,
    PendingApproval = 1,
    Shipped = 2,
}

[StringEnum]
[Flags]
internal enum EcPermission
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
}

/// <summary>No [StringEnum] and no metadata: left to the framework.</summary>
internal enum EcPlain
{
    Active = 0,
    InProgress = 1,
}

/// <summary>The same DTO is returned by every wire format endpoint.</summary>
internal sealed record EcOrder(int Id, EcStatus Status, EcPermission Permissions);

internal sealed record EcPlainOrder(int Id, EcPlain Plain);

internal sealed record EcStatusBody(EcStatus Status);

internal static class EcEcho
{
    public static string Of<T>(T? value) where T : struct, Enum => value?.ToString() ?? "null";

    public static string Of<T>(IEnumerable<T> values) where T : struct, Enum => string.Join("|", values);

    public static EcOrder Order => new(1, EcStatus.PendingApproval, EcPermission.Read | EcPermission.Write);
}
