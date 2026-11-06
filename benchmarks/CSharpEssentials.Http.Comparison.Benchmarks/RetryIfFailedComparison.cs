using BenchmarkDotNet.Attributes;
using Polly;

namespace CSharpEssentials.Http.Comparison.Benchmarks;

/// <summary>
/// Retries a <c>Func&lt;CancellationToken, Task&lt;Result&lt;int&gt;&gt;&gt;</c> with maxAttempts 3 and zero delay. With
/// <see cref="FailFirst"/> the operation returns a precomputed <c>Unexpected</c> failure on its first call and 42 on the
/// second. Cached looks the pipeline up in CSE's cache; Predicate passes a <c>shouldRetry</c> delegate, which builds a new
/// pipeline on every call by design; the Polly row is prebuilt with a predicate that mirrors CSE's classifier. The setup
/// also runs an always-failing operation and checks that every row runs it 4 times (maxAttempts counts retries).
/// </summary>
[MemoryDiagnoser]
public class RetryIfFailedComparison
{
    private int _calls;
    private Func<CancellationToken, Task<Result<int>>> _op = null!;
    private ResiliencePipeline<Result<int>> _pipeline = null!;

    [Params(false, true)]
    public bool FailFirst { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _pipeline = PollyPipelines.RetryResult();
        _op = _ => ++_calls == 1 && FailFirst ? Inputs.FailedResult : Inputs.DoneResult;
        string expected = $"42 {(FailFirst ? 2 : 1)}";
        Parity.Same(nameof(RetryIfFailedComparison),
            ("expected", expected),
            (nameof(CSharpEssentials_Cached), $"{Inputs.Run(CSharpEssentials_Cached()).Value} {_calls}"),
            (nameof(CSharpEssentials_Predicate), $"{Inputs.Run(CSharpEssentials_Predicate()).Value} {_calls}"),
            (nameof(Polly_Cached), $"{Inputs.Run(Polly_Cached()).Value} {_calls}"));

        _op = _ =>
        {
            _calls++;
            return Inputs.FailedResult;
        };
        Parity.Same("RetryIfFailedComparison.AlwaysFailing",
            ("expected", "Bench.Transient 4"),
            (nameof(CSharpEssentials_Cached), $"{Inputs.Run(CSharpEssentials_Cached()).FirstError.Code} {_calls}"),
            (nameof(CSharpEssentials_Predicate), $"{Inputs.Run(CSharpEssentials_Predicate()).FirstError.Code} {_calls}"),
            (nameof(Polly_Cached), $"{Inputs.Run(Polly_Cached()).FirstError.Code} {_calls}"));
        _op = _ => ++_calls == 1 && FailFirst ? Inputs.FailedResult : Inputs.DoneResult;
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Resilience/Readme.MD#result-retry
    [Benchmark(Baseline = true)]
    public ValueTask<Result<int>> CSharpEssentials_Cached()
    {
        _calls = 0;
        return _op.RetryIfFailed(maxAttempts: 3, delay: TimeSpan.Zero);
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Resilience/Readme.MD#result-retry
    [Benchmark]
    public ValueTask<Result<int>> CSharpEssentials_Predicate()
    {
        _calls = 0;
        return _op.RetryIfFailed(shouldRetry: static error => error.Type == ErrorType.Unexpected, maxAttempts: 3, delay: TimeSpan.Zero);
    }

    // https://github.com/App-vNext/Polly/blob/8.9.0/docs/strategies/retry.md
    [Benchmark]
    public ValueTask<Result<int>> Polly_Cached()
    {
        _calls = 0;
        return _pipeline.ExecuteAsync(static (op, token) => new ValueTask<Result<int>>(op(token)), _op, CancellationToken.None);
    }
}
