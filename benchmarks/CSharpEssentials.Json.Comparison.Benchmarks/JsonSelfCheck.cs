using System.Runtime.InteropServices;
using System.Text.Json;

namespace CSharpEssentials.Json.Comparison.Benchmarks;

/// <summary>Runs every comparison's [GlobalSetup] parity check for every parameter value, without BenchmarkDotNet; throws on the first mismatch.</summary>
public static class JsonSelfCheck
{
    public static void VerifyAll()
    {
        Console.WriteLine(
            $"System.Text.Json {typeof(JsonSerializer).Assembly.GetName().Version} on {AppContext.TargetFrameworkName} ({RuntimeInformation.FrameworkDescription})");
        new EnumComparison().Setup();
        new EnumFlagsComparison().Setup();
        new PolymorphicComparison().Setup();
        new DateTimeComparison().Setup();
        new GuiderComparison().Setup();
        foreach (string length in new[] { Inputs.Short, Inputs.Medium, Inputs.Long })
        {
            new CaseConversionComparison { Length = length }.Setup();
        }

        foreach (int percent in new[] { 1, 50, 99 })
        {
            new RandomItemsComparison { Percent = percent }.Setup();
        }

        var cancellation = new WithCancellationComparison();
        cancellation.Setup();
        cancellation.Cleanup();
        Console.WriteLine($"Self-check passed on {AppContext.TargetFrameworkName}.");
    }
}
