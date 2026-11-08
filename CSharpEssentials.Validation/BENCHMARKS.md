# CSharpEssentials.Validation: Performance benchmarks

CSharpEssentials.Validation (CSE) compared with FluentValidation 12.2.0, the DataAnnotations validator in the BCL and Microsoft.Extensions.Validation 10.0.14. Covered: the validation path on valid and invalid input, the enum validators, the `For` overloads, validator construction and the DI assembly scan. BenchmarkDotNet v0.14.0, .NET 8, 9, 10 and 11. Numbers are in [Results](#results).

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
- An invalid input must give `fail:` for every library. The comparison classes also compare each library with a hard-coded expected key.
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

23 older classes, on FluentValidation 12.2.0, with one CSE and one FluentValidation row per input and the same parity check. They predate the house style for benchmark code (several public types per file, section comments) and return `object`, so each CSE row also boxes its `Result<T>`; the comparison classes return the result type directly.

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

- Rule-level cascade: FluentValidation defaults to Continue (every validator on a property runs); CSE defaults to Stop. Per-property error counts can differ, so parity compares the failing paths only. Two earlier classes differ in count on their invalid input: `SimpleValidationBenchmarks` (CSE 4 errors, FluentValidation 5) and `ComplexValidationBenchmarks` (CSE 13, FluentValidation 14). In both, FluentValidation's `Tags` rule runs `NotEmpty` and a `Must` count check on the empty list. FluentValidation does more work on those two rows.
- DataAnnotations: a failing `[Required]` skips the property's other attributes. Without `validateAllProperties: true` only `[Required]` runs, so the suite always passes it. It does not recurse into nested objects or collections, it goes through `TypeDescriptor` by reflection, and `TryValidateObject` is `[RequiresUnreferencedCode]`.
- Microsoft.Extensions.Validation: runs on net10.0 and later only and is experimental (ASP0029). Validation is async only (`IValidatableInfo.ValidateAsync` returns `Task`). The type info is source-generated from `[ValidatableType]`, it recurses into nested types, and the rules are DataAnnotations attributes.
- Email: CSE checks the address with a regex; FluentValidation (default mode), DataAnnotations and Microsoft.Extensions.Validation check for an `@`. The invalid input has no `@`, so all four reject it, but the work differs.
- Enums: CSE's error text lists the allowed values. FluentValidation has no `IsOneOf`, so its row is a `Must` predicate with the generic message. The CSE `Generated` row reads the `[StringEnum]` metadata; the competitors treat that enum as a plain enum (reflection or `Enum.IsDefined`).
- Construction: FluentValidation builds its rule tree in the constructor from expression trees and caches it per instance. CSE runs `Configure` on every call and does no rule-building work in its constructor. `ValidatorConstructionBenchmarks` shows one side of that trade; the valid-path rows show the other.
- Assembly scan: FluentValidation registers each validator under its interface and its concrete type; CSE adds one descriptor per validator through `TryAddEnumerable`. The setup checks that each library registered every one of its own validators.

---

## Feature matrix

This table comes from reading CSE source and the competitors' source and docs. None of it was measured.

| Feature | CSE | FluentValidation 12.2.0 | DataAnnotations (BCL) | Microsoft.Extensions.Validation 10.0.14 |
|---|---|---|---|---|
| Definition style | code in `Configure` | `AbstractValidator` constructor | attributes | attributes plus the `[ValidatableType]` generator |
| Reflection on the validate path | No | Expression trees compiled at construction | Yes | No (generated) |
| NativeAOT-safe validate path | Yes (the assembly scan excluded) | Not annotated | No (`[RequiresUnreferencedCode]`) | Designed for it, experimental |
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

Full default-job runs on net8.0, net9.0, net10.0 and net11.0. Each cell is mean, then allocated, copied from the github report of that TFM. Ratios quoted in the text are the report's Ratio and Alloc Ratio columns, with CSE as the baseline (1.00). "n/a" means the row does not exist on that TFM. Reports: [net8.0](../benchmarks/results/validation/net8.0/results/), [net9.0](../benchmarks/results/validation/net9.0/results/), [net10.0](../benchmarks/results/validation/net10.0/results/), [net11.0](../benchmarks/results/validation/net11.0/results/).

### How these numbers were produced

- BenchmarkDotNet v0.14.0 default job (full runs, not `--quick`), run serially on one Linux machine. The CPU is in the header of each report. SDK 11.0.100-rc.1; runtimes .NET 8.0.30, 9.0.19, 10.0.11 and 11.0.0.
- `AssemblyScanColdComparison` uses its own `cold` job (`RunStrategy.ColdStart`, 15 launches, one iteration each).
- The 1-minute load average was 1.54 to 1.61 during the main run. Three reports were rerun because their CV was above 5%: `ValidatorConstructionBenchmarks` on net8.0 and net10.0 (twice) and `AssemblyScanColdComparison` on net8.0 (once). The load average during those reruns was 2.18 to 4.87.
- Rows still above 5% CV after two reruns are marked "noisy (CV above 5%)" where they appear: `ValidatorConstructionBenchmarks` `CSE-Ctor` on net8.0 (CV 8.2%) and net10.0 (CV 10.3%).
- No row in this suite is near 0 ns. The smallest rows are the hand-written enum checks at 1.271 ns to 1.591 ns, which are above timer resolution.
- Results path: `benchmarks/results/validation/{tfm}/results/` (`*-report-github.md` and `.csv`).

### Baseline comparisons: same model, four libraries

`BaselineSimpleComparison` (`SignupRequest`, four properties):

| Row | Valid | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| CSE | True | 372.2 ns, 624 B | 368.3 ns, 624 B | 311.0 ns, 560 B | 303.8 ns, 536 B |
| FluentValidation | True | 437.9 ns, 704 B | 410.8 ns, 704 B | 364.9 ns, 704 B | 347.0 ns, 704 B |
| DataAnnotations | True | 1,752.0 ns, 2632 B | 1,447.9 ns, 2328 B | 1,346.7 ns, 2288 B | 1,262.3 ns, 2256 B |
| Microsoft.Extensions.Validation (experimental) | True | n/a | n/a | 858.6 ns, 744 B | 802.7 ns, 696 B |
| CSE | False | 693.8 ns, 1584 B | 674.2 ns, 1536 B | 553.6 ns, 1472 B | 499.1 ns, 1448 B |
| FluentValidation | False | 4,318.2 ns, 7392 B | 3,982.8 ns, 7392 B | 3,638.9 ns, 7136 B | 3,372.4 ns, 7104 B |
| DataAnnotations | False | 3,013.0 ns, 3592 B | 2,560.4 ns, 3312 B | 2,374.5 ns, 3272 B | 2,205.2 ns, 3240 B |
| Microsoft.Extensions.Validation (experimental) | False | n/a | n/a | 1,767.3 ns, 2056 B | 1,495.4 ns, 1968 B |

On valid input the CSE lead over FluentValidation is small: Ratio 1.18 (net8.0), 1.12 (net9.0), 1.17 (net10.0), 1.14 (net11.0). On invalid input FluentValidation's Ratio is 5.91 to 6.76 across the four TFMs.

`BaselineWideComparison` (`WideRequest`, twelve required strings):

| Row | Valid | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| CSE | True | 743.2 ns, 1784 B | 728.0 ns, 1784 B | 655.6 ns, 1784 B | 656.7 ns, 1688 B |
| FluentValidation | True | 1,110.3 ns, 984 B | 984.9 ns, 984 B | 812.3 ns, 984 B | 780.1 ns, 984 B |
| DataAnnotations | True | 5,714.8 ns, 6744 B | 3,842.8 ns, 5864 B | 3,439.9 ns, 5760 B | 3,346.6 ns, 5664 B |
| Microsoft.Extensions.Validation (experimental) | True | n/a | n/a | 2,132.5 ns, 1312 B | 1,810.9 ns, 1304 B |
| CSE | False | 1,613.8 ns, 4632 B | 1,426.7 ns, 4632 B | 1,318.2 ns, 4632 B | 1,298.4 ns, 4536 B |
| FluentValidation | False | 9,061.2 ns, 14720 B | 8,808.2 ns, 14720 B | 7,942.1 ns, 13952 B | 8,061.0 ns, 13856 B |
| DataAnnotations | False | 7,389.3 ns, 8880 B | 6,221.8 ns, 8112 B | 5,973.8 ns, 8008 B | 5,678.6 ns, 7912 B |
| Microsoft.Extensions.Validation (experimental) | False | n/a | n/a | 3,902.2 ns, 3480 B | 3,439.1 ns, 3472 B |

CSE is faster on every row here, but it allocates more than FluentValidation on valid input (Alloc Ratio 0.55 on net8.0 to net10.0, 0.58 on net11.0) and more than Microsoft.Extensions.Validation on both inputs (Alloc Ratio 0.74 and 0.75 on net10.0, 0.77 and 0.77 on net11.0). `ForOverloadComparison` below shows where those bytes come from.

### `For` overloads

`ForOverloadComparison`, on the same `WideRequest` model but in a separate class. Baseline is `For(() => m.Fn)`.

| Row | Valid | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| `For(() => m.Fn)` | True | 775.5 ns, 1784 B | 749.0 ns, 1784 B | 664.1 ns, 1784 B | 646.6 ns, 1688 B |
| `For(m.Fn, nameof(m.Fn))` | True | 221.1 ns, 608 B | 202.4 ns, 608 B | 203.6 ns, 608 B | 179.7 ns, 608 B |
| inline `Validator.ValidateAsync` | True | 733.1 ns, 1784 B | 729.7 ns, 1784 B | 694.3 ns, 1784 B | 666.9 ns, 1688 B |
| `For(() => m.Fn)` | False | 1,606.9 ns, 4632 B | 1,432.5 ns, 4632 B | 1,349.4 ns, 4632 B | 1,264.8 ns, 4536 B |
| `For(m.Fn, nameof(m.Fn))` | False | 1,070.7 ns, 3456 B | 905.2 ns, 3456 B | 848.1 ns, 3456 B | 800.9 ns, 3456 B |
| inline `Validator.ValidateAsync` | False | 1,524.9 ns, 4632 B | 1,512.6 ns, 4632 B | 1,310.6 ns, 4632 B | 1,228.2 ns, 4536 B |

On valid input, `For(value, name)` has Ratio 0.27 to 0.31 and Alloc Ratio 0.34 to 0.36 against the lambda form. Its 608 B is below FluentValidation's 984 B on the wide valid row in `BaselineWideComparison`. The inline form allocates the same as the lambda form, because its body also uses `For(() => ...)`.

### Enum validators

`EnumDefinedComparison`. The hand-written row is `Enum.IsDefined` returning a bool, with no error object, so it does far less work than the validators.

| Row | Metadata | Valid | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| CSE `IsDefinedEnum` | Plain | True | 122.387 ns, 200 B | 102.996 ns, 200 B | 72.593 ns, 136 B | 74.420 ns, 136 B |
| FluentValidation `IsInEnum` | Plain | True | 1,136.881 ns, 816 B | 1,107.453 ns, 816 B | 912.618 ns, 816 B | 909.886 ns, 816 B |
| DataAnnotations `[EnumDataType]` | Plain | True | 745.837 ns, 1048 B | 709.908 ns, 960 B | 619.671 ns, 944 B | 573.525 ns, 936 B |
| hand-written | Plain | True | 1.434 ns, none | 1.591 ns, none | 1.356 ns, none | 1.309 ns, none |
| CSE `IsDefinedEnum` | Plain | False | 475.177 ns, 968 B | 443.807 ns, 888 B | 330.136 ns, 784 B | 332.980 ns, 784 B |
| FluentValidation `IsInEnum` | Plain | False | 2,299.606 ns, 2656 B | 2,174.562 ns, 2656 B | 1,869.646 ns, 2592 B | 1,819.170 ns, 2584 B |
| DataAnnotations `[EnumDataType]` | Plain | False | 1,145.331 ns, 1192 B | 990.302 ns, 1120 B | 877.089 ns, 1104 B | 796.986 ns, 1096 B |
| hand-written | Plain | False | 1.442 ns, none | 1.437 ns, none | 1.271 ns, none | 1.316 ns, none |
| CSE `IsDefinedEnum` | Generated | True | 70.830 ns, 200 B | 68.770 ns, 200 B | 50.615 ns, 136 B | 49.331 ns, 136 B |
| FluentValidation `IsInEnum` | Generated | True | 1,229.461 ns, 864 B | 1,219.284 ns, 864 B | 973.457 ns, 864 B | 985.342 ns, 864 B |
| DataAnnotations `[EnumDataType]` | Generated | True | 941.853 ns, 1048 B | 831.465 ns, 960 B | 664.971 ns, 944 B | 670.654 ns, 936 B |
| hand-written | Generated | True | 1.272 ns, none | 1.437 ns, none | 1.277 ns, none | 1.326 ns, none |
| CSE `IsDefinedEnum` | Generated | False | 354.379 ns, 920 B | 341.526 ns, 840 B | 243.459 ns, 736 B | 230.591 ns, 736 B |
| FluentValidation `IsInEnum` | Generated | False | 2,436.399 ns, 2704 B | 2,286.515 ns, 2704 B | 2,004.561 ns, 2640 B | 1,908.844 ns, 2632 B |
| DataAnnotations `[EnumDataType]` | Generated | False | 1,261.420 ns, 1192 B | 1,122.955 ns, 1120 B | 1,018.847 ns, 1104 B | 958.122 ns, 1096 B |
| hand-written | Generated | False | 1.275 ns, none | 1.442 ns, none | 1.284 ns, none | 1.317 ns, none |

`EnumFlagsComparison` (`[Flags]` property) and `EnumOneOfComparison` (`IsOneOf` against a FluentValidation `Must`):

| Class | Row | Valid | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|---|
| Flags | CSE `HasOnlyDefinedFlags` | True | 145.2 ns, 280 B | 134.0 ns, 280 B | 116.0 ns, 216 B | 102.3 ns, 216 B |
| Flags | FluentValidation `IsInEnum` | True | 1,929.2 ns, 1064 B | 1,854.2 ns, 1064 B | 1,407.2 ns, 1064 B | 1,372.0 ns, 1064 B |
| Flags | DataAnnotations `[EnumDataType]` | True | 1,181.6 ns, 1144 B | 1,063.6 ns, 1056 B | 988.1 ns, 1040 B | 854.1 ns, 1032 B |
| Flags | CSE `HasOnlyDefinedFlags` | False | 536.6 ns, 1096 B | 566.5 ns, 976 B | 422.1 ns, 952 B | 382.3 ns, 952 B |
| Flags | FluentValidation `IsInEnum` | False | 3,224.1 ns, 2936 B | 3,062.1 ns, 2936 B | 2,507.4 ns, 2872 B | 2,328.3 ns, 2864 B |
| Flags | DataAnnotations `[EnumDataType]` | False | 1,459.2 ns, 1248 B | 1,382.8 ns, 1176 B | 1,239.2 ns, 1160 B | 1,188.0 ns, 1152 B |
| OneOf | CSE `IsOneOf` | True | 79.34 ns, 200 B | 76.98 ns, 200 B | 48.13 ns, 136 B | 47.89 ns, 136 B |
| OneOf | FluentValidation `Must` | True | 151.31 ns, 600 B | 146.83 ns, 600 B | 114.06 ns, 600 B | 112.01 ns, 600 B |
| OneOf | CSE `IsOneOf` | False | 522.30 ns, 968 B | 479.02 ns, 888 B | 399.77 ns, 824 B | 323.09 ns, 784 B |
| OneOf | FluentValidation `Must` | False | 885.59 ns, 1904 B | 826.98 ns, 1904 B | 692.93 ns, 1840 B | 732.71 ns, 1832 B |

### Construction and assembly scan

| Class | Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|---|
| `ValidatorConstructionBenchmarks` | CSE `new` | 14.32 ns, 24 B (noisy, CV above 5%) | 7.015 ns, 24 B | 9.444 ns, 24 B (noisy, CV above 5%) | 5.422 ns, 24 B |
| `ValidatorConstructionBenchmarks` | FluentValidation `new` | 5,022.91 ns, 9192 B | 4,734.993 ns, 9096 B | 3,828.194 ns, 8968 B | 3,499.912 ns, 9000 B |
| `AssemblyScanComparison` (warm) | CSE | 10.09 μs, 8.22 KB | 8.641 μs, 8.22 KB | 9.243 μs, 8.52 KB | 8.069 μs, 8.52 KB |
| `AssemblyScanComparison` (warm) | FluentValidation | 24.44 μs, 31.94 KB | 20.176 μs, 31.94 KB | 19.603 μs, 31.88 KB | 19.030 μs, 30.97 KB |
| `AssemblyScanColdComparison` (`cold` job) | CSE | 5.038 ms, 62.83 KB | 4.704 ms, 62.83 KB | 5.346 ms, 67.38 KB | 4.781 ms, 67.32 KB |
| `AssemblyScanColdComparison` (`cold` job) | FluentValidation | 5.728 ms, 67.01 KB | 5.399 ms, 67.01 KB | 5.695 ms, 68.13 KB | 4.957 ms, 66.87 KB |

The CSE construction rows are noisy on net8.0 and net10.0 even after two reruns; the gap to FluentValidation is large enough that the noise does not change the order. The cold scan Ratio for FluentValidation is 1.14 (net8.0), 1.15 (net9.0), 1.07 (net10.0) and 1.04 (net11.0); JIT and type loading most likely dominate a cold first call.

### Earlier FluentValidation head-to-head classes (net10.0)

The 23 earlier classes have no baseline column, so their reports have no Ratio. The table shows net10.0; the other TFMs are in the linked report directories. On every TFM CSE has the lower mean on every row except `ConditionalValidationBenchmarks` business on net10.0, and the lower allocation on every row except `WideModelBenchmarks` valid (see [Where CSE loses](#where-cse-loses)).

| Class | Row | CSE | FluentValidation |
|---|---|---|---|
| `SimpleValidationBenchmarks` | valid | 327.9 ns, 592 B | 424.1 ns, 728 B |
| `SimpleValidationBenchmarks` | invalid (FluentValidation runs one more check) | 518.5 ns, 1464 B | 3,876.3 ns, 7288 B |
| `StringValidationBenchmarks` | valid | 284.9 ns, 312 B | 321.9 ns, 664 B |
| `ComparableValidationBenchmarks` | valid | 73.76 ns, 160 B | 162.87 ns, 624 B |
| `CollectionValidationBenchmarks` | valid | 77.26 ns, 168 B | 183.28 ns, 640 B |
| `NestedValidationBenchmarks` | valid | 263.2 ns, 440 B | 581.2 ns, 1272 B |
| `CollectionItemValidationBenchmarks` | valid | 508.4 ns, 1.2 KB | 1,430.0 ns, 2.92 KB |
| `ConditionalValidationBenchmarks` | business | 273.4 ns, 504 B | 255.4 ns, 696 B |
| `ConditionalValidationBenchmarks` | personal | 213.3 ns, 344 B | 259.5 ns, 664 B |
| `CustomPredicateBenchmarks` | valid | 53.08 ns, 168 B | 117.78 ns, 600 B |
| `CustomPredicateBenchmarks` | reserved name | 94.68 ns, 360 B | 277.31 ns, 1008 B |
| `AsyncPredicateBenchmarks` | valid | 1.152 μs, 847 B | 1.368 μs, 1463 B |
| `CascadeContinueBenchmarks` | weak password | 364.2 ns, 1 KB | 2,551.7 ns, 5.72 KB |
| `ComplexValidationBenchmarks` | valid | 1.281 μs, 2.05 KB | 3.150 μs, 3.66 KB |
| `ComplexValidationBenchmarks` | invalid (FluentValidation runs one more check) | 2.538 μs, 6.69 KB | 13.495 μs, 22.35 KB |
| `StringLengthRangeBenchmarks` | valid | 119.3 ns, 312 B | 143.4 ns, 600 B |
| `StringLengthRangeBenchmarks` | invalid | 274.7 ns, 768 B | 1,499.0 ns, 3808 B |
| `StringContentBenchmarks` | valid | 85.61 ns, 168 B | 131.42 ns, 600 B |
| `ExclusiveBetweenBenchmarks` | valid | 60.93 ns, 160 B | 111.50 ns, 600 B |
| `NullableValidationBenchmarks` | valid | 110.7 ns, 264 B | 213.3 ns, 656 B |
| `NullableValidationBenchmarks` | invalid | 199.9 ns, 776 B | 1,379.6 ns, 2848 B |
| `StringEqualityBenchmarks` | valid | 139.0 ns, 320 B | 176.5 ns, 632 B |
| `LargeCollectionBenchmarks` | 50 items | 10.75 μs, 25.45 KB | 29.22 μs, 49.36 KB |
| `WideModelBenchmarks` | valid | 721.2 ns, 1816 B | 807.3 ns, 984 B |
| `WideModelBenchmarks` | invalid | 1,393.4 ns, 4664 B | 8,051.2 ns, 13952 B |
| `DeepNestedBenchmarks` | valid | 455.8 ns, 792 B | 1,117.8 ns, 2088 B |
| `CascadeStopBenchmarks` | weak password | 217.3 ns, 640 B | 1,307.3 ns, 3360 B |
| `ValidatorConstructionBenchmarks` | `new` | 9.444 ns, 24 B (noisy, CV above 5%) | 3,828.194 ns, 8968 B |
| `MultipleRegexBenchmarks` | valid | 258.3 ns, 176 B | 465.1 ns, 632 B |
| `MultipleRegexBenchmarks` | weak password | 386.7 ns, 960 B | 2,311.0 ns, 4680 B |
| `PrecompiledRegexBenchmarks` | valid | 205.8 ns, 176 B | 320.6 ns, 632 B |

`AsyncPredicateBenchmarks` on net11.0 is 3.483 μs for CSE and 3.559 μs for FluentValidation, against 1.152 μs and 1.368 μs on net10.0. Both rows moved by the same amount, which points at the runtime's `Task.Yield` path on net11.0.

### Where CSE loses

- Allocations on the wide valid path: `BaselineWideComparison` Valid=True: CSE 1784 B against FluentValidation 984 B on net8.0, net9.0 and net10.0 (Alloc Ratio 0.55), and 1688 B against 984 B on net11.0 (0.58). `WideModelBenchmarks` valid: CSE 1816 B against 984 B on net8.0 to net10.0, and 1720 B against 984 B on net11.0. CSE is still faster on time in these rows. The cost is `For(() => ...)`: the same model with `For(value, name)` allocates 608 B.
- Allocations against Microsoft.Extensions.Validation on the wide model: net10.0: CSE 1784 B against 1312 B (valid) and 4632 B against 3480 B (invalid). net11.0: 1688 B against 1304 B and 4536 B against 3472 B.
- `ConditionalValidationBenchmarks` business order, net10.0: CSE 273.4 ns against FluentValidation 255.4 ns. On net8.0 (301.7 ns against 347.2 ns), net9.0 (285.9 ns against 311.7 ns) and net11.0 (251.1 ns against 259.6 ns) CSE is faster, so this row is close on every TFM.
- Cold assembly scan allocation, net11.0: CSE 67.32 KB against FluentValidation 66.87 KB (Alloc Ratio 0.99). On net10.0 the two are 67.38 KB and 68.13 KB.
- Against hand-written code: `EnumDefinedComparison`: a plain `Enum.IsDefined` check is 1.271 ns to 1.591 ns with no allocation on every TFM; the CSE validator ranges from 49.331 ns (net11.0, Generated, valid) to 475.177 ns (net8.0, Plain, invalid). The hand-written row builds no error object, so the gap is the cost of the validator pipeline itself.
- Small margins on valid input: on `BaselineSimpleComparison` valid, FluentValidation's Ratio is 1.12 to 1.18; on `StringValidationBenchmarks` net11.0 the rows are 278.2 ns and 298.9 ns.

---

## Caveats

- Microsoft.Extensions.Validation is experimental and documents no standalone idiom; its rows follow the API reference and may need changes when the API settles.
- Both assembly scans use reflection: CSE's Readme says to register validators with `AddValidator<T, TValidator>` under trimming or Native AOT. `TValidator` on that method carries `[DynamicallyAccessedMembers(PublicConstructors)]` (`CSharpEssentials.Validation/Extensions/ValidatorServiceCollectionExtensions.cs:26`), and the 2026-11-06 mediator Native AOT sample registers a validator with it and publishes with no IL warnings.
- No Native AOT sample: FluentValidation compiles expression trees and DataAnnotations is `[RequiresUnreferencedCode]`, so only the CSE and Microsoft.Extensions.Validation rows could run under AOT.
- Known CSE costs, with the rows that measure them:
  - `CSharpEssentials.Validation/RuleContext.cs:62-76`: `For(() => ...)` parses the member expression text with `PropertyPath.Extract` on every call and allocates a closure; `ForOverloadComparison` measures it against `For(value, name)` (see [`For` overloads](#for-overloads)).
  - `CSharpEssentials.Validation/Validators/EnumValidators.cs:201-204`: the plain-enum path converts through `Convert.ToInt64(object)`, which boxes. `EnumFlagsComparison` runs it; CSE is still below both competitors there (net10.0 valid: CSE 116.0 ns and 216 B, FluentValidation 1,407.2 ns and 1064 B, DataAnnotations 988.1 ns and 1040 B).
- The inline sync delegate form avoids a `Task`, not allocation: `ForOverloadComparison` measures it at 1784 B on valid input (net8.0 to net10.0), the same as `For(() => ...)` in a validator class.
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
