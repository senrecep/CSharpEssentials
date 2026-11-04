using BenchmarkDotNet.Running;

namespace CSharpEssentials.DependencyInjection.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args is ["--self-check"])
        {
            DiSelfCheck.VerifyAll();
            DiExtSelfCheck.VerifyAll();
            return;
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
