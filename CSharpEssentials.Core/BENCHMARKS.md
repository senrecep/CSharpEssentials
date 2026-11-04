# CSharpEssentials.Core: Performance benchmarks (draft)

This page compares four CSharpEssentials.Core (CSE) helpers with the BCL and with two libraries: string case conversion against `JsonNamingPolicy` and Humanizer.Core 3.0.10, URL-safe GUID text against `Base64Url` and CSharpVitamins.ShortGuid 2.0.0, unique random sampling against `RandomNumberGenerator`/`Random`, and `WithCancellation` against `Task.WaitAsync`. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11. The classes live in the Json comparison project, which is documented in [CSharpEssentials.Json/BENCHMARKS.md](../CSharpEssentials.Json/BENCHMARKS.md).

**Status: draft.** The suite builds, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet (see [Results](#results)).

Both competitors are pinned exactly in `Directory.Packages.props`, and each pin is the latest stable release on NuGet as of 2026-11-03:

| Package | Version | Published | License | Asset the suite loads |
|---|---|---|---|---|
| Humanizer.Core | 3.0.10 | 2026-03-07 | MIT (nuspec expression). The GitHub API reports `NOASSERTION` for the repository; the nuspec is the authority | `lib/net8.0` on net8 and net9, `lib/net10.0` on net10 and net11 |
| CSharpVitamins.ShortGuid | 2.0.0 | 2021-03-11 | MIT (`LICENSE.md` in the package, MIT text) | `lib/netstandard2.0` |

Humanizer 3.0.10 is marked trimmable and carries `RequiresUnreferencedCode`/`RequiresDynamicCode` annotations; its case inflectors are regex-based. ShortGuid uses no reflection. The Humanizer pin is scoped to this suite in `Directory.Packages.props`, because Roslyn's Workspaces package brings Humanizer.Core 2.x into the repo's code-fix projects and transitive pinning would otherwise move them to 3.x.

---

## What was measured

The code is in `benchmarks/CSharpEssentials.Json.Comparison.Benchmarks/`, in four BenchmarkDotNet classes for this package.
- Every class has `[MemoryDiagnoser]`.
- CSE is the baseline in every category that has a CSE row.
- Each class checks parity in `[GlobalSetup]`, and `--self-check` runs the same check; a row that fails its check stops the run (see the Parity column for what each class compares).
- Each library and BCL row has a comment above it that links to the documentation it follows; the hand-written Fisher-Yates reference row has none.

| # | Class | Measured unit | Rows | Parity |
|---|---|---|---|---|
| 1 | `CaseConversionComparison` | Convert a PascalCase name to snake, kebab and camel case. `[Params]` picks a 12-, 64- or 280-character input | CSE `ToSnakeCase`/`ToKebabCase`/`ToCamelCase`, BCL `JsonNamingPolicy.SnakeCaseLower`/`KebabCaseLower`/`CamelCase`, Humanizer `Underscore`/`Kebaberize`/`Camelize` | The exact output string, all three rows |
| 2 | `GuiderComparison` | Encode a `Guid` to 22 URL-safe characters, and decode each library's own text | CSE `ToStringFromGuid`/`ToGuidFromString`, ShortGuid implicit casts (`((ShortGuid)id).Value`, `((ShortGuid)text).Guid`), BCL `Base64Url.EncodeToString`/`DecodeFromChars` on net9 and later | Within each library: 22 characters, and its own decode returns the input |
| 3 | `RandomItemsComparison` | Draw K items from 1000 distinct ints. `[Params]` sets K to 1, 50 or 99 percent (10, 500, 990) | Unique: CSE `GetRandomItems(K)`, BCL `RandomNumberGenerator.Shuffle` on a copy + `[..K]`, BCL `Random.Shared.Shuffle` on a copy + `[..K]`, a hand-written partial Fisher-Yates with `Random.Shared` (reference row). WithReplacement: BCL `Random.Shared.GetItems` | Invariants: K items, all from the source, distinct for the Unique rows |
| 4 | `WithCancellationComparison` | Await a task under a live token that is never cancelled. Pending: wrap a running task, then complete it. Completed: wrap a cached completed task | CSE `WithCancellation`, BCL `WaitAsync` | Both return 42. The setup also cancels a fresh token over a pending task and checks both throw an `OperationCanceledException` |

Inputs:
- Case conversion: `OrderLineIds` (12), `CustomerShippingAddressLineValidationResultCacheEntryKeyValueIds` (64), and `CustomerShippingAddressLineValidationResultCacheEntryKey` repeated 5 times (280). They are ASCII PascalCase with no acronyms and no digits. The long input is above 256 characters, so it takes CSE's `ArrayPool` path.
- GUID: `c9a646d3-9c61-4cb7-bfcd-ee2522c8f633`. Its base64 contains `/`, so the alphabet difference below is real for this input.
- Random: `int[]` 0 to 999. n = 1000 is above 256, so CSE takes its `ArrayPool` path.

On net8.0 CSE runs its netstandard2.1 build, because CSharpEssentials.Core has no net8 target. Some Core paths differ there; for example `ToStringFromGuid` calls `Guid.ToByteArray()`, which allocates. On net9.0 and later CSE runs the build for that TFM.

---

## Semantic differences

The parity checks use inputs on which every row agrees. The libraries still differ in ways the numbers alone do not show. These were checked with a probe on net8 and net10 and are not timed.

- **Acronyms.** `"HTTPStatus"` becomes `httpstatus` with CSE and `http_status` with the BCL policy and Humanizer.
- **Digits.** CSE inserts a separator before a run of digits.
- **Non-Latin letters.** CSE drops them until a pending fix lands.
- **Camel case.** `JsonNamingPolicy.CamelCase` lowercases only the leading run of capitals. It does not convert from snake case.
- **GUID alphabet.** On this branch CSE writes `00amyWGct0y-ze4lIsj2Mw` for the input; ShortGuid and `Base64Url` write `00amyWGct0y_ze4lIsj2Mw`. CSE maps `/` to `-` and `+` to `_`, the reverse of RFC 4648, until a pending fix lands, so the texts differ whenever the base64 contains `+` or `/`. That is why parity is a round trip within each library and never a cross-library string comparison.
- **ShortGuid decode** re-encodes the result and compares it with the input, a strict check the other decoders do not do. That is extra work by design.
- **Random source.** CSE and the `RandomNumberGenerator` row draw from a cryptographic RNG. The `Random.Shared` and hand-written rows do not, so they are not a like-for-like comparison with CSE. The WithReplacement row can return duplicates and does a different job; it has no CSE row and no baseline.
- **Cancellation exception.** CSE throws `OperationCanceledException`; `WaitAsync` throws its subclass `TaskCanceledException`.

---

## Feature matrix

This table comes from reading source, docs and nuspecs, plus the probe noted above. None of it was measured. Humanizer links point at tag v3.0.10 (`90f0cb1`), ShortGuid links at tag v2.0.0 (`835267f`).

| Feature | CSE | BCL | Humanizer 3.0.10 | ShortGuid 2.0.0 |
|---|---|---|---|---|
| snake / kebab / camel | Yes | `JsonNamingPolicy` (STJ 8+) | [`Underscore`/`Kebaberize`/`Camelize`](https://github.com/Humanizr/Humanizer/blob/90f0cb1d7427635f38b46350bb1dc5fca2164f2d/readme.md#underscore-dasherize-kebaberize) | n/a |
| Acronym split (`HTTPStatus`) | No | Yes | Yes | n/a |
| URL-safe GUID text | Yes (swapped alphabet until the fix) | `Base64Url` (net9+) | n/a | [Yes](https://github.com/csharpvitamins/csharpvitamins.shortguid/blob/835267feaf85da0651293c7295d5eb2e9f012990/readme.md#using-the-shortguid), RFC alphabet, strict decode |
| Unique random sample | Yes, cryptographic RNG | `RandomNumberGenerator.Shuffle` + slice; `GetItems` samples with replacement | n/a | n/a |
| Await with cancellation | `WithCancellation` (Task and ValueTask) | `Task.WaitAsync` (net6+, Task only) | n/a | n/a |
| License | MIT | MIT | MIT | MIT |

---

## Results

**Pending.** No full run has been done yet. The numbers will come from the default BenchmarkDotNet job on net8.0, net9.0, net10.0 and net11.0, and the reports will be committed under `benchmarks/results/json/{tfm}/results/`. Short-job (`--quick`) numbers are not published here.

---

## Caveats

- The `Base64Url` rows exist on net9.0 and later only.
- On net8.0 CSE runs its netstandard2.1 build, whose paths differ from the net9+ build.
- Pending CSE changes affect this suite: one moves `Guider` to the RFC 4648 alphabet, and one changes case conversion for non-Latin letters. The self-check has to be run again once they land.
- The `Random.Shared` rows use a non-cryptographic RNG. They are there to show what the cryptographic RNG costs, not as like-for-like rows.
- The CSE column is the working tree on branch `perf/bench-json`, which may not match the version published on NuGet.
- The feature matrix comes from reading source and docs.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project json                       # all TFMs (net8-net11), Json and Core classes, default job
./run-benchmarks.sh --project json --tfm net10.0         # one TFM
./run-benchmarks.sh --project json --quick --tfm net10.0 # one TFM, Short job, one filter per class
dotnet run -c Release -f net10.0 --project CSharpEssentials.Json.Comparison.Benchmarks -- --self-check
```

Without `--tfm`, `--quick` runs on net9.0, the script default. Short-job numbers cannot be compared with default-job numbers. Results are stored per framework under `benchmarks/results/json/{tfm}/results/`.
