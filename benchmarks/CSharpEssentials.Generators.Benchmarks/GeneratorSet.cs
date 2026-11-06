using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Generators.Benchmarks;

/// <summary>
/// The generator sets each row runs. Only the named generator DLLs are loaded (never the code-fix DLLs in the same folder),
/// and every generator such a DLL exposes runs, as the compiler would run it.
/// </summary>
public static class GeneratorSet
{
    private static readonly Lazy<ImmutableArray<ISourceGenerator>> s_cseEndpoints = new(static () => Load("CSharpEssentials.Endpoints.Generators", "CSharpEssentials.Endpoints.Generators.dll"));
    private static readonly Lazy<ImmutableArray<ISourceGenerator>> s_cseDependencyInjection = new(static () => Load("CSharpEssentials.DependencyInjection.Generators", "CSharpEssentials.DependencyInjection.Generators.dll"));
    private static readonly Lazy<ImmutableArray<ISourceGenerator>> s_cseEnums = new(static () => Load("CSharpEssentials.Enums.Generators", "CSharpEssentials.Enums.Generators.dll"));
    private static readonly Lazy<ImmutableArray<ISourceGenerator>> s_immediateApis = new(static () => Load("Immediate.Apis", "Immediate.Apis.Generators.dll"));
    private static readonly Lazy<ImmutableArray<ISourceGenerator>> s_immediateHandlers = new(static () => Load("Immediate.Handlers", "Immediate.Handlers.Generators.dll"));
    private static readonly Lazy<ImmutableArray<ISourceGenerator>> s_injectio = new(static () => Load("Injectio", "Injectio.Generators.dll"));
    private static readonly Lazy<ImmutableArray<ISourceGenerator>> s_netEscapades = new(static () => Load("NetEscapades", "NetEscapades.EnumGenerators.Generators.dll"));
    private static readonly Lazy<ImmutableArray<DiagnosticAnalyzer>> s_cseEntityFrameworkCore = new(static () =>
        Reference("CSharpEssentials.EntityFrameworkCore.Generators", "CSharpEssentials.EntityFrameworkCore.Generators.dll").GetAnalyzers(LanguageNames.CSharp));

    public static ImmutableArray<ISourceGenerator> CseEndpoints => s_cseEndpoints.Value;

    public static ImmutableArray<ISourceGenerator> CseDependencyInjection => s_cseDependencyInjection.Value;

    public static ImmutableArray<ISourceGenerator> CseEnums => s_cseEnums.Value;

    public static ImmutableArray<ISourceGenerator> ImmediateApis => s_immediateApis.Value;

    public static ImmutableArray<ISourceGenerator> ImmediateApisWithHandlers => [.. s_immediateApis.Value, .. s_immediateHandlers.Value];

    public static ImmutableArray<ISourceGenerator> Injectio => s_injectio.Value;

    public static ImmutableArray<ISourceGenerator> NetEscapades => s_netEscapades.Value;

    public static ImmutableArray<DiagnosticAnalyzer> CseEntityFrameworkCore => s_cseEntityFrameworkCore.Value;

    public static ImmutableArray<ISourceGenerator> Load(string folder, string generatorDll)
    {
        ImmutableArray<ISourceGenerator> generators = Reference(folder, generatorDll).GetGenerators(LanguageNames.CSharp);
        return generators.IsEmpty
            ? throw new InvalidOperationException($"'{folder}/{generatorDll}' exposes no C# generator.")
            : generators;
    }

    private static AnalyzerFileReference Reference(string folder, string dll)
    {
        string path = BenchPaths.Analyzers(folder, dll);
        AnalyzerFileReference reference = new(path, new AnalyzerFolderLoader(Path.GetDirectoryName(path)!));
        reference.AnalyzerLoadFailed += static (_, e) =>
            throw new InvalidOperationException($"Loading {e.TypeName ?? "the assembly"} failed: {e.ErrorCode} {e.Message}", e.Exception);
        return reference;
    }
}
