using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CSharpEssentials.Core;
using Humanizer;

namespace CSharpEssentials.Json.Comparison.Benchmarks;

/// <summary>
/// Converts an ASCII PascalCase name to snake, kebab and camel case. The inputs have no acronyms and no digits, so all
/// three libraries give the same output, which parity checks. The long input (280 characters) is above CSE's 256-character
/// stack buffer. Acronyms, digits and non-Latin letters are converted differently and are not timed.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class CaseConversionComparison
{
    private const string Snake = "Snake";
    private const string Kebab = "Kebab";
    private const string Camel = "Camel";

    private string _input = null!;

    [Params(Inputs.Short, Inputs.Medium, Inputs.Long)]
    public string Length { get; set; } = Inputs.Short;

    [GlobalSetup]
    public void Setup()
    {
        _input = Inputs.PascalCase(Length);
        Parity.Same(Snake + " " + Length,
            (nameof(CSharpEssentials_Snake), CSharpEssentials_Snake()),
            (nameof(Bcl_Snake), Bcl_Snake()),
            (nameof(Humanizer_Snake), Humanizer_Snake()));
        Parity.Same(Kebab + " " + Length,
            (nameof(CSharpEssentials_Kebab), CSharpEssentials_Kebab()),
            (nameof(Bcl_Kebab), Bcl_Kebab()),
            (nameof(Humanizer_Kebab), Humanizer_Kebab()));
        Parity.Same(Camel + " " + Length,
            (nameof(CSharpEssentials_Camel), CSharpEssentials_Camel()),
            (nameof(Bcl_Camel), Bcl_Camel()),
            (nameof(Humanizer_Camel), Humanizer_Camel()));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Core/README.MD#string-case-conversions
    [Benchmark(Baseline = true), BenchmarkCategory(Snake)]
    public string CSharpEssentials_Snake() => _input.ToSnakeCase();

    // https://learn.microsoft.com/dotnet/api/system.text.json.jsonnamingpolicy.snakecaselower
    [Benchmark, BenchmarkCategory(Snake)]
    public string Bcl_Snake() => JsonNamingPolicy.SnakeCaseLower.ConvertName(_input);

    // https://github.com/Humanizr/Humanizer/blob/90f0cb1d7427635f38b46350bb1dc5fca2164f2d/readme.md#underscore-dasherize-kebaberize
    [Benchmark, BenchmarkCategory(Snake)]
    public string Humanizer_Snake() => _input.Underscore();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Core/README.MD#string-case-conversions
    [Benchmark(Baseline = true), BenchmarkCategory(Kebab)]
    public string CSharpEssentials_Kebab() => _input.ToKebabCase();

    // https://learn.microsoft.com/dotnet/api/system.text.json.jsonnamingpolicy.kebabcaselower
    [Benchmark, BenchmarkCategory(Kebab)]
    public string Bcl_Kebab() => JsonNamingPolicy.KebabCaseLower.ConvertName(_input);

    // https://github.com/Humanizr/Humanizer/blob/90f0cb1d7427635f38b46350bb1dc5fca2164f2d/readme.md#underscore-dasherize-kebaberize
    [Benchmark, BenchmarkCategory(Kebab)]
    public string Humanizer_Kebab() => _input.Kebaberize();

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Core/README.MD#string-case-conversions
    [Benchmark(Baseline = true), BenchmarkCategory(Camel)]
    public string CSharpEssentials_Camel() => _input.ToCamelCase();

    // https://learn.microsoft.com/dotnet/api/system.text.json.jsonnamingpolicy.camelcase
    [Benchmark, BenchmarkCategory(Camel)]
    public string Bcl_Camel() => JsonNamingPolicy.CamelCase.ConvertName(_input);

    // https://github.com/Humanizr/Humanizer/blob/90f0cb1d7427635f38b46350bb1dc5fca2164f2d/readme.md#titleize-pascalize-camelize
    [Benchmark, BenchmarkCategory(Camel)]
    public string Humanizer_Camel() => _input.Camelize();
}
