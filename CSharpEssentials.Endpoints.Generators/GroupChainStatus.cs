namespace CSharpEssentials.Endpoints.Generators;

internal enum GroupChainStatus
{
    Valid,
    MultipleGroupAttributes,
    InvalidGroupTarget,
    Cycle,
    InaccessibleGroup,
    SkippedGroup,
}
