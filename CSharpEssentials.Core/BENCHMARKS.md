# CSharpEssentials.Core: performance benchmarks

This page compares five CSharpEssentials.Core (CSE) helpers with the BCL and with two libraries: string case conversion against `JsonNamingPolicy` and Humanizer.Core 3.0.10, URL-safe GUID text against `Base64Url` and CSharpVitamins.ShortGuid 2.0.0, unique random sampling against `RandomNumberGenerator`/`Random`, `WithCancellation` against `Task.WaitAsync`, and GUID creation (`Guider.NewGuid`, `Guider.NewMonotonicGuid`) against `Guid.NewGuid` and `Guid.CreateVersion7`. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11. The first four classes live in the Json comparison project, which is documented in [CSharpEssentials.Json/BENCHMARKS.md](../CSharpEssentials.Json/BENCHMARKS.md). The GUID creation classes live in `benchmarks/CSharpEssentials.Core.Benchmarks/`.

Short version: the BCL is faster than CSE for case conversion, URL-safe GUID text and `WithCancellation` on a pending task. CSE is faster than Humanizer and ShortGuid on every row, and faster than `RandomNumberGenerator.Shuffle` for small samples. On one thread `Guider.NewGuid` is faster than `Guid.NewGuid` on every TFM; with 6 threads it is faster on net8.0 to net10.0 and level on net11.0. The numbers are in [Results](#results), and every row where CSE loses is listed in [Where CSE loses](#where-cse-loses).

Both competitors are pinned exactly in `Directory.Packages.props`, and each pin is the latest stable release on NuGet as of 2026-11-03:

| Package | Version | Published | License | Asset the suite loads |
|---|---|---|---|---|
| Humanizer.Core | 3.0.10 | 2026-03-07 | MIT (nuspec expression; the nuspec is the authority) | `lib/net8.0` on net8 and net9, `lib/net10.0` on net10 and net11 |
| CSharpVitamins.ShortGuid | 2.0.0 | 2021-03-11 | MIT (`LICENSE.md` in the package, MIT text) | `lib/netstandard2.0` |

Humanizer 3.0.10 is marked trimmable and carries `RequiresUnreferencedCode`/`RequiresDynamicCode` annotations; its case inflectors are regex-based. ShortGuid uses no reflection. The Humanizer pin is scoped to this suite in `Directory.Packages.props`, because Roslyn's Workspaces package brings Humanizer.Core 2.x into the repo's code-fix projects and transitive pinning would otherwise move them to 3.x.

---

## What was measured

The first four classes are in `benchmarks/CSharpEssentials.Json.Comparison.Benchmarks/`; the two GUID creation classes are in `benchmarks/CSharpEssentials.Core.Benchmarks/`.
- Every class has `[MemoryDiagnoser]`.
- CSE is the baseline in every comparison category that has a CSE row. The GUID creation classes use `Guid.NewGuid` as the baseline.
- Each comparison class checks parity in `[GlobalSetup]`, and `--self-check` runs the same check; a row that fails its check stops the run (see the Parity column for what each class compares). The GUID creation classes have no parity check.
- Each library and BCL row has a comment above it that links to the documentation it follows; the hand-written Fisher-Yates reference row has none.

| # | Class | Measured unit | Rows | Parity |
|---|---|---|---|---|
| 1 | `CaseConversionComparison` | Convert a PascalCase name to snake, kebab and camel case. `[Params]` picks a 12-, 64- or 280-character input | CSE `ToSnakeCase`/`ToKebabCase`/`ToCamelCase`, BCL `JsonNamingPolicy.SnakeCaseLower`/`KebabCaseLower`/`CamelCase`, Humanizer `Underscore`/`Kebaberize`/`Camelize` | The exact output string, all three rows |
| 2 | `GuiderComparison` | Encode a `Guid` to 22 URL-safe characters, and decode each library's own text | CSE `ToStringFromGuid`/`ToGuidFromString`, ShortGuid implicit casts (`((ShortGuid)id).Value`, `((ShortGuid)text).Guid`), BCL `Base64Url.EncodeToString`/`DecodeFromChars` on net9 and later | Within each library: 22 characters, and its own decode returns the input |
| 3 | `RandomItemsComparison` | Draw K items from 1000 distinct ints. `[Params]` sets K to 1, 50 or 99 percent (10, 500, 990) | Unique: CSE `GetRandomItems(K)`, BCL `RandomNumberGenerator.Shuffle` on a copy + `[..K]`, BCL `Random.Shared.Shuffle` on a copy + `[..K]`, a hand-written partial Fisher-Yates with `Random.Shared` (reference row). WithReplacement: BCL `Random.Shared.GetItems` | Invariants: K items, all from the source, distinct for the Unique rows |
| 4 | `WithCancellationComparison` | Await a task under a live token that is never cancelled. Pending: wrap a running task, then complete it. Completed: wrap a cached completed task | CSE `WithCancellation`, BCL `WaitAsync` | Both return 42. The setup also cancels a fresh token over a pending task and checks both throw an `OperationCanceledException` |
| 5 | `GuidBenchmarks` | Create one GUID on one thread | `Guid.NewGuid` (version 4, baseline), `Guid.CreateVersion7` (net9 and later), CSE `Guider.NewGuid` and `Guider.NewMonotonicGuid` (both version 7) | None |
| 6 | `GuidParallelBenchmarks` | 6 threads that each create 20,000 GUIDs; the reported time is per GUID (`OperationsPerInvoke` = 120,000) | Same rows as `GuidBenchmarks` | None |

Inputs:
- Case conversion: `OrderLineIds` (12), `CustomerShippingAddressLineValidationResultCacheEntryKeyValueIds` (64), and `CustomerShippingAddressLineValidationResultCacheEntryKey` repeated 5 times (280). They are ASCII PascalCase with no acronyms and no digits. CSE rents its buffer from `ArrayPool` when the buffer is longer than 512 characters. Snake and kebab size the buffer at twice the input, so the 280-character input takes the `ArrayPool` path there; camel case sizes it at the input plus one and stays on the stack.
- GUID text: `c9a646d3-9c61-4cb7-bfcd-ee2522c8f633`. Its base64 contains `/`, so the input exercises the URL-safe character mapping.
- Random: `int[]` 0 to 999. n = 1000 is above CSE's stack threshold of 256, so CSE rents from `ArrayPool`. K = 10 takes CSE's rejection loop; K = 500 and K = 990 are at least n / 4, so they take its partial shuffle.
- GUID creation: on every target `Guider.NewGuid` builds the version 7 layout from a per-thread pool of random bytes. `NewMonotonicGuid` also advances one shared value with a compare-and-swap, which is what the parallel class shows.

On net8.0 CSE runs its netstandard2.1 build, because CSharpEssentials.Core has no net8 target. Some Core paths differ there; for example `ToStringFromGuid` calls `Guid.ToByteArray()`, which allocates, and `WithCancellation` builds its own `TaskCompletionSource` and `Task.WhenAny` instead of calling `WaitAsync`. On net9.0 and later CSE runs the build for that TFM.

---

## Semantic differences

The parity checks use inputs on which every row agrees. The libraries still differ in ways the numbers alone do not show. These are not timed.

- Acronyms: `"HTTPStatus"` becomes `httpstatus` with CSE and `http_status` with the BCL policy and Humanizer.
- Digits: CSE inserts a separator before a run of digits.
- Non-Latin letters: CSE keeps them: letters and combining marks of every Unicode category count as word characters, not separators.
- Camel case: `JsonNamingPolicy.CamelCase` lowercases only the leading run of capitals. It does not convert from snake case.
- GUID alphabet: CSE, ShortGuid and `Base64Url` all use the RFC 4648 URL-safe alphabet and write `00amyWGct0y_ze4lIsj2Mw` for the input. The parity check is still a round trip within each library, not a cross-library string comparison.
- ShortGuid decode re-encodes the result and compares it with the input, a strict check the other decoders do not do. That is extra work by design.
- Random source: CSE and the `RandomNumberGenerator` row draw from a cryptographic RNG. The `Random.Shared` and hand-written rows do not, so they are not a like-for-like comparison with CSE. The WithReplacement row can return duplicates and does a different job; it has no CSE row and no baseline.
- Cancellation exception: On net9.0 and later CSE calls `WaitAsync`, so both throw `TaskCanceledException`. On net8.0 (the netstandard2.1 build) CSE throws `OperationCanceledException`, the base class. CSE also attaches a continuation that observes a fault of the wrapped task.
- GUID versions: `Guid.NewGuid` is version 4 (random). `Guid.CreateVersion7` and both `Guider` methods are version 7 (time-ordered). `NewMonotonicGuid` also keeps IDs from one process in order within a millisecond; the others do not.

---

## Feature matrix

This table comes from reading source, docs and nuspecs, plus the probe noted above. None of it was measured. Humanizer links point at tag v3.0.10 (`90f0cb1`), ShortGuid links at tag v2.0.0 (`835267f`).

| Feature | CSE | BCL | Humanizer 3.0.10 | ShortGuid 2.0.0 |
|---|---|---|---|---|
| snake / kebab / camel | Yes | `JsonNamingPolicy` (STJ 8+) | [`Underscore`/`Kebaberize`/`Camelize`](https://github.com/Humanizr/Humanizer/blob/90f0cb1d7427635f38b46350bb1dc5fca2164f2d/readme.md#underscore-dasherize-kebaberize) | n/a |
| Acronym split (`HTTPStatus`) | No | Yes | Yes | n/a |
| URL-safe GUID text | Yes, RFC alphabet | `Base64Url` (net9+) | n/a | [Yes](https://github.com/csharpvitamins/csharpvitamins.shortguid/blob/835267feaf85da0651293c7295d5eb2e9f012990/readme.md#using-the-shortguid), RFC alphabet, strict decode |
| Unique random sample | Yes, cryptographic RNG | `RandomNumberGenerator.Shuffle` + slice; `GetItems` samples with replacement | n/a | n/a |
| Await with cancellation | `WithCancellation` (Task and ValueTask) | `Task.WaitAsync` (net6+, Task only) | n/a | n/a |
| Version 7 GUID | `Guider.NewGuid` on every target, `NewMonotonicGuid` for in-process order | `Guid.CreateVersion7` (net9+) | n/a | n/a |
| License | MIT | MIT | MIT | MIT |

---

## Results

Each cell is `Mean (Ratio), Allocated`, copied from the BenchmarkDotNet github report for that TFM. In the comparison classes the ratio is against the CSE row of the same category, so a ratio below 1.00 means that row took less time than CSE. In the GUID creation classes the ratio is against `Guid.NewGuid`. Where a report shows `-` for Allocated, the cell says `0 B`; "no row" means the API does not exist on that TFM. No row in these classes was flagged as noisy (no row kept a CV above 5%), and no mean is near 0 ns.

Reports: `benchmarks/results/json/{tfm}/results/CSharpEssentials.Json.Comparison.Benchmarks.<Class>-report-github.md` for classes 1 to 4 (for example [net10.0 CaseConversionComparison](../benchmarks/results/json/net10.0/results/CSharpEssentials.Json.Comparison.Benchmarks.CaseConversionComparison-report-github.md)), and `benchmarks/results/core/{tfm}/results/CSharpEssentials.Core.Benchmarks.<Class>-report-github.md` for classes 5 and 6 (for example [net11.0 GuidParallelBenchmarks](../benchmarks/results/core/net11.0/results/CSharpEssentials.Core.Benchmarks.GuidParallelBenchmarks-report-github.md)).

### Case conversion (`CaseConversionComparison`)

Snake, 280 characters (Long):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Snake` | 1,014.75 ns (1.00), 672 B | 1,040.42 ns (1.00), 672 B | 1,017.55 ns (1.00), 672 B | 991.35 ns (1.00), 672 B |
| `Bcl_Snake` | 929.06 ns (0.92), 672 B | 744.68 ns (0.72), 672 B | 735.74 ns (0.72), 672 B | 894.45 ns (0.90), 672 B |
| `Humanizer_Snake` | 6,892.06 ns (6.79), 1344 B | 6,816.86 ns (6.55), 1344 B | 6,470.99 ns (6.36), 1344 B | 6,083.06 ns (6.14), 1344 B |

Snake, 64 characters (Medium):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Snake` | 241.94 ns (1.00), 176 B | 267.35 ns (1.00), 176 B | 257.22 ns (1.00), 176 B | 245.66 ns (1.00), 176 B |
| `Bcl_Snake` | 219.96 ns (0.91), 176 B | 183.85 ns (0.69), 176 B | 169.04 ns (0.66), 176 B | 189.92 ns (0.77), 176 B |
| `Humanizer_Snake` | 1,767.53 ns (7.31), 352 B | 1,705.91 ns (6.38), 352 B | 1,559.77 ns (6.06), 352 B | 1,535.71 ns (6.25), 352 B |

Snake, 12 characters (Short):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Snake` | 60.09 ns (1.00), 56 B | 61.74 ns (1.00), 56 B | 70.15 ns (1.00), 56 B | 58.95 ns (1.00), 56 B |
| `Bcl_Snake` | 54.70 ns (0.91), 56 B | 58.52 ns (0.95), 56 B | 42.27 ns (0.60), 56 B | 48.17 ns (0.82), 56 B |
| `Humanizer_Snake` | 553.68 ns (9.21), 112 B | 518.29 ns (8.40), 112 B | 469.49 ns (6.69), 112 B | 483.98 ns (8.21), 112 B |

Kebab, 280 characters (Long):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Kebab` | 1,015.86 ns (1.00), 672 B | 1,052.57 ns (1.00), 672 B | 1,009.14 ns (1.00), 672 B | 995.83 ns (1.00), 672 B |
| `Bcl_Kebab` | 919.25 ns (0.90), 672 B | 733.99 ns (0.70), 672 B | 733.56 ns (0.73), 672 B | 823.34 ns (0.83), 672 B |
| `Humanizer_Kebab` | 6,902.96 ns (6.80), 2016 B | 6,887.76 ns (6.54), 2016 B | 6,368.42 ns (6.31), 2016 B | 6,011.88 ns (6.04), 2016 B |

Kebab, 12 characters (Short):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Kebab` | 58.65 ns (1.00), 56 B | 60.73 ns (1.00), 56 B | 63.09 ns (1.00), 56 B | 58.08 ns (1.00), 56 B |
| `Bcl_Kebab` | 54.90 ns (0.94), 56 B | 58.97 ns (0.97), 56 B | 43.24 ns (0.69), 56 B | 45.86 ns (0.79), 56 B |
| `Humanizer_Kebab` | 562.44 ns (9.59), 168 B | 518.65 ns (8.54), 168 B | 504.43 ns (8.00), 168 B | 472.71 ns (8.14), 168 B |

Camel, 280 characters (Long):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Camel` | 893.53 ns (1.00), 584 B | 1,019.44 ns (1.00), 584 B | 1,047.50 ns (1.00), 584 B | 1,051.73 ns (1.00), 584 B |
| `Bcl_Camel` | 34.55 ns (0.04), 584 B | 35.97 ns (0.04), 584 B | 34.73 ns (0.03), 584 B | 32.43 ns (0.03), 584 B |
| `Humanizer_Camel` | 3,605.42 ns (4.04), 1576 B | 3,603.89 ns (3.54), 1576 B | 3,890.26 ns (3.71), 1576 B | 3,427.89 ns (3.26), 1576 B |

Camel, 12 characters (Short):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Camel` | 51.96 ns (1.00), 48 B | 54.22 ns (1.00), 48 B | 71.36 ns (1.00), 48 B | 56.52 ns (1.00), 48 B |
| `Bcl_Camel` | 13.95 ns (0.27), 48 B | 13.71 ns (0.25), 48 B | 11.73 ns (0.16), 48 B | 11.00 ns (0.19), 48 B |
| `Humanizer_Camel` | 447.87 ns (8.62), 504 B | 437.39 ns (8.07), 504 B | 394.95 ns (5.53), 504 B | 400.03 ns (7.08), 504 B |

Kebab Medium and Camel Medium follow the same pattern; they are in the reports.

### URL-safe GUID text (`GuiderComparison`)

Encode:

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Encode` | 56.72 ns (1.00), 112 B | 19.60 ns (1.00), 72 B | 21.96 ns (1.00), 72 B | 14.33 ns (1.00), 72 B |
| `ShortGuid_Encode` | 67.82 ns (1.20), 256 B | 70.02 ns (3.57), 256 B | 65.01 ns (2.96), 256 B | 53.79 ns (3.75), 256 B |
| `Bcl_Encode` | no row | 18.96 ns (0.97), 72 B | 19.63 ns (0.89), 72 B | 13.94 ns (0.97), 72 B |

Decode:

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Decode` | 99.13 ns (1.00), 0 B | 43.46 ns (1.00), 0 B | 35.82 ns (1.00), 0 B | 21.76 ns (1.00), 0 B |
| `ShortGuid_Decode` | 264.13 ns (2.66), 696 B | 253.47 ns (5.83), 696 B | 258.74 ns (7.22), 696 B | 188.53 ns (8.66), 696 B |
| `Bcl_Decode` | no row | 23.79 ns (0.55), 0 B | 22.56 ns (0.63), 0 B | 20.23 ns (0.93), 0 B |

### Unique random sample (`RandomItemsComparison`)

K = 10 (1 percent):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Unique` | 9,829.19 ns (1.00), 64 B | 10,039.69 ns (1.00), 64 B | 9,759.15 ns (1.00), 64 B | 10,037.72 ns (1.00), 64 B |
| `Bcl_Unique` | 1,298,059.88 ns (132.06), 4090 B | 1,314,428.67 ns (130.92), 4089 B | 1,308,046.47 ns (134.04), 4089 B | 1,299,519.62 ns (129.46), 4089 B |
| `BclShared_Unique` | 9,013.66 ns (0.92), 4088 B | 5,655.01 ns (0.56), 4088 B | 5,260.72 ns (0.54), 4088 B | 4,641.41 ns (0.46), 4088 B |
| `HandWritten_Unique` | 314.07 ns (0.03), 4088 B | 287.93 ns (0.03), 4088 B | 317.61 ns (0.03), 4088 B | 292.82 ns (0.03), 4088 B |

K = 500 (50 percent):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Unique` | 649,917.55 ns (1.000), 2025 B | 638,426.83 ns (1.000), 2025 B | 657,812.08 ns (1.000), 2025 B | 653,059.95 ns (1.000), 2024 B |
| `Bcl_Unique` | 1,318,920.55 ns (2.029), 6050 B | 1,310,431.14 ns (2.053), 6049 B | 1,286,621.72 ns (1.956), 6048 B | 1,302,465.55 ns (1.994), 6049 B |
| `BclShared_Unique` | 6,640.16 ns (0.010), 6048 B | 5,755.97 ns (0.009), 6048 B | 5,407.57 ns (0.008), 6048 B | 5,005.39 ns (0.008), 6048 B |
| `HandWritten_Unique` | 3,796.79 ns (0.006), 6048 B | 3,306.28 ns (0.005), 6048 B | 3,500.12 ns (0.005), 6048 B | 3,393.62 ns (0.005), 6048 B |

K = 990 (99 percent):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Unique` | 1,290,508.21 ns (1.000), 3986 B | 1,282,108.09 ns (1.000), 3986 B | 1,303,631.05 ns (1.000), 3984 B | 1,302,933.68 ns (1.000), 3984 B |
| `Bcl_Unique` | 1,323,100.60 ns (1.025), 8010 B | 1,295,998.06 ns (1.011), 8009 B | 1,282,952.20 ns (0.984), 8008 B | 1,288,284.27 ns (0.989), 8009 B |
| `BclShared_Unique` | 6,613.04 ns (0.005), 8008 B | 5,901.59 ns (0.005), 8008 B | 5,325.28 ns (0.004), 8008 B | 4,909.41 ns (0.004), 8008 B |
| `HandWritten_Unique` | 7,402.29 ns (0.006), 8008 B | 6,297.38 ns (0.005), 8008 B | 6,573.75 ns (0.005), 8008 B | 6,645.63 ns (0.005), 8008 B |

`Random.Shared.GetItems` (with replacement, no CSE row, no baseline) took 61.03 ns, 2,751.99 ns and 5,459.71 ns on net8.0 and 68.65 ns, 3,164.13 ns and 6,132.76 ns on net11.0 for K = 10, 500 and 990.

### WithCancellation (`WithCancellationComparison`)

Pending:

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Pending` | 283.00 ns (1.00), 584 B | 213.02 ns (1.00), 456 B | 169.72 ns (1.00), 456 B | 160.14 ns (1.00), 456 B |
| `Bcl_Pending` | 105.36 ns (0.37), 272 B | 90.41 ns (0.42), 248 B | 77.17 ns (0.45), 248 B | 76.85 ns (0.48), 248 B |

Completed:

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Completed` | 16.64 ns (1.00), 72 B | 16.15 ns (1.00), 72 B | 13.83 ns (1.00), 72 B | 13.37 ns (1.00), 72 B |
| `Bcl_Completed` | 17.48 ns (1.05), 72 B | 16.48 ns (1.02), 72 B | 13.94 ns (1.01), 72 B | 13.26 ns (0.99), 72 B |

### GUID creation, one thread (`GuidBenchmarks`)

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `GuidNewGuid` | 750.46 ns (1.00), 0 B | 759.69 ns (1.00), 0 B | 752.17 ns (1.00), 0 B | 57.94 ns (1.00), 0 B |
| `GuiderNewGuid` | 72.40 ns (0.10), 0 B | 71.90 ns (0.09), 0 B | 59.39 ns (0.08), 0 B | 52.95 ns (0.91), 0 B |
| `GuiderNewMonotonicGuid` | 86.79 ns (0.12), 0 B | 83.61 ns (0.11), 0 B | 71.34 ns (0.09), 0 B | 57.92 ns (1.00), 0 B |
| `GuidCreateVersion7` | no row | 805.80 ns (1.06), 0 B | 773.69 ns (1.03), 0 B | 90.15 ns (1.56), 0 B |

### GUID creation, 6 threads (`GuidParallelBenchmarks`)

Time per GUID:

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `GuidNewGuid` | 150.43 ns (1.00), 0 B | 150.82 ns (1.00), 0 B | 151.70 ns (1.00), 0 B | 12.58 ns (1.00), 0 B |
| `GuiderNewGuid` | 16.15 ns (0.11), 0 B | 15.67 ns (0.10), 0 B | 13.54 ns (0.09), 0 B | 12.61 ns (1.00), 0 B |
| `GuiderNewMonotonicGuid` | 56.79 ns (0.38), 0 B | 45.86 ns (0.30), 0 B | 45.28 ns (0.30), 0 B | 50.73 ns (4.03), 0 B |
| `GuidCreateVersion7` | no row | 160.20 ns (1.06), 0 B | 158.37 ns (1.04), 0 B | 18.15 ns (1.44), 0 B |

`Guid.NewGuid` took 750.46 ns to 759.69 ns per call on net8.0 to net10.0 and 57.94 ns on net11.0 on this machine. That change in the baseline is why the GUID ratios on net11.0 look so different from the other TFMs; compare the means, not only the ratios.

### Where CSE wins

- Case conversion against Humanizer on every length and TFM: ratio 3.26 to 9.59, and Humanizer allocates 2 to 10.50 times as much (Alloc Ratio 2.00 to 10.50). CSE allocates the same as the BCL.
- Guider against ShortGuid on every TFM: Encode ratio 1.20 to 3.75, Decode ratio 2.66 to 8.66. CSE decodes with 0 B, ShortGuid allocates 696 B.
- Unique sample against `RandomNumberGenerator.Shuffle` (the only other cryptographic row) at K = 10 (ratio 129.46 to 134.04) and K = 500 (ratio 1.956 to 2.053), and at K = 990 on net8.0 (1.025) and net9.0 (1.011). CSE allocates the least of all Unique rows at every K.
- `Guider.NewGuid` and `NewMonotonicGuid` against `Guid.NewGuid` and `Guid.CreateVersion7` on net8.0 to net10.0, on one thread and on 6 threads. On net10.0, one thread: 59.39 ns and 71.34 ns against 752.17 ns and 773.69 ns.
- `Guider.NewGuid` on net11.0, one thread: 52.95 ns against 57.94 ns for `Guid.NewGuid` (0.91) and 90.15 ns for `Guid.CreateVersion7`.

### Where CSE loses

Every row where CSE is slower or allocates more than a competitor:

- Case conversion against `JsonNamingPolicy`, every length and TFM: Snake and kebab: ratio 0.60 to 0.97 with the same output and the same allocation; on net10.0 Long snake took 735.74 ns against 1,017.55 ns. Camel: ratio 0.03 to 0.27; on net10.0 Long camel took 34.73 ns against 1,047.50 ns. The BCL camel policy only lowercases the leading capitals, so it does less work, but for these inputs the output is the same.
- Guider against `Base64Url`, net9.0 to net11.0: Encode ratio 0.89 to 0.97 (net11.0: 13.94 ns against 14.33 ns), so close to level on net9.0 and net11.0. Decode ratio 0.55 to 0.93 (net11.0: 20.23 ns against 21.76 ns). The text is the same and so is the allocation (72 B and 0 B).
- Guider Encode on net8.0 allocates 112 B, because the netstandard2.1 build calls `Guid.ToByteArray()`. On net9.0 and later it allocates 72 B. ShortGuid still allocates more (256 B).
- Unique sample against the `Random.Shared` and hand-written rows, every K and TFM: These rows use a non-cryptographic RNG, so they do a different job. At K = 10 `Random.Shared.Shuffle` has ratio 0.46 to 0.92 and the hand-written row 0.03; at K = 500 and K = 990 both are at 0.004 to 0.010.
- Unique sample at K = 990 against `RandomNumberGenerator.Shuffle` on net10.0 and net11.0: 1,282,952.20 ns against 1,303,631.05 ns (0.984) and 1,288,284.27 ns against 1,302,933.68 ns (0.989). Both draw from the cryptographic RNG once per item.
- WithCancellation Pending, every TFM: `WaitAsync` has ratio 0.37 to 0.48 and allocates less (net8.0: 272 B against 584 B; net9.0 to net11.0: 248 B against 456 B). On net9.0 and later CSE calls `WaitAsync` itself; the extra time and bytes come from the fault-observing continuation it attaches first.
- WithCancellation Completed on net11.0: 13.26 ns against 13.37 ns (0.99). Both return the completed task as is and allocate 72 B, so this is within run-to-run variation.
- GUID creation on net11.0, one thread: `NewMonotonicGuid` took 57.92 ns against 57.94 ns for `Guid.NewGuid` (ratio 1.00), so the two cannot be told apart.
- GUID creation on net11.0, 6 threads: `Guider.NewGuid` took 12.61 ns against 12.58 ns (ratio 1.00), level with `Guid.NewGuid`. `NewMonotonicGuid` took 50.73 ns (ratio 4.03), slower than `Guid.NewGuid` and than `Guid.CreateVersion7` (18.15 ns). The threads contend on its shared compare-and-swap value, which the other rows do not have; that value is what keeps the IDs in order.

### How these numbers were produced

- BenchmarkDotNet v0.14.0 default job (no `--quick`), full runs, one TFM after another on a 6-core, 12-thread x64 desktop CPU on Linux (RyuJIT AVX2).
- SDK 11.0.100-rc.1.26425.128. Runtimes: .NET 8.0.30, 9.0.19, 10.0.11 and 11.0.0. Classes 1 and 2 were rerun on main after pull requests #230 to #256 landed, with SDK 11.0.100-rc.3.26522.148 and .NET 8.0.33, 9.0.22, 10.0.14 and 11.0.0; the `integ/all` note below applies to classes 3 to 6.
- The CSE code is the local `integ/all` branch, which merges every open pull request (commit `fe91b26a`). It may not match the version published on NuGet.
- Load, classes 1 to 4 (json suite): the 1-minute load average was 1.43 at the start and 1.74 at the end.
- Load, classes 5 and 6 (core suite): the 1-minute load average was 3.35 at the start and 4.87 at the end (5-minute 3.45 and 3.86). The 6-thread class ran on a 12-thread machine with that background load. Only the start and end of each suite were sampled.
- Rows whose CV stayed above 5% were rerun up to twice; no row from these classes needed it.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project json                       # all TFMs (net8-net11), Json and Core classes, default job
./run-benchmarks.sh --project json --tfm net10.0         # one TFM
./run-benchmarks.sh --project json --quick --tfm net10.0 # one TFM, Short job, one filter per class
./run-benchmarks.sh --project core                       # GUID creation classes, all TFMs, default job
dotnet run -c Release -f net10.0 --project CSharpEssentials.Json.Comparison.Benchmarks -- --self-check
```

Without `--tfm`, `--quick` runs on net9.0, the script default. Short-job numbers cannot be compared with default-job numbers. Results are stored per framework under `benchmarks/results/json/{tfm}/results/` and `benchmarks/results/core/{tfm}/results/`.
