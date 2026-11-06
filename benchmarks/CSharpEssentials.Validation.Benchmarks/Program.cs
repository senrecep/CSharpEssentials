using BenchmarkDotNet.Running;

namespace CSharpEssentials.Validation.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args is ["--self-check"])
        {
            ValidationSelfCheck.VerifyAll();
            return;
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
