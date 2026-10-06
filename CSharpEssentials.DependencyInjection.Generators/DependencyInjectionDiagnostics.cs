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
        "'{0}' registers '{1}'{2} with RegistrationStrategy.Throw, but '{3}' registers the same service type and key in this compilation",
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
}
