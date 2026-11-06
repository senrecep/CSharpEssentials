using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>
/// A five-node <c>And()</c> tree whose last leaf is truly async (<c>FromPredicateAsync</c> with <c>await Task.Yield()</c>), on an
/// input where every rule passes: hand-written async baseline awaiting the same leaf, <c>await EvaluateAsync</c>, and
/// <c>Evaluate</c>, which blocks on the async leaf (the Readme warns: blocks a thread). Like-for-like.
/// </summary>
[MemoryDiagnoser]
public class MixedAsyncTreeComparison
{
    private LoanApplication _context = null!;

    [GlobalSetup]
    public void Setup()
    {
        _context = LoanInputs.Pass();
        Parity.Same(nameof(MixedAsyncTreeComparison),
            ("expected", "ok"),
            (nameof(HandWrittenAsync), Parity.Of(HandWrittenAsync().AsTask().GetAwaiter().GetResult())),
            (nameof(CseEvaluateAsync), Parity.Of(CseEvaluateAsync().AsTask().GetAwaiter().GetResult())),
            (nameof(CseEvaluateSyncOverAsync), Parity.Of(CseEvaluateSyncOverAsync())));
    }

    [Benchmark(Baseline = true)]
    public async ValueTask<Result> HandWrittenAsync() => await LoanRules.HandWrittenMixedAsync(_context);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Rules/Readme.MD#async-rules-evaluateasync
    [Benchmark]
    public async ValueTask<Result> CseEvaluateAsync() => await RuleEngine.EvaluateAsync(LoanRules.MixedAnd5, _context);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Rules/Readme.MD#async-rules-evaluateasync
    [Benchmark]
    public Result CseEvaluateSyncOverAsync() => RuleEngine.Evaluate(LoanRules.MixedAnd5, _context);
}
