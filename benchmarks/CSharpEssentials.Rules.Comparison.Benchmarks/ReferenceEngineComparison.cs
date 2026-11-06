using BenchmarkDotNet.Attributes;
using NRules;
using NRules.Fluent;
using RulesEngine.Models;

namespace CSharpEssentials.Rules.Comparison.Benchmarks;

/// <summary>
/// Reference only, a different model: NRules (Rete forward chaining over facts) and RulesEngine (string-expression workflows)
/// next to the CSE prebuilt <c>Linear()</c> chain and a hand-written baseline, on the same five rules. With
/// <see cref="RuleInput.FailLast"/> every engine evaluates all five rules and reports one failure, so the work is comparable.
/// NRules: the Rete network is compiled once in setup; each op creates a session as documented, inserts the application and a
/// per-op violations collector, and fires. RulesEngine: the workflow is built in setup and executed once there, because its
/// expression compilation (System.Linq.Dynamic.Core and FastExpressionCompiler) is lazy and cached; each op runs its async-only
/// <c>ExecuteAllRulesAsync</c>, which returns a result tree for every rule.
/// </summary>
[MemoryDiagnoser]
public class ReferenceEngineComparison
{
    private const string Workflow = "Loan";
    private ISessionFactory _nrules = null!;
    private global::RulesEngine.RulesEngine _rulesEngine = null!;
    private LoanApplication _context = null!;

    [Params(RuleInput.Pass, RuleInput.FailLast)]
    public RuleInput Input { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _context = Input == RuleInput.Pass ? LoanInputs.Pass() : LoanInputs.FailLastOfFive();

        var repository = new RuleRepository();
        repository.Load(x => x.From(typeof(AgeRule), typeof(IncomeRule), typeof(CreditScoreRule), typeof(CountryRule), typeof(DefaultsRule)));
        _nrules = repository.Compile();

        var workflow = new Workflow
        {
            WorkflowName = Workflow,
            Rules =
            [
                new ReRule { RuleName = "Loan.Age", Expression = "input1.Age >= 18", ErrorMessage = "Applicant must be at least 18." },
                new ReRule { RuleName = "Loan.Income", Expression = "input1.Income >= 30000", ErrorMessage = "Income must be at least 30000." },
                new ReRule { RuleName = "Loan.CreditScore", Expression = "input1.CreditScore >= 650", ErrorMessage = "Credit score must be at least 650." },
                new ReRule { RuleName = "Loan.Country", Expression = "input1.Country == \"DE\"", ErrorMessage = "Loans are offered in DE only." },
                new ReRule { RuleName = "Loan.Defaults", Expression = "input1.HasDefaults == false", ErrorMessage = "Applicant has defaults." },
            ],
        };
        _rulesEngine = new global::RulesEngine.RulesEngine([workflow], new ReSettings());

        Parity.Same(nameof(ReferenceEngineComparison),
            ("expected", Input == RuleInput.Pass ? "ok" : "fail:1:Loan.Defaults"),
            (nameof(CseLinear), Parity.Of(CseLinear())),
            (nameof(HandWritten), Parity.Of(HandWritten())),
            (nameof(NRulesReference), Parity.Of(NRulesReference())),
            (nameof(RulesEngineReference), Parity.Of(RulesEngineReference().AsTask().GetAwaiter().GetResult())));
    }

    // https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Rules/Readme.MD#linear-stop-on-first-failure
    [Benchmark(Baseline = true)]
    public Result CseLinear() => RuleEngine.Evaluate(LoanRules.Linear5, _context);

    [Benchmark]
    public Result HandWritten() => LoanRules.HandWrittenFive(_context);

    // https://nrules.net/articles/getting-started.html
    [Benchmark]
    public Violations NRulesReference()
    {
        ISession session = _nrules.CreateSession();
        var violations = new Violations();
        session.Insert(_context);
        session.Insert(violations);
        session.Fire();
        return violations;
    }

    // https://microsoft.github.io/RulesEngine/
    // https://github.com/microsoft/RulesEngine/blob/main/README.md
    [Benchmark]
    public ValueTask<List<RuleResultTree>> RulesEngineReference() => _rulesEngine.ExecuteAllRulesAsync(Workflow, _context);
}
