using System.Text.Json.Serialization;
using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.AspNetCore.EnumBinding;

[StringEnum]
public enum EbStatus
{
    Active = 0,
    InProgress = 1,
    HTTPError = 2,
}

[StringEnum]
public enum EbCustom
{
    [JsonStringEnumMemberName("custom")]
    Original = 0,
    Plain = 1,
}

[StringEnum]
[Flags]
public enum EbPermission
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
}

/// <summary>Not marked with [StringEnum]: untouched by the default CanBind predicate.</summary>
public enum EbPlain
{
    Active = 0,
    InProgress = 1,
}

public sealed class EbAsParametersClass
{
    public EbStatus Status { get; set; }

    [FromQuery(Name = "p")]
    public EbPermission Perm { get; set; }
}

public sealed record EbAsParametersRecord([FromQuery(Name = "st")] EbStatus Status, EbStatus? Other);

public sealed class EbQueryDto
{
    public EbStatus Status { get; set; }

    public EbStatus? Optional { get; set; }
}

internal static class EbEcho
{
    public static string Of<T>(T? value) where T : struct, Enum => value?.ToString() ?? "null";

    public static string Of<T>(IEnumerable<T> values) where T : struct, Enum => string.Join("|", values);
}
