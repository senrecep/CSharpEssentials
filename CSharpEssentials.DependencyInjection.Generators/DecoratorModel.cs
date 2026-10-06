namespace CSharpEssentials.DependencyInjection.Generators;

internal sealed record DecoratorModel(
    string DecoratorType,
    string SortName,
    int Order,
    string ServiceType,
    string? Key,
    EquatableArray<DecoratorParameterModel> Parameters);
