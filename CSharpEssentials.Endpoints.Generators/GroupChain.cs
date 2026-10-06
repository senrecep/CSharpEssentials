using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Endpoints.Generators;

internal sealed class GroupChain(GroupChainStatus status, IReadOnlyList<INamedTypeSymbol> groups)
{
    public GroupChainStatus Status { get; } = status;

    public IReadOnlyList<INamedTypeSymbol> Groups { get; } = groups;
}
