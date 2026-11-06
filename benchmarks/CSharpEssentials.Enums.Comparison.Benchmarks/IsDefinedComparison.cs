using BenchmarkDotNet.Attributes;
using CSharpEssentials.Enums.Comparison.Benchmarks.NetEscapadesGenerated;

namespace CSharpEssentials.Enums.Comparison.Benchmarks;

/// <summary>
/// Whether a <see cref="Status"/> value is a declared member: <see cref="Status.Cancelled"/> (75) or the undefined
/// <c>(Status)76</c>. All four agree on a non-flags enum. On <c>[Flags]</c> enums CSE <c>IsDefined</c> also returns true for
/// any combination of defined bits, while BCL, Enums.NET and NetEscapades return true only for a declared member, so
/// this scenario uses <see cref="Status"/> only.
/// </summary>
[MemoryDiagnoser]
public class IsDefinedComparison
{
    private Status _value;

    [Params(true, false)]
    public bool Defined { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _value = Defined ? Status.Cancelled : (Status)76;
        Parity.Same(nameof(IsDefinedComparison),
            ("expected", Parity.Of(Defined)),
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials())),
            (nameof(Bcl), Parity.Of(Bcl())),
            (nameof(EnumsNet), Parity.Of(EnumsNet())),
            (nameof(NetEscapades), Parity.Of(NetEscapades())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#generated-helpers
    [Benchmark(Baseline = true)]
    public bool CSharpEssentials() => StatusExtensions.IsDefined(_value);

    // https://learn.microsoft.com/dotnet/api/system.enum.isdefined
    [Benchmark]
    public bool Bcl() => Enum.IsDefined(_value);

    // https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L663
    [Benchmark]
    public bool EnumsNet() => EnumsNET.Enums.IsDefined(_value);

    // https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md
    [Benchmark]
    public bool NetEscapades() => StatusFastExtensions.IsDefined(_value);
}
