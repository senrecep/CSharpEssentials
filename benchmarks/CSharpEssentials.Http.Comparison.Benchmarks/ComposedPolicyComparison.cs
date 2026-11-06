using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Polly;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

/// <summary>
/// Retry(3, zero delay), then circuit breaker (10 calls, 0.5 ratio, 1 min sampling, 30 s break), then timeout(30 s), on
/// the success path. CSE builds one pipeline per strategy and nests them with <c>Merge</c>, Polly builds one pipeline;
/// FromPipeline runs that single Polly pipeline through CSE's wrapper, which separates the wrapper cost from the nesting
/// cost. Generic runs <c>Task&lt;Result&lt;int&gt;&gt;</c>; the Polly row's hand-written predicate mirrors CSE's classifier.
/// CSE's non-generic circuit breaker also counts failed Results, Polly's default counts exceptions only (no failure is fed).
/// </summary>
[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ComposedPolicyComparison
{
    private const string NonGeneric = "NonGeneric";
    private const string Generic = "Generic";

    private readonly Func<CancellationToken, Task<int>> _op = static _ => Inputs.Done;
    private readonly Func<CancellationToken, Task<Result<int>>> _resultOp = static _ => Inputs.DoneResult;
    private ResiliencePolicy _options;
    private ResiliencePolicy _fluent;
    private ResiliencePolicy _fromPipeline;
    private ResiliencePipeline _pipeline = null!;
    private ResiliencePolicy<int> _fluentT;
    private ResiliencePipeline<Result<int>> _pipelineT = null!;

    [GlobalSetup]
    public void Setup()
    {
        _options = ResiliencePolicy.Create(new ResiliencePolicyOptions
        {
            Retry = new RetryOptions { MaxAttempts = 3, Delay = TimeSpan.Zero },
            CircuitBreaker = new CircuitBreakerOptions(),
            Timeout = new TimeoutOptions { Timeout = PollyPipelines.Timeout },
        });
        _fluent = ResiliencePolicy.Create().WithRetry(3, TimeSpan.Zero).WithCircuitBreaker().WithTimeout(PollyPipelines.Timeout);
        _pipeline = PollyPipelines.RetryCircuitBreakerTimeout();
        _fromPipeline = ResiliencePolicy.FromPipeline(_pipeline);
        _fluentT = ResiliencePolicy<int>.Create().WithRetry(3, TimeSpan.Zero).WithCircuitBreaker().WithTimeout(PollyPipelines.Timeout);
        _pipelineT = PollyPipelines.RetryCircuitBreakerTimeoutResult();

        Parity.Same(NonGeneric,
            ("expected", Parity.Of(42)),
            (nameof(CSharpEssentials_Options), Parity.Of(Inputs.Run(CSharpEssentials_Options()).Value)),
            (nameof(CSharpEssentials_Fluent), Parity.Of(Inputs.Run(CSharpEssentials_Fluent()).Value)),
            (nameof(CSharpEssentials_FromPipeline), Parity.Of(Inputs.Run(CSharpEssentials_FromPipeline()).Value)),
            (nameof(Polly_SingleBuilder), Parity.Of(Inputs.Run(Polly_SingleBuilder()))));
        Parity.Same(Generic,
            ("expected", Parity.Of(42)),
            (nameof(CSharpEssentialsT_Fluent), Parity.Of(Inputs.Run(CSharpEssentialsT_Fluent()).Value)),
            (nameof(PollyT_SingleBuilder), Parity.Of(Inputs.Run(PollyT_SingleBuilder()).Value)));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Resilience/Readme.MD#configuration-options
    [Benchmark(Baseline = true), BenchmarkCategory(NonGeneric)]
    public Task<Result<int>> CSharpEssentials_Options() => _options.ExecuteAsync(_op);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Resilience/Readme.MD#pipeline-composition
    [Benchmark, BenchmarkCategory(NonGeneric)]
    public Task<Result<int>> CSharpEssentials_Fluent() => _fluent.ExecuteAsync(_op);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Resilience/Readme.MD#policy-builder
    [Benchmark, BenchmarkCategory(NonGeneric)]
    public Task<Result<int>> CSharpEssentials_FromPipeline() => _fromPipeline.ExecuteAsync(_op);

    // https://github.com/App-vNext/Polly/blob/8.9.0/docs/pipelines/index.md
    [Benchmark, BenchmarkCategory(NonGeneric)]
    public ValueTask<int> Polly_SingleBuilder() =>
        _pipeline.ExecuteAsync(static (op, token) => new ValueTask<int>(op(token)), _op, CancellationToken.None);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Resilience/Readme.MD#pipeline-composition
    [Benchmark(Baseline = true), BenchmarkCategory(Generic)]
    public Task<Result<int>> CSharpEssentialsT_Fluent() => _fluentT.ExecuteAsync(_resultOp);

    // https://github.com/App-vNext/Polly/blob/8.9.0/docs/pipelines/index.md
    [Benchmark, BenchmarkCategory(Generic)]
    public ValueTask<Result<int>> PollyT_SingleBuilder() =>
        _pipelineT.ExecuteAsync(static (op, token) => new ValueTask<Result<int>>(op(token)), _resultOp, CancellationToken.None);
}
