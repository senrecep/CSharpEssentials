# Blog notes: CSharpEssentials.Validation and Rules against FluentValidation, DataAnnotations, Microsoft.Extensions.Validation, NRules and RulesEngine

These are notes for a post, not the post. The numbers come from the full default-job runs under `benchmarks/results/validation/{tfm}` and `benchmarks/results/rules/{tfm}`, and the full tables are in `CSharpEssentials.Validation/BENCHMARKS.md` and `CSharpEssentials.Rules/BENCHMARKS.md`.

The suites run on .NET 8, 9, 10 and 11 with BenchmarkDotNet 0.14.0. The libraries compared are FluentValidation 12.2.0 with FluentValidation.DependencyInjectionExtensions 12.2.0 (Apache-2.0), the DataAnnotations validator in the BCL, Microsoft.Extensions.Validation 10.0.14 (MIT, net10.0 only, experimental), and, as reference columns for the rule engine, NRules 1.0.4 (MIT) and RulesEngine 6.0.1 (MIT).

## Framing

- Working title: "What does a validation library cost per call, and where does it go?"
- FluentValidation is the default most readers know, so it is the main column. DataAnnotations is the in-box baseline. Microsoft.Extensions.Validation is the new generated option, and it is experimental; say so every time it appears.
- CSE and FluentValidation pay at different times. FluentValidation builds an expression-tree rule tree once per validator instance; CSE runs `Configure` on every call and does no rule-building work in its constructor. The post should show both sides: construction and the per-call valid path.
- The Rules half is a separate story. Its baseline is hand-written if/else, and NRules and RulesEngine are reference columns from a different model, not competitors.

## What each library provides

- FluentValidation 12.2.0: `AbstractValidator<T>` with `RuleFor` in the constructor, Continue cascade by default, `SetValidator`/`RuleForEach` for nesting, `IsInEnum` for enums, `Must` for anything custom, and a separate DI package for the assembly scan. Ships net8.0.
- DataAnnotations (BCL): attributes plus `Validator.TryValidateObject`. `[Required]` short-circuits the property. No nesting, reflection through `TypeDescriptor`, `[RequiresUnreferencedCode]`.
- Microsoft.Extensions.Validation 10.0.14: DataAnnotations attributes plus a source generator (`[ValidatableType]`), an async-only `ValidateAsync`, nesting, and errors as `Dictionary<string, string[]>`. net10.0 only; every entry point is `[Experimental("ASP0029")]`; no documented standalone idiom.
- CSE Validation: `Validator<T>.Configure` over the live model, `Result<T>` output, Stop cascade by default, `IsDefinedEnum`/`HasOnlyDefinedFlags`/`IsOneOf`, `For(() => ...)` and `For(value, name)`, inline `Validator.ValidateAsync`, and a reflection-based assembly scan.
- CSE Rules: `Func`, class or lambda rules returning `Result`, combined with `Linear`, `And`, `Or` and `If`, with `Evaluate` and `EvaluateAsync`.
- NRules 1.0.4 (reference): forward chaining over facts with a Rete network, rule classes in a DSL, sessions.
- RulesEngine 6.0.1 (reference): workflows of string expressions compiled with System.Linq.Dynamic.Core and FastExpressionCompiler, async-only.

## Feature matrix

| Feature | CSE | FluentValidation 12.2.0 | DataAnnotations | M.E.Validation 10.0.14 |
|---|---|---|---|---|
| Definition style | code in `Configure` | constructor | attributes | attributes plus generator |
| Reflection on the validate path | No | expression trees at construction | Yes | No (generated) |
| NativeAOT-safe validate path | Yes (scan excluded) | Not annotated | No | Designed for it, experimental |
| Result type | `Result<T>` | `ValidationResult` | `List<ValidationResult>` | `Dictionary<string, string[]>` |
| Sync API | Yes | Yes | Yes | No |
| Async rules | Yes | Yes | No | No (sync attributes and `IValidatableObject`; only the `ValidateAsync` entry point is async) |
| Default cascade | Stop | Continue | `[Required]` short-circuit | same as DataAnnotations |
| Nested and collections | Yes | Yes | No | Yes |
| Enum validators | `IsDefinedEnum`, `HasOnlyDefinedFlags`, `IsOneOf` | `IsInEnum` | `[EnumDataType]` | `[EnumDataType]` |
| DI assembly scan | Yes (reflection) | Yes (separate package) | n/a | n/a |
| Target frameworks | netstandard2.1, net9.0-net11.0 | net8.0 | in-box | net10.0 |

| Rules feature | CSE | Hand-written | NRules 1.0.4 (ref) | RulesEngine 6.0.1 (ref) |
|---|---|---|---|---|
| Model | composable `Result` rules | if/else | Rete forward chaining | string-expression workflows |
| Short-circuit | Linear and non-generic And; `Result<T>` And no | yes | no | no |
| Async rules | yes | yes | no | async API only |
| Runtime code generation | none | none | expression compile | Dynamic LINQ plus FastExpressionCompiler |

## Methodology

- Two BenchmarkDotNet projects: `benchmarks/CSharpEssentials.Validation.Benchmarks/` (31 classes, 23 of them the earlier FluentValidation head-to-head) and `benchmarks/CSharpEssentials.Rules.Comparison.Benchmarks/` (6 classes). Every class has `[MemoryDiagnoser]`.
- Every class checks parity in `[GlobalSetup]`, and `--self-check` runs the same checks on every TFM without BenchmarkDotNet. Validation compares pass or fail plus the sorted failing property paths; Rules compares pass or fail, the error count and the first error code. The comparison classes also compare every row with a hard-coded expected key, so a row that validates nothing fails the run.
- Error counts per property are not compared: FluentValidation's default cascade is Continue and CSE's is Stop. The two earlier classes where the counts differ (Simple and Complex, invalid input) are labelled.
- Every competitor row follows its documented usage and carries a link to that documentation. Rows that do different work are labelled, not presented as like-for-like.
- Microsoft.Extensions.Validation rows exist on net10.0 and net11.0 only. net8.0 runs CSE's netstandard2.1 builds.

## How these numbers were produced

- BenchmarkDotNet v0.14.0 default job, full runs, serially on one Linux machine (CPU in each report header). Validation: SDK 11.0.100-rc.1, runtimes .NET 8.0.30, 9.0.19, 10.0.11 and 11.0.0; `AssemblyScanColdComparison` uses its own `cold` job (ColdStart, 15 launches). Rules: every report is from SDK 11.0.100-rc.3 with .NET 8.0.33, 9.0.22, 10.0.14 and 11.0.0, all six classes in one run on 2026-11-09 after #262.
- 1-minute load average: 1.54 to 1.61 for the Validation run (2.18 to 4.87 during the reruns of `ValidatorConstructionBenchmarks` net8.0 and net10.0 and `AssemblyScanColdComparison` net8.0). Not recorded for the Rules run.
- Noisy rows (CV above 5% after two reruns): `ValidatorConstructionBenchmarks` CSE on net8.0 (14.32 ns) and net10.0 (9.444 ns).
- Results: `benchmarks/results/validation/{tfm}/results/` and `benchmarks/results/rules/{tfm}/results/`. Full tables are in `CSharpEssentials.Validation/BENCHMARKS.md` and `CSharpEssentials.Rules/BENCHMARKS.md`.

## The numbers that matter

Mean and allocated, copied from the reports. net10.0 unless a TFM is named.

1. BaselineSimple (four properties). Valid: CSE 311.0 ns, 560 B; FluentValidation 364.9 ns, 704 B; DataAnnotations 1,346.7 ns, 2288 B; Microsoft.Extensions.Validation 858.6 ns, 744 B. Invalid: CSE 553.6 ns, 1472 B; FluentValidation 3,638.9 ns, 7136 B; DataAnnotations 2,374.5 ns, 3272 B; Microsoft.Extensions.Validation 1,767.3 ns, 2056 B.
2. BaselineWide (twelve strings). Valid: CSE 655.6 ns, 1784 B; FluentValidation 812.3 ns, 984 B; DataAnnotations 3,439.9 ns, 5760 B; Microsoft.Extensions.Validation 2,132.5 ns, 1312 B. Invalid: CSE 1,318.2 ns, 4632 B; FluentValidation 7,942.1 ns, 13952 B; DataAnnotations 5,973.8 ns, 8008 B; Microsoft.Extensions.Validation 3,902.2 ns, 3480 B.
3. ValidatorConstruction against the valid path. CSE `new` 9.444 ns, 24 B (noisy, CV above 5%); FluentValidation `new` 3,828.194 ns, 8968 B. Per call on valid input FluentValidation is 364.9 ns (Simple) and 812.3 ns (Wide) against CSE's 311.0 ns and 655.6 ns, so the prebuilt tree does not win back time per call in these models; it does win on allocation in the wide model.
4. ForOverload, valid: `For(() => m.Fn)` 664.1 ns, 1784 B; `For(m.Fn, nameof(m.Fn))` 203.6 ns, 608 B; inline `Validator.ValidateAsync` 694.3 ns, 1784 B.
5. EnumDefined, valid: Plain 72.593 ns, 136 B against Generated 50.615 ns, 136 B (FluentValidation 912.618 ns and 973.457 ns). EnumFlags valid: CSE 116.0 ns, 216 B; FluentValidation 1,407.2 ns, 1064 B; DataAnnotations 988.1 ns, 1040 B.
6. AssemblyScan. Warm: CSE 9.243 μs, 8.52 KB; FluentValidation 19.603 μs, 31.88 KB. Cold: CSE 5.346 ms, 67.38 KB; FluentValidation 5.695 ms, 68.13 KB.
7. Rules SyncTree, `EvaluateAsync` against `Evaluate` on sync trees: Linear5 Pass 315.547 ns against 71.667 ns, both no allocation; And5 Pass 372.233 ns against 47.302 ns, both no allocation; Or5 Pass 479.787 ns against 138.188 ns, both 224 B. Hand-written: 4.977 ns, 4.975 ns and 5.272 ns.
8. Rules LinearBuild, Count=5: prebuilt 71.929 ns, no allocation; `Linear(Func[], ctx)` 183.288 ns, 360 B; `IRule[].Linear()` 119.519 ns, 128 B; hand-written 5.200 ns.
9. Rules ShortCircuit: `Linear()` 16.193 ns against the early return's 14.149 ns, both 56 B. The generic And 429.854 ns, 56 B against a hand-written all-rules method at 117.368 ns, 144 B.
10. Rules ReferenceEngine (reference only), FailLast: CSE `Linear()` 73.195 ns, 56 B; hand-written 15.966 ns, 56 B; NRules 4,853.301 ns, 13864 B; RulesEngine 1,655.924 ns, 4360 B.

## Where CSE loses (state it, do not soften it)

- Wide valid allocation: `BaselineWideComparison` valid: CSE 1784 B against FluentValidation 984 B on net8.0 to net10.0, 1688 B against 984 B on net11.0. `WideModelBenchmarks` valid: 1816 B against 984 B (net8.0 to net10.0), 1720 B against 984 B (net11.0). The cause is `For(() => ...)` (`CSharpEssentials.Validation/RuleContext.cs:62-76`); `For(value, name)` brings the same model to 608 B.
- Microsoft.Extensions.Validation allocates less on the wide model: net10.0: 1312 B against CSE 1784 B (valid), 3480 B against 4632 B (invalid). net11.0: 1304 B against 1688 B, 3472 B against 4536 B. CSE is faster on time.
- `ConditionalValidationBenchmarks` business order on net10.0: CSE 273.4 ns, FluentValidation 255.4 ns. CSE is faster on the other three TFMs.
- Cold assembly scan allocation on net11.0: CSE 67.32 KB, FluentValidation 66.87 KB.
- The inline sync delegate avoids a `Task`, not allocation: it measures 1784 B on valid input (net8.0 to net10.0), the same as `For(() => ...)`.
- Rules `Or()`, net10.0: Or5 Pass 138.188 ns, 224 B against hand-written 5.272 ns; ten-rule Or with all failing 392.511 ns, 904 B against 6.910 ns.
- Rules generic `Result<T>` `And()`, net10.0: 429.854 ns, 56 B against 117.368 ns, 144 B for a hand-written method doing the same ten checks (net8.0: 531.32 ns against 49.83 ns). It allocates less but is slower.
- Rules `EvaluateAsync` on sync trees takes 2.49 to 8.46 times as long as `Evaluate` on the same tree. It allocates no task and no more than `Evaluate`, so the Readme's "without allocating a task" holds.
- Rules `Linear(Func[], ctx)` rebuilt per call, Count=5 on net10.0, is 183.288 ns and 360 B against 71.929 ns prebuilt (`CSharpEssentials.Rules/Engines/Linear.cs:25-64`).
- Rules against hand-written code: every CSE rules row is slower than the hand-written equivalent. In the classes with a hand-written baseline the Ratio runs from 1.04 (sync-over-async `Evaluate` on the mixed async tree, net10.0) to 171.48 (`EvaluateAsync` Or5 FailLast, net11.0).

Two expected losses did not show up. The plain-enum boxing in `HasOnlyDefinedFlags` (`CSharpEssentials.Validation/Validators/EnumValidators.cs:201-204`) still leaves CSE below both competitors, and the invalid-path enum message cost leaves CSE below them too (EnumDefined Plain invalid, net10.0: 330.136 ns against FluentValidation 1,869.646 ns and DataAnnotations 877.089 ns).

## Where CSE wins or ties

- Construction, net10.0: 9.444 ns and 24 B (noisy, CV above 5%) against FluentValidation's 3,828.194 ns and 8968 B.
- Invalid paths: BaselineSimple invalid has FluentValidation at Ratio 5.91 to 6.76 across the four TFMs; `ComplexValidationBenchmarks` invalid on net10.0 is 2.538 μs against 13.495 μs (FluentValidation runs one more check there).
- Valid paths: CSE is faster than FluentValidation on every valid row except `ConditionalValidationBenchmarks` business on net10.0, but by little on simple models (BaselineSimple Ratio 1.12 to 1.18).
- DataAnnotations: slower than CSE on every row, Ratio 3.80 to 7.69 on the baseline classes.

Reference only, not a ranking: in `ReferenceEngineComparison`, with CSE `Linear()` as the baseline (1.00), NRules has Ratio 32.11 to 76.37 and RulesEngine 17.41 to 25.65 across the four TFMs.

## Caveats to keep in the post

- Rule-level cascade: FluentValidation Continue, CSE Stop. Parity compares failing paths, not counts.
- DataAnnotations needs `validateAllProperties: true` to run more than `[Required]`, does not recurse, and is reflection-based.
- Microsoft.Extensions.Validation is net10+ only, experimental (ASP0029), async-only and has no documented standalone idiom.
- Email: CSE uses a regex; the others check for an `@`. The inputs make all four agree, but the work differs.
- Enums: CSE's error text lists the allowed values; FluentValidation has no `IsOneOf`; the CSE Generated row uses `[StringEnum]` metadata.
- Assembly scan: FluentValidation registers each validator twice (interface and concrete type).
- Rules: the generic And aggregates; NRules mutates a collector; RulesEngine always runs every rule.
- The CSE column is a working tree, not a NuGet release.

## Possible angles

- "Pay once or pay per call": FluentValidation's constructor against CSE's per-call `Configure`.
- "The cost of a lambda": `For(() => x)` against `For(x, name)`.
- "Generated validation in the BCL": Microsoft.Extensions.Validation against DataAnnotations on the same attributes.
- "Async that is not async": `EvaluateAsync` on a sync rule tree.
- "Do you need a rule engine?": hand-written if/else and CSE against NRules and RulesEngine on five rules.
