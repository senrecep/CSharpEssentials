using System.ComponentModel;

namespace CSharpEssentials.Tests.Fixtures.OpenApiSample;

public sealed class SampleOrder
{
    public SampleStatus Status { get; init; }

    public SampleStatus? Previous { get; init; }

    public SamplePermissions Permissions { get; init; }

    public SamplePermissions? OptionalPermissions { get; init; }

    public IReadOnlyList<SampleStatus> History { get; init; } = [];

    public IReadOnlyList<SampleStatus?> Gaps { get; init; } = [];

    [DefaultValue(SampleStatus.PendingApproval)]
    public SampleStatus Next { get; init; } = SampleStatus.PendingApproval;

    [DefaultValue(SamplePermissions.Read | SamplePermissions.Write)]
    public SamplePermissions DefaultPermissions { get; init; } = SamplePermissions.Read | SamplePermissions.Write;

    public SampleNaming Naming { get; init; }

    public SampleSize Size { get; init; }

    public SamplePlain Plain { get; init; }
}
