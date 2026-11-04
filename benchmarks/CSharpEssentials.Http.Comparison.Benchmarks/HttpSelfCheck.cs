using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using Polly;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

/// <summary>Runs every comparison's [GlobalSetup] parity check for every parameter value, without BenchmarkDotNet; throws on the first mismatch.</summary>
public static class HttpSelfCheck
{
    public static void VerifyAll()
    {
        Console.WriteLine(
            $"System.Text.Json {VersionOf(typeof(JsonSerializer))}, System.Net.Http.Json {VersionOf(typeof(JsonContent))}, Polly.Core {VersionOf(typeof(ResiliencePipeline))} on {AppContext.TargetFrameworkName} ({RuntimeInformation.FrameworkDescription})");
        foreach (int count in new[] { 1, 50 })
        {
            var get = new GetJsonComparison { Count = count };
            get.Setup();
            get.Cleanup();
        }

        var post = new PostJsonComparison();
        post.Setup();
        post.Cleanup();
        Console.WriteLine($"Request Content-Type: {post.ContentTypes}");
        var notFound = new NotFoundComparison();
        notFound.Setup();
        notFound.Cleanup();
        var build = new RequestBuildComparison();
        build.Setup();
        build.Cleanup();
        new QueryStringComparison().Setup();
        Console.WriteLine($"Query encoding: {QueryStringComparison.EncodingProbe()}");
        var resilience = new HttpResilienceComparison();
        resilience.Setup();
        resilience.Cleanup();
        new PolicyOverheadComparison().Setup();
        new ComposedPolicyComparison().Setup();
        foreach (bool failFirst in new[] { false, true })
        {
            new RetryIfFailedComparison { FailFirst = failFirst }.Setup();
            new ResultBoxingComparison { FailFirst = failFirst }.Setup();
        }

        Console.WriteLine($"Self-check passed on {AppContext.TargetFrameworkName}.");
    }

    private static string? VersionOf(Type type) =>
        type.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0];
}
