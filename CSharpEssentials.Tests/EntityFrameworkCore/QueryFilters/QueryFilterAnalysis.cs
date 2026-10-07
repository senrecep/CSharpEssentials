using System.Collections.Immutable;
using CSharpEssentials.Entity.Interfaces;
using CSharpEssentials.Tests.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;

namespace CSharpEssentials.Tests.EntityFrameworkCore.QueryFilters;

internal static class QueryFilterAnalysis
{
    public const string DiagnosticId = "CSE3001";

    // The test project runs on EF Core 9, which has no named overload; this stub has the EF Core 10 shape of both overloads.
    private const string EfCore10Source = """
        using System.Collections.Generic;
        using System.Linq;

        namespace Microsoft.EntityFrameworkCore;

        public static class EntityFrameworkQueryableExtensions
        {
            public static IQueryable<TEntity> IgnoreQueryFilters<TEntity>(this IQueryable<TEntity> source)
                where TEntity : class => source;

            public static IQueryable<TEntity> IgnoreQueryFilters<TEntity>(this IQueryable<TEntity> source, IReadOnlyCollection<string> filterKeys)
                where TEntity : class => source;
        }
        """;

    private const string QueryFilterNamesSource = """
        namespace CSharpEssentials.EntityFrameworkCore;

        public static class QueryFilterNames
        {
            public const string SoftDelete = "SoftDelete";

            public const string Tenant = "Tenant";
        }
        """;

    private static readonly Lazy<AnalyzerAssembly> Assembly =
        new(static () => AnalyzerAssembly.Load("CSharpEssentials.EntityFrameworkCore.Generators.dll"));

    private static readonly Lazy<MetadataReference> EfCore10 = new(static () => Emit("Microsoft.EntityFrameworkCore", EfCore10Source));

    private static readonly Lazy<MetadataReference> Names = new(static () => Emit("CSharpEssentials.EntityFrameworkCore", QueryFilterNamesSource));

    public static ImmutableArray<DiagnosticAnalyzer> Analyzers => Assembly.Value.Analyzers;

    public static MetadataReference EfCore10Reference => EfCore10.Value;

    public static MetadataReference QueryFilterNamesReference => Names.Value;

    public static MetadataReference EfCore9Reference { get; } =
        MetadataReference.CreateFromFile(typeof(Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions).Assembly.Location);

    public static CSharpCompilation CreateCompilation(string source, LanguageVersion languageVersion, params MetadataReference[] references)
    {
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([source], [typeof(ISoftDeletableBase).Assembly]).AddReferences(references);
        CSharpParseOptions parseOptions = new(languageVersion);
        return compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(
            compilation.SyntaxTrees.Select(tree => CSharpSyntaxTree.ParseText(tree.GetText(), parseOptions, tree.FilePath)));
    }

    public static async Task<ImmutableArray<Diagnostic>> GetDiagnosticsAsync(Compilation compilation) =>
        [.. (await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(compilation, Analyzers)).Where(static diagnostic => diagnostic.Id == DiagnosticId)];

    private static PortableExecutableReference Emit(string assemblyName, string source)
    {
        CSharpCompilation compilation = GeneratorHarness.CreateCompilation([source]).WithAssemblyName(assemblyName);
        using MemoryStream stream = new();
        EmitResult result = compilation.Emit(stream);
        if (!result.Success)
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.Diagnostics));

        return MetadataReference.CreateFromImage(stream.ToArray());
    }
}
