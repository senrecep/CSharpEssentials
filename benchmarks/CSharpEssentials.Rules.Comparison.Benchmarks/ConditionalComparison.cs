using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>
/// <c>RuleEngine.If(condition, success, failure, ctx)</c> with prebuilt parts: the age rule as the condition, the other four
/// rules as a <c>Linear()</c> chain on success and an underage rule on failure, against a hand-written if/else baseline. The
/// conditional node has no public prebuilt form, so it is built per call by <c>If</c>, and there is no <c>EvaluateAsync</c> row.
/// Like-for-like: both rows give the same outcome.
/// </summary>
[MemoryDiagnoser]
public class ConditionalComparison
{
    private LoanApplication _context = null!;

    [Params(RuleInput.Pass, RuleInput.FailLast)]
    public RuleInput Input { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _context = Input == RuleInput.Pass ? LoanInputs.Pass() : LoanInputs.FailLastOfFive();
        Parity.Same(nameof(ConditionalComparison),
            ("expected", Input == RuleInput.Pass ? "ok" : "fail:1:Loan.Defaults"),
            (nameof(HandWritten), Parity.Of(HandWritten())),
            (nameof(CseIf), Parity.Of(CseIf())));
    }

    [Benchmark(Baseline = true)]
    public Result HandWritten() => LoanRules.HandWrittenFive(_context);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Rules/Readme.MD#conditional-ifthenelse-branching
    [Benchmark]
    public Result CseIf() => RuleEngine.If(LoanRules.AgeCondition, LoanRules.AfterAge, LoanRules.Underage, _context);
}
