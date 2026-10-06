using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpEssentials.Tests.Generators;

public sealed class TrackedNamesGenerator : IIncrementalGenerator
{
    public const string NamesStep = "Names";

    public const string SymbolsStep = "Symbols";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<INamedTypeSymbol> symbols = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Sample.TrackedAttribute",
                static (node, _) => node is ClassDeclarationSyntax,
                static (ctx, _) => (INamedTypeSymbol)ctx.TargetSymbol)
            .WithTrackingName(SymbolsStep);

        IncrementalValuesProvider<string> names = symbols
            .Select(static (symbol, _) => symbol.Name)
            .WithTrackingName(NamesStep);

        context.RegisterSourceOutput(names, static (spc, name) =>
            spc.AddSource($"{name}.g.cs", $"namespace Sample; public static class {name}Info {{ public const string Name = \"{name}\"; }}"));
    }
}
