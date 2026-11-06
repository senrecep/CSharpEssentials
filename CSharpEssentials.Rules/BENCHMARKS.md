# CSharpEssentials.Rules: Performance benchmarks

This page measures CSharpEssentials.Rules (CSE) against hand-written if/else code, with NRules 1.0.4 and RulesEngine 6.0.1 as reference columns. It covers sync and async trees, prebuilt and rebuilt Linear chains, short-circuiting, the conditional node and the two rule engines on the same five rules. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11. Numbers are in [Results](#results).

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

Full default-job runs on net8.0, net9.0, net10.0 and net11.0. Each cell is mean, then allocated, copied from the github report of that TFM. "none" means the report shows no allocation. Ratios quoted in the text are the report's Ratio column against the class baseline named in [What was measured](#what-was-measured). Reports: [net8.0](../benchmarks/results/rules/net8.0/results/), [net9.0](../benchmarks/results/rules/net9.0/results/), [net10.0](../benchmarks/results/rules/net10.0/results/), [net11.0](../benchmarks/results/rules/net11.0/results/).

### How these numbers were produced

- BenchmarkDotNet v0.14.0 default job (full runs, not `--quick`), run serially on one Linux machine. The CPU is in the header of each report. SDK 11.0.100-rc.1; runtimes .NET 8.0.30, 9.0.19, 10.0.11 and 11.0.0.
- The 1-minute load average was 1.41 to 1.54 during the run. No class needed a rerun, and no row stayed above 5% CV.
- No row is near 0 ns. The smallest is 2.256 ns (`ConditionalComparison` hand-written Pass, net11.0), and every row is above timer resolution.
- Results path: `benchmarks/results/rules/{tfm}/results/` (`*-report-github.md` and `.csv`).

### Sync and async trees

`SyncTreeComparison`, a prebuilt five-rule tree. Baseline is hand-written if/else.

| Shape | Input | Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| Linear5 | Pass | hand-written | 5.474 ns, none | 5.219 ns, none | 5.244 ns, none | 2.845 ns, none |
| Linear5 | Pass | `Evaluate` | 139.916 ns, none | 124.880 ns, none | 94.004 ns, none | 100.534 ns, none |
| Linear5 | Pass | `EvaluateAsync` | 405.882 ns, none | 374.696 ns, none | 365.362 ns, none | 314.386 ns, none |
| Linear5 | FailLast | hand-written | 18.153 ns, 56 B | 16.628 ns, 56 B | 16.949 ns, 56 B | 11.982 ns, 56 B |
| Linear5 | FailLast | `Evaluate` | 152.224 ns, 56 B | 138.443 ns, 56 B | 107.903 ns, 56 B | 101.484 ns, 56 B |
| Linear5 | FailLast | `EvaluateAsync` | 423.177 ns, 56 B | 366.754 ns, 56 B | 304.708 ns, 56 B | 330.696 ns, 56 B |
| And5 | Pass | hand-written | 5.095 ns, none | 5.238 ns, none | 5.188 ns, none | 2.850 ns, none |
| And5 | Pass | `Evaluate` | 235.795 ns, none | 213.856 ns, none | 269.142 ns, none | 201.855 ns, none |
| And5 | Pass | `EvaluateAsync` | 603.761 ns, 312 B | 580.445 ns, 304 B | 479.170 ns, 304 B | 467.886 ns, 304 B |
| And5 | FailLast | hand-written | 17.996 ns, 56 B | 16.644 ns, 56 B | 16.645 ns, 56 B | 11.912 ns, 56 B |
| And5 | FailLast | `Evaluate` | 230.833 ns, 56 B | 216.887 ns, 56 B | 184.847 ns, 56 B | 196.711 ns, 56 B |
| And5 | FailLast | `EvaluateAsync` | 632.876 ns, 368 B | 575.495 ns, 360 B | 508.286 ns, 360 B | 476.290 ns, 360 B |
| Or5 | Pass | hand-written | 5.398 ns, none | 5.237 ns, none | 5.641 ns, none | 2.885 ns, none |
| Or5 | Pass | `Evaluate` | 404.980 ns, 408 B | 364.299 ns, 408 B | 316.453 ns, 408 B | 287.202 ns, 408 B |
| Or5 | Pass | `EvaluateAsync` | 789.588 ns, 720 B | 735.091 ns, 712 B | 624.226 ns, 712 B | 593.441 ns, 712 B |
| Or5 | FailLast | hand-written | 6.064 ns, none | 5.701 ns, none | 5.589 ns, none | 3.126 ns, none |
| Or5 | FailLast | `Evaluate` | 510.809 ns, 928 B | 472.594 ns, 928 B | 398.888 ns, 928 B | 377.785 ns, 928 B |
| Or5 | FailLast | `EvaluateAsync` | 940.984 ns, 1240 B | 842.448 ns, 1232 B | 721.615 ns, 1232 B | 654.554 ns, 1232 B |

`EvaluateAsync` on a tree of sync rules costs more than `Evaluate` on every shape and TFM. On Linear5 it allocates nothing beyond the result; on And5 and Or5 it allocates 304 B to 1240 B per call.

`MixedAsyncTreeComparison`, a five-node `And()` whose last leaf awaits `Task.Yield()`. Baseline is the hand-written async method.

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| hand-written async | 1.012 μs, 487 B | 990.7 ns, 486 B | 970.7 ns, 487 B | 3.377 μs, 485 B |
| `await EvaluateAsync` | 1.859 μs, 992 B | 1,686.4 ns, 983 B | 1,565.4 ns, 983 B | 3.578 μs, 982 B |
| `Evaluate` (sync over async) | 1.200 μs, 600 B | 1,128.0 ns, 600 B | 1,053.8 ns, 600 B | 3.520 μs, 592 B |

The sync-over-async row is faster than `await EvaluateAsync` on every TFM, but it blocks a thread and can deadlock under a `SynchronizationContext` (see [Semantic differences](#semantic-differences)). On net11.0 every row, the hand-written one included, is 3.377 μs to 3.578 μs, against 970.7 ns to 1,565.4 ns on net10.0. The hand-written row moves as much as the CSE rows, which points at the runtime's `Task.Yield` path.

### Conditional and Linear build

`ConditionalComparison` (baseline hand-written) and `LinearBuildComparison` (baseline prebuilt chain):

| Class | Param | Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| Conditional | Pass | hand-written | 4.484 ns, none | 4.635 ns, none | 4.607 ns, none | 2.256 ns, none |
| Conditional | Pass | `RuleEngine.If` | 177.246 ns, 40 B | 152.083 ns, 40 B | 141.324 ns, 40 B | 113.496 ns, 40 B |
| Conditional | FailLast | hand-written | 17.267 ns, 56 B | 16.835 ns, 56 B | 16.563 ns, 56 B | 11.396 ns, 56 B |
| Conditional | FailLast | `RuleEngine.If` | 191.622 ns, 96 B | 166.029 ns, 96 B | 129.871 ns, 96 B | 125.511 ns, 96 B |
| LinearBuild | Count=5 | prebuilt | 137.046 ns, none | 125.812 ns, none | 111.082 ns, none | 101.984 ns, none |
| LinearBuild | Count=5 | `Linear(Func[], ctx)` rebuilt | 302.857 ns, 360 B | 258.934 ns, 360 B | 239.537 ns, 360 B | 196.179 ns, 360 B |
| LinearBuild | Count=5 | `IRule[].Linear()` rebuilt | 203.861 ns, 128 B | 169.750 ns, 128 B | 134.668 ns, 128 B | 138.419 ns, 128 B |
| LinearBuild | Count=5 | hand-written | 5.014 ns, none | 5.281 ns, none | 4.970 ns, none | 2.834 ns, none |
| LinearBuild | Count=10 | prebuilt | 290.608 ns, none | 244.807 ns, none | 172.648 ns, none | 177.017 ns, none |
| LinearBuild | Count=10 | `Linear(Func[], ctx)` rebuilt | 612.153 ns, 680 B | 522.430 ns, 680 B | 435.532 ns, 680 B | 391.407 ns, 680 B |
| LinearBuild | Count=10 | `IRule[].Linear()` rebuilt | 412.810 ns, 288 B | 363.571 ns, 288 B | 296.987 ns, 288 B | 282.208 ns, 288 B |
| LinearBuild | Count=10 | hand-written | 13.220 ns, none | 13.716 ns, none | 8.443 ns, none | 6.633 ns, none |

Rebuilding a chain with `RuleEngine.Linear(Func[], ctx)` per call has Ratio 1.92 to 2.52 against the prebuilt chain. Build the chain once and keep it.

### Short-circuit

`ShortCircuitComparison`, ten rules where the first fails (Or: all ten fail). Baseline is the hand-written early return.

| Row | Compare with | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| hand-written early return | | 15.79 ns, 56 B | 15.70 ns, 56 B | 14.355 ns, 56 B | 11.945 ns, 56 B |
| `Linear()` | early return | 38.43 ns, 56 B | 35.19 ns, 56 B | 16.269 ns, 56 B | 14.404 ns, 56 B |
| non-generic `And()` | early return | 80.23 ns, 56 B | 69.64 ns, 56 B | 44.680 ns, 56 B | 60.193 ns, 56 B |
| hand-written, all ten rules | | 52.03 ns, 144 B | 51.13 ns, 144 B | 120.999 ns, 144 B | 112.882 ns, 144 B |
| `Result<int>` `And()` (evaluates all ten) | all ten rules | 1,192.10 ns, 2160 B | 1,103.53 ns, 2160 B | 956.349 ns, 2160 B | 952.463 ns, 2064 B |
| hand-written Or, all fail | | 13.72 ns, none | 12.08 ns, none | 6.887 ns, none | 5.482 ns, none |
| `Or()`, all fail | hand-written Or | 1,003.16 ns, 1904 B | 895.12 ns, 1904 B | 739.778 ns, 1904 B | 729.580 ns, 1904 B |

`Linear()` is close to the hand-written early return on net10.0 and net11.0 (Ratio 1.13 and 1.21). The hand-written all-rules row more than doubles from net9.0 to net10.0 (51.13 ns to 120.999 ns), so the generic And's Ratio against the early-return baseline (75.54, 70.30, 66.63, 79.75) moves with that baseline too. The non-generic `And()` is slower on net11.0 (60.193 ns) than on net10.0 (44.680 ns).

### Reference engines (reference only)

`ReferenceEngineComparison`, the same five rules. Baseline is CSE prebuilt `Linear()`. NRules and RulesEngine are a different model (see [Semantic differences](#semantic-differences)); these rows are not a ranking.

| Input | Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| Pass | CSE `Linear()` | 145.414 ns, none | 130.086 ns, none | 87.259 ns, none | 87.452 ns, none |
| Pass | hand-written | 4.593 ns, none | 4.461 ns, none | 4.596 ns, none | 2.285 ns, none |
| Pass | NRules (reference) | 3,739.372 ns, 10984 B | 3,791.449 ns, 10984 B | 3,515.930 ns, 10872 B | 3,452.054 ns, 10872 B |
| Pass | RulesEngine (reference) | 2,221.310 ns, 4296 B | 2,050.586 ns, 4296 B | 1,610.556 ns, 4240 B | 1,525.928 ns, 4240 B |
| FailLast | CSE `Linear()` | 165.604 ns, 56 B | 135.163 ns, 56 B | 103.876 ns, 56 B | 97.123 ns, 56 B |
| FailLast | hand-written | 17.210 ns, 56 B | 16.641 ns, 56 B | 16.490 ns, 56 B | 11.465 ns, 56 B |
| FailLast | NRules (reference) | 5,538.253 ns, 14016 B | 5,468.200 ns, 14016 B | 5,003.017 ns, 13864 B | 4,865.466 ns, 13864 B |
| FailLast | RulesEngine (reference) | 2,300.954 ns, 4416 B | 2,249.812 ns, 4416 B | 1,727.368 ns, 4360 B | 1,676.275 ns, 4360 B |

### Where CSE loses

Every CSE row is slower than the hand-written code that does the same work, on every TFM. The gaps that stand out:

- **`Or()`.** Or5 Pass on net10.0: `Evaluate` 316.453 ns and 408 B against hand-written 5.641 ns and none. `ShortCircuitComparison` Or with all ten failing, net10.0: 739.778 ns and 1904 B against 6.887 ns and none. The hand-written Or returns a prebuilt error array, so part of this gap is the per-call error list CSE builds.
- **Generic `Result<T>` `And()`.** Against a hand-written method that also checks all ten rules, net10.0: 956.349 ns and 2160 B against 120.999 ns and 144 B; net8.0: 1,192.10 ns and 2160 B against 52.03 ns and 144 B.
- **`EvaluateAsync` on sync trees.** And5 Pass allocates 304 B on net9.0 to net11.0 (312 B on net8.0), and Or5 FailLast 1232 B (1240 B on net8.0), where `Evaluate` on And5 Pass allocates nothing.
- **`await EvaluateAsync` on a mixed tree** is slower than the sync-over-async `Evaluate` row on every TFM (net10.0: 1,565.4 ns and 983 B against 1,053.8 ns and 600 B).
- **Conditional.** `RuleEngine.If` builds its node per call: 141.324 ns and 40 B against hand-written 4.607 ns and none on Pass, net10.0.

---

## Caveats

- **The reference columns are not a ranking.** NRules and RulesEngine are built for rule sets that change at run time or chain inferences; a five-rule check on one object is a small case for them. The rows show what that model costs on a small, fixed rule set.
- **Known CSE costs, with the rows that measure them:**
  - `CSharpEssentials.Rules/Engines/RuleEngineAsync.cs:116` and `:140`: `AndAsync` and `OrAsync` allocate a `List<Result>` per composite node, even when every child completes synchronously. The Readme says a tree of sync rules completes `EvaluateAsync` "without allocating a task". No task is allocated, but the And5 and Or5 rows of `SyncTreeComparison` show 304 B to 1240 B per call; only Linear5 allocates nothing.
  - `CSharpEssentials.Rules/Engines/Linear.cs:25-64`: the static `Linear(Func[], ctx)` overloads rebuild the adapter chain with LINQ on every call. `LinearBuildComparison` Count=5 on net10.0: 239.537 ns and 360 B rebuilt, 111.082 ns and none prebuilt.
  - `CSharpEssentials.Results/ResultFactory.cs:47` and `:69`: `Result.And` and `Result.Or` over `IEnumerable<Result>` go through `Cast<IResultBase>()`, which boxes each `Result`. The sync Or5 rows allocate 408 B (Pass) and 928 B (FailLast) on every TFM; the sync And5 Pass row allocates nothing, so the boxing does not show there.
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
