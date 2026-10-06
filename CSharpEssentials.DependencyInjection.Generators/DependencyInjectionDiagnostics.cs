using Microsoft.CodeAnalysis;

namespace CSharpEssentials.DependencyInjection.Generators;

internal static class DependencyInjectionDiagnostics
{
    public const string Category = "CSharpEssentials.DependencyInjection";

    public static readonly DiagnosticDescriptor ServiceNotImplemented = new(
        "CSE2001",
        "Service type is not implemented",
        "'{0}' cannot be registered or applied as '{1}': {2}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The class must implement the service type. An open-generic class must map its type parameters one-to-one, in order, onto the service type.");

    public static readonly DiagnosticDescriptor DuplicateThrowRegistration = new(
        "CSE2002",
        "Duplicate registration under RegistrationStrategy.Throw",
        "'{0}' registers '{1}'{2} with RegistrationStrategy.Throw, but '{3}' registers the same service type and key earlier in this compilation",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "RegistrationStrategy.Throw fails at startup when the service type and key are already registered.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor RegisteredAsSelf = new(
        "CSE2003",
        "Class is registered as itself",
        "'{0}' implements interfaces, but none is named '{1}', so it is registered as itself; pass the service type or set As to register an interface",
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Without an explicit service type or As, a class registers as its I{TypeName} interface and otherwise as itself.");

    public static readonly DiagnosticDescriptor DecoratorWithoutInner = new(
        "CSE2004",
        "Decorator has no usable inner parameter",
        "The constructor of decorator '{0}' must have exactly one parameter of the decorated service type '{1}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The generated factory passes the decorated instance to the single constructor parameter of the service type.");

    public static readonly DiagnosticDescriptor DecoratorConstructorCount = new(
        "CSE2005",
        "Decorator must have exactly one public constructor",
        "Decorator '{0}' must have exactly one public constructor, but has {1}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The generated factory calls the single public constructor of the decorator.");

    public static readonly DiagnosticDescriptor CaptiveDependency = new(
        "CSE2006",
        "Registered service captures a shorter-lived dependency",
        "{0} '{1}' depends on '{2}' through parameter '{3}', but '{4}' registers it as {5}",
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "A service keeps the instances it receives for its whole lifetime. A singleton that receives a scoped or transient service, or a scoped service that receives a transient one, holds that dependency longer than its registration intends. Only classes with a single constructor and attribute registrations in the same compilation are checked.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor NotConstructible = new(
        "CSE2007",
        "Type cannot be constructed by the generated registry",
        "'{0}' cannot be used by [{1}]: it is {2}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Registered classes and decorators must be concrete, non-static and accessible from generated code in the same assembly.");

    public static readonly DiagnosticDescriptor OpenGenericDecorator = new(
        "CSE2008",
        "Open-generic decorator is not generated",
        "Open-generic decorator '{0}' is not applied by the generated registry; call services.Decorate(typeof({1}), typeof({2})) at runtime",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The generated registry decorates closed registrations through direct factories only. Apply open-generic decorators with the runtime Decorate(Type, Type) method.");

    public static readonly DiagnosticDescriptor RegistryNameCollision = new(
        "CSE2009",
        "Referenced service registries share a name",
        "Assemblies {0} all generate the service registry '{1}', so AddAllServices skips them; give each assembly a distinct name with [assembly: ServiceRegistryName(\"...\")]",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Registry names are derived from assembly names with separators removed, so 'Foo.Api' and 'FooApi' both produce 'FooApiServiceRegistry'. The generated aggregate cannot refer to an ambiguous type and leaves those registries out until each has a distinct name.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);
}
