using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpEssentials.Generators.Benchmarks;

/// <summary>
/// An in-memory web app project of <c>Files</c> source files: the class's feature files, one GlobalUsings.cs with the Web SDK
/// implicit usings, one Program.cs and byte-identical filler files for the rest. Every tree is parsed once. The edited tree
/// changes one method body in filler file <see cref="EditedFiller"/>, which has no attributes, has base-list classes and has
/// no WithName call. The parse options define the preprocessor symbols the SDK defines for the host TFM in Release
/// (NET8_0_OR_GREATER and so on), because some generators emit code under those symbols.
/// </summary>
public sealed class ProjectShape
{
    public const int EditedFiller = 51;

    public const string AssemblyName = "Bench.App";

    public static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Latest, preprocessorSymbols: SdkSymbols(Environment.Version.Major));

    public static readonly CSharpCompilationOptions CompilationOptions = new(OutputKind.ConsoleApplication, nullableContextOptions: NullableContextOptions.Enable);

    private const string GlobalUsings = """
        global using System;
        global using System.Collections.Generic;
        global using System.Linq;
        global using System.Threading;
        global using System.Threading.Tasks;
        global using Microsoft.AspNetCore.Builder;
        global using Microsoft.AspNetCore.Http;
        global using Microsoft.AspNetCore.Routing;
        global using Microsoft.Extensions.DependencyInjection;
        """;

    private const string EntryPoint = "internal static class Program { private static void Main() { } }";

    private ProjectShape(ImmutableArray<SyntaxTree> trees, SyntaxTree original, SyntaxTree edited)
    {
        Trees = trees;
        Original = original;
        Edited = edited;
    }

    public ImmutableArray<SyntaxTree> Trees { get; }

    public SyntaxTree Original { get; }

    public SyntaxTree Edited { get; }

    public static ProjectShape Create(int files, IReadOnlyList<(string Name, string Text)> features) =>
        Create(files, features, $"Filler{EditedFiller:0000}", Filler(EditedFiller), Filler(EditedFiller, EditedFiller + 1));

    public static ProjectShape Create(int files, IReadOnlyList<(string Name, string Text)> features, string editedName, string originalText, string editedText)
    {
        int fillers = files - features.Count - 2;
        if (fillers <= EditedFiller)
        {
            throw new InvalidOperationException($"Files={files} leaves {fillers} filler files; at least {EditedFiller + 1} are needed.");
        }

        List<(string Name, string Text)> sources = [.. features, ("GlobalUsings", GlobalUsings), ("Program", EntryPoint)];
        sources.AddRange(Enumerable.Range(0, fillers).Select(static i => ($"Filler{i:0000}", Filler(i))));

        SyntaxTree[] trees = [.. sources.Select(static source => Parse(source.Name, source.Text))];
        SyntaxTree original = trees.Single(tree => tree.FilePath == PathOf(editedName));
        if (original.ToString() != originalText)
        {
            throw new InvalidOperationException($"Edit target {editedName} does not hold the expected text.");
        }

        if (originalText.Contains("WithName", StringComparison.Ordinal) || originalText == editedText)
        {
            throw new InvalidOperationException($"Edit target {editedName} must change and must not call WithName.");
        }

        return new ProjectShape([.. trees], original, Parse(editedName, editedText));
    }

    public CSharpCompilation CreateCompilation() =>
        CSharpCompilation.Create(AssemblyName, Trees, BenchReferences.All, CompilationOptions);

    public static string Filler(int index) => Filler(index, index);

    private static string Filler(int index, int added)
    {
        string i = index.ToString("0000", CultureInfo.InvariantCulture);
        string routes = index % 10 == 0
            ? $$"""

                public static class Filler{{i}}Routes
                {
                    public static RouteHandlerBuilder Map(IEndpointRouteBuilder app) => app.MapGet("/filler/{{i}}", () => {{index}}).WithName("Filler{{i}}");
                }

                """
            : string.Empty;
        return $$"""
            namespace Bench.App.Filler;

            public interface IFiller{{i}}
            {
                int Compute(int x);
            }

            public sealed class Filler{{i}} : IFiller{{i}}
            {
                private readonly List<int> _seen = [];

                public int Compute(int x)
                {
                    return x + {{added}};
                }

                public int Sum(IEnumerable<int> values)
                {
                    int total = 0;
                    foreach (int value in values)
                    {
                        total += Compute(value);
                    }

                    return total;
                }

                public string Describe(string name)
                {
                    _seen.Add(name.Length);
                    return $"{name}:{_seen.Count}";
                }
            }

            public sealed record Filler{{i}}Dto(int Id, string Name);

            public sealed class Filler{{i}}Validator : List<string>
            {
            }
            {{routes}}
            """;
    }

    public static string[] SdkSymbols(int major) =>
    [
        "TRACE",
        "RELEASE",
        "NET",
        "NETCOREAPP",
        $"NET{major}_0",
        .. Enumerable.Range(5, major - 4).Select(static v => $"NET{v}_0_OR_GREATER"),
        "NETCOREAPP1_0_OR_GREATER", "NETCOREAPP1_1_OR_GREATER", "NETCOREAPP2_0_OR_GREATER", "NETCOREAPP2_1_OR_GREATER",
        "NETCOREAPP2_2_OR_GREATER", "NETCOREAPP3_0_OR_GREATER", "NETCOREAPP3_1_OR_GREATER",
    ];

    private static SyntaxTree Parse(string name, string text) =>
        CSharpSyntaxTree.ParseText(text, ParseOptions, path: PathOf(name));

    private static string PathOf(string name) => $"Src/{name}.cs";
}
