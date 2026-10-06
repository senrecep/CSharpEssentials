using Microsoft.AspNetCore.Mvc;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

internal static class SampleHandlers
{
    public static SampleOrder GetOrder(
        SampleStatus status,
        SampleStatus? previous,
        [FromQuery] SampleStatus[] history,
        SamplePermissions permissions,
        SampleNaming naming,
        SamplePlain plain,
        SampleStatus next = SampleStatus.PendingApproval) =>
        new()
        {
            Status = status,
            Previous = previous,
            History = history,
            Permissions = permissions,
            Naming = naming,
            Plain = plain,
            Next = next,
        };

    public static SampleOrder CreateOrder(SampleOrder order) => order;

    public static SampleStatus[] GetStatuses() => [SampleStatus.Pending, SampleStatus.PendingApproval];

    public static SampleStatus EchoStatus([FromBody] SampleStatus? status) => status ?? SampleStatus.Unknown;

    public static SamplePlain GetPlain(SamplePlain plain) => plain;

    public static SampleChange GetChange() => new SampleGrantChange { Granted = SamplePermissions.Read | SamplePermissions.Write };
}
