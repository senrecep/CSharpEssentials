using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Endpoints.Generators;

internal static class EndpointDiagnostics
{
    public const string Category = "CSharpEssentials.Endpoints";

    private const string HelpLink = "https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Endpoints/Readme.MD#diagnostics";

    public static readonly DiagnosticDescriptor InaccessibleType = new(
        "CSE1001",
        "Endpoint type is not accessible from generated code",
        "Type '{0}' is not accessible from generated code and is not mapped; make it and its containing types public or internal and not file-local",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Generated registries reference endpoint and group types directly, so private, protected, file-local types and types nested in such types cannot be mapped.",
        helpLinkUri: HelpLink);

    public static readonly DiagnosticDescriptor GroupCycle = new(
        "CSE1002",
        "Endpoint group nesting cycle",
        "Group chain of '{0}' returns to '{0}'; endpoints in this chain are not mapped",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Following [EndpointGroup] attributes must end at a root group. A chain that returns to a type already in it cannot be mapped.",
        helpLinkUri: HelpLink);

    public static readonly DiagnosticDescriptor MultipleGroups = new(
        "CSE1003",
        "More than one endpoint group attribute",
        "Type '{0}' has more than one [EndpointGroup] attribute and is not mapped; keep exactly one",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An endpoint or group has exactly one parent group. Using both the typeof form and the generic form of [EndpointGroup] makes the parent ambiguous.",
        helpLinkUri: HelpLink);

    public static readonly DiagnosticDescriptor InstanceState = new(
        "CSE1004",
        "Endpoint declares instance state",
        "Endpoint '{0}' declares instance fields, auto-properties or a constructor with parameters, but endpoint types are never instantiated; take dependencies as handler parameters",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "IEndpoint.Map is static and endpoint types are never instantiated, so instance state and constructor dependencies are dead code.",
        helpLinkUri: HelpLink);

    public static readonly DiagnosticDescriptor DuplicateRoute = new(
        "CSE1005",
        "Duplicate HTTP method and route in the same group",
        "Endpoint '{0}' maps {1} '{2}', which '{3}' also maps in the same group",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Two endpoints with the same HTTP method and route pattern in one group make requests ambiguous at runtime. Only constant patterns passed directly to the Map parameter are compared.",
        helpLinkUri: HelpLink,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor SkippedType = new(
        "CSE1006",
        "Abstract or open-generic endpoint type is skipped",
        "Type '{0}' is abstract or open-generic and is not mapped",
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Only concrete, closed endpoint and group types are mapped. Abstract and open-generic types are skipped on purpose.",
        helpLinkUri: HelpLink);

    public static readonly DiagnosticDescriptor InvalidGroupTarget = new(
        "CSE1007",
        "Invalid endpoint group target",
        "Group target '{1}' of '{0}' must be a concrete, closed, non-ref struct type that implements IEndpointGroup; '{0}' is not mapped",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The typeof form of [EndpointGroup] cannot constrain its argument, so the target is checked here instead of failing in generated code.",
        helpLinkUri: HelpLink);

    public static readonly DiagnosticDescriptor RefStructType = new(
        "CSE1008",
        "Ref struct endpoint type cannot be mapped",
        "Type '{0}' is a ref struct and is not mapped; declare it as a class or a non-ref struct",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Generated code passes endpoint and group types as generic type arguments to EndpointMapper, which does not allow ref struct type arguments, so ref struct endpoints and groups are skipped.",
        helpLinkUri: HelpLink);

    public static readonly DiagnosticDescriptor RegistryNameCollision = new(
        "CSE1009",
        "Referenced endpoint registries share a name",
        "Assemblies {0} all generate the endpoint registry '{1}', so MapAllEndpoints leaves out the referenced ones; give each assembly a distinct name with [assembly: EndpointRegistryName(\"...\")]",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Registry names are derived from assembly names with separators removed, so 'Foo.Api' and 'FooApi' both produce 'FooApiEndpointRegistry'. The generated aggregate cannot refer to an ambiguous type and leaves the referenced registries out until each has a distinct name. When the project's own registry has the same name, the aggregate maps only the project's own registry.",
        helpLinkUri: HelpLink,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor DuplicateEndpointName = new(
        "CSE1010",
        "Explicit endpoint name is set more than once",
        "Endpoint name '{0}' is also set at another call site; endpoint names must be unique within an application",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Two call sites that set the same constant name with WithName(...) or with EndpointNameAttribute, EndpointNameMetadata or RouteNameMetadata in WithMetadata(...) produce duplicate operationIds and make link generation by name ambiguous. Endpoint names and route names are compared separately.",
        helpLinkUri: HelpLink,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor NonConstantEndpointName = new(
        "CSE1011",
        "Explicit endpoint name is not a constant",
        "This endpoint name is not a compile-time constant, so it cannot be reserved at build time and OperationNaming.TypeName can generate the same name",
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Generated registries reserve constant names set with WithName(...) or with EndpointNameAttribute, EndpointNameMetadata or RouteNameMetadata in WithMetadata(...). A name that is computed at runtime outside an IEndpoint or IEndpointGroup type is not known to OperationNaming.TypeName. Use a constant, or map the endpoint through an IEndpoint type.",
        helpLinkUri: HelpLink);
}
