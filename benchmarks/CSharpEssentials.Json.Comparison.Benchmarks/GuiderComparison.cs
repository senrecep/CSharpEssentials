#if NET9_0_OR_GREATER
using System.Buffers.Text;
#endif
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using CSharpEssentials.Core;
using CSharpVitamins;

namespace CSharpEssentials.Json.Comparison.Benchmarks;

/// <summary>
/// Encodes a <see cref="Guid"/> as 22 URL-safe base64 characters and decodes it back. The alphabets differ: CSE maps
/// <c>+</c> and <c>/</c> the other way round from RFC 4648, which ShortGuid and <c>Base64Url</c> follow, so for this input
/// (its base64 contains <c>/</c>) the text differs. Parity is therefore checked within each library: its own encoding is
/// 22 characters and its own decode returns the input. ShortGuid's decode also re-encodes to check the input strictly.
/// The <c>Base64Url</c> rows exist on .NET 9 and later only.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class GuiderComparison
{
    private const string Encode = "Encode";
    private const string Decode = "Decode";

    private readonly Guid _id = Inputs.Id;
    private string _cseText = null!;
    private string _sgText = null!;
#if NET9_0_OR_GREATER
    private string _bclText = null!;
#endif

    [GlobalSetup]
    public void Setup()
    {
        _cseText = CSharpEssentials_Encode();
        _sgText = ShortGuid_Encode();
#if NET9_0_OR_GREATER
        _bclText = Bcl_Encode();
#endif
        Parity.Same(Encode + "/" + Decode,
            ("expected", RoundTrip(22, true)),
            (nameof(CSharpEssentials_Encode), RoundTrip(_cseText.Length, CSharpEssentials_Decode() == _id)),
#if NET9_0_OR_GREATER
            (nameof(Bcl_Encode), RoundTrip(_bclText.Length, Bcl_Decode() == _id)),
#endif
            (nameof(ShortGuid_Encode), RoundTrip(_sgText.Length, ShortGuid_Decode() == _id)));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Core/README.MD#guid-utilities
    [Benchmark(Baseline = true), BenchmarkCategory(Encode)]
    public string CSharpEssentials_Encode() => _id.ToStringFromGuid();

    // https://github.com/csharpvitamins/csharpvitamins.shortguid/blob/835267feaf85da0651293c7295d5eb2e9f012990/readme.md#using-the-shortguid
    [Benchmark, BenchmarkCategory(Encode)]
    public string ShortGuid_Encode() => ((ShortGuid)_id).Value;

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Core/README.MD#guid-utilities
    [Benchmark(Baseline = true), BenchmarkCategory(Decode)]
    public Guid CSharpEssentials_Decode() => _cseText.ToGuidFromString();

    // https://github.com/csharpvitamins/csharpvitamins.shortguid/blob/835267feaf85da0651293c7295d5eb2e9f012990/readme.md#using-the-shortguid
    [Benchmark, BenchmarkCategory(Decode)]
    public Guid ShortGuid_Decode() => ((ShortGuid)_sgText).Guid;

#if NET9_0_OR_GREATER
    // https://learn.microsoft.com/dotnet/api/system.buffers.text.base64url.encodetostring
    [Benchmark, BenchmarkCategory(Encode)]
    public string Bcl_Encode()
    {
        Span<byte> bytes = stackalloc byte[16];
        _id.TryWriteBytes(bytes);
        return Base64Url.EncodeToString(bytes);
    }

    // https://learn.microsoft.com/dotnet/api/system.buffers.text.base64url.decodefromchars
    [Benchmark, BenchmarkCategory(Decode)]
    public Guid Bcl_Decode()
    {
        Span<byte> bytes = stackalloc byte[16];
        Base64Url.DecodeFromChars(_bclText, bytes);
        return new Guid(bytes);
    }
#endif

    private static string RoundTrip(int length, bool ok) => Parity.Of(length) + ":" + ok.ToString();
}
