using CSharpEssentials.Endpoints;
using CSharpEssentials.Tests.Generators;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Tests.Endpoints;

internal static class EndpointCompilations
{
    public const string IsTestProjectProperty = "build_property.IsTestProject";

    private const string EntryPoint = "internal static class Program { private static void Main() { } }";

    private static readonly Lazy<MetadataReference[]> AspNetCoreReferences = new(static () =>
    [
        .. Directory.GetFiles(Path.GetDirectoryName(typeof(RouteGroupBuilder).Assembly.Location)!, "*.dll")
            .Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path)),
    ]);

    public static Lazy<AnalyzerAssembly> Generators { get; } = new(static () => AnalyzerAssembly.Load("CSharpEssentials.Endpoints.Generators.dll"));

    public static CSharpCompilation Create(string assemblyName, OutputKind outputKind, params string[] sources) =>
        Create(assemblyName, outputKind, [], sources);

    public static CSharpCompilation Create(string assemblyName, OutputKind outputKind, MetadataReference[] references, params string[] sources) =>
        GeneratorHarness.CreateCompilation(
                outputKind == OutputKind.DynamicallyLinkedLibrary ? sources : [.. sources, EntryPoint],
                [typeof(IEndpoint).Assembly])
            .AddReferences(AspNetCoreReferences.Value)
            .AddReferences(references)
            .WithAssemblyName(assemblyName)
            .WithOptions(new CSharpCompilationOptions(outputKind, nullableContextOptions: NullableContextOptions.Enable));

    public static GeneratorRun Run(Compilation compilation, bool isTestProject = false) =>
        GeneratorHarness.Run(
            compilation,
            new TestAnalyzerConfigOptionsProvider(new Dictionary<string, string>
            {
                [IsTestProjectProperty] = isTestProject ? "true" : "false",
            }),
            [.. Generators.Value.Generators]);

    public static string[] HintNames(GeneratorRun run) =>
        [.. run.Result.Results.SelectMany(static result => result.GeneratedSources).Select(static source => source.HintName)];

    public static string GeneratedText(GeneratorRun run, string hintName) =>
        run.Result.Results.SelectMany(static result => result.GeneratedSources).Single(source => source.HintName == hintName).SourceText.ToString();
}
