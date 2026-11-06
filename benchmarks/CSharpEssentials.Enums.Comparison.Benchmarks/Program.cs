using BenchmarkDotNet.Running;

namespace CSharpEssentials.Enums.Comparison.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args is ["--self-check"])
        {
            EnumsSelfCheck.VerifyAll();
            EnumsExtSelfCheck.VerifyAll();
            return;
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
