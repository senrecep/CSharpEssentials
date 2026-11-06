using BenchmarkDotNet.Running;

namespace CSharpEssentials.Json.Comparison.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args is ["--self-check"])
        {
            JsonSelfCheck.VerifyAll();
            return;
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
