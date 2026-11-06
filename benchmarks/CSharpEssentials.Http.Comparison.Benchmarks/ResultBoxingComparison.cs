using BenchmarkDotNet.Attributes;
using Polly;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

/// <summary>
/// Retry(3, zero delay) over an operation that returns <c>Task&lt;Result&lt;int&gt;&gt;</c>, prebuilt once. The non-generic
/// CSE policy runs on <c>ResiliencePipeline</c> (outcomes typed object, so the Result is boxed for the predicate); the
/// generic policy and Polly run on <c>ResiliencePipeline&lt;Result&lt;int&gt;&gt;</c>. <see cref="FailFirst"/> works as in
/// <see cref="RetryIfFailedComparison"/>: one precomputed Unexpected failure, then 42.
/// </summary>
[MemoryDiagnoser]
public class ResultBoxingComparison
{
    private int _calls;
    private Func<CancellationToken, Task<Result<int>>> _op = null!;
    private ResiliencePolicy _policy;
    private ResiliencePolicy<int> _policyT;
    private ResiliencePipeline<Result<int>> _pipeline = null!;

    [Params(false, true)]
    public bool FailFirst { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _op = _ => ++_calls == 1 && FailFirst ? Inputs.FailedResult : Inputs.DoneResult;
        _policy = ResiliencePolicy.Create().WithRetry(3, TimeSpan.Zero);
        _policyT = ResiliencePolicy<int>.Create().WithRetry(3, TimeSpan.Zero);
        _pipeline = PollyPipelines.RetryResult();

        Parity.Same(nameof(ResultBoxingComparison),
            ("expected", $"42 {(FailFirst ? 2 : 1)}"),
            (nameof(CSharpEssentials_NonGeneric), $"{Inputs.Run(CSharpEssentials_NonGeneric()).Value} {_calls}"),
            (nameof(CSharpEssentials_Generic), $"{Inputs.Run(CSharpEssentials_Generic()).Value} {_calls}"),
            (nameof(Polly_Generic), $"{Inputs.Run(Polly_Generic()).Value} {_calls}"));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Resilience/Readme.MD#retrying-failed-results
    [Benchmark(Baseline = true)]
    public Task<Result<int>> CSharpEssentials_NonGeneric()
    {
        _calls = 0;
        return _policy.ExecuteAsync<int>(_op);
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Resilience/Readme.MD#retrying-failed-results
    [Benchmark]
    public Task<Result<int>> CSharpEssentials_Generic()
    {
        _calls = 0;
        return _policyT.ExecuteAsync(_op);
    }

    // https://github.com/App-vNext/Polly/blob/8.9.0/docs/strategies/retry.md
    [Benchmark]
    public ValueTask<Result<int>> Polly_Generic()
    {
        _calls = 0;
        return _pipeline.ExecuteAsync(static (op, token) => new ValueTask<Result<int>>(op(token)), _op, CancellationToken.None);
    }
}
