using Microsoft.CodeAnalysis;

namespace CSharpEssentials.DependencyInjection.Generators;

internal sealed class InspectedRegistration(RegistrationModel model, AttributeData attribute)
{
    public RegistrationModel Model { get; } = model;

    public AttributeData Attribute { get; } = attribute;
}
