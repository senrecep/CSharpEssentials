using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Endpoints.Generators;

internal sealed class GroupTarget(AttributeData attribute, ITypeSymbol? type)
{
    public AttributeData Attribute { get; } = attribute;

    public ITypeSymbol? Type { get; } = type;
}
