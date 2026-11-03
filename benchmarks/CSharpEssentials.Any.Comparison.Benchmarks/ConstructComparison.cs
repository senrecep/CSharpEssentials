using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using OneOf;

namespace CSharpEssentials.Any.Comparison.Benchmarks;

/// <summary>
/// Builds a two-case union from an int (42) or a string ("forty-two") read from a field, through each library's implicit
/// conversion, and returns it. Parity reads every constructed value back with the library's own Match: "42" and
/// "forty-two".
/// <para>
/// The storage models differ and that is the point of the class: CSE keeps the value in <c>object? Value</c>, so the int
/// case boxes (24 B on 64-bit); OneOf keeps one typed field per case and allocates nothing; a Dunet union is a record
/// class, so every value is a heap object by design. FromText is the control: there CSE stores a reference and does not
/// box, while Dunet still allocates its variant.
/// </para>
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ConstructComparison
{
    private const string FromInt = "FromInt";
    private const string FromText = "FromText";

    private int _int;
    private string _text = null!;

    [GlobalSetup]
    public void Setup()
    {
        _int = Inputs.IntValue;
        _text = Inputs.TextValue;
        Parity.Same(FromInt,
            ("expected", "42"),
            (nameof(CSharpEssentials_FromInt), Read(CSharpEssentials_FromInt())),
            (nameof(OneOf_FromInt), Read(OneOf_FromInt())),
            (nameof(Dunet_FromInt), Read(Dunet_FromInt())));
        Parity.Same(FromText,
            ("expected", Inputs.TextValue),
            (nameof(CSharpEssentials_FromText), Read(CSharpEssentials_FromText())),
            (nameof(OneOf_FromText), Read(OneOf_FromText())),
            (nameof(Dunet_FromText), Read(Dunet_FromText())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#creating-unions
    [Benchmark(Baseline = true), BenchmarkCategory(FromInt)]
    public Any<int, string> CSharpEssentials_FromInt()
    {
        Any<int, string> u = _int;
        return u;
    }

    // https://github.com/mcintyre321/OneOf/blob/6e02dbe75d0f20f198c640d4c04190a85c5ac9e6/README.md#as-a-method-return-value
    [Benchmark, BenchmarkCategory(FromInt)]
    public OneOf<int, string> OneOf_FromInt()
    {
        OneOf<int, string> u = _int;
        return u;
    }

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#implicit-conversions
    [Benchmark, BenchmarkCategory(FromInt)]
    public IntOrText Dunet_FromInt()
    {
        IntOrText u = _int;
        return u;
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Any/Readme.MD#creating-unions
    [Benchmark(Baseline = true), BenchmarkCategory(FromText)]
    public Any<int, string> CSharpEssentials_FromText()
    {
        Any<int, string> u = _text;
        return u;
    }

    // https://github.com/mcintyre321/OneOf/blob/6e02dbe75d0f20f198c640d4c04190a85c5ac9e6/README.md#as-a-method-return-value
    [Benchmark, BenchmarkCategory(FromText)]
    public OneOf<int, string> OneOf_FromText()
    {
        OneOf<int, string> u = _text;
        return u;
    }

    // https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#implicit-conversions
    [Benchmark, BenchmarkCategory(FromText)]
    public IntOrText Dunet_FromText()
    {
        IntOrText u = _text;
        return u;
    }

    private static string Read(Any<int, string> u) => u.Match(first: static v => Parity.Of(v), second: static s => s).Result!;

    private static string Read(OneOf<int, string> u) => u.Match(static v => Parity.Of(v), static s => s);

    private static string Read(IntOrText u) => u.Match(static n => Parity.Of(n.Value), static t => t.Value);
}
