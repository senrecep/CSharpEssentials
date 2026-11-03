# CSharpEssentials.Json: Performance benchmarks (draft)

This page compares CSharpEssentials.Json (CSE) with System.Text.Json (STJ) built-in features and with Macross.Json.Extensions 3.0.0. It covers string enums, flags enums, polymorphic types and multi-format `DateTime` reads. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11. The same project also holds the CSharpEssentials.Core classes, which are documented in [CSharpEssentials.Core/BENCHMARKS.md](../CSharpEssentials.Core/BENCHMARKS.md).

**Status: draft.** The suite builds, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet (see [Results](#results)).

The competitor is pinned exactly in `Directory.Packages.props`, and the pin is the latest stable release on NuGet as of 2026-11-03:

| Package | Version | Published | License | Asset the suite loads |
|---|---|---|---|---|
| Macross.Json.Extensions | 3.0.0 | 2022-06-15 | MIT (nuspec expression) | `lib/net6.0` on every TFM |

Macross 3.0.0 has no trim or AOT annotations. Its DLL uses `DynamicMethod` and `ILGenerator`, and its enum converter reflects over enum fields and `[EnumMember]` once per converter, so it is not AOT-ready.

The System.Text.Json version is the same for every row on a given TFM, but it is not always the copy in the framework:

| TFM | System.Text.Json the suite runs | Why |
|---|---|---|
| net8.0 | 9.0.4 from NuGet (assembly 9.0.0.0) | CSE.Json has no net8 target, so net8 runs its netstandard2.1 build, which references the STJ package. Macross's `lib/net6.0` asset has no package dependency; it binds to whichever System.Text.Json the app loads. The repo pins `System.Text.Json` `[9.0.4,)` with transitive pinning on, so every JSON row on net8, the BCL rows included, uses that package. |
| net9.0 | the framework copy (assembly 9.0.0.0) | The in-box assembly wins over the package. |
| net10.0 | the framework copy (assembly 10.0.0.0) | |
| net11.0 | the framework copy (assembly 11.0.0.0) | |

`--self-check` prints the loaded STJ assembly version and the TFM as its first line, which is where this table comes from.

---

## What was measured

The code is in `benchmarks/CSharpEssentials.Json.Comparison.Benchmarks/`, in four BenchmarkDotNet classes for this package.
- Every class has `[MemoryDiagnoser]`.
- CSE is the baseline in every category that has a CSE row.
- Each class checks parity in `[GlobalSetup]`, and `--self-check` runs the same check. Every row in a category must give the same result for the same input, or the run fails.
- Each library and BCL row has a comment above it that links to the documentation it follows.
- Every `JsonSerializerOptions` instance is built once in a `static readonly` field, from `new JsonSerializerOptions(JsonSerializerDefaults.Web)` unless noted.

| # | Class | Measured unit | Rows | Parity |
|---|---|---|---|---|
| 1 | `EnumComparison` | Write: `Serialize` of 16 `OrderStatus` values. Read: `Deserialize<OrderStatus[]>` of their wire names. ReadNumber: `Deserialize<OrderStatus>("2")`. InvalidThrow: read `"gone"` and catch the `JsonException`. InvalidFallback: read `"gone"` to the fallback member | CSE `AddEnumConventions(EnumConventions.Default)` (Data mode; Input mode for InvalidThrow), STJ `JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)`, Macross `JsonStringEnumMemberConverter(JsonNamingPolicy.SnakeCaseLower)`. InvalidFallback has CSE and Macross with `deserializationFailureFallbackValue` only | Write: exact JSON text. Read: the decoded values. InvalidThrow: `-1` for all |
| 2 | `EnumFlagsComparison` | Write and read `Permissions.Read \| Permissions.Write` | CSE, STJ, Macross, same options as above | The decoded value. Each payload is also pinned (see below) |
| 3 | `PolymorphicComparison` | `Serialize` of a dog typed as its abstract base, and `Deserialize` of each library's own payload | CSE `PolymorphicJsonConverterFactory` on Web options, STJ `[JsonPolymorphic]` + `[JsonDerivedType]` on plain Web options | Equal payload length, and `Dog:Rex:Labrador` after decoding |
| 4 | `DateTimeComparison` | `Deserialize<DateTime>` of `"2024-03-14T15:30:45.123Z"` (Iso) and of `"14-Mar-2024"` (LateList) | CSE `MultiFormatDateTimeConverterFactory` (as in its Readme), STJ built-in, STJ with the documented `DateTime.Parse` fallback converter. LateList has no built-in row: that converter rejects the input | Ticks and `Kind` (`Utc`) |

Inputs:
- `OrderStatus` is a `[StringEnum]` with `Pending, InProgress, OnHold, Shipped, Delivered, Cancelled` and an `[EnumFallback] Unknown`. All three libraries write the same snake_case names: `pending, in_progress, on_hold, shipped, delivered, cancelled, unknown`. The 16-value array cycles the six members other than `Unknown`, so only defined values are written.
- `Permissions` is a `[Flags, StringEnum]` with `None, Read, Write, Delete, Share`.
- The polymorphic rows use two hierarchies of the same shape (`CseAnimal`/`CseDog`/`CseCat` and `StjAnimal`/`StjDog`/`StjCat`, with `Name`, `Breed` and `Lives`). The STJ hierarchy declares `$type` as its discriminator and uses the CLR full name as the discriminator value, so both payloads put `$type` first and have the same length. Separate hierarchies are needed because a later CSE change leaves `[JsonPolymorphic]` types to STJ, and a shared type would then measure STJ in both rows.
- The fallback DateTime converter is the documented "Parse as a fallback" sample. It adds `CultureInfo.InvariantCulture` and `AdjustToUniversal | AssumeUniversal`, which the sample does not have, so the result is UTC like CSE's. Its Write uses the round-trip `O` format instead of the sample's `dd/MM/yyyy`; Write is not timed.

On net8.0 CSE runs its netstandard2.1 build, because CSharpEssentials.Json has no net8 target. On net9.0 and later CSE runs the build for that TFM.

---

## Semantic differences

The parity checks use inputs on which every row agrees. The libraries still differ in ways the numbers alone do not show. These were checked with a probe on net8 and net10 and are not timed.

- **Member-name and case-variant reads.** CSE and STJ accept `"InProgress"` and `"inprogress"`. Macross throws on both, or returns the fallback value when one is configured.
- **Undefined numbers.** STJ and Macross read `99` as `(OrderStatus)99`. CSE in Data mode returns `Unknown`, and in Input mode throws.
- **InvalidThrow.** Every row ends in a `JsonException` (CSE throws its subclass `EnumValueJsonException`). The messages and the data each library builds differ, and the cost of the exception dominates the row.
- **InvalidFallback.** STJ has no fallback member, so it has no row. The Macross fallback also catches member-name and case-variant reads, which CSE reads as the member itself.
- **Flags wire format.** The rows do not do the same work. CSE writes a JSON array; STJ and Macross write a comma string:

  | Library | Payload for `Read \| Write` |
  |---|---|
  | CSE | `["read","write"]` |
  | STJ | `"read, write"` |
  | Macross | `"read, write"` |

  Each row reads its own payload. The parity check compares the decoded value, never the text.
- **Polymorphism.** CSE finds the derived types by scanning the loaded assemblies once per base type (a `Lazy` cache, warmed in setup) and uses the CLR full name as the discriminator. STJ needs the `[JsonDerivedType]` list. The CSE row adds `PolymorphicJsonConverterFactory` to plain Web options, not the Readme's `ConvertToJson()`/`DefaultOptions`, which also turns on IgnoreCycles, relaxed escaping and ignore-nulls. Both rows then share the same base options, and the number is the factory's cost alone. CSE is reflection-based and marked `RequiresUnreferencedCode`/`RequiresDynamicCode`; the STJ attributes also work with source generation.
- **DateTime LateList.** CSE walks its fixed format list until one matches. `dd-MMM-yyyy` is entry 30 of 39 on this branch, and entry 27 of 34 after the pending format-list cleanup. The fallback converter calls `DateTime.Parse`, which uses culture heuristics instead of a list.
- **DateTime write.** Not covered: the formats differ (CSE writes `yyyy-MM-ddTHH:mm:ss.ffffffzzz`, STJ writes `O`).
- **Enum dictionary keys.** Not covered in this phase. Macross 3.0 says it supports them, but no parity probe was run.

---

## Feature matrix

This table comes from reading source, docs and nuspecs, plus the probe noted above. None of it was measured. Macross links point at commit `6c65085` (3.0.0).

| Feature | CSE | STJ built-in | Macross 3.0.0 |
|---|---|---|---|
| Enum as string with a naming policy | Yes | Yes, `JsonStringEnumConverter(policy)` | Yes |
| Custom member names | `[JsonStringEnumMemberName]` or `[EnumMember]` | `[JsonStringEnumMemberName]` (STJ 9+) | `[EnumMember]` or `[JsonPropertyName]` |
| Reads the member name in any case | Yes | Yes | No (probe) |
| Unknown string to a fallback member | `[EnumFallback]` (Data mode) | No | [`deserializationFailureFallbackValue`](https://github.com/Macross-Software/core/blob/6c650854a72533fa25d30e4c0ff27374275134f1/ClassLibraries/Macross.Json.Extensions/README.md#enumerations) |
| Strict mode rejects undefined numbers | Yes (Input mode) | No; `99` is accepted by default, and `allowIntegerValues: false` rejects every number, defined ones too | No; `99` is accepted by default, and `allowIntegerValues: false` rejects every number, defined ones too |
| Flags wire format | JSON array | Comma string | Comma string |
| AOT / source-gen friendly enum path | Yes (generated metadata) | `JsonStringEnumConverter<T>` yes, the non-generic converter no | No |
| Polymorphism discriminator | Automatic, CLR full name | Declared with `[JsonDerivedType]` | None |
| Polymorphism needs a derived-type list | No (assembly scan) | Yes | n/a |
| Polymorphism under AOT | No (RUC/RDC) | Yes | n/a |
| Multi-format DateTime read | A fixed list of 39 formats (34 after the cleanup) plus Unix seconds | ISO 8601-1:2019 only | Microsoft `/Date()/` only (not benchmarked) |
| 7-digit ISO fraction (the `O` round-trip form) | **No, see Caveats** | Yes | n/a |
| License | MIT | MIT | MIT |

---

## Results

**Pending.** No full run has been done yet. The numbers will come from the default BenchmarkDotNet job on net8.0, net9.0, net10.0 and net11.0, and the reports will be committed under `benchmarks/results/json/{tfm}/results/`. Short-job (`--quick`) numbers are not published here.

---

## Caveats

- **Known CSE bug.** `MultiFormatDateTimeConverter` has no format with a 7-digit fraction, so `"2024-03-14T15:30:45.1234567Z"` throws a `JsonException`. That is the form STJ itself writes for a `DateTime`, so a payload STJ wrote cannot be read back through `DefaultOptions`. The Iso input uses a 3-digit fraction for this reason.
- On net8.0 every JSON row runs the STJ 9.0.4 package, not the in-box STJ 8. Numbers for net8 are not numbers for STJ 8.
- Pending CSE changes affect this suite: one writes undefined enum values as their number and reworks enum writes, one leaves `[JsonPolymorphic]` types to STJ, and one trims the DateTime format list. The self-check has to be run again once they land.
- Macross 3.0.0 dates from 2022 and targets net6.0. It is still the latest stable release.
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
