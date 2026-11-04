using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>
/// A prebuilt five-rule tree of sync rules, evaluated with <c>Evaluate</c> and with <c>EvaluateAsync</c> (consumed without
/// <c>AsTask</c> when it completes synchronously), against a hand-written if/else baseline. The Readme says a tree made only of
/// sync rules completes <c>EvaluateAsync</c> synchronously without allocating a task. For <see cref="TreeShape.Or5"/>,
/// <see cref="RuleInput.Pass"/> means only the last rule passes (Or walks the whole array) and <see cref="RuleInput.FailLast"/>
/// means all five fail. Like-for-like: every row gives the same outcome.
/// </summary>
[MemoryDiagnoser]
public class SyncTreeComparison
{
    private IRuleBase<LoanApplication> _tree = null!;
    private Func<LoanApplication, Result> _handWritten = null!;
    private LoanApplication _context = null!;

    [Params(TreeShape.Linear5, TreeShape.And5, TreeShape.Or5)]
    public TreeShape Shape { get; set; }

    [Params(RuleInput.Pass, RuleInput.FailLast)]
    public RuleInput Input { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        bool or = Shape == TreeShape.Or5;
        _tree = Shape switch
        {
            TreeShape.Linear5 => LoanRules.Linear5,
            TreeShape.And5 => LoanRules.And5,
            TreeShape.Or5 => LoanRules.Or5,
            _ => throw new InvalidOperationException($"Unknown shape {Shape}."),
        };
        _handWritten = or ? LoanRules.HandWrittenOrFive : LoanRules.HandWrittenFive;
        _context = (or, Input) switch
        {
            (false, RuleInput.Pass) => LoanInputs.Pass(),
            (false, _) => LoanInputs.FailLastOfFive(),
            (true, RuleInput.Pass) => LoanInputs.OnlyLastOfFivePasses(),
            _ => LoanInputs.FailAll(),
        };
        string failure = or ? "fail:5:Loan.Age" : "fail:1:Loan.Defaults";
        string expected = Input == RuleInput.Pass ? "ok" : failure;
        Parity.Same(nameof(SyncTreeComparison),
            ("expected", expected),
            (nameof(HandWritten), Parity.Of(HandWritten())),
            (nameof(CseEvaluate), Parity.Of(CseEvaluate())),
            (nameof(CseEvaluateAsync), Parity.Of(CseEvaluateAsync())));
    }

    [Benchmark(Baseline = true)]
    public Result HandWritten() => _handWritten(_context);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Rules/Readme.MD#combining-rules
    [Benchmark]
    public Result CseEvaluate() => RuleEngine.Evaluate(_tree, _context);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Rules/Readme.MD#async-rules-evaluateasync
    [Benchmark]
    public Result CseEvaluateAsync() => RuleEngine.EvaluateAsync(_tree, _context).Run();
}
