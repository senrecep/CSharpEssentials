# Blog notes: CSharpEssentials.Enums, Enums.NET and NetEscapades.EnumGenerators

This is raw material for a post, not the post itself. Every number comes from `CSharpEssentials.Enums/BENCHMARKS.md`, which reads `benchmarks/results/enums/{tfm}/results/*-github.md` and `benchmarks/results/enums/aot/aot-report.md`. The machine had 6 cores / 12 threads (x64, RyuJIT AVX2) and ran a desktop session; load averages were not recorded for the BenchmarkDotNet runs. The SDK was 11.0.100-rc.1, with BenchmarkDotNet 0.14.0. Primary runtime is .NET 10.0.11. Competitors: Enums.NET 5.0.0 and NetEscapades.EnumGenerators 1.0.0-beta21, a prerelease, because that package has never shipped a stable version. The plain `System.Enum` API (BCL) is the baseline.

## Framing

- Working title: "Generated code is not automatically fast."
- CSE (CSharpEssentials.Enums) and NetEscapades are both source generators. Enums.NET is a runtime library with a per-type cache. The obvious guess is that the two generators win everything. They do not.
- What decides the result is what the generated method does. A generated `switch` over constants is about as fast as code gets. A generated method that forwards to a runtime dictionary is a runtime library with extra steps, and a well-built runtime cache (Enums.NET) beats it.
- CSE's job is one spelling per member across JSON, model binding, OpenAPI and EF Core, with aliases, a fallback member and readable errors. Speed is secondary to that, which explains the losses but does not excuse them.

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
  - `[StringEnum]` generates `ToOptimizedString`, `ToWireName`, `TryParseWire`, `IsDefined`, `GetValues`, `GetNames` and the enum's metadata, registered by a module initializer.
  - Wire names, aliases, a fallback member, Input and Data read modes, and integrations for JSON, ASP.NET Core binding, OpenAPI and EF Core.
  - Native AOT: 0 warnings, 1.9 MB, runs.

## The numbers that matter (net10 unless stated)

1. ToString, defined value. CSE 0.5439 ns, NetEscapades 0.5437 ns (tie), Enums.NET 1.1192 ns, BCL 8.6687 ns / 24 B. Both generators run a `switch`.
2. ToString, undefined `(Color)99`. CSE 12.8220 ns / 24 B, BCL 11.8073 ns / 24 B, Enums.NET 2.0876 ns, NetEscapades 1.6986 ns.
3. IsDefined. CSE 3.9082 ns (defined) and 2.8831 ns (undefined). Enums.NET 0.8611 / 1.2988 ns. NetEscapades 0.4209 / 0.7315 ns. BCL 4.4041 / 3.8319 ns.
4. Description. CSE 3.7150 ns, Enums.NET 2.6899 ns, NetEscapades 0.9021 ns.
5. Parse, ignore-case, valid. CSE 51.9075 ns, Enums.NET 35.9320 ns, NetEscapades 44.7521 ns, BCL 53.6330 ns.
6. Parse, invalid input. CSE 39.8918 ns (case-sensitive) and 60.5683 ns (ignore-case), both 88 B. Enums.NET 13.3405 / 14.6183 ns, 0 B.
7. GetNames. CSE 19.09 ns / 184 B, Enums.NET 16.30 ns / 0 B.
8. Native AOT. CSE, NetEscapades and the BCL sample: 0 IL warnings. Enums.NET: 4. All four run.

## Where CSE loses, and why (state it, do not soften it)

The code paths below were read, not profiled.

- IsDefined, every TFM, against both competitors (4.54× Enums.NET and 9.29× NetEscapades on net10, defined value; slower than the BCL too on net8 and net9).
  - The generated method is one line that forwards to the runtime: `EnumMetadata.Get<T>().IsDefined(value)` (`CSharpEssentials.Enums.Generators/StringEnumSourceWriter.cs:204`, with `metadata` built at line 146).
  - `Get<T>()` reads a static cache property with a null check (`CSharpEssentials.Enums/Metadata/EnumInfoCache.cs:10-20`, `CSharpEssentials.Enums/Metadata/EnumMetadata.cs:33-34`).
  - `IsDefined` widens the value through a `Func<TEnum, ulong>` delegate (`CSharpEssentials.Enums/Metadata/EnumInfo.cs:23`, `:127`) and checks a `Dictionary<ulong, ...>` (`EnumInfo.cs:25`, `:272-273`).
  - NetEscapades generates a `switch` over the constants. CSE already writes `switch` code for `ToOptimizedString` (`StringEnumSourceWriter.cs:64`) and `IsDefined(string)` (`StringEnumSourceWriter.cs:284`), and those rows are fast (0.5439 ns and 0.7398 ns).
- Description, every TFM (1.38× Enums.NET, 4.12× NetEscapades on net10).
  - There is no generated description method. The row goes through `EnumMetadata.Get<T>().TryGetMember`, the same delegate plus dictionary (`EnumInfo.cs:121-122`).
- Invalid parse input allocates 88 B on net9, net10 and net11, and nothing in the competitors.
  - When no lookup matches, `TryParse` builds an error: `error = ok ? null : CreateError(ToText(text), input)` (`EnumInfo.cs:179`). `ToText` copies the input into a new string (`EnumInfo.cs:354`) and `CreateError` allocates an `EnumValueError` with the accepted wire names (`EnumInfo.cs:263-264`).
  - `TryParseWire` passes `out _` and throws the error away (`StringEnumSourceWriter.cs:187`). The allocation buys nothing on that path.
- Undefined ToString allocates 24 B and is 6.14× Enums.NET and 7.55× NetEscapades.
  - The generated `switch` ends in `default: return value.ToString();` (`StringEnumSourceWriter.cs:64`), which boxes the enum. The competitors format the unboxed underlying int.
- Valid parse is slow.
  - net10: slower than Enums.NET in both modes and than NetEscapades in ignore-case; only the BCL is slower (53.6330 vs 51.9075 ns).
  - net8 and net9: the slowest of all four columns in both modes. net11 ignore-case: tied with NetEscapades for slowest.
  - `TryParseWire` reads with data conventions, which ignore case. For a member name in lower case it probes the exact wire name, the wire name ignoring case, then the member name ignoring case (`EnumInfo.cs:275-283`). The third probe hits, so a valid member name costs three hash lookups.
- net8 parse allocates on every call: 112 B to 248 B per call, valid or not.
  - The package has no net8 target, so net8 runs the netstandard2.1 build. Its name lookup is `_items.TryGetValue(key.ToString(), ...)` (`CSharpEssentials.Enums/Metadata/EnumNameLookup.cs:39`), a string copy per probe. net9+ uses a `FrozenDictionary` alternate lookup on spans (`EnumNameLookup.cs:13`, `:28`) and allocates nothing.
- Smaller losses, net10:
  - GetNames: 19.09 ns / 184 B against Enums.NET 16.30 ns / 0 B. The generated method returns a new array per call (`StringEnumSourceWriter.cs:346-349`); Enums.NET returns a cached list.
  - GetValues: 11.98 vs 11.46 ns against NetEscapades.
  - Flags format: 36.1870 vs 34.8196 ns against the BCL.
  - Native AOT binary: 1.9 MB against 1.2 MB for NetEscapades.

## Where CSE wins or ties

- ToString of a defined value: ties NetEscapades on net10 and net8; 2.06× faster than Enums.NET and 15.94× faster than the BCL on net10. NetEscapades is faster on net9 and net11.
- Case-sensitive valid parse on net10: 22.1890 ns against NetEscapades 28.2404 ns and BCL 43.0744 ns. That row is the runtime `EnumValueParser`, not generated code. The BCL win is net10 only and weak evidence (see the caveat on the net10 BCL rows): the BCL beats CSE in every parse row on net8, net9 and net11 (net11 valid case-sensitive 14.7735 vs 20.6890 ns).
- GetValues and GetNames against the BCL on every TFM.
- Flags format against Enums.NET on every TFM (36.1870 ns / 96 B against 84.5527 ns / 280 B on net10).
- Native AOT: 0 IL warnings and runs, like NetEscapades.

## Where the competitors lose

- Enums.NET: 4 IL warnings under Native AOT; flags format 2.34× CSE with 280 B.
- NetEscapades: invalid case-sensitive parse is 57.3845 ns against CSE 39.8918 ns on net10, and CSE also wins on net9 and net11 (net8: CSE is slower, 75.539 vs 58.871 ns). The generated `TryParse` is a `switch` of `s.Equals(name, comparison)` guards, so a miss tests every name in turn ([SourceGenerationHelper.cs#L1212-L1230](https://github.com/andrewlock/NetEscapades.EnumGenerators/blob/v1.0.0-beta21/src/NetEscapades.EnumGenerators.Generators/SourceGenerationHelper.cs#L1212-L1230)). It also ships only as a prerelease.
- BCL: allocates 24 B on every ToString and 104 B / 184 B on GetValues / GetNames.

## Caveats to keep in the post

- NetEscapades 1.0.0-beta21 is a prerelease. No stable version exists.
- Rows under about 1 ns sit near the timer and loop overhead; quote them as orders of magnitude.
- The net10 BCL parse rows are far above the other TFMs. net10 ran first; the cause was not investigated.
- GetValues allocation drops to 0 B on net10 and net11 for both generators with the same generated code. That is a runtime effect, and it was not investigated.
- The net8 column runs CSE's netstandard2.1 build.
- Semantics differ outside the parity inputs: CSE accepts aliases and defined numbers, rejects undefined numbers and whitespace, and returns a reasoned error. `ToWireName` rows are different work and are not compared.
- The "why" notes come from reading the code, not from a profiler.

## Possible angles

- "Generated code is not automatically fast": the same generator writes a `switch` for `ToOptimizedString` (ties the fastest) and a forwarding call for `IsDefined` (slowest of the libraries). Show both generated methods side by side.
- "The cost of a good error message": 88 B per rejected input, paid even when `TryParseWire` discards the error.
- "Your package's net8 story is its netstandard build": parse allocates on every call on net8 only.
- "Where CSE loses": IsDefined, Description, undefined ToString, parse speed on valid and invalid input (invalid is 2.99× and 4.14× Enums.NET on net10, and the BCL beats CSE parse on net8, net9 and net11), invalid-input allocation, net8 parse allocation on every call, GetNames allocation and AOT binary size. Name the losses first.
