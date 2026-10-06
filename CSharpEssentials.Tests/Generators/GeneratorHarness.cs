using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Tests.Generators;

public static class GeneratorHarness
{
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest);

    private static readonly Lazy<ImmutableArray<MetadataReference>> FrameworkReferences = new(LoadFrameworkReferences);

    public static CSharpCompilation CreateCompilation(IEnumerable<string> sources, IEnumerable<Assembly>? additionalReferences = null)
    {
        SyntaxTree[] syntaxTrees = [.. sources.Select(static (source, index) =>
            CSharpSyntaxTree.ParseText(source, ParseOptions, path: $"Source{index}.cs"))];

        IEnumerable<MetadataReference> references = FrameworkReferences.Value.Concat(
            (additionalReferences ?? []).Distinct().Select(static assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location)));

        return CSharpCompilation.Create(
            "GeneratorTests",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }

    public static Compilation AddSource(Compilation compilation, string source) =>
        compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, ParseOptions, path: $"Source{compilation.SyntaxTrees.Count()}.cs"));

    public static GeneratorDriver CreateDriver(params ISourceGenerator[] generators) =>
        CreateDriver(null, generators);

    public static GeneratorDriver CreateDriver(AnalyzerConfigOptionsProvider? optionsProvider, params ISourceGenerator[] generators) =>
        CSharpGeneratorDriver.Create(
            generators,
            parseOptions: ParseOptions,
            optionsProvider: optionsProvider,
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

    public static GeneratorRun Run(Compilation compilation, params ISourceGenerator[] generators) =>
        Run(compilation, null, generators);

    public static GeneratorRun Run(Compilation compilation, AnalyzerConfigOptionsProvider? optionsProvider, params ISourceGenerator[] generators)
    {
        GeneratorDriver driver = CreateDriver(optionsProvider, generators)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out ImmutableArray<Diagnostic> generatorDiagnostics);

        return new GeneratorRun(driver, driver.GetRunResult(), outputCompilation, generatorDiagnostics);
    }

    private static ImmutableArray<MetadataReference> LoadFrameworkReferences()
    {
        string runtimeDirectory = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        string trustedAssemblies = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;

        return [.. trustedAssemblies
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Where(path => string.Equals(Path.GetDirectoryName(path), runtimeDirectory, StringComparison.Ordinal))
            .Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))];
    }
}
