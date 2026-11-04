using System.ComponentModel;
using NetEscapades.EnumGenerators;

namespace CSharpEssentials.Enums.Comparison.Benchmarks;

/// <summary>Twenty members with non-contiguous values; <see cref="PendingApproval"/> carries the only description.</summary>
[StringEnum]
[EnumExtensions(ExtensionClassNamespace = Generated.Namespace, ExtensionClassName = "StatusFastExtensions", MetadataSource = MetadataSource.DescriptionAttribute)]
public enum Status
{
    /// <summary>Draft.</summary>
    Draft = 0,

    /// <summary>Submitted.</summary>
    Submitted = 3,

    /// <summary>In review.</summary>
    InReview = 7,

    /// <summary>Changes requested.</summary>
    ChangesRequested = 10,

    /// <summary>Resubmitted.</summary>
    Resubmitted = 12,

    /// <summary>Escalated.</summary>
    Escalated = 15,

    /// <summary>On hold.</summary>
    OnHold = 20,

    /// <summary>Scheduled.</summary>
    Scheduled = 22,

    /// <summary>Assigned.</summary>
    Assigned = 27,

    /// <summary>In progress.</summary>
    InProgress = 31,

    /// <summary>Pending approval.</summary>
    [Description("Waiting for approval")]
    PendingApproval = 40,

    /// <summary>Approved.</summary>
    Approved = 44,

    /// <summary>Rejected.</summary>
    Rejected = 50,

    /// <summary>Shipped.</summary>
    Shipped = 53,

    /// <summary>Delivered.</summary>
    Delivered = 61,

    /// <summary>Returned.</summary>
    Returned = 64,

    /// <summary>Refunded.</summary>
    Refunded = 70,

    /// <summary>Cancelled.</summary>
    Cancelled = 75,

    /// <summary>Archived.</summary>
    Archived = 90,

    /// <summary>Closed.</summary>
    Closed = 100,
}
