namespace CSharpEssentials.DependencyInjection.Generators;

internal sealed record ServiceTypeModel(
    string SortName,
    EquatableArray<RegistrationModel> Registrations,
    EquatableArray<DecoratorModel> Decorators);
