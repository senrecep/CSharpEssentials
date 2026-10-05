using System.Text.Json.Serialization;
using CSharpEssentials.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CSharpEssentials.Tests.AspNetCore.EnumBinding;

[StringEnum]
internal enum EbStatus
{
    Active = 0,
    InProgress = 1,
    HTTPError = 2,
}

[StringEnum]
internal enum EbCustom
{
    [JsonStringEnumMemberName("custom")]
    Original = 0,
    Plain = 1,
}

[StringEnum]
[Flags]
internal enum EbPermission
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
}

/// <summary>Not marked with [StringEnum]: untouched by the default CanBind predicate.</summary>
internal enum EbPlain
{
    Active = 0,
    InProgress = 1,
}

internal sealed class EbAsParametersClass
{
    public EbStatus Status { get; set; }

    [FromQuery(Name = "p")]
    public EbPermission Perm { get; set; }
}

internal sealed record EbAsParametersRecord([FromQuery(Name = "st")] EbStatus Status, EbStatus? Other);

internal sealed class EbQueryDto
{
    public EbStatus Status { get; set; }

    public EbStatus? Optional { get; set; }
}

internal sealed class EbNestedDto
{
    public EbQueryDto? Inner { get; set; }
}

internal sealed class EbCycleDto
{
    public EbStatus Status { get; set; }

    public EbCycleDto? Next { get; set; }
}

internal sealed class EbExcludedDto
{
    public EbStatus Status { get; set; }

    [BindNever]
    public EbStatus Hidden { get; set; }
}

internal static class EbEcho
{
    public static string Describe(EbNestedDto dto) =>
        dto.Inner is null ? "null" : $"{dto.Inner.Status}|{Of(dto.Inner.Optional)}";

    public static string Of<T>(T? value) where T : struct, Enum => value?.ToString() ?? "null";

    public static string Of<T>(IEnumerable<T> values) where T : struct, Enum => string.Join("|", values);
}
