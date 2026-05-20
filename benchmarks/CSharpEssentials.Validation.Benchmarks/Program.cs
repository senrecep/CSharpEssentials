using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace CSharpEssentials.Validation.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        IConfig config = DefaultConfig.Instance
            .AddJob(Job.Default.WithId("default"));

        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args, config);
    }
}
