using BenchmarkDotNet.Running;

namespace CSharpEssentials.Any.Comparison.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args is ["--self-check"])
        {
            AnySelfCheck.VerifyAll();
            return;
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
