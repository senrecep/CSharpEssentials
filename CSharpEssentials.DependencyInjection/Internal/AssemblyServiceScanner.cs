using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.DependencyInjection;

[RequiresUnreferencedCode(UnreferencedCodeMessage)]
[RequiresDynamicCode(DynamicCodeMessage)]
internal static class AssemblyServiceScanner
{
    public const string UnreferencedCodeMessage =
        "Scans assemblies for registration attributes; use the generated Add{Asm}Services for trimmed/AOT apps.";

    public const string DynamicCodeMessage = "Builds decorator factories and closes generic types at runtime.";

    private const string AttributeNamespace = "CSharpEssentials.DependencyInjection";

    private static readonly string[] ExcludedInterfaces =
    [
        "System.IDisposable",
        "System.IAsyncDisposable",
        "System.IEquatable`1",
        "System.IComparable`1",
        "System.Collections.IEnumerable",
    ];

    public static void Scan(IServiceCollection services, ILogger? logger, IEnumerable<Assembly> assemblies)
    {
        List<(ServiceDescriptor Descriptor, RegistrationStrategy Strategy)> registrations = [];
        List<Action<IServiceCollection>> decorators = [];

        foreach (Assembly assembly in assemblies.Distinct())
        {
            if (assembly.IsDefined(typeof(ExcludeFromRegistrationAttribute), inherit: false))
            {
                continue;
            }

            List<(int Order, string Name, Action<IServiceCollection> Apply)> assemblyDecorators = [];
            IEnumerable<Type> candidates = GetLoadableTypes(assembly, logger)
                .Where(static type => type.IsClass && !type.IsDefined(typeof(ExcludeFromRegistrationAttribute), inherit: false))
                .OrderBy(static type => type.FullName, StringComparer.Ordinal);

            foreach (Type type in candidates)
            {
                foreach (CustomAttributeData attribute in type.GetCustomAttributesData())
                {
                    if (TryGetLifetime(attribute.AttributeType, out ServiceLifetime lifetime))
                    {
                        registrations.AddRange(CreateRegistrations(type, attribute, lifetime));
                    }
                    else if (IsAttribute(attribute.AttributeType, "DecoratesAttribute"))
                    {
                        (int order, Action<IServiceCollection> apply) = CreateDecorator(type, attribute);
                        assemblyDecorators.Add((order, type.FullName ?? type.Name, apply));
                    }
                }
            }

            decorators.AddRange(assemblyDecorators
                .OrderBy(static decorator => decorator.Order)
                .ThenBy(static decorator => decorator.Name, StringComparer.Ordinal)
                .Select(static decorator => decorator.Apply));
        }

        foreach ((ServiceDescriptor descriptor, RegistrationStrategy strategy) in registrations)
        {
            ServiceRegistration.Apply(services, logger, descriptor, strategy);
        }

        foreach (Action<IServiceCollection> decorator in decorators)
        {
            decorator(services);
        }
    }

    private static Type[] GetLoadableTypes(Assembly assembly, ILogger? logger)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            string loaderErrors = string.Join(
                "; ",
                exception.LoaderExceptions.OfType<Exception>().Select(static error => error.Message).Distinct(StringComparer.Ordinal));
            string assemblyName = assembly.FullName ?? assembly.ToString();
            if (logger is null)
            {
                throw new InvalidOperationException(
                    $"Types of assembly '{assemblyName}' could not be loaded: {loaderErrors}. " +
                    "Pass a logger to AddServicesFromAssemblies to skip them with a warning.",
                    exception);
            }

            if (logger.IsEnabled(LogLevel.Warning))
            {
                ServiceRegistrationLog.TypesNotLoaded(logger, assemblyName, loaderErrors);
            }

            return [.. exception.Types.OfType<Type>()];
        }
    }

    private static bool IsAttribute(Type attributeType, string name) =>
        string.Equals(attributeType.Namespace, AttributeNamespace, StringComparison.Ordinal) &&
        (string.Equals(attributeType.Name, name, StringComparison.Ordinal) ||
         string.Equals(attributeType.Name, name + "`1", StringComparison.Ordinal));

    private static bool TryGetLifetime(Type attributeType, out ServiceLifetime lifetime)
    {
        lifetime = ServiceLifetime.Scoped;
        if (IsAttribute(attributeType, "RegisterScopedAttribute"))
        {
            return true;
        }

        lifetime = ServiceLifetime.Singleton;
        if (IsAttribute(attributeType, "RegisterSingletonAttribute"))
        {
            return true;
        }

        lifetime = ServiceLifetime.Transient;
        return IsAttribute(attributeType, "RegisterTransientAttribute");
    }

    private static List<(ServiceDescriptor Descriptor, RegistrationStrategy Strategy)> CreateRegistrations(
        Type implementation,
        CustomAttributeData attribute,
        ServiceLifetime lifetime)
    {
        EnsureConstructible(implementation, attribute);

        Type? explicitService = ReadServiceType(attribute);
        object? key = null;
        ServiceAs? serviceAs = null;
        RegistrationStrategy strategy = RegistrationStrategy.Add;
        foreach (CustomAttributeNamedArgument argument in attribute.NamedArguments)
        {
            object? value = ReadValue(argument.TypedValue);
            switch (argument.MemberName)
            {
                case nameof(RegisterScopedAttribute.Key):
                    key = value;
                    break;
                case nameof(RegisterScopedAttribute.As):
                    serviceAs = (ServiceAs)value!;
                    break;
                case nameof(RegisterScopedAttribute.Strategy):
                    strategy = (RegistrationStrategy)value!;
                    break;
                default:
                    break;
            }
        }

        List<Type> serviceTypes = ResolveServiceTypes(implementation, explicitService, serviceAs);
        foreach (Type serviceType in serviceTypes)
        {
            EnsureImplements(implementation, serviceType);
        }

        bool forward = serviceAs == ServiceAs.SelfWithInterfaces && !implementation.IsGenericTypeDefinition;
        return [.. serviceTypes.Select(serviceType => (
            forward && serviceType != implementation
                ? CreateForwarding(serviceType, implementation, key, lifetime)
                : new ServiceDescriptor(serviceType, key, implementation, lifetime),
            strategy))];
    }

    private static (int Order, Action<IServiceCollection> Apply) CreateDecorator(Type decoratorType, CustomAttributeData attribute)
    {
        EnsureConstructible(decoratorType, attribute);

        Type serviceType = ReadServiceType(attribute)!;
        object? key = null;
        int order = 0;
        foreach (CustomAttributeNamedArgument argument in attribute.NamedArguments)
        {
            object? value = ReadValue(argument.TypedValue);
            switch (argument.MemberName)
            {
                case nameof(DecoratesAttribute.Key):
                    key = value;
                    break;
                case nameof(DecoratesAttribute.Order):
                    order = (int)value!;
                    break;
                default:
                    break;
            }
        }

        EnsureImplements(decoratorType, serviceType);
        if (decoratorType.IsGenericTypeDefinition)
        {
            return (order, services => ServiceDecoration.DecorateOpenGeneric(services, serviceType, decoratorType, candidate => Equals(candidate, key)));
        }

        Func<object, IServiceProvider, object> factory = DecoratorActivator.Create(decoratorType, serviceType, key);
        void Apply(IServiceCollection services)
        {
            if (!ServiceDecoration.TryDecorate(services, serviceType, key, factory))
            {
                throw ServiceDecoration.NothingToDecorate(serviceType, key, decoratorType);
            }
        }

        return (order, Apply);
    }

    private static Type? ReadServiceType(CustomAttributeData attribute)
    {
        if (attribute.AttributeType.IsGenericType)
        {
            return attribute.AttributeType.GetGenericArguments()[0];
        }

        return attribute.ConstructorArguments.Count == 1 ? attribute.ConstructorArguments[0].Value as Type : null;
    }

    private static object? ReadValue(CustomAttributeTypedArgument argument) =>
        argument.Value is not null && argument.ArgumentType.IsEnum
            ? Enum.ToObject(argument.ArgumentType, argument.Value)
            : argument.Value;

    private static List<Type> ResolveServiceTypes(Type implementation, Type? explicitService, ServiceAs? serviceAs)
    {
        List<Type> serviceTypes = [];
        if (explicitService is not null)
        {
            serviceTypes.Add(explicitService);
        }

        if (serviceAs is { } selection)
        {
            if (selection != ServiceAs.ImplementedInterfaces)
            {
                serviceTypes.Add(implementation);
            }

            if (selection != ServiceAs.Self)
            {
                serviceTypes.AddRange(GetServiceInterfaces(implementation));
            }
        }
        else if (explicitService is null)
        {
            string conventionalName = "I" + StripArity(implementation.Name);
            List<Type> conventional = [.. GetServiceInterfaces(implementation)
                .Where(@interface =>
                    string.Equals(StripArity(@interface.Name), conventionalName, StringComparison.Ordinal) &&
                    @interface.GetGenericArguments().Length == implementation.GetGenericArguments().Length)];
            if (conventional.Count > 0)
            {
                serviceTypes.AddRange(conventional);
            }
            else
            {
                serviceTypes.Add(implementation);
            }
        }

        return [.. serviceTypes.Distinct()];
    }

    private static IEnumerable<Type> GetServiceInterfaces(Type implementation) =>
        implementation.GetInterfaces()
            .Where(static @interface => !ExcludedInterfaces.Contains(GetDefinition(@interface).FullName ?? @interface.Name, StringComparer.Ordinal))
            .Select(@interface => MapToService(implementation, @interface))
            .OfType<Type>()
            .Distinct()
            .OrderBy(static @interface => @interface.FullName ?? @interface.Name, StringComparer.Ordinal);

    private static Type? MapToService(Type implementation, Type @interface)
    {
        if (!implementation.IsGenericTypeDefinition)
        {
            return @interface;
        }

        return @interface.IsGenericType && @interface.GetGenericArguments().SequenceEqual(implementation.GetGenericArguments())
            ? @interface.GetGenericTypeDefinition()
            : null;
    }

    private static Type GetDefinition(Type type) => type.IsGenericType ? type.GetGenericTypeDefinition() : type;

    private static string StripArity(string name)
    {
        int index = name.IndexOf('`');
        return index < 0 ? name : name[..index];
    }

    private static void EnsureConstructible(Type type, CustomAttributeData attribute)
    {
        if (type.IsAbstract)
        {
            throw new InvalidOperationException(
                $"'{type.FullName}' is abstract or static and cannot be used by [{attribute.AttributeType.Name}].");
        }
    }

    private static void EnsureImplements(Type implementation, Type serviceType)
    {
        bool valid = implementation.IsGenericTypeDefinition
            ? serviceType == implementation || GetServiceTypes(implementation).Contains(serviceType)
            : !serviceType.ContainsGenericParameters && serviceType.IsAssignableFrom(implementation);
        if (!valid)
        {
            throw new InvalidOperationException(
                $"'{implementation.FullName}' cannot be registered as '{serviceType.FullName}': it does not implement the service type" +
                (implementation.IsGenericTypeDefinition
                    ? " with its type parameters mapped one-to-one in order."
                    : "."));
        }
    }

    private static IEnumerable<Type> GetServiceTypes(Type implementation)
    {
        for (Type? baseType = implementation.BaseType; baseType is not null; baseType = baseType.BaseType)
        {
            if (MapToService(implementation, baseType) is { } mapped)
            {
                yield return mapped;
            }
        }

        foreach (Type @interface in implementation.GetInterfaces())
        {
            if (MapToService(implementation, @interface) is { } mapped)
            {
                yield return mapped;
            }
        }
    }

    private static ServiceDescriptor CreateForwarding(Type serviceType, Type implementation, object? key, ServiceLifetime lifetime) =>
        key is null
            ? new ServiceDescriptor(serviceType, provider => provider.GetRequiredService(implementation), lifetime)
            : new ServiceDescriptor(serviceType, key, (provider, _) => provider.GetRequiredKeyedService(implementation, key), lifetime);
}
