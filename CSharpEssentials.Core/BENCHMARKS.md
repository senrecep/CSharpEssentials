# CSharpEssentials.Core: Performance benchmarks

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

- **Acronyms.** `"HTTPStatus"` becomes `httpstatus` with CSE and `http_status` with the BCL policy and Humanizer.
- **Digits.** CSE inserts a separator before a run of digits.
- **Non-Latin letters.** CSE keeps them: letters and combining marks of every Unicode category count as word characters, not separators.
- **Camel case.** `JsonNamingPolicy.CamelCase` lowercases only the leading run of capitals. It does not convert from snake case.
- **GUID alphabet.** CSE, ShortGuid and `Base64Url` all use the RFC 4648 URL-safe alphabet and write `00amyWGct0y_ze4lIsj2Mw` for the input. The parity check is still a round trip within each library, not a cross-library string comparison.
- **ShortGuid decode** re-encodes the result and compares it with the input, a strict check the other decoders do not do. That is extra work by design.
- **Random source.** CSE and the `RandomNumberGenerator` row draw from a cryptographic RNG. The `Random.Shared` and hand-written rows do not, so they are not a like-for-like comparison with CSE. The WithReplacement row can return duplicates and does a different job; it has no CSE row and no baseline.
- **Cancellation exception.** On net9.0 and later CSE calls `WaitAsync`, so both throw `TaskCanceledException`. On net8.0 (the netstandard2.1 build) CSE throws `OperationCanceledException`, the base class. CSE also attaches a continuation that observes a fault of the wrapped task.
- **GUID versions.** `Guid.NewGuid` is version 4 (random). `Guid.CreateVersion7` and both `Guider` methods are version 7 (time-ordered). `NewMonotonicGuid` also keeps IDs from one process in order within a millisecond; the others do not.

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
| `CSharpEssentials_Snake` | 2,640.85 ns (1.00), 672 B | 2,517.16 ns (1.00), 672 B | 2,406.81 ns (1.00), 672 B | 2,500.28 ns (1.00), 672 B |
| `Bcl_Snake` | 927.29 ns (0.35), 672 B | 743.06 ns (0.30), 672 B | 734.42 ns (0.31), 672 B | 836.97 ns (0.33), 672 B |
| `Humanizer_Snake` | 6,860.30 ns (2.60), 1344 B | 6,615.43 ns (2.63), 1344 B | 6,249.49 ns (2.60), 1344 B | 6,057.94 ns (2.42), 1344 B |

Snake, 64 characters (Medium):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Snake` | 635.57 ns (1.00), 176 B | 581.66 ns (1.00), 176 B | 558.58 ns (1.00), 176 B | 592.39 ns (1.00), 176 B |
| `Bcl_Snake` | 216.74 ns (0.34), 176 B | 184.92 ns (0.32), 176 B | 165.51 ns (0.30), 176 B | 193.58 ns (0.33), 176 B |
| `Humanizer_Snake` | 1,780.52 ns (2.80), 352 B | 1,644.08 ns (2.83), 352 B | 1,539.08 ns (2.76), 352 B | 1,537.28 ns (2.60), 352 B |

Snake, 12 characters (Short):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Snake` | 131.27 ns (1.00), 56 B | 121.29 ns (1.00), 56 B | 110.23 ns (1.00), 56 B | 113.12 ns (1.00), 56 B |
| `Bcl_Snake` | 55.30 ns (0.42), 56 B | 58.74 ns (0.48), 56 B | 43.11 ns (0.39), 56 B | 47.90 ns (0.42), 56 B |
| `Humanizer_Snake` | 541.08 ns (4.12), 112 B | 508.16 ns (4.19), 112 B | 464.84 ns (4.22), 112 B | 457.72 ns (4.05), 112 B |

Kebab, 280 characters (Long):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Kebab` | 2,468.21 ns (1.00), 672 B | 2,567.24 ns (1.00), 672 B | 2,408.47 ns (1.00), 672 B | 2,503.77 ns (1.00), 672 B |
| `Bcl_Kebab` | 925.45 ns (0.37), 672 B | 756.91 ns (0.29), 672 B | 736.50 ns (0.31), 672 B | 833.69 ns (0.33), 672 B |
| `Humanizer_Kebab` | 7,051.23 ns (2.86), 2016 B | 6,715.63 ns (2.62), 2016 B | 6,475.46 ns (2.69), 2016 B | 6,090.09 ns (2.43), 2016 B |

Kebab, 12 characters (Short):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Kebab` | 123.10 ns (1.00), 56 B | 122.89 ns (1.00), 56 B | 109.92 ns (1.00), 56 B | 113.40 ns (1.00), 56 B |
| `Bcl_Kebab` | 55.60 ns (0.45), 56 B | 58.86 ns (0.48), 56 B | 43.18 ns (0.39), 56 B | 47.22 ns (0.42), 56 B |
| `Humanizer_Kebab` | 553.61 ns (4.50), 168 B | 535.44 ns (4.36), 168 B | 481.01 ns (4.38), 168 B | 492.32 ns (4.34), 168 B |

Camel, 280 characters (Long):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Camel` | 2,484.74 ns (1.00), 584 B | 2,425.49 ns (1.00), 584 B | 2,350.00 ns (1.00), 584 B | 2,447.31 ns (1.00), 584 B |
| `Bcl_Camel` | 39.21 ns (0.02), 584 B | 38.43 ns (0.02), 584 B | 36.82 ns (0.02), 584 B | 35.44 ns (0.01), 584 B |
| `Humanizer_Camel` | 3,595.10 ns (1.45), 1576 B | 3,588.17 ns (1.48), 1576 B | 3,483.56 ns (1.48), 1576 B | 3,507.09 ns (1.43), 1576 B |

Camel, 12 characters (Short):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Camel` | 118.80 ns (1.00), 48 B | 121.44 ns (1.00), 48 B | 106.88 ns (1.00), 48 B | 111.71 ns (1.00), 48 B |
| `Bcl_Camel` | 14.22 ns (0.12), 48 B | 14.15 ns (0.12), 48 B | 11.95 ns (0.11), 48 B | 11.28 ns (0.10), 48 B |
| `Humanizer_Camel` | 502.78 ns (4.23), 504 B | 455.77 ns (3.75), 504 B | 397.74 ns (3.72), 504 B | 398.58 ns (3.57), 504 B |

Kebab Medium and Camel Medium follow the same pattern; they are in the reports.

### URL-safe GUID text (`GuiderComparison`)

Encode:

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Encode` | 56.79 ns (1.00), 112 B | 47.68 ns (1.00), 72 B | 46.85 ns (1.00), 72 B | 47.72 ns (1.00), 72 B |
| `ShortGuid_Encode` | 72.78 ns (1.28), 256 B | 71.67 ns (1.50), 256 B | 68.64 ns (1.47), 256 B | 57.23 ns (1.20), 256 B |
| `Bcl_Encode` | no row | 19.52 ns (0.41), 72 B | 22.26 ns (0.48), 72 B | 15.03 ns (0.32), 72 B |

Decode:

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Decode` | 101.99 ns (1.00), 0 B | 102.01 ns (1.00), 0 B | 80.81 ns (1.00), 0 B | 56.49 ns (1.00), 0 B |
| `ShortGuid_Decode` | 268.40 ns (2.63), 696 B | 257.93 ns (2.53), 696 B | 253.70 ns (3.14), 696 B | 197.32 ns (3.49), 696 B |
| `Bcl_Decode` | no row | 24.14 ns (0.24), 0 B | 22.12 ns (0.27), 0 B | 20.26 ns (0.36), 0 B |

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

- Case conversion against Humanizer on every length and TFM: ratio 1.43 to 4.50, and Humanizer allocates 2 to 10.50 times as much (Alloc Ratio 2.00 to 10.50). CSE allocates the same as the BCL.
- Guider against ShortGuid on every TFM: Encode ratio 1.20 to 1.50, Decode ratio 2.53 to 3.49. CSE decodes with 0 B, ShortGuid allocates 696 B.
- Unique sample against `RandomNumberGenerator.Shuffle` (the only other cryptographic row) at K = 10 (ratio 129.46 to 134.04) and K = 500 (ratio 1.956 to 2.053), and at K = 990 on net8.0 (1.025) and net9.0 (1.011). CSE allocates the least of all Unique rows at every K.
- `Guider.NewGuid` and `NewMonotonicGuid` against `Guid.NewGuid` and `Guid.CreateVersion7` on net8.0 to net10.0, on one thread and on 6 threads. On net10.0, one thread: 59.39 ns and 71.34 ns against 752.17 ns and 773.69 ns.
- `Guider.NewGuid` on net11.0, one thread: 52.95 ns against 57.94 ns for `Guid.NewGuid` (0.91) and 90.15 ns for `Guid.CreateVersion7`.

### Where CSE loses

Every row where CSE is slower or allocates more than a competitor:

- **Case conversion against `JsonNamingPolicy`, every length and TFM.** Snake and kebab: ratio 0.29 to 0.48 with the same output and the same allocation; on net10.0 Long snake took 734.42 ns against 2,406.81 ns. Camel: ratio 0.01 to 0.12; on net10.0 Long camel took 36.82 ns against 2,350.00 ns. The BCL camel policy only lowercases the leading capitals, so it does less work, but for these inputs the output is the same.
- **Guider against `Base64Url`, net9.0 to net11.0.** Encode ratio 0.32 to 0.48 (net11.0: 15.03 ns against 47.72 ns), Decode ratio 0.24 to 0.36 (net11.0: 20.26 ns against 56.49 ns). The text is the same and so is the allocation (72 B and 0 B).
- **Guider Encode on net8.0 allocates 112 B**, because the netstandard2.1 build calls `Guid.ToByteArray()`. On net9.0 and later it allocates 72 B. ShortGuid still allocates more (256 B).
- **Unique sample against the `Random.Shared` and hand-written rows, every K and TFM.** These rows use a non-cryptographic RNG, so they do a different job. At K = 10 `Random.Shared.Shuffle` has ratio 0.46 to 0.92 and the hand-written row 0.03; at K = 500 and K = 990 both are at 0.004 to 0.010.
- **Unique sample at K = 990 against `RandomNumberGenerator.Shuffle` on net10.0 and net11.0:** 1,282,952.20 ns against 1,303,631.05 ns (0.984) and 1,288,284.27 ns against 1,302,933.68 ns (0.989). Both draw from the cryptographic RNG once per item.
- **WithCancellation Pending, every TFM:** `WaitAsync` has ratio 0.37 to 0.48 and allocates less (net8.0: 272 B against 584 B; net9.0 to net11.0: 248 B against 456 B). On net9.0 and later CSE calls `WaitAsync` itself; the extra time and bytes come from the fault-observing continuation it attaches first.
- **WithCancellation Completed on net11.0:** 13.26 ns against 13.37 ns (0.99). Both return the completed task as is and allocate 72 B, so this is within run-to-run variation.
- **GUID creation on net11.0, one thread:** `NewMonotonicGuid` took 57.92 ns against 57.94 ns for `Guid.NewGuid` (ratio 1.00), so the two cannot be told apart.
- **GUID creation on net11.0, 6 threads:** `Guider.NewGuid` took 12.61 ns against 12.58 ns (ratio 1.00), level with `Guid.NewGuid`. `NewMonotonicGuid` took 50.73 ns (ratio 4.03), slower than `Guid.NewGuid` and than `Guid.CreateVersion7` (18.15 ns). The threads contend on its shared compare-and-swap value, which the other rows do not have; that value is what keeps the IDs in order.

### How these numbers were produced

- BenchmarkDotNet v0.14.0 default job (no `--quick`), full runs, one TFM after another on one Linux machine with 6 cores and 12 threads (x64, RyuJIT AVX2). The report headers have the exact CPU and OS.
- SDK 11.0.100-rc.1.26425.128. Runtimes: .NET 8.0.30, 9.0.19, 10.0.11 and 11.0.0.
- The CSE code is the local `integ/all` branch, which merges every open pull request (commit `fe91b26a`). It may not match the version published on NuGet.
- Load, classes 1 to 4 (json suite): the 1-minute load average was 1.43 at the start and 1.74 at the end.
- Load, classes 5 and 6 (core suite): the 1-minute load average was 3.35 at the start and 4.87 at the end (5-minute 3.45 and 3.86). The 6-thread class ran on a 12-thread machine with that background load. Only the start and end of each suite were sampled.
- Rows whose CV stayed above 5% were rerun up to twice; no row from these classes needed it.
- Results paths: `benchmarks/results/json/{tfm}/results/` and `benchmarks/results/core/{tfm}/results/`.

---

## Caveats

- The `Base64Url` rows and the `Guid.CreateVersion7` rows exist on net9.0 and later only.
- On net8.0 CSE runs its netstandard2.1 build, whose paths differ from the net9+ build.
- The `Random.Shared` rows use a non-cryptographic RNG. They are there to show what the cryptographic RNG costs, not as like-for-like rows.
- The CSE column is the `integ/all` integration branch, which may not match the version published on NuGet.
- The feature matrix comes from reading source and docs.

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
