# CSharpEssentials.Rules: Performance benchmarks (draft)

This page measures CSharpEssentials.Rules (CSE) against hand-written if/else code, with NRules 1.0.4 and RulesEngine 6.0.1 as reference columns. It covers sync and async trees, prebuilt and rebuilt Linear chains, short-circuiting, the conditional node and the two rule engines on the same five rules. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11.

**Status: draft.** The suite builds, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet (see [Results](#results)).

The reference engines are pinned exactly in `Directory.Packages.props`. Each pin is the latest stable release on NuGet as of 2026-11-03:

| Package | Version | License | Asset the suite loads |
|---|---|---|---|
| NRules (with NRules.Fluent, NRules.Runtime 1.0.4) | 1.0.4 | MIT (nuspec expression) | `lib/netstandard2.1` on every TFM |
| RulesEngine | 6.0.1 | MIT (`LICENSE` file in the package) | `lib/net8.0` on net8.0, `lib/net9.0` on net9.0, `lib/net10.0` on net10.0 and net11.0 |

RulesEngine brings FastExpressionCompiler 5.0.2, System.Linq.Dynamic.Core 1.6.2 and FluentValidation (11.11.0 or later; the central pin resolves it to 12.2.0). The self-check builds the RulesEngine workflow, which RulesEngine validates with FluentValidation, on every TFM, so a break from the newer FluentValidation would fail the run. net8.0 runs CSE.Rules' netstandard2.1 build.

**The NRules and RulesEngine rows are reference only.** Both are a different model from CSE (see [Semantic differences](#semantic-differences)). They are in one class, `ReferenceEngineComparison`, on an input where every engine evaluates all five rules, and every table that shows them says so.

---

## What was measured

The code is in `benchmarks/CSharpEssentials.Rules.Comparison.Benchmarks/`, in six classes.
- Every class has `[MemoryDiagnoser]`.
- Each class checks parity in `[GlobalSetup]`, and `--self-check` runs the same check for every parameter value without BenchmarkDotNet. A mismatch fails the run.
- The parity key is `ok`, or `fail:<error count>:<first error code>`. For NRules and RulesEngine the first code is the first failing rule name in ordinal order. Every class compares each row with a hard-coded expected key.
- The rules are `Func<LoanApplication, Result>` fields that return static `Error` instances from a `LoanErrors` catalogue (the Readme's domain error pattern). The trees are built once into static fields. The hand-written rows are plain static methods with if/else that return `Result`, and they need no documentation link.
- Each CSE row has a comment above it that links to its Readme section; the NRules and RulesEngine rows link to their getting-started docs.

| # | Class | Measured unit | Rows | Label |
|---|---|---|---|---|
| 1 | `SyncTreeComparison` | a prebuilt 5-rule tree, `Shape` = Linear5, And5 or Or5, `Input` = Pass or FailLast | hand-written (baseline), `RuleEngine.Evaluate`, `RuleEngine.EvaluateAsync` consumed without `AsTask` when it completed | like-for-like |
| 2 | `ConditionalComparison` | `RuleEngine.If(age rule, the other four as Linear, underage rule, ctx)`, `Input` = Pass or FailLast | hand-written (baseline), `RuleEngine.If` | like-for-like |
| 3 | `MixedAsyncTreeComparison` | a 5-node `And()` whose last leaf is `FromPredicateAsync` with `await Task.Yield()`, every rule passes | hand-written async (baseline, awaits the same leaf), `await RuleEngine.EvaluateAsync`, `RuleEngine.Evaluate` | the `Evaluate` row blocks on the async leaf, which the Readme warns about |
| 4 | `LinearBuildComparison` | a Linear chain of `Count` = 5 or 10 passing rules | prebuilt (baseline), `RuleEngine.Linear(Func[], ctx)` (converts each `Func` with `ToRule` and rebuilds the chain per call), `RuleEngine.Evaluate(IRule[].Linear(), ctx)` (rebuilds the chain per call from an `IRule[]` converted once in setup; there is no `Func[].Linear()` extension), hand-written | same chain and result; the `RebuiltStatic` minus `RebuiltExtension` gap is the per-call `Select`/`ToRule`/`ToArray` |
| 5 | `ShortCircuitComparison` | ten rules, the first fails; Or on an input where all ten fail | hand-written (baseline), `Linear()`, non-generic `And()`; hand-written all-rules against the `Result<int>` `And()`; hand-written Or against `Or()` | the generic And row is a semantic difference, see below |
| 6 | `ReferenceEngineComparison` | the five rules, `Input` = Pass or FailLast | CSE prebuilt `Linear()` (baseline), hand-written, NRules, RulesEngine | **reference only** |

### Inputs

`LoanApplication` has `Age`, `Income`, `CreditScore`, `Country` and `HasDefaults` for the five-rule set, plus `Dependents`, `Debt`, `EmploymentYears`, `Verified` and `Email` for the ten-rule set.

| Input | Meaning | Key, five rules | Key, ten rules |
|---|---|---|---|
| Pass | every rule passes | `ok` | `ok` |
| FailFirst | only `Loan.Age` fails | | `fail:1:Loan.Age` |
| FailLast | only the last rule fails (`Loan.Defaults` or `Loan.Email`) | `fail:1:Loan.Defaults` | |
| FailAll | every rule fails | Or: `fail:5:Loan.Age` | Or: `fail:10:Loan.Age` |

For the Or5 shape, `Pass` uses an input where only the last rule passes, so Or walks the whole array before it succeeds, and `FailLast` uses FailAll. With Linear or And input, Or would succeed at the first rule.

`ReferenceEngineComparison` with `FailLast` makes every engine evaluate all five rules and report one failure, so the work is comparable:
- NRules: five rule classes deriving `NRules.Fluent.Dsl.Rule` (`AgeRule` ... `DefaultsRule`), each matching the application and a `Violations` collector and adding its name when its condition fails. The Rete network is compiled once in setup. Each op creates a session as documented, inserts the application and a new collector, and fires.
- RulesEngine: one workflow with five rules whose expressions are strings over `input1` (`input1.Age >= 18`, ...), built in setup and executed once there, because RulesEngine compiles lazily and caches. Each op awaits `ExecuteAllRulesAsync` and the key counts the results with `IsSuccess == false`.

---

## Semantic differences

- **Generic And does not short-circuit.** The `Result<T>` `And()` evaluates every child and aggregates the errors, while the non-generic `And()` stops at the first failure. The Readme documents both. `ShortCircuitComparison` therefore compares the generic row with a hand-written method that checks all ten rules, not with the early-return baseline. On FailFirst both report one error, but they evaluate ten rules.
- **Or aggregates.** When every rule fails, `Or()` returns the errors of all of them. The hand-written Or returns a prebuilt array of all errors; CSE collects them into a list per call.
- **Conditional has no prebuilt node.** The conditional adapter is internal, and every public `RuleEngine.If` overload evaluates at once, so `If` builds the node per call and there is no `EvaluateAsync` row for it. The condition and both branches are prebuilt.
- **Sync over async.** `RuleEngine.Evaluate` on a tree with an async leaf blocks the calling thread until the leaf finishes. The Readme warns that this can deadlock under a `SynchronizationContext` and ties up a thread-pool thread.
- **NRules** is forward chaining over facts with a Rete network and a session. Its actions mutate a collector instead of returning a result, and a session per op is its documented usage.
- **RulesEngine** compiles string expressions over `input1` with System.Linq.Dynamic.Core and FastExpressionCompiler, has an async-only API, and always runs every rule in the workflow, returning a `RuleResultTree` for each.

---

## Feature matrix

This table comes from reading CSE source and the engines' source and docs. None of it was measured.

| Feature | CSE | Hand-written | NRules 1.0.4 (reference) | RulesEngine 6.0.1 (reference) |
|---|---|---|---|---|
| Model | composable `Result` rules | if/else | Rete forward chaining | JSON or string-expression workflows |
| Short-circuit | Linear and non-generic And yes, `Result<T>` And no | yes | no | no (the Performance mode only skips nested rules) |
| Async rules | yes | yes | no | async API only |
| Rule definition | class, `Func` or lambda | code | DSL class | string expression |
| Runtime code generation | none | none | expression compile | Dynamic LINQ plus FastExpressionCompiler |
| AOT | no reflection beyond `GetType().Name` in an error text (the AOT-compatible flag is not set) | yes | no (expression compile) | no |
| Error shape | `Result` with `Error[]` | any | user collector | `RuleResultTree` with `ErrorMessage` |
| License | MIT | n/a | MIT | MIT |

---

## Results

**Pending the full default-job run on net8.0-net11.0.** The reports will be committed under `benchmarks/results/rules/{tfm}/results/`. Short-job (`--quick`) numbers are not published here.

---

## Caveats

- **The reference columns are not a ranking.** NRules and RulesEngine are built for rule sets that change at run time or chain inferences; a five-rule check on one object is a small case for them. The rows show what that model costs on a small, fixed rule set.
- **Expected CSE costs, from reading the code** (not yet measured):
  - `CSharpEssentials.Rules/Engines/RuleEngineAsync.cs:116` and `:140`: `AndAsync` and `OrAsync` allocate a `List<Result>` per composite node, even when every child completes synchronously. `SyncTreeComparison` tests the Readme's line that a tree of sync rules completes `EvaluateAsync` "without allocating a task" against this.
  - `CSharpEssentials.Rules/Engines/Linear.cs:25-64`: the static `Linear(Func[], ctx)` overloads rebuild the adapter chain with LINQ on every call. `LinearBuildComparison` isolates it.
  - `CSharpEssentials.Results/ResultFactory.cs:47` and `:69`: `Result.And` and `Result.Or` over `IEnumerable<Result>` go through `Cast<IResultBase>()`, which boxes each `Result`. The sync And and Or nodes use them.
  - The sync engine wraps every node in `Result.Try` with a closure (`Engines/RuleEngine.cs`), and the And and Or adapters are `record struct`s boxed to `IRuleBase`.
- The CSE column is the working tree, which may not match the version published on NuGet.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project rules                        # all TFMs (net8-net11), default job
./run-benchmarks.sh --project rules --tfm net10.0          # one TFM
./run-benchmarks.sh --project rules --quick                # net9.0, Short job, one filter per class
dotnet run -c Release -f net10.0 --project CSharpEssentials.Rules.Comparison.Benchmarks -- --self-check
```

Short-job numbers cannot be compared with default-job numbers. Results are stored per framework under `benchmarks/results/rules/{tfm}/results/`.
