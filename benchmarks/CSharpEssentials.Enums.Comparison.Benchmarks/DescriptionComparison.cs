using BenchmarkDotNet.Attributes;
using CSharpEssentials.Enums.Comparison.Benchmarks.NetEscapadesGenerated;
using EnumsNET;

namespace CSharpEssentials.Enums.Comparison.Benchmarks;

/// <summary>
/// Reads the <c>[Description]</c> text of <see cref="Status.PendingApproval"/> ("Waiting for approval"). CSE generates
/// no description method; its row is a lookup in the generated metadata (<c>EnumMetadata.Get&lt;T&gt;().TryGetMember</c>).
/// The BCL has no description API (only reflection over the field), so it has no column.
/// Members without <c>[Description]</c> differ: CSE returns the member's XML doc summary, Enums.NET returns null and
/// NetEscapades returns the member name, so the input is the one member that carries the attribute.
/// </summary>
[MemoryDiagnoser]
public class DescriptionComparison
{
    private Status _value;

    [GlobalSetup]
    public void Setup()
    {
        _value = Status.PendingApproval;
        Parity.Same(nameof(DescriptionComparison),
            ("expected", "Waiting for approval"),
            (nameof(CSharpEssentials), Parity.Of(CSharpEssentials())),
            (nameof(EnumsNet), Parity.Of(EnumsNet())),
            (nameof(NetEscapades), Parity.Of(NetEscapades())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Enums/Readme.MD#conventions-and-runtime-api
    [Benchmark(Baseline = true)]
    public string? CSharpEssentials() =>
        EnumMetadata.Get<Status>().TryGetMember(_value, out EnumMemberInfo<Status>? member) ? member.Description : null;

    // https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L713
    [Benchmark]
    public string? EnumsNet() => EnumsNET.Enums.AsString(_value, EnumFormat.Description);

    // https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md
    [Benchmark]
    public string NetEscapades() => StatusFastExtensions.ToStringFast(_value, useMetadataAttributes: true);
}
