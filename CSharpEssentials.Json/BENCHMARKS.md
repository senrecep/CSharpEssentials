# CSharpEssentials.Json: performance benchmarks

CSharpEssentials.Json (CSE) compared with System.Text.Json (STJ) built-in features and with Macross.Json.Extensions 3.0.0. It covers string enums, flags enums, polymorphic types and multi-format `DateTime` reads. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11. The same project also holds the CSharpEssentials.Core classes, which are documented in [CSharpEssentials.Core/BENCHMARKS.md](../CSharpEssentials.Core/BENCHMARKS.md).

Short version: CSE is slower than STJ in every timed category except the invalid-input rows, and it is faster than Macross on enum writes. The numbers are in [Results](#results), and every row where CSE loses is listed in [Where CSE loses](#where-cse-loses).

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
- The polymorphic rows use two hierarchies of the same shape (`CseAnimal`/`CseDog`/`CseCat` and `StjAnimal`/`StjDog`/`StjCat`, with `Name`, `Breed` and `Lives`). The STJ hierarchy declares `$type` as its discriminator and uses the CLR full name as the discriminator value, so both payloads put `$type` first and have the same length. Separate hierarchies are needed because CSE leaves `[JsonPolymorphic]` types to STJ, so a shared type would measure STJ in both rows.
- The fallback DateTime converter is the documented "Parse as a fallback" sample. It adds `CultureInfo.InvariantCulture` and `AdjustToUniversal | AssumeUniversal`, which the sample does not have, so the result is UTC like CSE's. Its Write uses the round-trip `O` format instead of the sample's `dd/MM/yyyy`; Write is not timed.

On net8.0 CSE runs its netstandard2.1 build, because CSharpEssentials.Json has no net8 target. On net9.0 and later CSE runs the build for that TFM.

---

## Semantic differences

The parity checks use inputs on which every row agrees. The libraries still differ in ways the numbers alone do not show. These were checked with a probe on net8 and net10 and are not timed.

- Member-name and case-variant reads: CSE and STJ accept `"InProgress"` and `"inprogress"`. Macross throws on both, or returns the fallback value when one is configured.
- Undefined numbers: STJ and Macross read `99` as `(OrderStatus)99`. CSE in Data mode returns `Unknown`, and in Input mode throws.
- InvalidThrow: every row ends in a `JsonException` (CSE throws its subclass `EnumValueJsonException`). The messages and the data each library builds differ, and the cost of the exception dominates the row.
- InvalidFallback: STJ has no fallback member, so it has no row. The Macross fallback also catches member-name and case-variant reads, which CSE reads as the member itself.
- Flags wire format: the rows do not do the same work. CSE writes a JSON array; STJ and Macross write a comma string:

  | Library | Payload for `Read \| Write` |
  |---|---|
  | CSE | `["read","write"]` |
  | STJ | `"read, write"` |
  | Macross | `"read, write"` |

  Each row reads its own payload. The parity check compares the decoded value, never the text.
- Polymorphism: CSE finds the derived types by scanning the loaded assemblies once per base type (a `Lazy` cache, warmed in setup) and uses the CLR full name as the discriminator. STJ needs the `[JsonDerivedType]` list. The CSE row adds `PolymorphicJsonConverterFactory` to plain Web options, not the Readme's `ConvertToJson()`/`DefaultOptions`, which also turns on IgnoreCycles, relaxed escaping and ignore-nulls. Both rows then share the same base options, and the number is the factory's cost alone. The factory is reflection-based and marked `RequiresUnreferencedCode`/`RequiresDynamicCode`; the STJ attributes also work with source generation. CSE's generated path (`[JsonTypeDiscriminator]` with `AddPolymorphism()`, built on STJ polymorphism, trim and AOT safe) is not benchmarked.
- DateTime Iso: CSE reads the ISO shapes of its leading formats straight from the UTF-8 bytes (`TryReadIso`) before it looks at the format list, so the Iso row does not walk the list.
- DateTime LateList: CSE walks its fixed format list until one matches. `dd-MMM-yyyy` is entry 27 of 34. The fallback converter calls `DateTime.Parse`, which uses culture heuristics instead of a list.
- DateTime write: not covered, because the formats differ (CSE writes `yyyy-MM-ddTHH:mm:ss.ffffffzzz`, STJ writes `O`).
- Enum dictionary keys: not covered. Macross 3.0 says it supports them, but no parity probe was run.

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
| Polymorphism discriminator | `PolymorphicJsonConverterFactory`: automatic, CLR full name. `[JsonTypeDiscriminator]` + `AddPolymorphism()`: declared per derived type | Declared with `[JsonDerivedType]` | None |
| Polymorphism needs a derived-type list | No: assembly scan (factory) or generated registrations (`AddPolymorphism`) | Yes | n/a |
| Polymorphism under AOT | Factory no (RUC/RDC); `AddPolymorphism` yes (`benchmarks/json-aot`) | Yes | n/a |
| Multi-format DateTime read | A fixed list of 35 formats plus Unix seconds | ISO 8601-1:2019 only | The `/Date()/` format only (not benchmarked) |
| 7-digit ISO fraction (the `O` round-trip form) | Yes, since the fix after this run; see Caveats | Yes | n/a |
| License | MIT | MIT | MIT |

---

## Results

Each cell is `Mean (Ratio), Allocated`, copied from the BenchmarkDotNet github report for that TFM. The ratio is against the CSE row of the same category, so a ratio below 1.00 means that row took less time than CSE. Where a report shows `-` for Allocated, the cell says `0 B`. No row in this suite was flagged as noisy (no row kept a CV above 5%), and no mean is near 0 ns. Reports: `benchmarks/results/json/{tfm}/results/CSharpEssentials.Json.Comparison.Benchmarks.<Class>-report-github.md`, for example [net10.0 EnumComparison](../benchmarks/results/json/net10.0/results/CSharpEssentials.Json.Comparison.Benchmarks.EnumComparison-report-github.md).

### Enums (`EnumComparison`)

Write, 16 values:

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Write` | 388.36 ns (1.00), 384 B | 431.13 ns (1.00), 384 B | 394.33 ns (1.00), 384 B | 372.48 ns (1.00), 384 B |
| `Bcl_Write` | 368.09 ns (0.95), 384 B | 352.30 ns (0.82), 384 B | 322.93 ns (0.82), 384 B | 321.64 ns (0.86), 384 B |
| `Macross_Write` | 582.15 ns (1.50), 384 B | 677.14 ns (1.57), 384 B | 572.93 ns (1.45), 384 B | 488.50 ns (1.31), 384 B |

Read, 16 values:

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Read` | 1,196.57 ns (1.00), 304 B | 1,155.36 ns (1.00), 304 B | 1,143.03 ns (1.00), 304 B | 970.96 ns (1.00), 304 B |
| `Bcl_Read` | 1,120.36 ns (0.94), 968 B | 988.82 ns (0.86), 304 B | 1,112.77 ns (0.97), 304 B | 838.82 ns (0.86), 304 B |
| `Macross_Read` | 1,149.52 ns (0.96), 968 B | 1,163.00 ns (1.01), 968 B | 910.53 ns (0.80), 968 B | 925.57 ns (0.95), 968 B |

ReadNumber:

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_ReadNumber` | 71.30 ns (1.00), 0 B | 75.99 ns (1.00), 0 B | 60.20 ns (1.00), 0 B | 62.14 ns (1.00), 0 B |
| `Bcl_ReadNumber` | 61.87 ns (0.87), 0 B | 64.75 ns (0.85), 0 B | 51.49 ns (0.86), 0 B | 52.99 ns (0.85), 0 B |
| `Macross_ReadNumber` | 120.86 ns (1.70), 24 B | 79.43 ns (1.05), 24 B | 64.33 ns (1.07), 24 B | 64.82 ns (1.04), 24 B |

InvalidThrow:

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_InvalidThrow` | 26,026.04 ns (1.00), 1360 B | 9,767.04 ns (1.00), 1208 B | 5,348.06 ns (1.00), 1208 B | 4,306.06 ns (1.00), 1208 B |
| `Bcl_InvalidThrow` | 27,470.87 ns (1.06), 1488 B | 10,369.04 ns (1.06), 1648 B | 5,668.48 ns (1.06), 1648 B | 4,708.88 ns (1.09), 1648 B |
| `Macross_InvalidThrow` | 25,562.08 ns (0.98), 1504 B | 9,575.05 ns (0.98), 1432 B | 5,267.60 ns (0.98), 1432 B | 4,181.40 ns (0.97), 1432 B |

InvalidFallback (no STJ row, STJ has no fallback):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_InvalidFallback` | 100.89 ns (1.00), 0 B | 97.71 ns (1.00), 0 B | 80.49 ns (1.00), 0 B | 84.08 ns (1.00), 0 B |
| `Macross_InvalidFallback` | 111.77 ns (1.11), 32 B | 106.25 ns (1.09), 32 B | 88.53 ns (1.10), 32 B | 83.54 ns (0.99), 32 B |

### Flags (`EnumFlagsComparison`)

The rows do not do the same work: CSE writes and reads a JSON array, STJ and Macross a comma string.

FlagsWrite:

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_FlagsWrite` | 157.9 ns (1.00), 56 B | 139.3 ns (1.00), 56 B | 138.46 ns (1.00), 56 B | 122.73 ns (1.00), 56 B |
| `Bcl_FlagsWrite` | 112.2 ns (0.71), 48 B | 105.6 ns (0.76), 48 B | 88.02 ns (0.64), 48 B | 77.50 ns (0.63), 48 B |
| `Macross_FlagsWrite` | 123.4 ns (0.78), 48 B | 119.2 ns (0.86), 48 B | 103.99 ns (0.75), 48 B | 84.21 ns (0.69), 48 B |

FlagsRead:

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_FlagsRead` | 173.0 ns (1.00), 0 B | 174.4 ns (1.00), 0 B | 150.69 ns (1.00), 0 B | 149.19 ns (1.00), 0 B |
| `Bcl_FlagsRead` | 112.7 ns (0.65), 48 B | 106.4 ns (0.61), 0 B | 81.97 ns (0.54), 0 B | 82.61 ns (0.55), 0 B |
| `Macross_FlagsRead` | 106.2 ns (0.61), 48 B | 111.3 ns (0.64), 48 B | 88.04 ns (0.58), 48 B | 86.60 ns (0.58), 48 B |

### Polymorphism (`PolymorphicComparison`)

Serialize:

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Serialize` | 871.4 ns (1.00), 624 B | 875.9 ns (1.00), 624 B | 763.5 ns (1.00), 640 B | 673.1 ns (1.00), 640 B |
| `Stj_Serialize` | 347.9 ns (0.40), 528 B | 352.8 ns (0.40), 528 B | 296.2 ns (0.39), 528 B | 286.3 ns (0.43), 528 B |

Deserialize:

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Deserialize` | 827.5 ns (1.00), 232 B | 781.2 ns (1.00), 232 B | 668.7 ns (1.00), 232 B | 659.0 ns (1.00), 232 B |
| `Stj_Deserialize` | 467.6 ns (0.57), 232 B | 438.8 ns (0.56), 232 B | 378.5 ns (0.57), 232 B | 392.9 ns (0.60), 232 B |

### DateTime (`DateTimeComparison`)

Iso, `"2024-03-14T15:30:45.123Z"`:

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_Iso` | 108.01 ns (1.00), 0 B | 121.1 ns (1.00), 0 B | 94.65 ns (1.00), 0 B | 96.76 ns (1.00), 0 B |
| `Bcl_Iso` | 89.51 ns (0.83), 0 B | 100.7 ns (0.83), 0 B | 82.41 ns (0.87), 0 B | 83.16 ns (0.86), 0 B |
| `BclFallback_Iso` | 107.70 ns (1.00), 0 B | 103.8 ns (0.86), 0 B | 84.04 ns (0.89), 0 B | 86.84 ns (0.90), 0 B |

LateList, `"14-Mar-2024"` (no built-in row, that converter rejects the input):

| Row | net8.0 | net9.0 | net10.0 | net11.0 |
|---|---|---|---|---|
| `CSharpEssentials_LateList` | 1,251.91 ns (1.00), 48 B | 1,263.0 ns (1.00), 48 B | 1,249.50 ns (1.00), 48 B | 1,306.05 ns (1.00), 48 B |
| `BclFallback_LateList` | 323.40 ns (0.26), 48 B | 319.0 ns (0.25), 48 B | 288.28 ns (0.23), 48 B | 280.85 ns (0.22), 48 B |

### Where CSE wins

- Enum Write against Macross on every TFM: 388.36 ns against 582.15 ns on net8.0 (ratio 1.50), 372.48 ns against 488.50 ns on net11.0 (ratio 1.31). All three libraries allocate 384 B.
- Enum ReadNumber against Macross: ratio 1.70 on net8.0, 1.04 to 1.07 on net9.0 to net11.0. CSE allocates 0 B, Macross 24 B.
- Enum InvalidFallback against Macross on net8.0 to net10.0 (ratio 1.09 to 1.11), with 0 B against 32 B. On net11.0 Macross is faster (see below).
- Enum InvalidThrow against STJ on every TFM (ratio 1.06 to 1.09), and CSE allocates the least of the three rows (1208 B on net9.0 to net11.0, 1360 B on net8.0).
- Enum Read on net8.0: CSE allocates 304 B, STJ 9.0.4 and Macross 968 B. On net9.0 and later STJ also allocates 304 B; Macross stays at 968 B.
- FlagsRead allocates 0 B with CSE; Macross allocates 48 B on every TFM and STJ 48 B on net8.0.

### Where CSE loses

Every row where CSE is slower or allocates more than a competitor:

- Enum Write against STJ, every TFM: net8.0 368.09 ns against CSE 388.36 ns (0.95); net9.0 352.30 ns against 431.13 ns (0.82); net10.0 322.93 ns against 394.33 ns (0.82); net11.0 321.64 ns against 372.48 ns (0.86).
- Enum Read against STJ, every TFM (ratio 0.86 to 0.97), and against Macross on net8.0 (1,149.52 ns against 1,196.57 ns, 0.96), net10.0 (910.53 ns against 1,143.03 ns, 0.80) and net11.0 (925.57 ns against 970.96 ns, 0.95). Macross is slower only on net9.0 (1.01).
- Enum ReadNumber against STJ, every TFM (ratio 0.85 to 0.87), for example 51.49 ns against 60.20 ns on net10.0.
- Enum InvalidThrow against Macross, every TFM (ratio 0.97 to 0.98), for example 4,181.40 ns against 4,306.06 ns on net11.0. CSE still allocates less.
- Enum InvalidFallback against Macross on net11.0: 83.54 ns against 84.08 ns (0.99). CSE still allocates 0 B against 32 B.
- Flags, every row and every TFM. FlagsWrite: STJ ratio 0.63 to 0.76, Macross 0.69 to 0.86, and CSE allocates 56 B against 48 B. FlagsRead: STJ ratio 0.54 to 0.65, Macross 0.58 to 0.64. The payloads differ, so this is not the same work.
- Polymorphic Serialize, every TFM: STJ ratio 0.39 to 0.43, and CSE allocates more (624 B on net8.0 and net9.0, 640 B on net10.0 and net11.0, against 528 B). On net10.0: 763.5 ns against 296.2 ns. CSE serializes the value to a `JsonElement` and then copies every property behind its `$type`.
- Polymorphic Deserialize, every TFM: STJ ratio 0.56 to 0.60, same allocation (232 B). On net10.0: 668.7 ns against 378.5 ns. CSE scans the object once with a copy of the reader to find `$type`, then deserializes the derived type.
- DateTime Iso against the built-in converter, every TFM (ratio 0.83 to 0.87), and against the fallback converter on net9.0 to net11.0 (0.86 to 0.90). On net8.0 the fallback converter and CSE are level (107.70 ns against 108.01 ns, ratio 1.00).
- DateTime LateList, every TFM: the `DateTime.Parse` fallback converter has ratio 0.22 to 0.26 at the same 48 B. On net11.0: 280.85 ns against 1,306.05 ns. CSE tries the list in order and `dd-MMM-yyyy` is entry 27.

### How these numbers were produced

- BenchmarkDotNet v0.14.0 default job (no `--quick`), full runs, one TFM after another on a 6-core, 12-thread x64 desktop CPU on Linux (RyuJIT AVX2). The report headers have the exact OS and runtime builds.
- SDK 11.0.100-rc.1.26425.128. Runtimes: .NET 8.0.30, 9.0.19, 10.0.11 and 11.0.0.
- The CSE code is the local `integ/all` branch, which merges every open pull request (commit `fe91b26a`). It may not match the version published on NuGet.
- Load: the 1-minute load average was 1.43 when the json suite started and 1.74 when it ended (5-minute 1.47 and 1.77). Only the start and end were sampled.
- Rows whose CV stayed above 5% were rerun up to twice; no row from this suite needed it.
- Results path: `benchmarks/results/json/{tfm}/results/` (github md, csv, html and json for each class).

---

## Caveats

- The benchmark found a CSE bug, since fixed: `MultiFormatDateTimeConverter` had no format with a 7-digit fraction and its ISO fast path did not take that length, so `"2024-03-14T15:30:45.1234567Z"`, the form STJ writes for a `DateTime`, threw a `JsonException`. The default list now has `yyyy-MM-ddTHH:mm:ss.FFFFFFFK` and the fast path takes 1 to 7 fraction digits. The Iso input still uses a 3-digit fraction, and the numbers here come from the run before the fix.
- On net8.0 every JSON row runs the STJ 9.0.4 package, not the in-box STJ 8. Numbers for net8 are not numbers for STJ 8.
- Macross 3.0.0 dates from 2022 and targets net6.0. It is still the latest stable release.

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
