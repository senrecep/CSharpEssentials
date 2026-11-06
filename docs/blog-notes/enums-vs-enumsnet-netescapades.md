# Blog notes: CSharpEssentials.Enums, Enums.NET and NetEscapades.EnumGenerators

This is raw material for a post, not the post itself. Every number comes from `CSharpEssentials.Enums/BENCHMARKS.md`, which reads `benchmarks/results/enums/{tfm}/results/*-github.md` and `benchmarks/results/enums/aot/aot-report.md`. Competitors: Enums.NET 5.0.0 and NetEscapades.EnumGenerators 1.0.0-beta21, a prerelease, because that package has never shipped a stable version. The plain `System.Enum` API (BCL) is the baseline.

## How these numbers were produced

- BenchmarkDotNet 0.14.0, default job, full runs of all eight classes on .NET 8.0.30, 9.0.19, 10.0.11 and 11.0.0 (SDK 11.0.100-rc.1), one TFM after another. Primary runtime is net10.
- Machine: 6 cores / 12 threads (x64, RyuJIT AVX2), Linux, with a desktop session running. The 1-minute load average was between 1.18 and 1.83 at the start and end of the suite and of each rerun.
- Rows with a CV above 5% were rerun up to twice. net8 and net11 flags and net9 IsDefined come from those reruns. Two rows stayed noisy: net8 BCL `HasFlag` (CV 12.0%) and net8 NetEscapades `HasFlagFast` (CV 11.8%).
- CSE is the 7.0.0 working tree on branch `integ/all`. net8 runs its netstandard2.1 build.
- Results: `benchmarks/results/enums/{tfm}/results/`. The Native AOT report predates the CSE code measured here and was not rerun.

## Framing

- Working title: "Generated code is not automatically fast."
- CSE (CSharpEssentials.Enums) and NetEscapades are both source generators. Enums.NET is a runtime library with a per-type cache. The obvious guess is that the two generators win everything. They do not.
- What decides the result is what the code does on the hot path. CSE's generated `switch` helpers and its generated flags formatter are at or near the front. Its runtime APIs that take an `object` or a `Type`, or go through the metadata dictionary, are behind a well-built runtime cache (Enums.NET).
- CSE's job is one spelling per member across JSON, model binding, OpenAPI and EF Core, with aliases, a fallback member and readable errors. Speed is secondary to that, which explains some losses but does not excuse them.

## What each library provides

- Enums.NET 5.0.0:
  - Works on any enum without an attribute. Generic API over a cached member table built with reflection.
  - The richest flags API (`FlagEnums`), `EnumFormat` spellings, custom formats and validators.
  - Native AOT: publishes with 4 IL warnings (IL2071, IL2075, IL2080, IL3050) and still runs the sample.
- NetEscapades.EnumGenerators 1.0.0-beta21:
  - `[EnumExtensions]` generates `ToStringFast`, `TryParse`, `IsDefined`, `GetValues`, `GetNames` and `HasFlagFast` as `switch` code.
  - Span overloads, optional metadata matching, usage analyzers, experimental interceptors.
  - Native AOT: 0 warnings, 1.2 MB, runs. The package is still a prerelease.
- CSE 7.0.0:
  - `[StringEnum]` generates `ToOptimizedString`, `ToWireName`, `TryParseWire`, `IsDefined`, `ToDescription`, `GetValues`, `GetNames`, cached `Names` and `Values`, and the enum's metadata, registered by a module initializer.
  - Wire names, aliases, a fallback member, Input and Data read modes, and integrations for JSON, ASP.NET Core binding, OpenAPI and EF Core.
  - Native AOT (older build): 0 warnings, 1.9 MB, runs.

## The numbers that matter (net10 unless stated)

1. ToString, defined value. CSE 0.5418 ns, NetEscapades 0.5414 ns (tie), Enums.NET 1.1186 ns, BCL 9.0266 ns / 24 B. Both generators run a `switch`. Sub-nanosecond.
2. ToString, undefined `(Color)99`. CSE 3.3626 ns, Enums.NET 1.9853 ns, NetEscapades 1.7021 ns, all 0 B. BCL 12.4800 ns / 24 B.
3. Parse. CSE is the fastest column in three of four categories on every TFM. net10: valid case-sensitive 8.6618 ns (Enums.NET 18.3746 ns, NetEscapades 28.8248 ns), valid ignore-case 27.5424 ns (Enums.NET 39.2357 ns), invalid ignore-case 8.1523 ns (Enums.NET 15.1186 ns).
4. Parse, invalid case-sensitive with the error-reporting overload. CSE 22.5530 ns / 88 B, Enums.NET 13.2294 ns / 0 B.
5. IsDefined. CSE 0.4480 ns (defined) and 0.7609 ns (undefined). NetEscapades 0.4532 / 0.7258 ns. Enums.NET 0.7049 / 1.3005 ns. BCL 4.1192 / 3.5401 ns. Every CSE and NetEscapades row is below 1 ns.
6. Flags format, undeclared combination. CSE 25.0906 ns / 72 B, NetEscapades 37.0195 ns / 96 B, BCL 39.2608 ns / 96 B, Enums.NET 82.6982 ns / 280 B.
7. Description (metadata path). CSE 3.6495 ns, Enums.NET 2.6886 ns, NetEscapades 0.9027 ns.
8. GetNames. CSE 20.57 ns / 184 B, Enums.NET 16.13 ns / 0 B, NetEscapades 19.74 ns / 184 B.
9. Boxed format. CSE 15.084 ns, BCL 5.552 ns, Enums.NET 5.168 ns, all 0 B. The typed CSE call is 3.344 ns.
10. Reflection-built names from a `Type`. CSE 38.36 ns / 0 B, BCL 32.81 ns / 184 B, Enums.NET 21.55 ns / 0 B.
11. Native AOT (older CSE build). CSE, NetEscapades and the BCL sample: 0 IL warnings. Enums.NET: 4. All four run.

## Where CSE loses, and why (state it, do not soften it)

The code paths below were read, not profiled.

- Boxed formatting, every TFM. `EnumValueFormatter.Format(object, ...)` is 15.084 ns on net10 against 5.552 ns for `object.ToString()` and 5.168 ns for Enums.NET. net8: 30.357 against 8.836 and 14.803 ns.
  - With the default `CanHandle` the formatter now does one registry lookup (`CSharpEssentials.Enums/Parsing/EnumValueFormatter.cs:138`), and `CanHandle = _ => true` is within 1% of it on net10 (15.155 ns). The typed call on the same value takes 3.344 ns. Where the rest goes was not profiled.
- Boxed list formatting, every TFM. `TryFormatMany` 284.671 ns against hand-written BCL and Enums.NET loops at 157.674 and 151.202 ns on net10, all 224 B.
- Reflection-built names, every TFM. 38.36 ns on net10, slower than the BCL (32.81 ns, which allocates 184 B) and Enums.NET (21.55 ns).
  - Each call reads a `ConcurrentDictionary` keyed by `(Type, EnumNaming)` and then checks the registry (`CSharpEssentials.Enums/Metadata/EnumMetadata.cs:71-72`). The names of an info held since setup still take 23.57 ns against Enums.NET's 21.55 ns.
- Reflection-built values against Enums.NET, every TFM, worst on net8 and net9: 137.76 vs 30.52 ns (net8) and 125.39 vs 28.56 ns (net9). net10 is 32.70 vs 29.72 ns. The row reads 20 members through `IReadOnlyList<IEnumMemberInfo>`; the net8/net9 cost was not explained. CSE beats the BCL on every TFM.
- Description, every TFM (Ratio 0.74 for Enums.NET and 0.25 for NetEscapades on net10).
  - The measured row goes through `EnumMetadata.Get<T>().TryGetMember`: a delegate widens the value and a `Dictionary<ulong, ...>` finds the member (`CSharpEssentials.Enums/Metadata/EnumInfo.cs:140-141`). CSE now also generates a `ToDescription()` switch (`CSharpEssentials.Enums.Generators/StringEnumSourceWriter.cs:271`), which the suite does not measure. Measure it before claiming anything about it.
- Invalid case-sensitive parse allocates 88 B on every TFM and is slower than Enums.NET on every TFM (and than the BCL on net11, 21.8815 vs 18.7848 ns).
  - The benchmark calls the `EnumValueParser.TryParse` overload with `out EnumValueError`, which builds the error when nothing matches (`CSharpEssentials.Enums/Metadata/EnumInfo.cs:165`). The overload without the error and `TryParseWire` skip it, and the invalid ignore-case row allocates nothing.
- Undefined ToString on net10: 3.3626 ns against 1.9853 (Enums.NET) and 1.7021 ns (NetEscapades). NetEscapades is also faster on net9 and Enums.NET on net11.
  - The generated `switch` now ends in `((int)value).ToString()` (`StringEnumSourceWriter.cs:66`), so it no longer boxes and allocates 0 B. Why net10 is slower than CSE's own net8 (1.7688 ns) and net11 (1.4798 ns) times was not investigated.
- Smaller losses:
  - GetNames, every TFM: Enums.NET is faster and allocates 0 B (net10 16.13 ns against 20.57 ns / 184 B). NetEscapades is faster on net10 and net11 and ties on net9 (20.34 vs 20.36 ns). `GetNames()` returns a new array per call (`StringEnumSourceWriter.cs:410`). The cached `Names` list is not measured.
  - GetValues: NetEscapades on net10 (11.42 vs 11.95 ns) and net9 (15.77 vs 16.46 ns); Enums.NET on net8 (15.62 vs 16.69 ns) and net9 (15.55 ns), where CSE allocates 104 B and Enums.NET 0 B.
  - IsDefined of an undefined value: NetEscapades on net10 (0.7258 vs 0.7609 ns) and net8 (0.3707 vs 0.4484 ns). Sub-nanosecond.
  - Native AOT binary (older build): 1.9 MB against 1.2 MB for NetEscapades.

## Where CSE wins or ties

- Parse: the fastest column in valid case-sensitive, valid ignore-case and invalid ignore-case on every TFM, all 0 B. net11 valid ignore-case: 13.3809 ns against BCL 17.9229, Enums.NET 27.6431 and NetEscapades 33.7045 ns. `TryParseWire` does two lookups for a lower-case member name: the exact wire-name table, then one ignore-case table of wire and member names (`CSharpEssentials.Enums/Metadata/EnumInfo.cs:324-331`).
- Flags format: the fastest column and the smallest allocation on every TFM. The generator writes a formatter that picks flags from the largest value down and builds the string with `string.Create` (`StringEnumSourceWriter.cs:463`).
- IsDefined: a generated `switch` (`StringEnumSourceWriter.cs:237`). Ties NetEscapades on the defined value on net10 and net8 (net8 0.4483 vs 0.4454 ns, NetEscapades lower by mean), beats Enums.NET and the BCL on every TFM, and is the fastest column on net9 and net11.
- ToString of a defined value: ties NetEscapades on net10 and net11, faster on net8 and net9; 2.06× faster than Enums.NET and 16.66× faster than the BCL on net10 (BenchmarkDotNet Ratio). Sub-nanosecond.
- GetValues and GetNames against the BCL on every TFM.
- Reflection-built values against the BCL on every TFM (net10 32.70 ns / 0 B against 206.56 ns / 584 B).
- Native AOT (older build): 0 IL warnings and runs, like NetEscapades.

## Where the competitors lose

- Enums.NET: 4 IL warnings under Native AOT; flags format 82.6982 ns / 280 B on net10.
- NetEscapades: invalid parse takes 57.316 to 59.474 ns on every TFM in both modes (net10 57.9610 ns case-sensitive, 58.2002 ns ignore-case). The generated `TryParse` is a `switch` of `s.Equals(name, comparison)` guards, so a miss tests every name in turn ([SourceGenerationHelper.cs#L1212-L1230](https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/src/NetEscapades.EnumGenerators.Generators/SourceGenerationHelper.cs#L1212-L1230)). It also ships only as a prerelease.
- BCL: allocates 24 B on every ToString and 104 B / 184 B on GetValues / GetNames; reflection values 206.56 ns / 584 B on net10.

## Caveats to keep in the post

- NetEscapades 1.0.0-beta21 is a prerelease. No stable version exists.
- Rows under about 1 ns sit near the timer and loop overhead; quote them as orders of magnitude. The HasFlag rows (0.1666 to 0.8052 ns) are at timer resolution and the BCL and NetEscapades rows cannot be told apart; the net8 ones are also noisy (CV 12.0% and 11.8%).
- The net10 BCL parse rows (45.5131 to 79.3068 ns) are far above the other TFMs, in two runs with different TFM orders. The cause was not investigated, so the net10 ratios against the BCL overstate CSE's lead.
- GetValues allocation drops to 0 B on net10 and net11 for both generators with the same generated code. That is a runtime effect, and it was not investigated.
- The net8 column runs CSE's netstandard2.1 build.
- The Native AOT numbers come from a CSE build older than the one benchmarked.
- Semantics differ outside the parity inputs: CSE accepts aliases and defined numbers, rejects undefined numbers and whitespace, and the error-reporting overload returns a reasoned error. `ToWireName` rows are different work and are not compared.
- The "why" notes come from reading the code, not from a profiler.

## Possible angles

- "Generated code is not automatically fast": generated `switch` helpers and the generated flags formatter lead or tie, while the runtime paths that take an `object` or a `Type` trail Enums.NET's cache. Show a generated method next to the boxed formatter.
- "The cost of a good error message": 88 B per rejected input on the error-reporting overload, 0 B on the overload without it.
- "Formatting when you only have an object": the typed CSE call (3.344 ns) next to the boxed one (15.084 ns) and the BCL (5.552 ns).
- "Where CSE loses": boxed formatting, boxed list formatting, reflection-built names and values, description lookup, invalid case-sensitive parse allocation, undefined ToString on net10, GetNames allocation and the older AOT binary size. Name the losses first.

## Boxed formatting and reflection-built metadata

These two classes were added after the first enums run and are measured in this one.

- Boxed formatting. The caller has an enum only as an `object`, as an HTTP client adapter does. CSE's `EnumValueFormatter.Format(object, conventions)` is compared with `object.ToString()` and Enums.NET's `AsString(Type, object)`. A second category formats a boxed array of eight values into one list. Only CSE has a collection formatter, so the BCL and Enums.NET rows are hand-written loops. NetEscapades has no column: it has no API that takes an `object`.
- Reflection-built names and values. The enum has no attribute and the caller has only a `Type`. CSE's `GetOrCreateWithReflection` is compared with `Enum.GetNames(Type)`, `Enum.GetValuesAsUnderlyingType(Type)` and Enums.NET's cached `GetNames(Type)` and `GetMembers(Type)`.

Things to say with the numbers:
- Format: the value is boxed once in setup, and no row boxes it again. FormatMany: every row, CSE's `TryFormatMany` included, enumerates the `Shade[]` through the non-generic `IEnumerable`, which boxes each of the eight elements per call. That cost is the same in every row (224 B).
- `Shade` uses PascalCase wire names so every library writes the same text. With CSE's default snake_case naming the outputs would differ.
- There is no undefined-value row. CSE throws for an undefined value while the BCL writes the number.
- Not measured: reflection-built infos convert the value on every format, `IsDefined` and member lookup.
