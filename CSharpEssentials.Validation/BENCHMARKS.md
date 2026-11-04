# CSharpEssentials.Validation: Performance benchmarks (draft)

This page compares CSharpEssentials.Validation (CSE) with FluentValidation 12.2.0, the DataAnnotations validator in the BCL and Microsoft.Extensions.Validation 10.0.14. It covers the validation path on valid and invalid input, the enum validators, the `For` overloads, validator construction and the DI assembly scan. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11.

**Status: draft.** The suite builds, and its parity self-check passes on every TFM. No full default-job run has been done with these pins, so this page has no numbers yet (see [Results](#results)).

The competitors are pinned exactly in `Directory.Packages.props`. Each pin is the latest stable release on NuGet as of 2026-11-03:

| Package | Version | License (nuspec) | Asset the suite loads |
|---|---|---|---|
| FluentValidation | 12.2.0 | Apache-2.0 | `lib/net8.0` on every TFM |
| FluentValidation.DependencyInjectionExtensions | 12.2.0 | Apache-2.0 | `lib/net8.0`, assembly scan rows only |
| Microsoft.Extensions.Validation | 10.0.14 | MIT | `lib/net10.0` (the only asset) on net10.0 and net11.0 |
| System.ComponentModel.DataAnnotations | in-box | MIT (runtime) | the shared framework of each TFM |

Microsoft.Extensions.Validation ships `net10.0` only, and every entry point is marked `[Experimental("ASP0029")]`. Its rows exist on net10.0 and net11.0, and the project suppresses ASP0029 on those two TFMs only. Microsoft.Extensions.Validation 10.0.14 needs Microsoft.Extensions.DependencyInjection.Abstractions and Options 10.0.14, so restore fails with NU1109 against the central 9.0.4 pins while transitive pinning is on. Transitive pinning is therefore off for this project, as it is for the DI, mediator, endpoints, Http, AspNetCore and EntityFrameworkCore suites.

What each TFM runs:

| TFM | CSE.Validation build | Rows |
|---|---|---|
| net8.0 | netstandard2.1 (the package has no net8.0 target) | CSE, FluentValidation, DataAnnotations |
| net9.0 | net9.0 | CSE, FluentValidation, DataAnnotations |
| net10.0 | net10.0 | all four |
| net11.0 | net11.0 | all four |

---

## What was measured

The code is in `benchmarks/CSharpEssentials.Validation.Benchmarks/`, in 31 classes.
- Every class has `[MemoryDiagnoser]`.
- Each class checks parity in `[GlobalSetup]`, and `--self-check` runs the same check for every parameter value without BenchmarkDotNet. A mismatch fails the run.
- The parity key is `ok`, or `fail:` followed by the sorted, distinct failing property paths. CSE paths come from the error code up to its last `.` (`Items[0].Sku.NotEmpty` gives `Items[0].Sku`), FluentValidation paths from `PropertyName`, DataAnnotations paths from `MemberNames`, and Microsoft.Extensions.Validation paths from the keys of `ValidationErrors`. The error count per property is not in the key (see [Semantic differences](#semantic-differences)).
- An invalid input must give `fail:` for every library. The new classes also compare each library with a hard-coded expected key.
- In the 8 comparison classes, every competitor and BCL row has a comment above it that links to the documentation it follows, and each CSE row links to its Readme section. The 23 earlier classes predate this and carry no links.
- CSE is called as `validator.ValidateAsync(x)` and consumed synchronously without `AsTask` when the `ValueTask` has completed, which matches the Readme's sync call site.

### Comparison classes

| # | Class | Measured unit | Rows | Label |
|---|---|---|---|---|
| 1 | `BaselineSimpleComparison` | `SignupRequest` (Name, Email, Age, Tags), `Valid` = true or false | CSE (baseline), FluentValidation, DataAnnotations `TryValidateObject(validateAllProperties: true)`, Microsoft.Extensions.Validation (net10+) | like-for-like on outcome; email checks differ |
| 2 | `BaselineWideComparison` | `WideRequest`, 12 required strings of at most 100 characters, `Valid` = true or false | same four | like-for-like |
| 3 | `ForOverloadComparison` | `WideRequest`, `Valid` = true or false | `For(() => m.Fn)` (baseline, the Readme default), `For(m.Fn, nameof(m.Fn))` (the Readme's hot-path idiom), inline `Validator.ValidateAsync` with a static lambda | CSE only; the error codes must match exactly |
| 4 | `EnumDefinedComparison` | one enum property, `Metadata` = Plain or Generated, `Valid` = true or false | CSE `IsDefinedEnum` (baseline), FluentValidation `IsInEnum`, DataAnnotations `[EnumDataType]`, hand-written `Enum.IsDefined` | like-for-like on outcome; error text differs |
| 5 | `EnumFlagsComparison` | one `[Flags]` property, `Valid` = true or false | CSE `HasOnlyDefinedFlags` (baseline), FluentValidation `IsInEnum`, DataAnnotations `[EnumDataType]` | like-for-like on outcome |
| 6 | `EnumOneOfComparison` | one enum property, `Valid` = true or false | CSE `IsOneOf` (baseline), FluentValidation `Must(Array.IndexOf(...) >= 0)` | different work on the invalid path |
| 7 | `AssemblyScanComparison` | `new ServiceCollection()` plus a scan of the benchmark assembly, warm | CSE `AddValidatorsFromAssembly` (baseline), FluentValidation `AddValidatorsFromAssembly` | descriptor counts differ by design |
| 8 | `AssemblyScanColdComparison` | the same scan as the first call in a fresh process (`RunStrategy.ColdStart`, 15 launches) | same two | Allocated is warm (see the class doc) |

The DataAnnotations rows return the `List<ValidationResult>` they fill. The Microsoft.Extensions.Validation rows resolve `ValidationOptions` and the generated `IValidatableInfo` once in setup (`AddValidation()`, then `TryGetValidatableTypeInfo`), and create a `ValidateContext` with a new `ValidationContext` per call before awaiting `ValidateAsync`. Microsoft.Extensions.Validation documents no standalone (non-ASP.NET) idiom, so these rows follow the API reference.

The enum rows of `EnumDefinedComparison` return whether the input is valid; the setup checks the full parity key of the same validator instances.

### Earlier classes (FluentValidation head-to-head)

The 23 classes from the earlier suite are kept as they were, now running on FluentValidation 12.2.0. Each has one CSE and one FluentValidation row per input. Their setups gained the parity check; nothing else changed. They predate the house style for benchmark code (several public types per file, section comments) and return `object`, so each CSE row also boxes its `Result<T>`; the new classes return the result type directly.

| Class | Inputs | Parity key on the invalid input | Label |
|---|---|---|---|
| `SimpleValidationBenchmarks` | valid, invalid user | `fail:Age,Email,Name,Tags` | invalid row: semantic difference, FluentValidation runs an extra `Must` on `Tags` (5 vs 4 errors) |
| `StringValidationBenchmarks` | valid user | | |
| `ComparableValidationBenchmarks` | valid user | | |
| `CollectionValidationBenchmarks` | valid user | | |
| `NestedValidationBenchmarks` | valid order | | |
| `CollectionItemValidationBenchmarks` | valid order | | |
| `ConditionalValidationBenchmarks` | business and personal order, both valid | | |
| `CustomPredicateBenchmarks` | valid user, reserved name | `fail:Name` | |
| `AsyncPredicateBenchmarks` | valid user (`MustAsync` with `Task.Yield`) | | |
| `CascadeContinueBenchmarks` | weak password | `fail:Password` | |
| `ComplexValidationBenchmarks` | valid, invalid order | `fail:CompanyName,CustomerAge,CustomerEmail,CustomerName,Items[0].Price,Items[0].Quantity,Items[0].Sku,Password,ShippingAddress.City,ShippingAddress.ZipCode,Tags` | invalid row: semantic difference, FluentValidation runs an extra `Must` on `Tags` (14 vs 13 errors) |
| `StringLengthRangeBenchmarks` | valid, invalid user | `fail:Name` | |
| `StringContentBenchmarks` | valid user | | |
| `ExclusiveBetweenBenchmarks` | valid user | | |
| `NullableValidationBenchmarks` | valid, invalid product | `fail:Price,Score` | |
| `StringEqualityBenchmarks` | valid product | | |
| `LargeCollectionBenchmarks` | order with 50 valid items | | |
| `WideModelBenchmarks` | valid, invalid wide model | `fail:F1,...,F12` | |
| `DeepNestedBenchmarks` | valid three-level order | | |
| `CascadeStopBenchmarks` | weak password | `fail:Password` | |
| `ValidatorConstructionBenchmarks` | `new` of each validator | no result; both must be non-null | |
| `MultipleRegexBenchmarks` | valid, weak password | `fail:Password` | |
| `PrecompiledRegexBenchmarks` | valid password | | |

Every one of the 23 classes agrees on the key on every TFM, so none needed a per-library expected key.

### Inputs

- `SignupRequest`: valid is `Name = "John Doe"`, `Email = "john.doe@example.com"`, `Age = 30`, `Tags = ["customer"]`. Invalid is `Name = ""`, `Email = "not-an-email"`, `Age = -1`, `Tags = []`. Each property fails exactly one rule in every library, so the error counts match too (4 each).
- `WideRequest`: valid is `"Value n"` in `Fn`; invalid is `""` in all twelve (12 errors in every library).
- Enums: `ShipmentStatus` (plain), `ShipmentChannel` (`[StringEnum]`, generated metadata), `ShipmentPermissions` (`[Flags]`: Read, Write, Delete). Each scenario has its own model with one property, so DataAnnotations checks one property, the same work as the other rows. Invalid values are `(ShipmentStatus)42`, `(ShipmentChannel)42`, `(ShipmentPermissions)8` and, for `IsOneOf`, `Delivered` against the allowed `Pending, Shipped`.
- The assembly scan reads `typeof(SignupRequest).Assembly`, which holds every CSE and FluentValidation validator of the suite.

---

## Semantic differences

- **Rule-level cascade.** FluentValidation defaults to Continue (every validator on a property runs); CSE defaults to Stop. Per-property error counts can differ, so parity compares the failing paths only. Two earlier classes differ in count on their invalid input: `SimpleValidationBenchmarks` (CSE 4 errors, FluentValidation 5) and `ComplexValidationBenchmarks` (CSE 13, FluentValidation 14). In both, FluentValidation's `Tags` rule runs `NotEmpty` and a `Must` count check on the empty list. FluentValidation does more work on those two rows.
- **DataAnnotations.** A failing `[Required]` skips the property's other attributes. Without `validateAllProperties: true` only `[Required]` runs, so the suite always passes it. It does not recurse into nested objects or collections, it goes through `TypeDescriptor` by reflection, and `TryValidateObject` is `[RequiresUnreferencedCode]`.
- **Microsoft.Extensions.Validation.** It runs on net10.0 and later only and is experimental (ASP0029). Validation is async only (`IValidatableInfo.ValidateAsync` returns `Task`). The type info is source-generated from `[ValidatableType]`, it recurses into nested types, and the rules are DataAnnotations attributes.
- **Email.** CSE checks the address with a regex; FluentValidation (default mode), DataAnnotations and Microsoft.Extensions.Validation check for an `@`. The invalid input has no `@`, so all four reject it, but the work differs.
- **Enums.** CSE's error text lists the allowed values. FluentValidation has no `IsOneOf`, so its row is a `Must` predicate with the generic message. The CSE `Generated` row reads the `[StringEnum]` metadata; the competitors treat that enum as a plain enum (reflection or `Enum.IsDefined`).
- **Construction.** FluentValidation builds its rule tree in the constructor from expression trees and caches it per instance. CSE runs `Configure` on every call and does no rule-building work in its constructor. `ValidatorConstructionBenchmarks` shows one side of that trade; the valid-path rows show the other.
- **Assembly scan.** FluentValidation registers each validator under its interface and its concrete type; CSE adds one descriptor per validator through `TryAddEnumerable`. The setup checks that each library registered every one of its own validators.

---

## Feature matrix

This table comes from reading CSE source and the competitors' source and docs. None of it was measured.

| Feature | CSE | FluentValidation 12.2.0 | DataAnnotations (BCL) | Microsoft.Extensions.Validation 10.0.14 |
|---|---|---|---|---|
| Definition style | code in `Configure` | `AbstractValidator` constructor | attributes | attributes plus the `[ValidatableType]` generator |
| Reflection on the validate path | No | Expression trees compiled at construction | Yes | No (generated) |
| NativeAOT-safe validate path | Yes (the assembly scan excluded; `AddValidator` IL2087 open) | Not annotated | No (`[RequiresUnreferencedCode]`) | Designed for it, experimental |
| Result type | `Result<T>` | `ValidationResult` | `List<ValidationResult>` | `Dictionary<string, string[]>` |
| Sync API | Yes (sync-completing `ValueTask`) | Yes | Yes | No |
| Async rules | Yes | Yes | No | No (sync attributes and `IValidatableObject`; only the `ValidateAsync` entry point is async) |
| Default cascade | Stop | Continue | `[Required]` short-circuits the property | same as DataAnnotations |
| Nested and collection validation | `SetValidatorAsync` / `ForEach` | `SetValidator` / `RuleForEach` | No | Yes |
| Enum validators | `IsDefinedEnum`, `HasOnlyDefinedFlags`, `IsOneOf` | `IsInEnum` | `[EnumDataType]` | `[EnumDataType]` |
| DI assembly scan | Yes (reflection) | Yes (separate package) | n/a | n/a |
| Target frameworks | netstandard2.1, net9.0, net10.0, net11.0 | net8.0 | in-box | net10.0 |
| License | MIT | Apache-2.0 | MIT | MIT |

---

## Results

**Pending the full default-job run on net8.0-net11.0.** The reports will be committed under `benchmarks/results/validation/{tfm}/results/`. Short-job (`--quick`) numbers are not published here.

### Superseded run

An earlier run of the 23 FluentValidation classes used FluentValidation 11.11.0 on a different machine, on net9.0, net10.0 and net11.0, with the job id `default`. Its reports are still under `benchmarks/results/{tfm}/results/`. They are kept for reference only: FluentValidation has moved to 12.2.0 since, and the new run uses BenchmarkDotNet's default job without a custom id. The post-merge run will replace them with `benchmarks/results/validation/{tfm}/results/` and delete the old directories.

---

## Caveats

- **Microsoft.Extensions.Validation is experimental** and documents no standalone idiom; its rows follow the API reference and may need changes when the API settles.
- **Both assembly scans use reflection.** CSE's Readme says to register validators with `AddValidator<T, TValidator>` under trimming or Native AOT. That method has an open IL2087 trim warning (`CSharpEssentials.Validation/Extensions/ValidatorServiceCollectionExtensions.cs:34-37`), so the matrix does not call the registration path AOT-clean yet.
- **No Native AOT sample.** FluentValidation compiles expression trees and DataAnnotations is `[RequiresUnreferencedCode]`, so only the CSE and Microsoft.Extensions.Validation rows could run under AOT.
- **Expected CSE costs, from reading the code** (not yet measured):
  - `CSharpEssentials.Validation/RuleContext.cs:62-76`: `For(() => ...)` parses the member expression text with `PropertyPath.Extract` on every call and allocates a closure. `ForOverloadComparison` isolates it.
  - `CSharpEssentials.Validation/Validators/EnumValidators.cs:201-204`: the plain-enum path converts through `Convert.ToInt64(object)`, which boxes. `EnumFlagsComparison` runs it.
- The CSE column is the working tree, which may not match the version published on NuGet.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project validation                   # all TFMs (net8-net11), default job
./run-benchmarks.sh --project validation --tfm net10.0     # one TFM
./run-benchmarks.sh --project validation --quick           # net10.0, Short job, one filter per class
dotnet run -c Release -f net10.0 --project CSharpEssentials.Validation.Benchmarks -- --self-check
```

Without `--tfm`, `--quick` runs on net10.0, because the Microsoft.Extensions.Validation rows exist only there and on net11.0. Short-job numbers cannot be compared with default-job numbers. Results are stored per framework under `benchmarks/results/validation/{tfm}/results/`.
