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
}
