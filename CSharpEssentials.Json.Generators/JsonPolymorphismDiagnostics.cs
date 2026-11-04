using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Json.Generators;

internal static class JsonPolymorphismDiagnostics
{
    public const string Category = "Usage";

    public static readonly DiagnosticDescriptor DuplicateDiscriminator = new(
        "CSE4001",
        "Two derived types share a type discriminator",
        "'{0}' and '{1}' both use the type discriminator '{2}' under '{3}'",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Values come from [JsonTypeDiscriminator], [JsonDerivedType] or, for a derived type without a value, its full CLR " +
            "name. The first use of the base throws at run time. Give each derived type its own value.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor ListedOnBase = new(
        "CSE4002",
        "[JsonTypeDiscriminator] is ignored because the base lists the type",
        "[JsonTypeDiscriminator] on '{0}' is ignored under '{1}', which lists it in [JsonDerivedType]; the base value wins",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Remove the [JsonDerivedType] entry from the base or the [JsonTypeDiscriminator] from the type.");

    public static readonly DiagnosticDescriptor NotConcrete = new(
        "CSE4003",
        "[JsonTypeDiscriminator] on a type that cannot be serialized as itself",
        "[JsonTypeDiscriminator] on '{0}' has no effect: it is abstract, an interface or a generic type definition",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Only concrete, non-generic classes and structs can be derived types.");

    public static readonly DiagnosticDescriptor NameConflict = new(
        "CSE4004",
        "The discriminator property name equals a member name",
        "The discriminator property '{0}' of '{1}' equals the JSON name of member '{2}' on '{3}'",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Names are compared ignoring case because the naming policy is not known at build time. System.Text.Json 10 " +
            "throws for such a conflict, earlier versions write the key twice. Rename the member or the discriminator property.");

    public static readonly DiagnosticDescriptor MissingDiscriminator = new(
        "CSE4005",
        "Derived type of an open polymorphic base has no [JsonTypeDiscriminator]",
        "'{0}' derives from '{1}' but has no [JsonTypeDiscriminator]; AddPolymorphismWithReflection writes its CLR name '{2}' and AddPolymorphism leaves it out",
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "A CLR name changes when the type is renamed or moved, which breaks stored payloads. Add [JsonTypeDiscriminator] " +
            "with a stable value such as \"order.created.v1\". Reported as a warning when EnableAotAnalyzer is true (PublishAot or " +
            "IsAotCompatible), because AddPolymorphism is then the only path.");

    public static readonly DiagnosticDescriptor MissingFromContext = new(
        "CSE4006",
        "JsonSerializerContext lists a polymorphic base but not one of its derived types",
        "'{0}' lists '{1}' but not its derived type '{2}'; AddPolymorphism leaves the type out",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Add [JsonSerializable(typeof(...))] for each derived type registered with [JsonTypeDiscriminator].",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor OwnConverter = new(
        "CSE4007",
        "Derived type has its own [JsonConverter]",
        "'{0}' has its own [JsonConverter], so it is not added as a derived type of '{1}'",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "System.Text.Json cannot write the discriminator through a custom converter. Remove the converter or list the type " +
            "on the base and handle the discriminator in the converter.");

    public static readonly DiagnosticDescriptor NoPolymorphicBase = new(
        "CSE4008",
        "[JsonTypeDiscriminator] on a type without a polymorphic base",
        "[JsonTypeDiscriminator] on '{0}' has no effect: none of its bases has [JsonPolymorphic] or [JsonDerivedType]",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Add [JsonPolymorphic] to the base. A base without it keeps PolymorphicJsonConverterFactory, which writes CLR names.");

    public static readonly DiagnosticDescriptor PropertyNotPolymorphic = new(
        "CSE4009",
        "[JsonPolymorphicProperty] on a member that holds no polymorphic base",
        "[JsonPolymorphicProperty] on '{0}' has no effect: '{1}' is not, and does not contain, a type with [JsonPolymorphic] or [JsonDerivedType]",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Arrays, collections, dictionary values and Nullable<T> are unwrapped before the check.");

    public static readonly DiagnosticDescriptor Unreachable = new(
        "CSE4010",
        "Type is not reachable from generated code",
        "'{0}' is private, protected, file-local or generic, so generated code cannot register it; only AddPolymorphismWithReflection finds it",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Make the type internal or public and non-generic to use AddPolymorphism.");

    public static readonly DiagnosticDescriptor NotListed = new(
        "CSE4011",
        "Derived type of a closed polymorphic base is neither listed nor valued",
        "'{0}' derives from '{1}', which lists its derived types, but it is not listed and has no [JsonTypeDiscriminator]; writing it throws",
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "Add [JsonTypeDiscriminator] to the type or a [JsonDerivedType] entry to the base.");

    public static readonly DiagnosticDescriptor PositionalParameter = new(
        "CSE4012",
        "[JsonPolymorphicProperty] on a positional record parameter",
        "[property: JsonPolymorphicProperty] on parameter '{0}' is not seen by generated code; declare the property explicitly or use AddPolymorphismWithReflection",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The generator reads [JsonPolymorphicProperty] from property and field declarations only. Under AddPolymorphism " +
            "the property keeps the base's discriminator name.");
}
