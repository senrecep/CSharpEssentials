using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>
/// Ten rules where the first fails. <c>Linear()</c> and the non-generic <c>And()</c> stop at the first failure, like the
/// hand-written baseline. Semantic difference, labelled: the <c>Result&lt;T&gt;</c> <c>And()</c> evaluates every child and
/// aggregates (documented in the Readme), so it is compared with a hand-written all-rules baseline instead. <c>Or()</c> on an
/// input where all ten fail evaluates every rule and aggregates ten errors, compared with a hand-written Or.
/// </summary>
[MemoryDiagnoser]
public class ShortCircuitComparison
{
    private LoanApplication _failFirst = null!;
    private LoanApplication _failAll = null!;

    [GlobalSetup]
    public void Setup()
    {
        _failFirst = LoanInputs.FailFirst();
        _failAll = LoanInputs.FailAll();
        Parity.Same(nameof(ShortCircuitComparison),
            ("expected", "fail:1:Loan.Age"),
            (nameof(HandWritten), Parity.Of(HandWritten())),
            (nameof(CseLinear), Parity.Of(CseLinear())),
            (nameof(CseAnd), Parity.Of(CseAnd())));
        Parity.Same(nameof(ShortCircuitComparison),
            ("expected", "fail:1:Loan.Age"),
            (nameof(HandWrittenAll), Parity.Of(HandWrittenAll())),
            (nameof(CseAndGeneric), Parity.Of(CseAndGeneric())));
        Parity.Same(nameof(ShortCircuitComparison),
            ("expected", "fail:10:Loan.Age"),
            (nameof(HandWrittenOrAll), Parity.Of(HandWrittenOrAll())),
            (nameof(CseOrAllFail), Parity.Of(CseOrAllFail())));
    }

    [Benchmark(Baseline = true)]
    public Result HandWritten() => LoanRules.HandWrittenTen(_failFirst);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Rules/Readme.MD#linear-stop-on-first-failure
    [Benchmark]
    public Result CseLinear() => RuleEngine.Evaluate(LoanRules.Linear10, _failFirst);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Rules/Readme.MD#and-all-must-pass
    [Benchmark]
    public Result CseAnd() => RuleEngine.Evaluate(LoanRules.And10, _failFirst);

    [Benchmark]
    public Result<int> HandWrittenAll() => LoanRules.HandWrittenAllTen(_failFirst);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Rules/Readme.MD#and-all-must-pass
    [Benchmark]
    public Result<int> CseAndGeneric() => RuleEngine.Evaluate(LoanRules.And10Generic, _failFirst);

    [Benchmark]
    public Result HandWrittenOrAll() => LoanRules.HandWrittenOrTen(_failAll);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Rules/Readme.MD#or-at-least-one-must-pass
    [Benchmark]
    public Result CseOrAllFail() => RuleEngine.Evaluate(LoanRules.Or10, _failAll);
}
