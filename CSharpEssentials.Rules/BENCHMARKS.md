# CSharpEssentials.Rules: Performance benchmarks

CSharpEssentials.Rules (CSE) against hand-written if/else code, with NRules 1.0.4 and RulesEngine 6.0.1 as reference columns: sync and async trees, prebuilt and rebuilt Linear chains, short-circuiting, the conditional node and the two rule engines on the same five rules. BenchmarkDotNet v0.14.0, .NET 8, 9, 10 and 11. Numbers are in [Results](#results).

The reference engines are pinned exactly in `Directory.Packages.props`. Each pin is the latest stable release on NuGet as of 2026-11-03:

| Package | Version | License | Asset the suite loads |
|---|---|---|---|
| NRules (with NRules.Fluent, NRules.Runtime 1.0.4) | 1.0.4 | MIT (nuspec expression) | `lib/netstandard2.1` on every TFM |
| RulesEngine | 6.0.1 | MIT (`LICENSE` file in the package) | `lib/net8.0` on net8.0, `lib/net9.0` on net9.0, `lib/net10.0` on net10.0 and net11.0 |

RulesEngine brings FastExpressionCompiler 5.0.2, System.Linq.Dynamic.Core 1.6.2 and FluentValidation (11.11.0 or later; the central pin resolves it to 12.2.0). The self-check builds the RulesEngine workflow, which RulesEngine validates with FluentValidation, on every TFM, so a break from the newer FluentValidation would fail the run. net8.0 runs CSE.Rules' netstandard2.1 build.

The NRules and RulesEngine rows are reference only. Both are a different model from CSE (see [Semantic differences](#semantic-differences)). They are in one class, `ReferenceEngineComparison`, on an input where every engine evaluates all five rules, and every table that shows them says so.

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
| 6 | `ReferenceEngineComparison` | the five rules, `Input` = Pass or FailLast | CSE prebuilt `Linear()` (baseline), hand-written, NRules, RulesEngine | reference only |

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

- Generic And does not short-circuit: the `Result<T>` `And()` evaluates every child and aggregates the errors, while the non-generic `And()` stops at the first failure. The Readme documents both. `ShortCircuitComparison` therefore compares the generic row with a hand-written method that checks all ten rules, not with the early-return baseline. On FailFirst both report one error, but they evaluate ten rules.
- Or aggregates: when every rule fails, `Or()` returns the errors of all of them. The hand-written Or returns a prebuilt array of all errors; CSE copies them into a new array per call.
- Conditional has no prebuilt node: the conditional adapter is internal, and every public `RuleEngine.If` overload evaluates at once, so `If` builds the node per call and there is no `EvaluateAsync` row for it. The condition and both branches are prebuilt.
- Sync over async: `RuleEngine.Evaluate` on a tree with an async leaf blocks the calling thread until the leaf finishes. The Readme warns that this can deadlock under a `SynchronizationContext` and ties up a thread-pool thread.
- NRules is forward chaining over facts with a Rete network and a session. Its actions mutate a collector instead of returning a result, and a session per op is its documented usage.
- RulesEngine compiles string expressions over `input1` with System.Linq.Dynamic.Core and FastExpressionCompiler, has an async-only API, and always runs every rule in the workflow, returning a `RuleResultTree` for each.

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
| AOT | `IsAotCompatible` on the net9.0 and later targets (not netstandard2.1); no reflection beyond `GetType().Name` in an error text | yes | no (expression compile) | no |
| Error shape | `Result` with `Error[]` | any | user collector | `RuleResultTree` with `ErrorMessage` |
| License | MIT | n/a | MIT | MIT |

---

## Results

Full default-job runs on net8.0, net9.0, net10.0 and net11.0. Each cell is mean, then allocated, copied from the github report of that TFM. "none" means the report shows no allocation. Ratios quoted in the text are the report's Ratio column against the class baseline named in [What was measured](#what-was-measured). Reports: [net8.0](../benchmarks/results/rules/net8.0/results/), [net9.0](../benchmarks/results/rules/net9.0/results/), [net10.0](../benchmarks/results/rules/net10.0/results/), [net11.0](../benchmarks/results/rules/net11.0/results/).

### How these numbers were produced

- BenchmarkDotNet v0.14.0 default job (full runs, not `--quick`), run serially on one Linux machine. The CPU is in the header of each report. Every report comes from SDK 11.0.100-rc.3 with .NET 8.0.33, 9.0.22, 10.0.14 and 11.0.0, all six classes in one run on 2026-11-09, after #262 started dispatching sync leaf rules directly in `RuleEngine.Evaluate`.
- The load average was not recorded. No row is above 5% CV.
- No row is near 0 ns. The smallest is 2.242 ns (`ConditionalComparison` hand-written Pass, net11.0), and every row is above timer resolution.
- Results path: `benchmarks/results/rules/{tfm}/results/` (`*-report-github.md` and `.csv`).

### Sync and async trees

`SyncTreeComparison`, a prebuilt five-rule tree. Baseline is hand-written if/else.

| Shape | Input | Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| Linear5 | Pass | hand-written | 5.488 ns, none | 5.254 ns, none | 4.977 ns, none | 2.845 ns, none |
| Linear5 | Pass | `Evaluate` | 116.535 ns, none | 100.557 ns, none | 71.667 ns, none | 69.061 ns, none |
| Linear5 | Pass | `EvaluateAsync` | 400.122 ns, none | 371.538 ns, none | 315.547 ns, none | 316.352 ns, none |
| Linear5 | FailLast | hand-written | 18.139 ns, 56 B | 16.683 ns, 56 B | 16.233 ns, 56 B | 11.291 ns, 56 B |
| Linear5 | FailLast | `Evaluate` | 128.454 ns, 56 B | 110.133 ns, 56 B | 92.459 ns, 56 B | 81.956 ns, 56 B |
| Linear5 | FailLast | `EvaluateAsync` | 413.906 ns, 56 B | 384.542 ns, 56 B | 320.147 ns, 56 B | 325.959 ns, 56 B |
| And5 | Pass | hand-written | 5.036 ns, none | 5.230 ns, none | 4.975 ns, none | 2.836 ns, none |
| And5 | Pass | `Evaluate` | 71.092 ns, none | 53.358 ns, none | 47.302 ns, none | 44.559 ns, none |
| And5 | Pass | `EvaluateAsync` | 447.177 ns, none | 429.482 ns, none | 372.233 ns, none | 376.943 ns, none |
| And5 | FailLast | hand-written | 18.725 ns, 56 B | 16.536 ns, 56 B | 16.211 ns, 56 B | 11.361 ns, 56 B |
| And5 | FailLast | `Evaluate` | 81.714 ns, 56 B | 71.919 ns, 56 B | 60.573 ns, 56 B | 58.944 ns, 56 B |
| And5 | FailLast | `EvaluateAsync` | 484.083 ns, 56 B | 446.346 ns, 56 B | 389.920 ns, 56 B | 357.473 ns, 56 B |
| Or5 | Pass | hand-written | 5.239 ns, none | 5.275 ns, none | 5.272 ns, none | 2.894 ns, none |
| Or5 | Pass | `Evaluate` | 178.868 ns, 224 B | 150.585 ns, 224 B | 138.188 ns, 224 B | 119.258 ns, 224 B |
| Or5 | Pass | `EvaluateAsync` | 565.863 ns, 224 B | 520.468 ns, 224 B | 479.787 ns, 224 B | 442.495 ns, 224 B |
| Or5 | FailLast | hand-written | 6.067 ns, none | 5.455 ns, none | 5.614 ns, none | 3.110 ns, none |
| Or5 | FailLast | `Evaluate` | 273.962 ns, 464 B | 241.583 ns, 464 B | 219.347 ns, 464 B | 190.202 ns, 464 B |
| Or5 | FailLast | `EvaluateAsync` | 681.265 ns, 464 B | 648.539 ns, 464 B | 587.084 ns, 464 B | 533.287 ns, 464 B |

`EvaluateAsync` on a tree of sync rules takes 2.49 to 8.46 times as long as `Evaluate` on every shape and TFM. It allocates the same as `Evaluate`: nothing beyond the result on Linear5 and And5, 224 B and 464 B on Or5. The range is wider than before #262 because the direct leaf dispatch applies to `Evaluate` only.

`MixedAsyncTreeComparison`, a five-node `And()` whose last leaf awaits `Task.Yield()`. Baseline is the hand-written async method.

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| hand-written async | 1,007.0 ns, 487 B | 995.8 ns, 487 B | 968.3 ns, 487 B | 771.9 ns, 485 B |
| `await EvaluateAsync` | 1,558.0 ns, 670 B | 1,592.4 ns, 668 B | 1,395.1 ns, 671 B | 1,217.2 ns, 669 B |
| `Evaluate` (sync over async) | 1,092.0 ns, 600 B | 1,047.5 ns, 600 B | 1,003.4 ns, 600 B | 828.1 ns, 591 B |

The sync-over-async row is faster than `await EvaluateAsync` on every TFM, but it blocks a thread and can deadlock under a `SynchronizationContext` (see [Semantic differences](#semantic-differences)).

### Conditional and Linear build

`ConditionalComparison` (baseline hand-written) and `LinearBuildComparison` (baseline prebuilt chain):

| Class | Param | Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| Conditional | Pass | hand-written | 4.501 ns, none | 4.786 ns, none | 5.174 ns, none | 2.242 ns, none |
| Conditional | Pass | `RuleEngine.If` | 117.543 ns, 40 B | 99.885 ns, 40 B | 72.622 ns, 40 B | 73.909 ns, 40 B |
| Conditional | FailLast | hand-written | 18.248 ns, 56 B | 16.404 ns, 56 B | 16.010 ns, 56 B | 11.300 ns, 56 B |
| Conditional | FailLast | `RuleEngine.If` | 132.820 ns, 96 B | 112.688 ns, 96 B | 84.361 ns, 96 B | 81.990 ns, 96 B |
| LinearBuild | Count=5 | prebuilt | 116.335 ns, none | 101.655 ns, none | 71.929 ns, none | 82.697 ns, none |
| LinearBuild | Count=5 | `Linear(Func[], ctx)` rebuilt | 284.706 ns, 360 B | 242.523 ns, 360 B | 183.288 ns, 360 B | 177.912 ns, 360 B |
| LinearBuild | Count=5 | `IRule[].Linear()` rebuilt | 175.910 ns, 128 B | 153.666 ns, 128 B | 119.519 ns, 128 B | 112.161 ns, 128 B |
| LinearBuild | Count=5 | hand-written | 5.015 ns, none | 5.246 ns, none | 5.200 ns, none | 2.825 ns, none |
| LinearBuild | Count=10 | prebuilt | 284.153 ns, none | 219.249 ns, none | 154.847 ns, none | 188.251 ns, none |
| LinearBuild | Count=10 | `Linear(Func[], ctx)` rebuilt | 613.439 ns, 680 B | 495.101 ns, 680 B | 387.168 ns, 680 B | 348.203 ns, 680 B |
| LinearBuild | Count=10 | `IRule[].Linear()` rebuilt | 400.395 ns, 288 B | 370.070 ns, 288 B | 258.988 ns, 288 B | 280.888 ns, 288 B |
| LinearBuild | Count=10 | hand-written | 13.198 ns, none | 12.875 ns, none | 7.747 ns, none | 6.592 ns, none |

Rebuilding a chain with `RuleEngine.Linear(Func[], ctx)` per call has Ratio 1.85 to 2.55 against the prebuilt chain. Build the chain once and keep it.

### Short-circuit

`ShortCircuitComparison`, ten rules where the first fails (Or: all ten fail). Baseline is the hand-written early return.

| Row | Compare with | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| hand-written early return | | 16.86 ns, 56 B | 14.29 ns, 56 B | 14.149 ns, 56 B | 11.434 ns, 56 B |
| `Linear()` | early return | 38.12 ns, 56 B | 34.89 ns, 56 B | 16.193 ns, 56 B | 13.841 ns, 56 B |
| non-generic `And()` | early return | 50.42 ns, 56 B | 41.41 ns, 56 B | 20.415 ns, 56 B | 17.011 ns, 56 B |
| hand-written, all ten rules | | 49.83 ns, 144 B | 50.90 ns, 144 B | 117.368 ns, 144 B | 110.819 ns, 144 B |
| `Result<int>` `And()` (evaluates all ten) | all ten rules | 531.32 ns, 56 B | 439.43 ns, 56 B | 429.854 ns, 56 B | 399.326 ns, 56 B |
| hand-written Or, all fail | | 13.46 ns, none | 11.92 ns, none | 6.910 ns, none | 5.424 ns, none |
| `Or()`, all fail | hand-written Or | 483.47 ns, 904 B | 446.61 ns, 904 B | 392.511 ns, 904 B | 375.735 ns, 904 B |

`Linear()` is close to the hand-written early return on net10.0 and net11.0 (Ratio 1.14 and 1.21). The hand-written all-rules row more than doubles from net9.0 to net10.0 (50.90 ns to 117.368 ns), so the generic And's Ratio against the early-return baseline (31.52, 30.75, 30.38, 34.92) moves with that baseline too. The generic And allocates 56 B, less than the hand-written all-rules method's 144 B.

### Reference engines (reference only)

`ReferenceEngineComparison`, the same five rules. Baseline is CSE prebuilt `Linear()`. NRules and RulesEngine are a different model (see [Semantic differences](#semantic-differences)); these rows are not a ranking.

| Input | Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| Pass | CSE `Linear()` | 116.115 ns, none | 99.992 ns, none | 62.157 ns, none | 58.080 ns, none |
| Pass | hand-written | 4.482 ns, none | 4.602 ns, none | 4.616 ns, none | 2.245 ns, none |
| Pass | NRules (reference) | 3,728.061 ns, 10984 B | 3,722.919 ns, 10984 B | 3,454.014 ns, 10872 B | 3,230.602 ns, 10872 B |
| Pass | RulesEngine (reference) | 2,153.313 ns, 4296 B | 1,981.387 ns, 4296 B | 1,579.112 ns, 4240 B | 1,489.506 ns, 4240 B |
| FailLast | CSE `Linear()` | 133.305 ns, 56 B | 113.096 ns, 56 B | 73.195 ns, 56 B | 67.526 ns, 56 B |
| FailLast | hand-written | 18.030 ns, 56 B | 17.283 ns, 56 B | 15.966 ns, 56 B | 11.556 ns, 56 B |
| FailLast | NRules (reference) | 5,429.812 ns, 14016 B | 5,185.864 ns, 14016 B | 4,853.301 ns, 13864 B | 5,156.951 ns, 13864 B |
| FailLast | RulesEngine (reference) | 2,321.007 ns, 4416 B | 2,198.416 ns, 4416 B | 1,655.924 ns, 4360 B | 1,637.418 ns, 4360 B |

### Where CSE loses

Every CSE row is slower than the hand-written code that does the same work, on every TFM. The gaps that stand out:

- `Or()`, Or5 Pass on net10.0: `Evaluate` 138.188 ns and 224 B against hand-written 5.272 ns and none. `ShortCircuitComparison` Or with all ten failing, net10.0: 392.511 ns and 904 B against 6.910 ns and none. The hand-written Or returns a prebuilt error array, so part of this gap is the per-call error array CSE builds.
- Generic `Result<T>` `And()`: against a hand-written method that also checks all ten rules, net10.0: 429.854 ns and 56 B against 117.368 ns and 144 B; net8.0: 531.32 ns and 56 B against 49.83 ns and 144 B. It allocates less, but takes 3.6 to 10.7 times as long.
- `EvaluateAsync` on sync trees: 2.49 to 8.46 times `Evaluate` (Or5 FailLast, net10.0: 587.084 ns against 219.347 ns), see [Sync and async trees](#sync-and-async-trees).
- `await EvaluateAsync` on a mixed tree is slower than the sync-over-async `Evaluate` row on every TFM (net10.0: 1,395.1 ns and 671 B against 1,003.4 ns and 600 B).
- Conditional: `RuleEngine.If` builds its node per call: 72.622 ns and 40 B against hand-written 5.174 ns and none on Pass, net10.0.

---

## Caveats

- The reference columns are not a ranking. NRules and RulesEngine are built for rule sets that change at run time or chain inferences; a five-rule check on one object is a small case for them. The rows show what that model costs on a small, fixed rule set.
- Known CSE costs, with the rows that measure them:
  - `CSharpEssentials.Rules/Engines/RuleEngineAsync.cs`: a tree of sync rules completes `EvaluateAsync` without allocating a task, as the Readme says; the time cost is in [Sync and async trees](#sync-and-async-trees).
  - `CSharpEssentials.Rules/Engines/Linear.cs:25-64`: the static `Linear(Func[], ctx)` overloads rebuild the adapter chain with LINQ on every call. `LinearBuildComparison` Count=5 on net10.0: 183.288 ns and 360 B rebuilt, 71.929 ns and none prebuilt.
  - `CSharpEssentials.Rules/Engines/RuleEngine.cs:185` and `RuleEngineAsync.cs:139`: the Or5 rows allocate 224 B (Pass) and 464 B (FailLast) on every TFM, for `Evaluate` and `EvaluateAsync` alike. That matches 56 B per failing child result (four on Pass, five on FailLast) plus, when every child fails, one flattened `Error[]` (184 B for five errors); the ten-rule `Or()` row's 904 B matches the same count. The hand-written Or creates no child results. This is arithmetic from the reports, not a profile.
  - The And and Or adapters are `record struct`s boxed to `IRuleBase`.
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
