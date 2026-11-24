using System.Reflection;
using System.Runtime.ExceptionServices;
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
    public static IReadOnlyList<Diagnostic> Compile(string source, LanguageVersion languageVersion) =>
        [.. Create(source, languageVersion).GetDiagnostics().Where(d => d.Severity >= DiagnosticSeverity.Warning)];

    /// <summary>
    /// Compiles <paramref name="source"/> at C# 15, loads it and calls the public static parameterless method
    /// <paramref name="method"/> of the <c>Probe</c> class. An exception thrown by the method is rethrown as is.
    /// </summary>
    public static object? Run(string source, string method)
    {
        using var stream = new MemoryStream();
        Microsoft.CodeAnalysis.Emit.EmitResult result = Create(source, LanguageVersion.CSharp15).Emit(stream);
        if (!result.Success)
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics));

        MethodInfo probe = Assembly.Load(stream.ToArray()).GetType("Probe", throwOnError: true)!.GetMethod(method)
            ?? throw new InvalidOperationException($"Probe.{method} was not found.");
        try
        {
            return probe.Invoke(null, null);
        }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private static CSharpCompilation Create(string source, LanguageVersion languageVersion) =>
        CSharpCompilation.Create(
            "TypedResultProbe",
            [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(languageVersion))],
            ReferencePack.Current.References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

    public static IReadOnlySet<string> Ids(this IEnumerable<Diagnostic> diagnostics) =>
        diagnostics.Select(d => d.Id).ToHashSet(StringComparer.Ordinal);
}
