using System.ComponentModel;
using System.Text.Json.Serialization;

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

/// <summary>A polymorphic body: the enum property lives on the derived type only.</summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(SampleGrantChange), "grant")]
public class SampleChange
{
    public string? Note { get; init; }
}

public sealed class SampleGrantChange : SampleChange
{
    public SamplePermissions Granted { get; init; }
}
