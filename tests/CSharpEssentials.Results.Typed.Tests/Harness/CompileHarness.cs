using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Results.Typed.Tests.Harness;

/// <summary>
/// Compiles a consumer snippet in memory with Roslyn against <see cref="ReferencePack.Current"/> and returns its diagnostics.
/// </summary>
/// <remarks>
/// The in-process compiler is not the one a consumer builds with. tests/TypedResultConsumer checks the shipped experience
/// with the real SDK.
/// </remarks>
internal static class CompileHarness
{
    public static IReadOnlyList<Diagnostic> Compile(string source, LanguageVersion languageVersion)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(languageVersion));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "TypedResultProbe",
            [tree],
            ReferencePack.Current.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        return [.. compilation.GetDiagnostics().Where(d => d.Severity >= DiagnosticSeverity.Warning)];
    }

    public static IReadOnlySet<string> Ids(this IEnumerable<Diagnostic> diagnostics) =>
        diagnostics.Select(d => d.Id).ToHashSet(StringComparer.Ordinal);
}
