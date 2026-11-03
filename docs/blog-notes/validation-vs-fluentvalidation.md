# Blog notes: CSharpEssentials.Validation and Rules against FluentValidation, DataAnnotations, Microsoft.Extensions.Validation, NRules and RulesEngine (draft)

These are notes for a post, not the post; numbers pending `benchmarks/results/validation/{tfm}` and `benchmarks/results/rules/{tfm}`. They will come from `CSharpEssentials.Validation/BENCHMARKS.md` and `CSharpEssentials.Rules/BENCHMARKS.md` once the full default-job run is committed. There are no numbers yet.

The suites run on .NET 8, 9, 10 and 11 with BenchmarkDotNet 0.14.0. The libraries compared are FluentValidation 12.2.0 with FluentValidation.DependencyInjectionExtensions 12.2.0 (Apache-2.0), the DataAnnotations validator in the BCL, Microsoft.Extensions.Validation 10.0.14 (MIT, net10.0 only, experimental), and, as reference columns for the rule engine, NRules 1.0.4 (MIT) and RulesEngine 6.0.1 (MIT).

## Framing

- Working title: "What does a validation library cost per call, and where does it go?"
- FluentValidation is the default most readers know, so it is the main column. DataAnnotations is the in-box baseline. Microsoft.Extensions.Validation is the new generated option, and it is experimental; say so every time it appears.
- CSE and FluentValidation pay at different times. FluentValidation builds an expression-tree rule tree once per validator instance; CSE runs `Configure` on every call and does no rule-building work in its constructor. The post should show both sides: construction and the per-call valid path.
- The Rules half is a separate story. Its baseline is hand-written if/else, and NRules and RulesEngine are reference columns from a different model, not competitors.

## What each library provides

- **FluentValidation 12.2.0:** `AbstractValidator<T>` with `RuleFor` in the constructor, Continue cascade by default, `SetValidator`/`RuleForEach` for nesting, `IsInEnum` for enums, `Must` for anything custom, and a separate DI package for the assembly scan. Ships net8.0.
- **DataAnnotations (BCL):** attributes plus `Validator.TryValidateObject`. `[Required]` short-circuits the property. No nesting, reflection through `TypeDescriptor`, `[RequiresUnreferencedCode]`.
- **Microsoft.Extensions.Validation 10.0.14:** DataAnnotations attributes plus a source generator (`[ValidatableType]`), an async-only `ValidateAsync`, nesting, and errors as `Dictionary<string, string[]>`. net10.0 only; every entry point is `[Experimental("ASP0029")]`; no documented standalone idiom.
- **CSE Validation:** `Validator<T>.Configure` over the live model, `Result<T>` output, Stop cascade by default, `IsDefinedEnum`/`HasOnlyDefinedFlags`/`IsOneOf`, `For(() => ...)` and `For(value, name)`, inline `Validator.ValidateAsync`, and a reflection-based assembly scan.
- **CSE Rules:** `Func`, class or lambda rules returning `Result`, combined with `Linear`, `And`, `Or` and `If`, with `Evaluate` and `EvaluateAsync`.
- **NRules 1.0.4 (reference):** forward chaining over facts with a Rete network, rule classes in a DSL, sessions.
- **RulesEngine 6.0.1 (reference):** workflows of string expressions compiled with System.Linq.Dynamic.Core and FastExpressionCompiler, async-only.

## Feature matrix

| Feature | CSE | FluentValidation 12.2.0 | DataAnnotations | M.E.Validation 10.0.14 |
|---|---|---|---|---|
| Definition style | code in `Configure` | constructor | attributes | attributes plus generator |
| Reflection on the validate path | No | expression trees at construction | Yes | No (generated) |
| NativeAOT-safe validate path | Yes (scan excluded; `AddValidator` IL2087 open) | Not annotated | No | Designed for it, experimental |
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
- Every class checks parity in `[GlobalSetup]`, and `--self-check` runs the same checks on every TFM without BenchmarkDotNet. Validation compares pass or fail plus the sorted failing property paths; Rules compares pass or fail, the error count and the first error code. The new classes also compare every row with a hard-coded expected key, so a row that validates nothing fails the run.
- Error counts per property are not compared: FluentValidation's default cascade is Continue and CSE's is Stop. The two earlier classes where the counts differ (Simple and Complex, invalid input) are labelled.
- Every competitor row follows its documented usage and carries a link to that documentation. Rows that do different work are labelled, not presented as like-for-like.
- Microsoft.Extensions.Validation rows exist on net10.0 and net11.0 only. net8.0 runs CSE's netstandard2.1 builds.

## The numbers that matter

Pending the full default-job run. These are the rows to pull:

1. BaselineSimple and BaselineWide, valid and invalid: all four libraries on the same model.
2. ValidatorConstruction against the valid-path rows: when FluentValidation's prebuilt tree pays off.
3. ForOverload: `For(() => ...)` against `For(value, name)` on twelve properties.
4. EnumDefined Plain against Generated, and EnumFlags: the metadata path against the plain-enum path.
5. AssemblyScan warm and cold.
6. Rules SyncTree: `EvaluateAsync` against `Evaluate` on a tree of sync rules, with the Allocated column.
7. Rules LinearBuild: prebuilt against rebuilt per call.
8. Rules ShortCircuit: the generic And against the early-return baseline.
9. Rules ReferenceEngine: CSE and hand-written against NRules and RulesEngine, labelled reference only.

## Where CSE is expected to lose (state it, do not soften it)

These are candidates from reading the code. Nothing has been measured yet.

- **`For(() => ...)` parses the member expression on every call** and allocates a closure (`CSharpEssentials.Validation/RuleContext.cs:62-76`). FluentValidation resolves property names once, in the constructor. `ForOverloadComparison` shows how much `For(value, name)` saves.
- **The plain-enum path boxes** through `Convert.ToInt64(object)` (`CSharpEssentials.Validation/Validators/EnumValidators.cs:201-204`), on every `HasOnlyDefinedFlags` call for a plain `[Flags]` enum.
- **Per-call `Configure` against a prebuilt tree on valid input.** CSE rebuilds its rule chains each call; FluentValidation walks a tree it built once. Wide valid models are where this would show.
- **Invalid-path message cost on enums:** CSE formats the allowed values into every enum error.
- **Rules `EvaluateAsync` on sync trees** allocates a `List<Result>` per And and Or node (`CSharpEssentials.Rules/Engines/RuleEngineAsync.cs:116`, `:140`), against the Readme's "without allocating a task".
- **Rules `RuleEngine.Linear(Func[], ctx)`** rebuilds the chain with LINQ per call (`CSharpEssentials.Rules/Engines/Linear.cs:25-64`), and `Result.And`/`Or` box through `Cast<IResultBase>()` (`CSharpEssentials.Results/ResultFactory.cs:47`, `:69`).
- **The generic `Result<T>` And** evaluates every rule, so it loses to an early-return baseline by design.

## Where CSE may win or tie

Candidates only, to be confirmed by the run:
- Construction, since CSE builds nothing up front.
- Invalid paths with the Stop cascade, where FluentValidation runs extra validators per property.
- DataAnnotations rows, which go through reflection.
- The rules baselines against NRules and RulesEngine, which do far more per call (reference only).

## Caveats to keep in the post

- Rule-level cascade: FluentValidation Continue, CSE Stop. Parity compares failing paths, not counts.
- DataAnnotations needs `validateAllProperties: true` to run more than `[Required]`, does not recurse, and is reflection-based.
- Microsoft.Extensions.Validation is net10+ only, experimental (ASP0029), async-only and has no documented standalone idiom.
- Email: CSE uses a regex; the others check for an `@`. The inputs make all four agree, but the work differs.
- Enums: CSE's error text lists the allowed values; FluentValidation has no `IsOneOf`; the CSE Generated row uses `[StringEnum]` metadata.
- Assembly scan: FluentValidation registers each validator twice (interface and concrete type).
- Rules: the generic And aggregates; NRules mutates a collector; RulesEngine always runs every rule.
- The earlier 23 classes were last run on FluentValidation 11.11.0 on a different machine; those numbers are superseded and not quoted.
- The CSE column is a working tree, not a NuGet release.

## Possible angles

- "Pay once or pay per call": FluentValidation's constructor against CSE's per-call `Configure`.
- "The cost of a lambda": `For(() => x)` against `For(x, name)`.
- "Generated validation in the BCL": Microsoft.Extensions.Validation against DataAnnotations on the same attributes.
- "Async that is not async": `EvaluateAsync` on a sync rule tree.
- "Do you need a rule engine?": hand-written if/else and CSE against NRules and RulesEngine on five rules.
