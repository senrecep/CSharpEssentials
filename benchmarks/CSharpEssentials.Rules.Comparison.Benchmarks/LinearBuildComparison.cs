using BenchmarkDotNet.Attributes;

namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>
/// A Linear chain of 5 or 10 passing rules: prebuilt once, rebuilt per call by the static <c>RuleEngine.Linear(Func[], ctx)</c>
/// shortcut, rebuilt per call with <c>.Linear()</c>, and a hand-written if/else. <c>RebuiltExtension</c> starts from an
/// <c>IRule[]</c> converted once in setup (there is no <c>Func[].Linear()</c> extension), so the <c>RebuiltStatic</c> minus
/// <c>RebuiltExtension</c> gap is the per-call <c>Select</c>/<c>ToRule</c>/<c>ToArray</c>.
/// </summary>
[MemoryDiagnoser]
public class LinearBuildComparison
{
    private IRuleBase<LoanApplication> _prebuilt = null!;
    private Func<LoanApplication, Result>[] _funcs = null!;
    private IRule<LoanApplication>[] _rules = null!;
    private Func<LoanApplication, Result> _handWritten = null!;
    private LoanApplication _context = null!;

    [Params(5, 10)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        bool five = Count == 5;
        _prebuilt = five ? LoanRules.Linear5 : LoanRules.Linear10;
        _funcs = five ? LoanRules.Five : LoanRules.Ten;
        _rules = five ? LoanRules.FiveRules : LoanRules.TenRules;
        _handWritten = five ? LoanRules.HandWrittenFive : LoanRules.HandWrittenTen;
        _context = LoanInputs.Pass();
        Parity.Same(nameof(LinearBuildComparison),
            ("expected", "ok"),
            (nameof(Prebuilt), Parity.Of(Prebuilt())),
            (nameof(RebuiltStatic), Parity.Of(RebuiltStatic())),
            (nameof(RebuiltExtension), Parity.Of(RebuiltExtension())),
            (nameof(HandWritten), Parity.Of(HandWritten())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Rules/Readme.MD#linear-stop-on-first-failure
    [Benchmark(Baseline = true)]
    public Result Prebuilt() => RuleEngine.Evaluate(_prebuilt, _context);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Rules/Readme.MD#static-shortcuts
    [Benchmark]
    public Result RebuiltStatic() => RuleEngine.Linear(_funcs, _context);

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Rules/Readme.MD#linear-stop-on-first-failure
    [Benchmark]
    public Result RebuiltExtension() => RuleEngine.Evaluate(_rules.Linear(), _context);

    [Benchmark]
    public Result HandWritten() => _handWritten(_context);
}
