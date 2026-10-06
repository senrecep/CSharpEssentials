using System.Collections.Immutable;
using System.Reflection;
using CSharpEssentials.DependencyInjection;
using CSharpEssentials.Tests.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.DependencyInjection.Generation;

internal static class ServiceGeneration
{
    public const string IsTestProjectProperty = "build_property.IsTestProject";

    private static readonly Lazy<AnalyzerAssembly> GeneratorAssembly =
        new(static () => AnalyzerAssembly.Load("CSharpEssentials.DependencyInjection.Generators.dll"));

    private static readonly Assembly[] RuntimeAssemblies =
    [
        typeof(RegisterScopedAttribute).Assembly,
        typeof(IServiceCollection).Assembly,
        typeof(ILogger).Assembly,
    ];

    public static ImmutableArray<ISourceGenerator> Generators => GeneratorAssembly.Value.Generators;

    public static ImmutableArray<DiagnosticAnalyzer> Analyzers => GeneratorAssembly.Value.Analyzers;

    public static CSharpCompilation CreateCompilation(
        string source,
        OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary,
        string assemblyName = "GeneratorTests",
        params MetadataReference[] references)
    {
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([source], RuntimeAssemblies);
        return compilation
            .WithAssemblyName(assemblyName)
            .WithOptions(compilation.Options.WithOutputKind(outputKind))
            .AddReferences(references);
    }

    public static CSharpCompilation WithLanguageVersion(CSharpCompilation compilation, LanguageVersion languageVersion)
    {
        CSharpParseOptions parseOptions = new(languageVersion);
        return compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(
            compilation.SyntaxTrees.Select(tree => CSharpSyntaxTree.ParseText(tree.GetText(), parseOptions, tree.FilePath)));
    }

    public static GeneratorDriver CreateDriver(bool isTestProject = false, LanguageVersion languageVersion = LanguageVersion.Latest)
    {
        Dictionary<string, string> options = isTestProject ? new() { [IsTestProjectProperty] = "true" } : [];
        return CSharpGeneratorDriver.Create(
            Generators,
            parseOptions: new CSharpParseOptions(languageVersion),
            optionsProvider: new ServiceGeneratorOptionsProvider(options),
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));
    }

    public static GeneratorRun Run(Compilation compilation, bool isTestProject = false, LanguageVersion languageVersion = LanguageVersion.Latest)
    {
        GeneratorDriver driver = CreateDriver(isTestProject, languageVersion)
            .RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out ImmutableArray<Diagnostic> diagnostics);

        return new GeneratorRun(driver, driver.GetRunResult(), outputCompilation, diagnostics);
    }

    public static IReadOnlyDictionary<string, string> GeneratedSources(GeneratorRun run) =>
        run.Result.Results
            .SelectMany(static result => result.GeneratedSources)
            .ToDictionary(static source => source.HintName, static source => source.SourceText.ToString(), StringComparer.Ordinal);

    public static MetadataReference CreateLibraryReference(string assemblyName)
    {
        string source = $$"""
            using CSharpEssentials.DependencyInjection;

            namespace {{assemblyName.Replace(".", "_", StringComparison.Ordinal)}}Library;

            public interface IWorker;

            [RegisterScoped]
            public sealed class Worker : IWorker;
            """;
        return EmitReference(Run(CreateCompilation(source, assemblyName: assemblyName)).OutputCompilation);
    }

    public static MetadataReference EmitReference(Compilation compilation)
    {
        using MemoryStream stream = new();
        Microsoft.CodeAnalysis.Emit.EmitResult result = compilation.Emit(stream);
        if (!result.Success)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics));
        }

        return MetadataReference.CreateFromImage(stream.ToArray());
    }
}
