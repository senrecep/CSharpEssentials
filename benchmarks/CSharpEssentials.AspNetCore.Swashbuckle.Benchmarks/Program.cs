using BenchmarkDotNet.Running;

namespace CSharpEssentials.AspNetCore.Swashbuckle.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args is ["--self-check"])
        {
            SwashbuckleSelfCheck.VerifyAll();
            return;
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
