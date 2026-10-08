using BenchmarkDotNet.Running;

namespace CSharpEssentials.Results.Benchmarks;

public static class Program
{
    // No job is added here: BenchmarkDotNet uses its default job unless --job is passed, so --job Short or Dry
    // replaces it instead of running next to it.
    public static void Main(string[] args) =>
        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args);
}
