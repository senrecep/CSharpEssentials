using System.Reflection;
using System.Runtime.InteropServices;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>Runs every comparison's [GlobalSetup] parity check for every parameter value, without BenchmarkDotNet; throws on the first mismatch.</summary>
public static class AspNetCoreSelfCheck
{
    public static void VerifyAll()
    {
        Console.WriteLine(
            $"ASP.NET Core {VersionOf(typeof(WebApplication))}, Hellang {VersionOf(typeof(Hellang.Middleware.ProblemDetails.ProblemDetailsMiddleware))}, IdempotentAPI {VersionOf(typeof(IdempotentAPI.Core.IdempotencyOptions))}, IdempotentAPI.MinimalAPI {VersionOf(typeof(IdempotentAPI.MinimalAPI.IdempotentAPIEndpointFilter))}, Marvin.Cache.Headers {VersionOf(typeof(Marvin.Cache.Headers.HttpCacheHeadersMiddleware))}, Serilog.AspNetCore {VersionOf(typeof(Serilog.AspNetCore.RequestLoggingOptions))}, FluentValidation {VersionOf(typeof(FluentValidation.AbstractValidator<>))}, SharpGrip {VersionOf(typeof(SharpGrip.FluentValidation.AutoValidation.Endpoints.Filters.FluentValidationAutoValidationEndpointFilter))} on {AppContext.TargetFrameworkName} ({RuntimeInformation.FrameworkDescription})");
        foreach (string outcome in new[] { "Success", "Failure" })
        {
            var result = new ResultFilterComparison { Outcome = outcome };
            result.Setup();
            result.Cleanup();
            if (outcome == "Failure")
                Console.WriteLine($"Result failure bodies:{Environment.NewLine}  {result.Bodies}");
        }

        foreach (string input in new[] { "Canonical", "Normalized", "Invalid" })
        {
            var binding = new EnumBindingComparison { Input = input };
            binding.Setup();
            binding.Cleanup();
            Console.WriteLine($"Enum binding {input}: framework {binding.BclOutcome}");
        }

        foreach (int size in new[] { 1024, 65536 })
        {
            var conditional = new ConditionalGetComparison { Size = size };
            conditional.Setup();
            conditional.Cleanup();
            Console.WriteLine($"ConditionalGet {size}: {conditional.ETags}");
        }

        var problems = new ProblemDetailsComparison();
        problems.Setup();
        problems.Cleanup();
        Console.Write($"Problem bodies:{Environment.NewLine}{problems.Bodies}");

        foreach (int rules in new[] { 1, 3 })
        {
            foreach (string outcome in new[] { "Valid", "Invalid" })
            {
                var validation = new ValidationFilterComparison { Rules = rules, Outcome = outcome };
                validation.Setup();
                validation.Cleanup();
                if (outcome == "Invalid")
                    Console.Write($"Validation bodies, {rules} rule(s):{Environment.NewLine}{validation.Bodies}");
            }
        }

        foreach (int size in new[] { 1024, 262144 })
        {
            var replay = new IdempotentReplayComparison { ResponseSize = size };
            replay.Setup();
            Console.WriteLine($"Idempotent replay {size}: {replay.Headers}");
            if (size == 262144)
                Console.WriteLine($"Idempotent replay full cache, {replay.FullCacheProbe(1400)}");
            replay.Cleanup();
        }

        new IdempotencyExecutorBenchmarks().Setup();
        foreach (int size in new[] { 0, 1024, 65536 })
            new IdempotencyStoreBenchmarks { PayloadSize = size }.Setup();
        foreach (int threads in new[] { 8, 32 })
            new IdempotencyStoreContentionBenchmarks { Threads = threads }.Setup();
        new IdempotencyKeyBenchmarks().Setup();

        foreach (int size in new[] { 1024, 65536, 1048576 })
        {
            var logging = new RequestLoggingComparison { BodySize = size };
            logging.Setup();
            logging.Cleanup();
            Console.WriteLine($"Request logging {size}: {logging.Facts}");
        }

        var metadata = new RequestLoggingMetadataComparison();
        metadata.Setup();
        metadata.Cleanup();
        Console.WriteLine($"Request logging metadata:{Environment.NewLine}  {metadata.Facts}");

        Console.WriteLine($"Self-check passed on {AppContext.TargetFrameworkName}.");
    }

    private static string? VersionOf(Type type) =>
        type.Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0];
}
