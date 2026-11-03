using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Polly;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

/// <summary>
/// Runs a cached <c>Func&lt;CancellationToken, Task&lt;int&gt;&gt;</c> over a completed task that returns 42. Empty has no
/// strategy: CSE wraps the value in <c>Result&lt;int&gt;</c> inside a try/catch (labelled), Polly runs
/// <c>ResiliencePipeline.Empty</c>. Retry is retry(3, zero delay) on the success path, prebuilt once; the CSE predicate
/// also checks failed Results while Polly's default handles every exception except cancellation (minor, labelled).
/// RetryPerCall (labelled) builds the policy on every call, the shape of the CSE Readme Quick Start; Polly's docs say to
/// reuse pipelines.
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class PolicyOverheadComparison
{
    private const string Empty = "Empty";
    private const string Retry = "Retry";

    private readonly Func<CancellationToken, Task<int>> _op = static _ => Inputs.Done;
    private ResiliencePolicy _retryPolicy;
    private ResiliencePipeline _retryPipeline = null!;

    [GlobalSetup]
    public void Setup()
    {
        _retryPolicy = ResiliencePolicy.Create().WithRetry(3, TimeSpan.Zero);
        _retryPipeline = PollyPipelines.Retry();

        Parity.Same(Empty,
            ("expected", Parity.Of(42)),
            (nameof(Direct_Await), Parity.Of(Inputs.Run(Direct_Await()))),
            (nameof(Polly_Empty), Parity.Of(Inputs.Run(Polly_Empty()))),
            (nameof(CSharpEssentials_Empty), Parity.Of(Inputs.Run(CSharpEssentials_Empty()).Value)));
        Parity.Same(Retry,
            ("expected", Parity.Of(42)),
            (nameof(CSharpEssentials_Retry), Parity.Of(Inputs.Run(CSharpEssentials_Retry()).Value)),
            (nameof(Polly_Retry), Parity.Of(Inputs.Run(Polly_Retry()))),
            (nameof(CSharpEssentials_RetryPerCall), Parity.Of(Inputs.Run(CSharpEssentials_RetryPerCall()).Value)),
            (nameof(Polly_RetryPerCall), Parity.Of(Inputs.Run(Polly_RetryPerCall()))));
    }

    // https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/
    [Benchmark, BenchmarkCategory(Empty)]
    public async Task<int> Direct_Await() => await _op(CancellationToken.None).ConfigureAwait(false);

    // https://github.com/App-vNext/Polly/blob/8.9.0/docs/advanced/performance.md#use-static-lambdas
    [Benchmark, BenchmarkCategory(Empty)]
    public ValueTask<int> Polly_Empty() =>
        ResiliencePipeline.Empty.ExecuteAsync(static (op, token) => new ValueTask<int>(op(token)), _op, CancellationToken.None);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Resilience/Readme.MD#policy-builder
    [Benchmark(Baseline = true), BenchmarkCategory(Empty)]
    public Task<Result<int>> CSharpEssentials_Empty() => ResiliencePolicy.Create().ExecuteAsync(_op);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Resilience/Readme.MD#policy-builder
    [Benchmark(Baseline = true), BenchmarkCategory(Retry)]
    public Task<Result<int>> CSharpEssentials_Retry() => _retryPolicy.ExecuteAsync(_op);

    // https://github.com/App-vNext/Polly/blob/8.9.0/docs/strategies/retry.md
    [Benchmark, BenchmarkCategory(Retry)]
    public ValueTask<int> Polly_Retry() =>
        _retryPipeline.ExecuteAsync(static (op, token) => new ValueTask<int>(op(token)), _op, CancellationToken.None);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Resilience/Readme.MD#quick-start
    [Benchmark, BenchmarkCategory(Retry)]
    public Task<Result<int>> CSharpEssentials_RetryPerCall() => ResiliencePolicy.Create().WithRetry(3, TimeSpan.Zero).ExecuteAsync(_op);

    // https://github.com/App-vNext/Polly/blob/8.9.0/docs/advanced/performance.md#reuse-resilience-pipeline-instances
    [Benchmark, BenchmarkCategory(Retry)]
    public ValueTask<int> Polly_RetryPerCall() =>
        PollyPipelines.Retry().ExecuteAsync(static (op, token) => new ValueTask<int>(op(token)), _op, CancellationToken.None);
}
