using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.DependencyInjection.Generators;

internal static class ServiceTypeInspector
{
    public const string AttributeNamespace = "CSharpEssentials.DependencyInjection";

    public const string ExcludeAttribute = "ExcludeFromRegistrationAttribute";

    public const string DecoratesAttribute = "DecoratesAttribute";

    public const string StrategyType = "global::CSharpEssentials.DependencyInjection.RegistrationStrategy";

    public const string ThrowStrategy = StrategyType + ".Throw";

    private const string ContainerNamespace = "Microsoft.Extensions.DependencyInjection";

    private const string DefaultStrategy = StrategyType + ".Add";

    private const int Self = 0;

    private const int SelfWithInterfaces = 1;

    private const int ImplementedInterfaces = 2;

    private static readonly string[] Lifetimes = ["Scoped", "Singleton", "Transient"];

    private static readonly string[] ExcludedInterfaces =
    [
        "System.IDisposable",
        "System.IAsyncDisposable",
        "System.IEquatable`1",
        "System.IComparable`1",
        "System.Collections.IEnumerable",
    ];

    public static ServiceTypeInspection Inspect(INamedTypeSymbol type, CancellationToken cancellationToken)
    {
        List<InspectionIssue> issues = [];
        List<InspectedRegistration> registrations = [];
        List<DecoratorModel> decorators = [];
        string sortName = TypeNames.MetadataFullName(type);
        if (type.TypeKind != TypeKind.Class || HasAttribute(type.GetAttributes(), ExcludeAttribute))
        {
            return new ServiceTypeInspection(sortName, issues, registrations, decorators);
        }

        foreach (AttributeData attribute in type.GetAttributes())
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (TryGetLifetime(attribute.AttributeClass, out string lifetime))
            {
                InspectRegistration(type, attribute, lifetime, issues, registrations);
            }
            else if (IsAttribute(attribute.AttributeClass, DecoratesAttribute))
            {
                InspectDecorator(type, attribute, issues, decorators);
            }
        }

        return new ServiceTypeInspection(sortName, issues, registrations, decorators);
    }

    public static bool HasAttribute(IEnumerable<AttributeData> attributes, string name) =>
        attributes.Any(attribute => IsAttribute(attribute.AttributeClass, name));

    public static bool IsAttribute(INamedTypeSymbol? attributeClass, string name) =>
        attributeClass is not null &&
        string.Equals(attributeClass.Name, name, StringComparison.Ordinal) &&
        string.Equals(attributeClass.ContainingNamespace?.ToDisplayString(), AttributeNamespace, StringComparison.Ordinal);

    public static bool IsContainerAttribute(AttributeData attribute, string name) =>
        attribute.AttributeClass is { } attributeClass &&
        string.Equals(attributeClass.Name, name, StringComparison.Ordinal) &&
        string.Equals(attributeClass.ContainingNamespace?.ToDisplayString(), ContainerNamespace, StringComparison.Ordinal);

    private static bool TryGetLifetime(INamedTypeSymbol? attributeClass, out string lifetime)
    {
        foreach (string candidate in Lifetimes)
        {
            if (IsAttribute(attributeClass, "Register" + candidate + "Attribute"))
            {
                lifetime = candidate;
                return true;
            }
        }

        lifetime = string.Empty;
        return false;
    }

    private static void InspectRegistration(
        INamedTypeSymbol type,
        AttributeData attribute,
        string lifetime,
        List<InspectionIssue> issues,
        List<InspectedRegistration> registrations)
    {
        if (!CheckConstructible(type, attribute, issues))
        {
            return;
        }

        ITypeSymbol? explicitService = ReadServiceType(attribute);
        if (explicitService is not null && !Implements(type, explicitService))
        {
            issues.Add(NotImplemented(type, explicitService, attribute));
            return;
        }

        int? serviceAs = ReadNamed(attribute, "As")?.Value as int?;
        List<ITypeSymbol> services = ResolveServices(type, explicitService, serviceAs);
        if (explicitService is null && serviceAs is null &&
            SymbolEqualityComparer.Default.Equals(services[0], type) && GetServiceInterfaces(type).Count > 0)
        {
            issues.Add(new InspectionIssue(DependencyInjectionDiagnostics.RegisteredAsSelf, attribute, type.ToDisplayString(), "I" + type.Name));
        }

        bool forward = serviceAs == SelfWithInterfaces && !type.IsGenericType;
        ServiceModel[] models = [.. services
            .Select(service => new ServiceModel(
                TypeNames.ForTypeOf(service),
                forward && !SymbolEqualityComparer.Default.Equals(service, type)))];
        TypedConstant? strategy = ReadNamed(attribute, "Strategy");
        RegistrationModel model = new(
            TypeNames.ForTypeOf(type),
            lifetime,
            ReadKey(attribute),
            strategy is { IsNull: false } value ? ConstantFormatter.Format(value) : DefaultStrategy,
            new EquatableArray<ServiceModel>(models));
        registrations.Add(new InspectedRegistration(model, attribute));
    }

    private static void InspectDecorator(INamedTypeSymbol type, AttributeData attribute, List<InspectionIssue> issues, List<DecoratorModel> decorators)
    {
        if (!CheckConstructible(type, attribute, issues))
        {
            return;
        }

        ITypeSymbol? service = ReadServiceType(attribute);
        if (service is null || !Implements(type, service))
        {
            issues.Add(NotImplemented(type, service, attribute));
            return;
        }

        if (type.IsGenericType)
        {
            issues.Add(new InspectionIssue(
                DependencyInjectionDiagnostics.OpenGenericDecorator,
                attribute,
                type.ToDisplayString(),
                TypeNames.ForTypeOf(service),
                TypeNames.ForTypeOf(type)));
            return;
        }

        IMethodSymbol[] constructors = [.. type.InstanceConstructors
            .Where(static constructor => constructor.DeclaredAccessibility == Accessibility.Public)];
        if (constructors.Length != 1)
        {
            issues.Add(new InspectionIssue(DependencyInjectionDiagnostics.DecoratorConstructorCount, attribute, type.ToDisplayString(), constructors.Length));
            return;
        }

        IMethodSymbol constructor = constructors[0];
        if (constructor.Parameters.Count(parameter => SymbolEqualityComparer.Default.Equals(parameter.Type, service)) != 1)
        {
            issues.Add(new InspectionIssue(DependencyInjectionDiagnostics.DecoratorWithoutInner, attribute, type.ToDisplayString(), service.ToDisplayString()));
            return;
        }

        decorators.Add(new DecoratorModel(
            TypeNames.FullyQualified(type),
            TypeNames.MetadataFullName(type),
            ReadNamed(attribute, "Order")?.Value as int? ?? 0,
            TypeNames.FullyQualified(service),
            ReadKey(attribute),
            new EquatableArray<DecoratorParameterModel>([.. constructor.Parameters.Select(parameter => CreateParameter(parameter, service))])));
    }

    private static DecoratorParameterModel CreateParameter(IParameterSymbol parameter, ITypeSymbol service)
    {
        string type = TypeNames.FullyQualified(parameter.Type);
        if (SymbolEqualityComparer.Default.Equals(parameter.Type, service))
        {
            return new DecoratorParameterModel(DecoratorParameterKind.Inner, type, null);
        }

        ImmutableArray<AttributeData> attributes = parameter.GetAttributes();
        if (attributes.Any(static attribute => IsContainerAttribute(attribute, "ServiceKeyAttribute")))
        {
            return new DecoratorParameterModel(DecoratorParameterKind.ServiceKey, type, null);
        }

        AttributeData? fromKeyed = attributes.FirstOrDefault(static attribute => IsContainerAttribute(attribute, "FromKeyedServicesAttribute"));
        if (fromKeyed is not null)
        {
            string key = fromKeyed.ConstructorArguments.Length == 1 &&
                fromKeyed.AttributeConstructor?.Parameters[0].Type.SpecialType == SpecialType.System_Object
                    ? ConstantFormatter.Format(fromKeyed.ConstructorArguments[0])
                    : "null";
            return new DecoratorParameterModel(DecoratorParameterKind.Keyed, type, key);
        }

        return parameter.HasExplicitDefaultValue
            ? new DecoratorParameterModel(DecoratorParameterKind.Optional, type, ConstantFormatter.FormatDefault(parameter.Type, parameter.ExplicitDefaultValue))
            : new DecoratorParameterModel(DecoratorParameterKind.Required, type, null);
    }

    private static bool CheckConstructible(INamedTypeSymbol type, AttributeData attribute, List<InspectionIssue> issues)
    {
        string? reason = GetNotConstructibleReason(type);
        if (reason is null)
        {
            return true;
        }

        string attributeName = attribute.AttributeClass?.Name ?? string.Empty;
        if (attributeName.EndsWith("Attribute", StringComparison.Ordinal))
        {
            attributeName = attributeName.Substring(0, attributeName.Length - "Attribute".Length);
        }

        issues.Add(new InspectionIssue(DependencyInjectionDiagnostics.NotConstructible, attribute, type.ToDisplayString(), attributeName, reason));
        return false;
    }

    private static string? GetNotConstructibleReason(INamedTypeSymbol type)
    {
        if (type.IsStatic)
        {
            return "static";
        }

        if (type.IsAbstract)
        {
            return "abstract";
        }

        if (type.IsFileLocal)
        {
            return "file-local";
        }

        for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility is Accessibility.Private or Accessibility.Protected or Accessibility.ProtectedAndInternal)
            {
                return "not accessible from generated code";
            }

            if (!SymbolEqualityComparer.Default.Equals(current, type) && current.IsGenericType)
            {
                return "nested in a generic type";
            }
        }

        return null;
    }

    private static InspectionIssue NotImplemented(INamedTypeSymbol type, ITypeSymbol? service, AttributeData attribute) =>
        new(
            DependencyInjectionDiagnostics.ServiceNotImplemented,
            attribute,
            type.ToDisplayString(),
            service?.ToDisplayString() ?? "null",
            type.IsGenericType
                ? "it does not implement the service type with its type parameters mapped one-to-one in order"
                : "it does not implement the service type");

    private static ITypeSymbol? ReadServiceType(AttributeData attribute)
    {
        if (attribute.AttributeClass is { IsGenericType: true } generic)
        {
            return generic.TypeArguments[0];
        }

        return attribute.ConstructorArguments.Length == 1 ? attribute.ConstructorArguments[0].Value as ITypeSymbol : null;
    }

    private static TypedConstant? ReadNamed(AttributeData attribute, string name)
    {
        foreach (KeyValuePair<string, TypedConstant> argument in attribute.NamedArguments)
        {
            if (string.Equals(argument.Key, name, StringComparison.Ordinal))
            {
                return argument.Value;
            }
        }

        return null;
    }

    private static string? ReadKey(AttributeData attribute) =>
        ReadNamed(attribute, "Key") is { IsNull: false } key ? ConstantFormatter.Format(key) : null;

    private static bool Implements(INamedTypeSymbol type, ITypeSymbol service)
    {
        bool unboundService = service is INamedTypeSymbol { IsUnboundGenericType: true };
        if (!type.IsGenericType)
        {
            return !unboundService &&
                (SymbolEqualityComparer.Default.Equals(type, service) ||
                 GetBaseTypes(type).Concat(type.AllInterfaces).Any(candidate => SymbolEqualityComparer.Default.Equals(candidate, service)));
        }

        if (!unboundService)
        {
            return false;
        }

        INamedTypeSymbol definition = ((INamedTypeSymbol)service).OriginalDefinition;
        return SymbolEqualityComparer.Default.Equals(definition, type.OriginalDefinition) ||
            GetBaseTypes(type).Concat(type.AllInterfaces)
                .Select(candidate => MapToService(type, candidate))
                .Any(mapped => SymbolEqualityComparer.Default.Equals(mapped, definition));
    }

    private static IEnumerable<INamedTypeSymbol> GetBaseTypes(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
        {
            yield return current;
        }
    }

    private static INamedTypeSymbol? MapToService(INamedTypeSymbol type, INamedTypeSymbol candidate)
    {
        if (!type.IsGenericType)
        {
            return candidate;
        }

        return candidate.IsGenericType &&
            candidate.TypeArguments.SequenceEqual(type.TypeParameters, static (argument, parameter) => SymbolEqualityComparer.Default.Equals(argument, parameter))
                ? candidate.OriginalDefinition
                : null;
    }

    private static List<ITypeSymbol> ResolveServices(INamedTypeSymbol type, ITypeSymbol? explicitService, int? serviceAs)
    {
        List<ITypeSymbol> services = [];
        if (explicitService is not null)
        {
            services.Add(explicitService is INamedTypeSymbol { IsUnboundGenericType: true } unbound ? unbound.OriginalDefinition : explicitService);
        }

        if (serviceAs is { } selection)
        {
            if (selection != ImplementedInterfaces)
            {
                services.Add(type);
            }

            if (selection != Self)
            {
                services.AddRange(GetServiceInterfaces(type));
            }
        }
        else if (explicitService is null)
        {
            string conventionalName = "I" + type.Name;
            List<INamedTypeSymbol> conventional = [.. GetServiceInterfaces(type)
                .Where(candidate => string.Equals(candidate.Name, conventionalName, StringComparison.Ordinal) && candidate.Arity == type.Arity)];
            if (conventional.Count > 0)
            {
                services.AddRange(conventional);
            }
            else
            {
                services.Add(type);
            }
        }

        return [.. services.Distinct<ITypeSymbol>(SymbolEqualityComparer.Default)];
    }

    private static List<INamedTypeSymbol> GetServiceInterfaces(INamedTypeSymbol type) =>
        [.. type.AllInterfaces
            .Where(static candidate => !ExcludedInterfaces.Contains(TypeNames.MetadataFullName(candidate.OriginalDefinition), StringComparer.Ordinal))
            .Select(candidate => MapToService(type, candidate))
            .OfType<INamedTypeSymbol>()
            .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
            .OrderBy(static candidate => TypeNames.MetadataFullName(candidate), StringComparer.Ordinal)
            .ThenBy(static candidate => candidate.ToDisplayString(), StringComparer.Ordinal)];
}
