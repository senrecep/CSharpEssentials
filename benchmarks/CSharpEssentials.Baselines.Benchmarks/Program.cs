using BenchmarkDotNet.Running;

namespace CSharpEssentials.Baselines.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args is ["--self-check"])
        {
            BaselinesSelfCheck.VerifyAll();
            return;
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
