using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CSharpEssentials.Tests.Generators;

public sealed class TestAnalyzerConfigOptions(IReadOnlyDictionary<string, string> values) : AnalyzerConfigOptions
{
    public static TestAnalyzerConfigOptions Empty { get; } = new(new Dictionary<string, string>());

    public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) =>
        values.TryGetValue(key, out value);
}
