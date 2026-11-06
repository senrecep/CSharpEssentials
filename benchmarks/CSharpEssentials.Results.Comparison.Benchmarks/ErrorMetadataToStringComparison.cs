using System.Globalization;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Results.Comparison.Benchmarks;

[MemoryDiagnoser]
public class ErrorMetadataToStringComparison
{
    private ErrorMetadata _metadata = [];

    [Params(1, 8, 32)]
    public int Entries { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _metadata = [];
        for (int i = 0; i < Entries; i++)
            _metadata.AddMetadata("key" + i.ToString(CultureInfo.InvariantCulture), i);

        Parity.Same(nameof(ErrorMetadataToStringComparison),
            (nameof(CSharpEssentials), CSharpEssentials()),
            (nameof(HandWritten), HandWritten()));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Errors/Readme.MD#error-metadata
    [Benchmark(Baseline = true)]
    public string CSharpEssentials() => _metadata.ToString();

    [Benchmark]
    public string HandWritten()
    {
        var sb = new StringBuilder("[");
        bool first = true;
        foreach (KeyValuePair<string, object?> entry in _metadata)
        {
            if (!first)
                sb.Append(", ");
            first = false;
            sb.Append("{ ").Append(entry.Key).Append(": ")
                .Append(Convert.ToString(entry.Value, CultureInfo.InvariantCulture)).Append(" }");
        }

        return sb.Append(']').ToString();
    }
}
