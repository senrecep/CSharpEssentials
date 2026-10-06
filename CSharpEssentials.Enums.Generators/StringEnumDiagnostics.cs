using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Enums.Generators;

internal static class StringEnumDiagnostics
{
    public const string Category = "Usage";

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
