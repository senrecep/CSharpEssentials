using System.Collections.Immutable;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Generators.Benchmarks;

/// <summary>
/// Enum extensions for 30 enums of 8 members in a web app of <see cref="Files"/> files. Each library gets its opt-in
/// attribute on the same enums. The shared references include NetEscapades.EnumGenerators.RuntimeDependencies, as the
/// recommended meta-package install does, so NetEscapades uses those shared types instead of nested copies. Cold runs a
/// fresh compilation and driver; Incremental runs a warm driver after a method-body edit in a filler file.
/// <para>
/// Semantic differences (different output): CSE emits per enum an extensions class with EnumInfo metadata, and per assembly
/// a module-initializer registry (plus a ModuleInitializerAttribute polyfill when the target lacks it). NetEscapades emits
/// a larger extensions class (ToStringFast, IsDefined, TryParse, GetValues, GetNames and more) and no registry.
/// EmptyDriver_Cold is compilation and driver object overhead, not a floor for generator work.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class EnumsGeneratorComparison
{
    public const int Enums = 30;

    private const string Cold = "Cold";
    private const string Incremental = "Incremental";

    private static readonly Regex s_names = new(@"\bEnum\d{3}\b", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

    private GeneratorRow _empty = null!;
    private GeneratorRow _cse = null!;
    private GeneratorRow _netEscapades = null!;

    [Params(200, 1000)]
    public int Files { get; set; } = 200;

    [GlobalSetup]
    public void Setup()
    {
        var cseShape = ProjectShape.Create(Files, Feature("CSharpEssentials.Enums", "StringEnum"));
        var netEscapadesShape = ProjectShape.Create(Files, Feature("NetEscapades.EnumGenerators", "EnumExtensions"));
        _empty = new GeneratorRow("EmptyDriver", false, [], cseShape);
        _cse = new GeneratorRow("CSharpEssentials", true, GeneratorSet.CseEnums, cseShape);
        _netEscapades = new GeneratorRow("NetEscapades", false, GeneratorSet.NetEscapades, netEscapadesShape);

        string[] expected = [.. Enumerable.Range(1, Enums).Select(static n => $"Enum{n:000}")];
        foreach (GeneratorRow row in Rows())
        {
            GeneratorParity.Check(nameof(EnumsGeneratorComparison), Files, row, s_names, expected, checkErrors: true);
        }
    }

    public IEnumerable<GeneratorRow> Rows() => [_cse, _netEscapades];

    // https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md
    [Benchmark, BenchmarkCategory(Cold)]
    public GeneratorDriver EmptyDriver_Cold() => _empty.Cold();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#declare-an-enum
    [Benchmark(Baseline = true), BenchmarkCategory(Cold)]
    public GeneratorDriver CSharpEssentials_Cold() => _cse.Cold();

    // https://github.com/andrewlock/NetEscapades.EnumGenerators#adding-netescapadesenumgenerators-to-your-project
    [Benchmark, BenchmarkCategory(Cold)]
    public GeneratorDriver NetEscapades_Cold() => _netEscapades.Cold();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#declare-an-enum
    [Benchmark(Baseline = true), BenchmarkCategory(Incremental)]
    public GeneratorDriver CSharpEssentials_Incremental() => _cse.Incremental();

    // https://github.com/andrewlock/NetEscapades.EnumGenerators#adding-netescapadesenumgenerators-to-your-project
    [Benchmark, BenchmarkCategory(Incremental)]
    public GeneratorDriver NetEscapades_Incremental() => _netEscapades.Incremental();

    private static ImmutableArray<(string Name, string Text)> Feature(string attributeNamespace, string attribute) =>
        [.. Enumerable.Range(1, Enums).Select(n => ($"Enum{n:000}", Enum(attributeNamespace, attribute, $"{n:000}")))];

    private static string Enum(string attributeNamespace, string attribute, string id) => $$"""
        using {{attributeNamespace}};

        namespace Bench.App.Enums;

        [{{attribute}}]
        public enum Enum{{id}}
        {
            Alpha,
            Beta,
            Gamma,
            Delta,
            Epsilon,
            Zeta,
            Eta,
            Theta,
        }
        """;
}
