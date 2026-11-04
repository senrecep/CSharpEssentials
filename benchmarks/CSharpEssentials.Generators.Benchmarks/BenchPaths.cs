namespace CSharpEssentials.Generators.Benchmarks;

/// <summary>Resolves the Analyzers/ and References/ files the project copies next to the benchmark binary.</summary>
public static class BenchPaths
{
    public static string Analyzers(string folder, string file) => Require(Path.Combine(AppContext.BaseDirectory, "Analyzers", folder, file));

    public static string References(string file) => Require(Path.Combine(AppContext.BaseDirectory, "References", file));

    public static string Require(string path) =>
        File.Exists(path)
            ? path
            : throw new InvalidOperationException($"Benchmark input '{path}' is missing. Rebuild the project so the CopyGeneratorsForBenchmarks target copies it.");
}
