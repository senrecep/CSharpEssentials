# CSharpEssentials.Enums: Performance benchmarks

CSharpEssentials.Enums (CSE) compared with Enums.NET 5.0.0, NetEscapades.EnumGenerators 1.0.0-beta21 and the plain `System.Enum` API (BCL). Measured with BenchmarkDotNet v0.14.0 under .NET 8, 9, 10 and 11, plus a Native AOT publish of one sample per library.

NetEscapades.EnumGenerators has never shipped a stable release. Every 1.x version on NuGet is a beta, so the suite pins the latest prerelease, 1.0.0-beta21 (published 2026-03-09), exactly in `Directory.Packages.props`. Enums.NET 5.0.0 is its latest stable release.

CSE is a source generator plus a small runtime library. `[StringEnum]` makes the generator write an `{Enum}Extensions` class and the enum's metadata. Some generated helpers are `switch` statements over constants (`ToOptimizedString`, `ToWireName`, `IsDefined(string)`, `GetNames`, `GetValues`). Others forward to the runtime metadata (`IsDefined(value)`, `TryParseWire`, descriptions). That split explains most of the results below.

The same suite has two newer classes that have not had a full run yet: boxed formatting and reflection-built names and values. They are described in [Pending: boxed formatting and reflection-built metadata](#pending-boxed-formatting-and-reflection-built-metadata).

---

## What was measured

The suite has six BenchmarkDotNet classes in `benchmarks/CSharpEssentials.Enums.Comparison.Benchmarks/` plus a Native AOT script. Every class uses `[MemoryDiagnoser]` and has CSE as the baseline where CSE has a column. Each class checks parity in `[GlobalSetup]` (also run with `--self-check`): every column must return the same string, the same parsed value, the same bool or the same sum for the same input, or the run fails. Rows that do different work sit in their own category and are compared with nothing.

| # | Class | Measured unit | Columns |
|---|---|---|---|
| 1 | `ToStringComparison` | Format `Color.Indigo` (7th of 8 members) as "Indigo", and the undefined `(Color)99` as "99" | CSE `ToOptimizedString`, BCL `ToString`, Enums.NET `AsString`, NetEscapades `ToStringFast`. Separate: CSE `ToWireName` ("indigo") |
| 2 | `ParseComparison` | Parse `Status.PendingApproval` (11th of 20 members) by name, case-sensitive ("PendingApproval") and ignore-case ("pendingapproval"), plus an unknown name ("Unknown" / "unknown") | CSE `EnumValueParser.TryParse` (case-sensitive) and generated `TryParseWire` (ignore-case), CSE `IsDefined(string)` (bool only), BCL `Enum.TryParse`, Enums.NET `Enums.TryParse`, NetEscapades generated `TryParse` |
| 3 | `IsDefinedComparison` | Whether `Status.Cancelled` (75) and the undefined `(Status)76` are declared | CSE, BCL, Enums.NET, NetEscapades `IsDefined` |
| 4 | `GetValuesComparison` | Get all 20 `Status` values (summed as ints) or names (summed lengths) | CSE, BCL, Enums.NET, NetEscapades `GetValues` / `GetNames` |
| 5 | `FlagsComparison` | `Permissions` value `Write \| Execute \| Delete`: `HasFlag(Write)` and formatting as "Write, Execute, Delete" | Format: CSE `ToOptimizedString`, BCL `ToString`, Enums.NET `FlagEnums.FormatFlags`, NetEscapades `ToStringFast`. HasFlag: BCL `HasFlag` (baseline), Enums.NET `HasAllFlags`, NetEscapades `HasFlagFast`. Separate: CSE `ToWireName` ("write,execute,delete") |
| 6 | `DescriptionComparison` | Read the `[Description]` text of `Status.PendingApproval` ("Waiting for approval") | CSE `EnumMetadata.Get<T>().TryGetMember(...).Description`, Enums.NET `AsString(EnumFormat.Description)`, NetEscapades `ToStringFast(useMetadataAttributes: true)` |
| 7 | Native AOT (`benchmarks/enums-aot/run-enums-aot.sh`, not BenchmarkDotNet) | One sample per library, `PublishAot=true`, net10, linux-x64 | BclAot, CseAot, EnumsNetAot, NetEscapadesAot |

Columns without an idiomatic equivalent are left out, and the class XML doc says why. CSE has no `HasFlag` helper, so the HasFlag category has no CSE column. The BCL has no description API, so `DescriptionComparison` has no BCL column.

Inputs: `Color` (8 contiguous int members), `Status` (20 members with non-contiguous values; one member carries `[Description]`) and `Permissions` (`[Flags]`, 8 bits, `None = 0`, one combination member). Each enum carries every library's attribute at once: `[StringEnum]` for CSE, `[EnumExtensions]` for NetEscapades and none for Enums.NET.

The numbers come from the reports committed under `benchmarks/results/enums/{tfm}/results/` and `benchmarks/results/enums/aot/aot-report.md`.

Environment:

| Property | Value |
|---|---|
| Machine | 6 cores / 12 threads (x64, RyuJIT AVX2), desktop session in the background |
| OS | Linux, kernel 7.3.0-4-cachyos |
| Run order | AOT script (finished 12:36:56 local), then net10 (12:37) → net9 (12:58) → net8 (13:19) → net11 (13:40). Load averages were recorded only for the AOT script (1.92 2.23 2.70 at its start) |
| Tool | BenchmarkDotNet v0.14.0, default job, [MemoryDiagnoser] |
| .NET SDK | 11.0.100-rc.1.26425.128 |
| Runtimes tested | .NET 8.0.30, .NET 9.0.19, .NET 10.0.11, .NET 11.0.0 |
| Enums.NET | 5.0.0 (latest stable) |
| NetEscapades.EnumGenerators | 1.0.0-beta21 (prerelease; no stable release exists) |
| CSharpEssentials.Enums (CSE) | 7.0.0 working tree, referenced as a project with its generator as an analyzer reference. It targets net9.0, net10.0, net11.0, netstandard2.1 and netstandard2.0, so the net8 column runs its netstandard2.1 build |

---

## Executive summary

Primary runtime: net10. A tie is a difference within the sum of the two BenchmarkDotNet error margins. Ratios are recomputed from the means and can differ by 0.01 from BenchmarkDotNet's Ratio column.

CSE losses come first. Every row below that names a loss is outside error on net10 unless the row says otherwise.

| Finding | Details |
|---|---|
| IsDefined(value): CSE is slower than both competitors on every TFM (CSE loss) | net10: 3.9082 ns (defined) and 2.8831 ns (undefined) against Enums.NET 0.8611 / 1.2988 ns and NetEscapades 0.4209 / 0.7315 ns (4.54× and 9.29× for the defined value). CSE beats the BCL on net10 and net11 but is slower than the BCL on net8 (8.1692 vs 5.0849 ns) and net9 (7.0025 vs 5.7329 ns). The generated method forwards to the runtime metadata: a delegate call plus a `Dictionary<ulong, ...>` lookup |
| Description: CSE is the slowest column on every TFM (CSE loss) | net10: 3.7150 ns against Enums.NET 2.6899 ns (1.38×) and NetEscapades 0.9021 ns (4.12×). CSE generates no description method; the row is a metadata lookup through the same path as IsDefined |
| Invalid parse input allocates 88 B in CSE and nothing in the competitors (CSE loss) | net10, net9 and net11: 88 B in both CSE parse rows; BCL, Enums.NET and NetEscapades allocate 0 B. CSE builds an `EnumValueError` for every rejected input, and `TryParseWire` discards it |
| Undefined value ToString: CSE boxes and allocates 24 B (CSE loss) | net10: 12.8220 ns / 24 B against Enums.NET 2.0876 ns and NetEscapades 1.6986 ns, both 0 B (6.14× and 7.55×). The BCL is also faster on net10 (11.8073 ns, 0.92×), net9 and net11; CSE beats it only on net8. `ToOptimizedString` falls back to `Enum.ToString()` for an undefined value |
| Valid parse: CSE is the slowest library in some modes, by TFM (CSE loss) | net10: CSE is slower than Enums.NET in both modes (22.1890 vs 17.8628 ns case-sensitive, 51.9075 vs 35.9320 ns ignore-case) and slower than NetEscapades in ignore-case (51.9075 vs 44.7521 ns). Only the BCL is slower there (53.6330 ns). On net8 and net9 CSE is the slowest column of all four in both modes (net8 ignore-case 96.458 ns against 28.935-36.372 ns). On net11 ignore-case CSE ties NetEscapades for slowest (33.1584 vs 33.2026 ns). The BCL is faster than CSE on valid input on every TFM except net10: net11 14.7735 vs 20.6890 ns case-sensitive and 17.9225 vs 33.1584 ns ignore-case |
| net8 parse allocates on every call, valid or not (CSE loss) | net8 runs the netstandard2.1 build, whose name lookup copies the input into a string for every probe: 112 B (case-sensitive valid), 208 B (case-sensitive invalid), 168 B (ignore-case valid), 248 B (ignore-case invalid). Competitors allocate 0 B. net9+ uses an allocation-free `FrozenDictionary` span lookup |
| Invalid parse: CSE is slower than Enums.NET in both modes and than NetEscapades in ignore-case (CSE loss) | net10: 39.8918 vs 13.3405 ns (2.99×) case-sensitive; 60.5683 vs 14.6183 ns (4.14×) and vs NetEscapades 58.1841 ns (1.04×) ignore-case. On net11 ignore-case CSE ties NetEscapades (58.9579 vs 58.3552 ns); on net8 NetEscapades is also faster case-sensitive (58.871 vs 75.539 ns). The BCL is faster than CSE in both modes on net8, net9 and net11: case-sensitive 38.349 vs 75.539 ns, 35.976 vs 55.568 ns and 18.2234 vs 40.7442 ns; ignore-case 48.218 vs 104.523 ns, 42.050 vs 73.439 ns and 24.9442 vs 58.9579 ns |
| GetNames: Enums.NET is faster and allocates nothing (CSE loss) | net10: 19.09 ns / 184 B against 16.30 ns / 0 B (1.17×). Enums.NET returns a cached list; CSE builds a new array per call. CSE ties NetEscapades (18.77 ns) |
| GetValues: NetEscapades is slightly faster (CSE loss) | net10: 11.98 vs 11.46 ns (1.05×), both 0 B. net11: 9.415 vs 9.331 ns, outside error. On net8 and net9 the three are within 0.25 ns (net8 15.79 vs 15.61/15.62 ns, net9 15.84 vs 15.61/15.61 ns) and CSE allocates 104 B per call (so does NetEscapades); Enums.NET allocates 0 B |
| Flags format: the BCL is slightly faster (CSE loss on net10 and net8) | net10: 36.1870 vs 34.8196 ns (1.04×), same 96 B. net8: 48.2274 ns against BCL 44.2197 ns and NetEscapades 37.9471 ns. Ties on net9 and net11. CSE falls back to `Enum.ToString()` for an undeclared combination |
| Native AOT: CSE's binary is the second largest (CSE loss) | 1.9 MB against 1.2 MB for NetEscapades and the BCL sample; Enums.NET is 2.0 MB. All four publish and run; CSE has 0 IL warnings |
| ToString of a defined value: CSE ties NetEscapades on net10 and wins against Enums.NET and the BCL | net10: 0.5439 ns against NetEscapades 0.5437 ns (tie), Enums.NET 1.1192 ns (2.06×) and BCL 8.6687 ns / 24 B (15.94×). NetEscapades is faster on net9 (0.6273 vs 0.9135 ns) and net11 (0.5599 vs 0.8345 ns); net8 is a tie |
| Valid case-sensitive parse: CSE beats NetEscapades and the BCL on net10 | 22.1890 ns against NetEscapades 28.2404 ns and BCL 43.0744 ns. The BCL win holds on net10 only and is weak evidence (see Cross-TFM notes); the BCL is faster than CSE in every parse row on net8, net9 and net11. This row is the runtime `EnumValueParser`, not generated code. The generated `IsDefined(string)` name check takes 0.7398 ns but returns only a bool |
| GetValues and GetNames: CSE beats the BCL on every TFM | net10: 11.98 vs 31.23 ns and 19.09 vs 33.94 ns |
| Flags format: CSE beats Enums.NET on every TFM | net10: 36.1870 ns / 96 B against 84.5527 ns / 280 B |
| Native AOT: CSE, NetEscapades and the BCL publish with 0 IL warnings and run; Enums.NET publishes with 4 and runs | Enums.NET 5.0.0: IL2071, IL2075, IL2080, IL3050 from its reflection-based cache, all ILC. Every sample prints `parsed 20 checksum 971 ok` |

---

## Results by scenario

All tables are net10 unless stated. "vs CSE" divides by the CSE mean of the same category.

### 1. ToString (member name)

| Input | Variant | Mean | Error | vs CSE | Allocated |
|---|---|---:|---:|---:|---:|
| Defined | CSE `ToOptimizedString` | 0.5439 ns | 0.0031 ns | 1.00 | 0 B |
| Defined | BCL `ToString` | 8.6687 ns | 0.0652 ns | 15.94 | 24 B |
| Defined | Enums.NET `AsString` | 1.1192 ns | 0.0016 ns | 2.06 | 0 B |
| Defined | NetEscapades `ToStringFast` | 0.5437 ns | 0.0039 ns | 1.00 | 0 B |
| Undefined | CSE `ToOptimizedString` | 12.8220 ns | 0.1146 ns | 1.00 | 24 B |
| Undefined | BCL `ToString` | 11.8073 ns | 0.0705 ns | 0.92 | 24 B |
| Undefined | Enums.NET `AsString` | 2.0876 ns | 0.0037 ns | 0.16 | 0 B |
| Undefined | NetEscapades `ToStringFast` | 1.6986 ns | 0.0045 ns | 0.13 | 0 B |
| WireName (different work) | CSE `ToWireName` | 0.5423 ns | 0.0015 ns | n/a | 0 B |

- For a defined value CSE and NetEscapades both run a generated `switch` that returns a string literal. They tie on net10 and net8. NetEscapades is faster on net9 (0.6273 vs 0.9135 ns) and net11 (0.5599 vs 0.8345 ns). The cause of the net9/net11 gap was not investigated.
- For an undefined value every column returns "99". CSE and the BCL box the enum (24 B) and call `Enum.ToString()`. Enums.NET and NetEscapades format the unboxed underlying int and allocate nothing.
- `ToWireName` returns the snake_case wire name "indigo". It is not a member name, so no comparison is claimed. It throws for an undefined value, so it has no undefined row.

### 2. Parse by name

| Mode | Input | Variant | Mean | Error | vs CSE | Allocated |
|---|---|---|---:|---:|---:|---:|
| Case-sensitive | Valid | CSE `EnumValueParser.TryParse` | 22.1890 ns | 0.0514 ns | 1.00 | 0 B |
| Case-sensitive | Valid | CSE `IsDefined(string)` (bool only) | 0.7398 ns | 0.0026 ns | 0.03 | 0 B |
| Case-sensitive | Valid | BCL `Enum.TryParse` | 43.0744 ns | 0.0413 ns | 1.94 | 0 B |
| Case-sensitive | Valid | Enums.NET `Enums.TryParse` | 17.8628 ns | 0.0443 ns | 0.81 | 0 B |
| Case-sensitive | Valid | NetEscapades `TryParse` | 28.2404 ns | 0.0220 ns | 1.27 | 0 B |
| Case-sensitive | Invalid | CSE `EnumValueParser.TryParse` | 39.8918 ns | 0.1920 ns | 1.00 | 88 B |
| Case-sensitive | Invalid | CSE `IsDefined(string)` (bool only) | 0.9333 ns | 0.0026 ns | 0.02 | 0 B |
| Case-sensitive | Invalid | BCL `Enum.TryParse` | 70.4925 ns | 0.0149 ns | 1.77 | 0 B |
| Case-sensitive | Invalid | Enums.NET `Enums.TryParse` | 13.3405 ns | 0.0351 ns | 0.33 | 0 B |
| Case-sensitive | Invalid | NetEscapades `TryParse` | 57.3845 ns | 0.0244 ns | 1.44 | 0 B |
| Ignore-case | Valid | CSE `TryParseWire` | 51.9075 ns | 0.0681 ns | 1.00 | 0 B |
| Ignore-case | Valid | BCL `Enum.TryParse` | 53.6330 ns | 0.0462 ns | 1.03 | 0 B |
| Ignore-case | Valid | Enums.NET `Enums.TryParse` | 35.9320 ns | 0.0390 ns | 0.69 | 0 B |
| Ignore-case | Valid | NetEscapades `TryParse` | 44.7521 ns | 0.0310 ns | 0.86 | 0 B |
| Ignore-case | Invalid | CSE `TryParseWire` | 60.5683 ns | 0.2606 ns | 1.00 | 88 B |
| Ignore-case | Invalid | BCL `Enum.TryParse` | 77.5124 ns | 0.0156 ns | 1.28 | 0 B |
| Ignore-case | Invalid | Enums.NET `Enums.TryParse` | 14.6183 ns | 0.0365 ns | 0.24 | 0 B |
| Ignore-case | Invalid | NetEscapades `TryParse` | 58.1841 ns | 0.0089 ns | 0.96 | 0 B |

- The generated helpers have no case-sensitive parse, so the case-sensitive CSE row is the runtime parser, `EnumValueParser.TryParse` with `EnumReadMode.Input` and `CaseInsensitive = false`. It probes the exact wire name, then the exact member name, which hits.
- `TryParseWire` always ignores case. For "pendingapproval" it probes the exact wire name, the wire name ignoring case and then the member name ignoring case. The third probe hits. That is three hash lookups before success.
- An invalid input misses every lookup. CSE then builds an `EnumValueError` (the message data for a 400 response or an exception), and `TryParseWire` throws it away. That is the 88 B.
- `IsDefined(string)` is a generated `switch` over member names. It returns only whether the name exists, so it is not a parse result and is not counted as a win.
- net8 (netstandard2.1 build): CSE allocates on every call, 112 B / 208 B case-sensitive and 168 B / 248 B ignore-case (valid / invalid), and is the slowest column in all four rows (62.067, 75.539, 96.458 and 104.523 ns).
- net11: BenchmarkDotNet printed a Median column for this class, and the CSE case-sensitive invalid row has a StdDev of 1.6982 ns. Treat small net11 parse gaps as weak evidence.

### 3. IsDefined(value)

| Input | Variant | Mean | Error | vs CSE | Allocated |
|---|---|---:|---:|---:|---:|
| Defined | CSE | 3.9082 ns | 0.0071 ns | 1.00 | 0 B |
| Defined | BCL | 4.4041 ns | 0.0030 ns | 1.13 | 0 B |
| Defined | Enums.NET | 0.8611 ns | 0.0134 ns | 0.22 | 0 B |
| Defined | NetEscapades | 0.4209 ns | 0.0044 ns | 0.11 | 0 B |
| Undefined | CSE | 2.8831 ns | 0.0069 ns | 1.00 | 0 B |
| Undefined | BCL | 3.8319 ns | 0.0012 ns | 1.33 | 0 B |
| Undefined | Enums.NET | 1.2988 ns | 0.0100 ns | 0.45 | 0 B |
| Undefined | NetEscapades | 0.7315 ns | 0.0005 ns | 0.25 | 0 B |

- The generated `IsDefined(this Status)` is one line: `EnumMetadata.Get<Status>().IsDefined(value)`. That reads a static cache property, calls a `Func<TEnum, ulong>` delegate to widen the value and looks it up in a `Dictionary<ulong, ...>`. NetEscapades generates a `switch` over the constants.
- On `[Flags]` enums CSE also accepts any combination of defined bits, while the others accept only declared members, so this scenario uses the non-flags `Status`.
- net8 and net9: CSE is slower than the BCL too (8.1692 vs 5.0849 ns and 7.0025 vs 5.7329 ns, defined).

### 4. GetValues / GetNames

| Category | Variant | Mean | Error | vs CSE | Allocated |
|---|---|---:|---:|---:|---:|
| GetValues | CSE | 11.98 ns | 0.017 ns | 1.00 | 0 B |
| GetValues | BCL | 31.23 ns | 0.270 ns | 2.61 | 104 B |
| GetValues | Enums.NET | 14.02 ns | 0.133 ns | 1.17 | 0 B |
| GetValues | NetEscapades | 11.46 ns | 0.022 ns | 0.96 | 0 B |
| GetNames | CSE | 19.09 ns | 0.122 ns | 1.00 | 184 B |
| GetNames | BCL | 33.94 ns | 0.178 ns | 1.78 | 184 B |
| GetNames | Enums.NET | 16.30 ns | 0.193 ns | 0.85 | 0 B |
| GetNames | NetEscapades | 18.77 ns | 0.216 ns | 0.98 | 184 B |

- CSE, NetEscapades and the BCL return a new array on every call. Enums.NET returns a cached `IReadOnlyList`.
- GetValues shows 0 B for CSE and NetEscapades on net10 and net11, but 104 B on net8 and net9. The generated code is the same on every TFM, so this is a runtime effect, not a CSE improvement. The cause was not investigated.
- CSE and NetEscapades tie on GetNames. NetEscapades is 0.52 ns faster on GetValues.

### 5. Flags

| Category | Variant | Mean | Error | vs CSE | Allocated |
|---|---|---:|---:|---:|---:|
| Format | CSE `ToOptimizedString` | 36.1870 ns | 0.6990 ns | 1.00 | 96 B |
| Format | BCL `ToString` | 34.8196 ns | 0.1248 ns | 0.96 | 96 B |
| Format | Enums.NET `FormatFlags` | 84.5527 ns | 0.4612 ns | 2.34 | 280 B |
| Format | NetEscapades `ToStringFast` | 39.8499 ns | 0.2198 ns | 1.10 | 96 B |
| HasFlag (no CSE column) | BCL `HasFlag` (baseline) | 0.1638 ns | 0.0145 ns | n/a | 0 B |
| HasFlag (no CSE column) | Enums.NET `HasAllFlags` | 0.5823 ns | 0.0165 ns | n/a | 0 B |
| HasFlag (no CSE column) | NetEscapades `HasFlagFast` | 0.1761 ns | 0.0188 ns | n/a | 0 B |
| WireName (different work) | CSE `ToWireName` | 56.6902 ns | 0.2961 ns | n/a | 152 B |

- `Write | Execute | Delete` is not a declared member. CSE and NetEscapades both fall back to `Enum.ToString()`, so all three are close to the BCL. Enums.NET joins its cached single-flag names.
- CSE generates no `HasFlag` helper. `Enum.HasFlag` is a JIT intrinsic and does not box, so the BCL row is the CSE answer.
- `ToWireName` returns "write,execute,delete" (wire names of single flags, no space). That is different output, so no comparison is claimed.
- net8: CSE 48.2274 ns is slower than the BCL (44.2197 ns) and NetEscapades (37.9471 ns).

### 6. Description attribute

| Variant | Mean | Error | vs CSE | Allocated |
|---|---:|---:|---:|---:|
| CSE `EnumMetadata.Get<T>().TryGetMember(...).Description` | 3.7150 ns | 0.0064 ns | 1.00 | 0 B |
| Enums.NET `AsString(EnumFormat.Description)` | 2.6899 ns | 0.0128 ns | 0.72 | 0 B |
| NetEscapades `ToStringFast(useMetadataAttributes: true)` | 0.9021 ns | 0.0006 ns | 0.24 | 0 B |

- CSE generates no description method. The row is a metadata lookup through the same delegate and dictionary as IsDefined. NetEscapades generates a `switch`.
- Members without `[Description]` differ: CSE returns the member's XML doc summary, Enums.NET returns null and NetEscapades returns the member name. The input is the one member that has the attribute.

### 7. Native AOT (net10, linux-x64)

> This section is script-driven, not BenchmarkDotNet. `dotnet publish -c Release -r linux-x64 -p:TreatWarningsAsErrors=false -p:TrimmerSingleWarn=false` from a clean obj. `TrimmerSingleWarn=false` lists each warning per member, so the counts are higher than a default publish shows. Each sample formats, parses and enumerates the same 20-member `Status` enum and prints a checksum.

| Variant | AOT publish | IL warnings (distinct) | Native binary | Exit code | Output |
|---|---|---:|---:|---:|---|
| BclAot | yes | 0 | 1.2 MB | 0 | `parsed 20 checksum 971 ok` |
| CseAot | yes | 0 | 1.9 MB | 0 | `parsed 20 checksum 971 ok` |
| EnumsNetAot | yes | 4 (IL2071 ×1, IL2075 ×1, IL2080 ×1, IL3050 ×1; all ILC) | 2.0 MB | 0 | `parsed 20 checksum 971 ok` |
| NetEscapadesAot | yes | 0 | 1.2 MB | 0 | `parsed 20 checksum 971 ok` |

- Enums.NET's warnings come from `Enums.GetEnumValidatorAttribute(Type)` (`MakeGenericType`, `GetInterfaces`) and `Cacher.CreateCache` (`GetFields`). The sample still runs. It uses the net7.0 build of Enums.NET, which has no package dependencies.
- CSE's binary is 0.7 MB larger than NetEscapades'. It carries the runtime library (metadata, parser, formatter). The size was not broken down.
- Wall time was 2 ms for every sample (median of 5). That is not a startup result.

---

## Cross-TFM

Mean, with allocation where it is not 0 B. Run order net10 → net9 → net8 → net11.

| Scenario | Variant | net8 | net9 | net10 | net11 |
|---|---|---|---|---|---|
| ToString, defined | CSE | 0.5580 ns | 0.9135 ns | 0.5439 ns | 0.8345 ns |
| | Enums.NET | 0.8226 ns | 0.9925 ns | 1.1192 ns | 1.2649 ns |
| | NetEscapades | 0.5428 ns | 0.6273 ns | 0.5437 ns | 0.5599 ns |
| | BCL | 11.5723 ns / 24 B | 10.2164 ns / 24 B | 8.6687 ns / 24 B | 9.9005 ns / 24 B |
| ToString, undefined | CSE | 14.2804 ns / 24 B | 14.7163 ns / 24 B | 12.8220 ns / 24 B | 9.8920 ns / 24 B |
| | Enums.NET | 2.5721 ns | 2.6346 ns | 2.0876 ns | 1.5561 ns |
| | NetEscapades | 1.9885 ns | 1.8384 ns | 1.6986 ns | 1.5464 ns |
| | BCL | 16.1811 ns / 24 B | 12.1915 ns / 24 B | 11.8073 ns / 24 B | 9.6459 ns / 24 B |
| Parse case-sensitive, valid | CSE | 62.067 ns / 112 B | 35.099 ns | 22.1890 ns | 20.6890 ns |
| | Enums.NET | 21.061 ns | 20.505 ns | 17.8628 ns | 21.8771 ns |
| | NetEscapades | 29.390 ns | 29.360 ns | 28.2404 ns | 28.6613 ns |
| | BCL | 27.087 ns | 27.178 ns | 43.0744 ns | 14.7735 ns |
| Parse case-sensitive, invalid | CSE | 75.539 ns / 208 B | 55.568 ns / 88 B | 39.8918 ns / 88 B | 40.7442 ns / 88 B |
| | Enums.NET | 21.048 ns | 18.007 ns | 13.3405 ns | 12.1470 ns |
| | NetEscapades | 58.871 ns | 57.942 ns | 57.3845 ns | 57.7573 ns |
| | BCL | 38.349 ns | 35.976 ns | 70.4925 ns | 18.2234 ns |
| Parse ignore-case, valid | CSE | 96.458 ns / 168 B | 46.664 ns | 51.9075 ns | 33.1584 ns |
| | Enums.NET | 28.935 ns | 39.212 ns | 35.9320 ns | 24.5205 ns |
| | NetEscapades | 36.372 ns | 33.996 ns | 44.7521 ns | 33.2026 ns |
| | BCL | 32.012 ns | 28.519 ns | 53.6330 ns | 17.9225 ns |
| Parse ignore-case, invalid | CSE | 104.523 ns / 248 B | 73.439 ns / 88 B | 60.5683 ns / 88 B | 58.9579 ns / 88 B |
| | Enums.NET | 20.993 ns | 17.530 ns | 14.6183 ns | 14.7859 ns |
| | NetEscapades | 58.850 ns | 58.490 ns | 58.1841 ns | 58.3552 ns |
| | BCL | 48.218 ns | 42.050 ns | 77.5124 ns | 24.9442 ns |
| IsDefined, defined | CSE | 8.1692 ns | 7.0025 ns | 3.9082 ns | 3.3945 ns |
| | Enums.NET | 0.6977 ns | 0.6959 ns | 0.8611 ns | 0.6909 ns |
| | NetEscapades | 0.4639 ns | 0.7306 ns | 0.4209 ns | 0.3851 ns |
| | BCL | 5.0849 ns | 5.7329 ns | 4.4041 ns | 3.8336 ns |
| IsDefined, undefined | CSE | 7.8643 ns | 6.7179 ns | 2.8831 ns | 2.9813 ns |
| | Enums.NET | 1.3184 ns | 1.3052 ns | 1.2988 ns | 1.2766 ns |
| | NetEscapades | 0.4271 ns | 0.4432 ns | 0.7315 ns | 0.7192 ns |
| | BCL | 5.0596 ns | 5.3258 ns | 3.8319 ns | 3.6894 ns |
| GetValues | CSE | 15.79 ns / 104 B | 15.84 ns / 104 B | 11.98 ns | 9.415 ns |
| | Enums.NET | 15.61 ns | 15.61 ns | 14.02 ns | 13.882 ns |
| | NetEscapades | 15.62 ns / 104 B | 15.61 ns / 104 B | 11.46 ns | 9.331 ns |
| | BCL | 44.19 ns / 104 B | 34.94 ns / 104 B | 31.23 ns / 104 B | 32.363 ns / 104 B |
| GetNames | CSE | 20.37 ns / 184 B | 19.22 ns / 184 B | 19.09 ns / 184 B | 19.966 ns / 184 B |
| | Enums.NET | 17.45 ns | 17.41 ns | 16.30 ns | 15.949 ns |
| | NetEscapades | 19.78 ns / 184 B | 19.30 ns / 184 B | 18.77 ns / 184 B | 20.270 ns / 184 B |
| | BCL | 31.41 ns / 184 B | 31.09 ns / 184 B | 33.94 ns / 184 B | 30.191 ns / 184 B |
| Flags format | CSE | 48.2274 ns / 96 B | 40.5164 ns / 96 B | 36.1870 ns / 96 B | 44.6887 ns / 96 B |
| | Enums.NET | 89.9792 ns / 280 B | 86.9533 ns / 280 B | 84.5527 ns / 280 B | 83.5635 ns / 280 B |
| | NetEscapades | 37.9471 ns / 96 B | 41.1450 ns / 96 B | 39.8499 ns / 96 B | 44.6293 ns / 96 B |
| | BCL | 44.2197 ns / 96 B | 40.7572 ns / 96 B | 34.8196 ns / 96 B | 44.9506 ns / 96 B |
| Description | CSE | 4.5478 ns | 4.4799 ns | 3.7150 ns | 3.4683 ns |
| | Enums.NET | 4.3340 ns | 3.7137 ns | 2.6899 ns | 2.7856 ns |
| | NetEscapades | 0.9012 ns | 0.5407 ns | 0.9021 ns | 0.9217 ns |

- The IsDefined and Description losses, and the invalid-parse loss against Enums.NET, hold on every TFM. The invalid-parse loss against NetEscapades does not: on net11 ignore-case CSE ties it (58.9579 vs 58.3552 ns), and case-sensitive CSE is faster on net9, net10 and net11 but slower on net8 (75.539 vs 58.871 ns).
- net8 is CSE's weakest runtime. It runs the netstandard2.1 build: parse allocates on every call and IsDefined takes about 2-2.7× its net10 time (8.1692 vs 3.9082 ns defined, 7.8643 vs 2.8831 ns undefined).
- The net10 BCL parse rows (43.0744 to 77.5124 ns) are well above the BCL's net8, net9 and net11 values. net10 ran first. The cause was not investigated, so the net10 "CSE beats the BCL" parse rows are weak evidence.

---

## Semantic differences

The parity checks pick inputs where every column agrees. Outside those inputs the libraries do different things, so speed is not the only difference.

- CSE's primary spelling is a wire name (snake_case by default, or `[JsonStringEnumMemberName]` / `[EnumMember]` / a naming policy), which its JSON, ASP.NET Core, OpenAPI and EF Core integrations use. `ToOptimizedString` returns the member name and `ToWireName` the wire name. The `ToWireName` rows do different work and are not compared with anything.
- CSE parsing accepts `[EnumAlias]` spellings. Enums.NET can accept extra spellings through `EnumFormat` values or custom formats. NetEscapades accepts one metadata spelling per member and has no alias attribute.
- For numeric strings, CSE accepts the number of a defined member and rejects undefined numbers such as "99". The BCL and Enums.NET accept any number in range. NetEscapades accepts any int unless number parsing is turned off.
- The BCL and Enums.NET trim whitespace; CSE and NetEscapades do not. The BCL parses "A, B" lists on any enum and Enums.NET only on `[Flags]` enums. NetEscapades does not parse lists, and CSE parsing rejects them.
- `TryParseWire` always ignores case. Case-sensitive parsing needs the runtime `EnumValueParser` with `CaseInsensitive = false`.
- CSE's parser reports why a value was rejected (`EnumValueError`, with the accepted wire names), which feeds 400 responses and exceptions. That is what an invalid input pays for.
- On `[Flags]` enums CSE `IsDefined` returns true for any combination of defined flags. The others return true only for a declared member.
- For an undefined value `ToWireName` throws and `ToOptimizedString` returns the number, as Enums.NET and NetEscapades do.

---

## Feature matrix

Enums.NET links point at tag [`5.0.0`](https://github.com/TylerBrinkley/Enums.NET/tree/5.0.0). NetEscapades links point at tag [`v1.0.0-beta21`](https://github.com/andrewlock/NetEscapades.EnumGenerators/tree/v1.0.0-beta21); its repository has no root README at that tag, so docs links go to [docs/README.md](https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md). CSE cells come from this repository. The Native AOT row comes from section 7; every other row comes from reading source, nuspecs or docs, not from running code.

| Feature | Enums.NET 5.0.0 | NetEscapades.EnumGenerators 1.0.0-beta21 | CSharpEssentials.Enums 7.0.0 |
|---|---|---|---|
| Runtime reflection | Yes: `GetFields` and attribute reads when the per-enum cache is built, and `MakeGenericType` for the non-generic API ([Enums.cs#L100](https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L100), [Enums.cs#L4352](https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L4352)) | None in the generated helpers, which are `switch` expressions over constants ([SourceGenerationHelper.cs#L149](https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/src/NetEscapades.EnumGenerators.Generators/SourceGenerationHelper.cs#L149)) | None on the `[StringEnum]` path. Reflection is opt-in only (`EnumMetadata.GetOrCreateWithReflection`, annotated `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]`) |
| Source generator | No; a cached generic `EnumCache` per enum type ([EnumCache.cs](https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/EnumCache.cs)) | Yes: an `{Enum}Extensions` class per `[EnumExtensions]` enum ([docs/README.md](https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md#adding-netescapadesenumgenerators-to-your-project)) | Yes: an `{Enum}Extensions` class and the enum's metadata per `[StringEnum]` enum, bundled in the main package |
| Native AOT publish without warnings, and runs (measured) | No warnings-free publish: 4 IL warnings (all ILC). It runs | Yes: 0 warnings, runs | Yes: 0 warnings, runs |
| `IsAotCompatible` / trim annotations | Neither ([Enums.NET.csproj](https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.NET.csproj)) | Not declared in any project file ([RuntimeDependencies.csproj](https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/src/NetEscapades.EnumGenerators.RuntimeDependencies/NetEscapades.EnumGenerators.RuntimeDependencies.csproj)) | `IsAotCompatible=true` on the net9.0, net10.0 and net11.0 builds, not on the netstandard builds that net8 consumers get (`CSharpEssentials.Enums/CSharpEssentials.Enums.csproj:29`) |
| Extra runtime dependencies | net7.0 and netcoreapp3.0: none (the AOT sample uses the net7.0 build). netstandard2.0/2.1: `System.ComponentModel.Annotations` 5.0.0 and `System.Runtime.CompilerServices.Unsafe` 6.0.0; net461: `Unsafe` 6.0.0 ([nuspec](https://api.nuget.org/v3-flatcontainer/enums.net/5.0.0/enums.net.nuspec)) | `NetEscapades.EnumGenerators.RuntimeDependencies` 1.0.0-beta21 (holds `EnumParseOptions`, `SerializationOptions`), plus the attributes assembly from the Generators package ([nuspec](https://api.nuget.org/v3-flatcontainer/netescapades.enumgenerators/1.0.0-beta21/netescapades.enumgenerators.nuspec), [avoiding runtime dependencies](https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md#avoiding-runtime-dependencies)) | None: the csproj has no `PackageReference` or `ProjectReference` (checked in `CSharpEssentials.Enums/CSharpEssentials.Enums.csproj`; the packed nuspec was not checked) |
| Attribute required on the enum | No; works on any enum ([README](https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/README.md#enumsnet-demo)) | Yes, `[EnumExtensions]`, or `[EnumExtensions<T>]` for an enum you do not own ([docs/README.md#L346](https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md?plain=1#L346)) | Yes, `[StringEnum]` |
| Base class required | No | No | No |
| Instantiation per request | No; static generic API over a per-type cache | No; static generated methods | No; static generated methods and one metadata object per enum |
| DI registration required | No | No | No; a generated `[ModuleInitializer]` registers the metadata (`CSharpEssentials.Enums.Generators/StringEnumSourceWriter.cs:94-95`) |
| Fast ToString | `AsString()` from the cached member table ([Enums.cs#L691](https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L691)) | Generated `ToStringFast()` ([docs/README.md#L127](https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md?plain=1#L127)) | Generated `ToOptimizedString()` (member name) and `ToWireName()` (wire name) |
| Parse | `Enums.TryParse` with `ignoreCase`, names and numbers, extra `EnumFormat` spellings ([Enums.cs#L1370](https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L1370)) | Generated `TryParse` with `ignoreCase`, span overloads, optional metadata match ([docs/README.md#L221](https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md?plain=1#L221)) | Generated `TryParseWire` (wire names, member names in any case, aliases, defined numbers); runtime `EnumValueParser` with conventions |
| Flags API | `FlagEnums`: `HasAllFlags`, `HasAnyFlags`, `GetFlags`, `FormatFlags` and more ([FlagEnums.cs](https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/FlagEnums.cs)) | Generated `HasFlagFast()` ([docs/README.md#L284](https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md?plain=1#L284)) | Wire formatting and parsing of flag lists, analyzers CSE0005-CSE0008; no `HasFlag` helper |
| Description metadata | `EnumFormat.Description` and `EnumFormat.DisplayName` ([EnumFormat.cs#L58-L66](https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/EnumFormat.cs#L58-L66)) | `[Description]` or `[Display]`, one source per enum ([docs/README.md#L291-L307](https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/docs/README.md?plain=1#L291-L307)) | `[Description]` or the XML summary, in the metadata (`IEnumMemberInfo.Description`); no generated method |
| JSON, ASP.NET Core binding, OpenAPI, EF Core | None | None | Yes, in `CSharpEssentials.Json`, `.AspNetCore`, `.AspNetCore.OpenApi` / `.Swashbuckle` and `.EntityFrameworkCore` |
| License | MIT | MIT | MIT |

---

## Caveats

- NetEscapades.EnumGenerators 1.0.0-beta21 is a prerelease. No stable version exists to compare against.
- Nanosecond rows below about 1 ns are close to the timer and loop overhead. Treat the defined ToString, IsDefined and HasFlag gaps among the fastest columns as orders of magnitude, not exact ratios.
- The net8 column runs CSE's netstandard2.1 build; the other columns run the matching TFM build.
- The net10 BCL parse rows are far above the other TFMs. net10 ran first; the cause was not investigated.
- GetValues allocation drops to 0 B on net10 and net11 for both generated libraries without any code change. The cause was not investigated.
- net11 parse rows show a Median column and one wide StdDev (CSE case-sensitive invalid, 1.6982 ns).
- Load averages were not recorded for the BenchmarkDotNet runs. The AOT script ran just before net10 and finished before it started.
- The "why" notes on CSE losses come from reading the source. They were not profiled.
- AOT warning counts use `TrimmerSingleWarn=false`. The 2 ms AOT wall time is not a result.
- The feature matrix comes from reading source and docs, except the measured AOT row.
- A difference within the sum of the two error margins is reported as a tie.
- The CSE column is the 7.0.0 working tree on branch `perf/bench-enums`, which may differ from the last version published to NuGet.

---

## Pending: boxed formatting and reflection-built metadata

**Status: draft.** These two classes build on net8.0 to net11.0, and their parity self-check passes on every TFM. No full default-job run has been done, so this section has no numbers yet. The reports will be committed under `benchmarks/results/enums/{tfm}/results/` next to the ones above. Short-job (`--quick`) numbers are not published. The full run (`./run-benchmarks.sh --project enums`) includes them, and the `--quick` patterns select them too. [How to reproduce](#how-to-reproduce) still says six classes. With these two, the suite has eight.

The rules are the same as for the six classes above. Every class has `[MemoryDiagnoser]`. CSE is the baseline in each category. A doc URL sits above every row. `[GlobalSetup]` runs the parity check, and `--self-check` runs it without BenchmarkDotNet.

Pinned versions, checked on NuGet on 2026-11-04:

| Package | Version | License (nuspec) |
|---|---|---|
| Enums.NET | 5.0.0 (latest stable) | MIT |
| NetEscapades.EnumGenerators | 1.0.0-beta21 (no stable release exists). Not used by these two classes | MIT |

| Class | Measured unit | Rows | Label |
|---|---|---|---|
| `BoxedFormatComparison`, category Format | Format `Shade.Indigo`, boxed once in setup, as "Indigo" | CSE `EnumValueFormatter.Format(object, EnumConventions.Default)` (baseline), BCL `object.ToString()`, Enums.NET `Enums.AsString(Type, object)` | Like-for-like |
| `BoxedFormatComparison`, category Format | Same | CSE with `CanHandle = _ => true` | CSE configuration row with the same output. It shows the cost of the default registry check |
| `BoxedFormatComparison`, category Format | Same, unboxed | CSE `EnumValueFormatter.Format(Shade, EnumWireFormat.String)` | Reference only. It shows what boxing and the visitor cost |
| `BoxedFormatComparison`, category FormatMany | A boxed `Shade[]` of all eight members into one list. Returns the entry count plus the summed lengths | CSE `TryFormatMany` (baseline), a BCL loop over `ToString()`, an Enums.NET loop over `AsString(Type, object)` | Like-for-like on output. The BCL and Enums.NET rows are hand-written loops, because neither library has a collection formatter |
| `ReflectionInfoComparison`, category Names | All 20 names of `PlainStatus` from a `Type`, summed lengths | CSE `GetOrCreateWithReflection(Type, PascalCase).WireNames` (baseline), BCL `Enum.GetNames(Type)`, Enums.NET `Enums.GetNames(Type)` | Like-for-like |
| `ReflectionInfoComparison`, category Names | Same | CSE `WireNames` of an info held from setup | Reference only. No lookup, so the lookup cost is visible |
| `ReflectionInfoComparison`, category Values | All 20 values of `PlainStatus` from a `Type`, summed as `long` | CSE `Members[i].RawValue` (baseline), BCL `Enum.GetValuesAsUnderlyingType(Type)` with `Convert.ToInt64`, Enums.NET `Enums.GetMembers(Type)` with `ToInt64()` | Like-for-like on the result |

Methodology:
- Format: the value is boxed once in setup, and no row boxes it again. FormatMany: every row, CSE's `TryFormatMany` included, enumerates the `Shade[]` through the non-generic `IEnumerable`, which boxes each of the eight elements per call. That cost is the same in every row.
- `Shade` has `[StringEnum(Naming = EnumNaming.PascalCase)]`. Its wire names equal its member names, so CSE, the BCL and Enums.NET write the same text. The existing `Color` enum uses snake_case wire names, so it is not reused.
- `PlainStatus` has the members and values of `Status` and no attribute at all. CSE has no generated metadata for it, so `GetOrCreateWithReflection` takes the reflection path. The self-check fails if `PlainStatus` is ever registered.
- Every CSE row in `ReflectionInfoComparison` except the held-info reference row (`CSharpEssentials_HeldNames`) calls `GetOrCreateWithReflection` per invocation. That is the documented API: it builds the info once and caches it. The enum type is held in a field, as a caller that has only a `Type` would hold it.
- NetEscapades.EnumGenerators has no column. Its generated methods take the typed enum, never an `object`, and need `[EnumExtensions]` on the enum.
- There is no undefined-value row. For an undefined value CSE throws `EnumValueException` while the BCL returns the number ("99"), so the outputs differ.
- The BCL `GetValuesAsUnderlyingType(Type)` returns a new array per call, and `Array.GetValue` boxes each element. That is the cost of the non-generic API. CSE lists members in declaration order and Enums.NET in value order. The sums do not depend on order, and `PlainStatus` values ascend anyway.

What the parity check enforces:
- Format: every row returns "Indigo".
- FormatMany: every row returns the same total, computed from `Enum.GetNames<Shade>()`, and the same list text "Red,Orange,Yellow,Green,Cyan,Blue,Indigo,Violet".
- Names and Values: every row returns the sum computed from `Enum.GetNames<Status>()` and `Enum.GetValues<Status>()`.

Feature matrix for these classes, from reading each library's source and docs (not measured):

| Feature | BCL | Enums.NET 5.0.0 | NetEscapades.EnumGenerators 1.0.0-beta21 | CSharpEssentials.Enums 7.0.0 |
|---|---|---|---|---|
| Format a boxed enum (`object`) | `object.ToString()` on the boxed enum | `Enums.AsString(Type, object)` ([Enums.cs#L3439](https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L3439)) | None: generated methods take the typed enum | `EnumValueFormatter.Format` and `TryFormat(object, EnumConventions)`. Needs generated metadata |
| Format a collection of boxed enums | None | None | None | `EnumValueFormatter.TryFormatMany` (one entry per value; flags split into single flags) |
| Names and values from a `Type` for an enum without an attribute | `Enum.GetNames(Type)`, `Enum.GetValuesAsUnderlyingType(Type)`: a new array per call | `Enums.GetNames(Type)`, `Enums.GetMembers(Type)`: cached per type ([Enums.cs#L3040](https://github.com/TylerBrinkley/Enums.NET/blob/5.0.0/Src/Enums.NET/Enums.cs#L3040)) | None: needs `[EnumExtensions]` | `EnumMetadata.GetOrCreateWithReflection(Type, EnumNaming)`: cached, annotated `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]` |

Expected losses, read from the source (not fixed in this work; check them against the full run):
- Boxed format. `EnumValueFormatter.cs:110-112`: the default `CanHandle` (`EnumConventions.cs:42-43`) calls `EnumMetadata.IsRegistered`, which looks the type up in the registry. Line 112 then looks it up again, so each boxed format does two registry lookups. `:57`, `:78` and `:92` allocate a visitor per call, and `:83` allocates a `List<string>` per collection. Expect the CSE boxed rows to be slower and to allocate more than the BCL and Enums.NET. The `CanHandle = _ => true` row shows the share of the first lookup.
- Reflection names and values. `EnumMetadata.cs:45-59` with `:80`: for an enum without `[StringEnum]`, every `GetOrCreateWithReflection` call misses the registry snapshot, checks `IsEnum`, calls `RuntimeHelpers.RunModuleConstructor`, misses again, and then looks up a `ConcurrentDictionary` with a `(Type, EnumNaming)` key. Expect `CSharpEssentials_Names` to be well above the held-info row, and likely above Enums.NET's cached `GetNames(Type)`.
- Not measured here: reflection-built infos box the value with `Convert.ToInt64(object)` on every `Format`, `IsDefined` and member lookup (`ReflectionEnumInfoBuilder.cs:55` and `:61-64`), and `FromRawValue` boxes through `Enum.ToObject` (`:56`). The Names and Values rows read precomputed `RawValue`s, so they do not reach this code.

Left out of this phase: a `Format` or `IsDefined` row on a reflection-built info (it would show the boxing above), `EnumMetadata.TryGet(Type)` against a `ConcurrentDictionary` baseline, and an invalid-parse row for the netstandard2.1 build.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project enums                       # all TFMs (net8-net11), all six classes, committed configuration
./run-benchmarks.sh --project enums --tfm net10.0         # one TFM
./run-benchmarks.sh --project enums --quick --tfm net10.0 # one TFM, Short job
./enums-aot/run-enums-aot.sh                              # Native AOT report (all four samples)
```

`--quick` without `--tfm` runs on net9.0, the script default. Its patterns select each of the six classes; the Short job's numbers are not comparable with the committed ones. Results are stored per framework under `benchmarks/results/enums/{tfm}/results/` (GitHub Markdown, CSV, JSON and HTML). The AOT report is `benchmarks/results/enums/aot/aot-report.md`; raw publish and run logs go to `benchmarks/results/enums/aot/logs/`. `run-enums-aot.sh` accepts variant names to run a subset and `RUNS` (default 5) for the wall-time median.
