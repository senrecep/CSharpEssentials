namespace CSharpEssentials.DependencyInjection.Generators;

internal sealed record RegistrationModel(
    string ImplementationType,
    string Lifetime,
    string? Key,
    string Strategy,
    EquatableArray<ServiceModel> Services);
