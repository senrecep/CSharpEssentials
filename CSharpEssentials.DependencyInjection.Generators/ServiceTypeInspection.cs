namespace CSharpEssentials.DependencyInjection.Generators;

internal sealed class ServiceTypeInspection(
    string sortName,
    IReadOnlyList<InspectionIssue> issues,
    IReadOnlyList<InspectedRegistration> registrations,
    IReadOnlyList<DecoratorModel> decorators)
{
    public string SortName { get; } = sortName;

    public IReadOnlyList<InspectionIssue> Issues { get; } = issues;

    public IReadOnlyList<InspectedRegistration> Registrations { get; } = registrations;

    public IReadOnlyList<DecoratorModel> Decorators { get; } = decorators;

    public bool HasErrors => Issues.Any(static issue => issue.IsError);

    public ServiceTypeModel? ToModel()
    {
        if (HasErrors || Registrations.Count == 0 && Decorators.Count == 0)
        {
            return null;
        }

        return new ServiceTypeModel(
            SortName,
            new EquatableArray<RegistrationModel>([.. Registrations.Select(static registration => registration.Model)]),
            new EquatableArray<DecoratorModel>([.. Decorators]));
    }
}
