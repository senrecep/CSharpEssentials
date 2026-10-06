using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Enums.Generators;

internal static class StringEnumDiagnostics
{
    public const string Category = "Usage";

    public static readonly DiagnosticDescriptor WireNameCollision = new(
        "CSE0002",
        "Two members produce the same wire name",
        "'{0}' produces the wire name '{1}', which '{2}' produces as well",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Wire names are compared ignoring case because data reads match them ignoring case. Rename a member or give it " +
            "[JsonStringEnumMemberName]. The generator skips the enum until the collision is fixed.");

    public static readonly DiagnosticDescriptor AliasCollision = new(
        "CSE0003",
        "An alias collides with another member",
        "The alias '{0}' of '{1}' equals {2}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An alias must not equal the wire name, member name or alias of another member (ignoring case), otherwise a read " +
            "returns the wrong member. The generator skips the enum until the collision is fixed.");

    public static readonly DiagnosticDescriptor MultipleFallbacks = new(
        "CSE0004",
        "More than one [EnumFallback] member",
        "'{0}' is marked [EnumFallback] as well as '{1}'; keep one fallback member",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Unknown values in data reads map to the single [EnumFallback] member. The generator skips the enum until one fallback remains.");

    public static readonly DiagnosticDescriptor ImplicitIntegerValues = new(
        "CSE0005",
        "Enum stored as an integer has members without explicit values",
        "'{0}' is stored as an integer and {1} no explicit value; reordering or inserting members changes stored data",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The effective storage is Integer when [StringEnum(Storage = EnumStorage.Integer)] is set, or for a [Flags] enum " +
            "with the default storage (EnumConventions.FlagsStorage is Integer). Give every member an explicit value.");

    public static readonly DiagnosticDescriptor FlagsWithoutZero = new(
        "CSE0006",
        "[Flags] enum without a zero member",
        "[Flags] enum '{0}' has no member with the value 0",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The empty set is written as '0' without a zero member. Add 'None = 0'.");

    public static readonly DiagnosticDescriptor InvalidFlagValue = new(
        "CSE0007",
        "[Flags] member is neither a single bit nor a combination of other members",
        "[Flags] member '{0}' is neither a single bit nor a combination of other members",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A flags value is written as its single flags. A member whose bits no other members cover cannot be written or read back by name.");

    public static readonly DiagnosticDescriptor FallbackOnFlags = new(
        "CSE0008",
        "[EnumFallback] on a [Flags] enum",
        "'{0}' is marked [EnumFallback] but '{1}' is a [Flags] enum; unknown flag bits are rejected, so remove the attribute",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Flags reads reject unknown bits instead of mapping them to a member. The generator skips the enum until the attribute is removed.");

    public static readonly DiagnosticDescriptor InvalidWireName = new(
        "CSE0009",
        "Wire name or alias cannot be read back",
        "The {0} '{1}' of '{2}' is invalid because {3}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Wire names and aliases must not be empty, contain whitespace or a comma (which separates flags), or start with a digit, " +
            "a sign or a dot (such text is read as a number). The generator skips the enum until the name is fixed.");

    public static readonly DiagnosticDescriptor EnumWithoutStringEnum = new(
        "CSE0010",
        "Enum in a public API has no [StringEnum]",
        "'{0}' is used by the public member '{1}' but has no [StringEnum]; it takes the reflection path",
        Category,
        DiagnosticSeverity.Info,
        isEnabledByDefault: false,
        description: "Enums without [StringEnum] have no generated metadata, so JSON, binding and EF Core use framework defaults or the " +
            "reflection opt-in. Enable this rule in .editorconfig to find them.");

    public static readonly DiagnosticDescriptor InvalidNamingProperty = new(
        "CSE0012",
        "Invalid CSharpEssentialsEnumNaming value",
        "The CSharpEssentialsEnumNaming value '{0}' is not valid; use one of {1}. SnakeCaseLower is used instead.",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public static readonly DiagnosticDescriptor AttributeWithoutStringEnum = new(
        "CSE0013",
        "Enum attribute is ignored without [StringEnum]",
        "[{0}] on '{1}' is ignored because '{2}' has no [StringEnum]; only the opt-in reflection fallback reads it",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "[EnumAlias] and [EnumFallback] are read from generated metadata, and from the reflection fallback only when it is opted in. " +
            "Add [StringEnum] to the enum or remove the attribute.");

    public static readonly DiagnosticDescriptor MissingMetadata = new(
        "CSE0015",
        "[StringEnum] enum gets no generated metadata",
        "'{0}' gets no generated metadata because {1}; JSON converter creation for it throws instead of writing numbers",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The generator registers metadata for [StringEnum] enums that a namespace-level class can reach, in C# 9 or newer. " +
            "Make the enum and its containing types public or internal, do not nest it in a generic type, and use C# 9 or newer.");

    public static readonly DiagnosticDescriptor ExtensionsClassCollision = new(
        "CSE0016",
        "[StringEnum] enums generate the same extensions class",
        "'{0}' generates the extensions class '{1}', which '{2}' generates as well; rename one of the enums or its containing type",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The generated class name joins the containing type names with '_', so Order.State and a namespace-level " +
            "Order_State both generate Order_StateExtensions in the same namespace.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);
}
