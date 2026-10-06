using Microsoft.CodeAnalysis;

namespace CSharpEssentials.DependencyInjection.Generators;

internal sealed class InspectionIssue(DiagnosticDescriptor descriptor, AttributeData attribute, params object?[] arguments)
{
    public DiagnosticDescriptor Descriptor { get; } = descriptor;

    public AttributeData Attribute { get; } = attribute;

    public object?[] Arguments { get; } = arguments;

    public bool IsError => Descriptor.DefaultSeverity == DiagnosticSeverity.Error;
}
