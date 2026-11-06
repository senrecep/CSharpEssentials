namespace CSharpEssentials.Validation.Benchmarks;

/// <summary>Which enum <see cref="EnumDefinedComparison"/> validates.</summary>
public enum MetadataKind
{
    /// <summary><see cref="ShipmentStatus"/>: a plain enum.</summary>
    Plain,

    /// <summary><see cref="ShipmentChannel"/>: a <c>[StringEnum]</c> enum with CSE-generated metadata.</summary>
    Generated,
}
