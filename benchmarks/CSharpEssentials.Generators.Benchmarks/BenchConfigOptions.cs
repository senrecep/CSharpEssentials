using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Generators.Benchmarks;

public sealed class BenchConfigOptions(IReadOnlyDictionary<string, string> values) : AnalyzerConfigOptions
{
    public static BenchConfigOptions Empty { get; } = new(new Dictionary<string, string>(KeyComparer));

    public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) => values.TryGetValue(key, out value);
}
