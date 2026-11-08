# Blog notes: CSharpEssentials.Json and Core against the BCL, Macross, Humanizer and ShortGuid

These are notes for a post, not the post. Every number comes from `CSharpEssentials.Json/BENCHMARKS.md` and `CSharpEssentials.Core/BENCHMARKS.md`, which read `benchmarks/results/json/{tfm}/results/*-report-github.md` and `benchmarks/results/core/{tfm}/results/*-report-github.md`.

The suite runs on .NET 8, 9, 10 and 11 with BenchmarkDotNet 0.14.0, default job, on one Linux machine with 6 cores and 12 threads (x64, RyuJIT AVX2). The SDK was 11.0.100-rc.1; the runtimes were .NET 8.0.30, 9.0.19, 10.0.11 and 11.0.0. The case conversion and Guider classes were rerun on main after pull requests #230 to #256 landed, with SDK 11.0.100-rc.3 and .NET 8.0.33, 9.0.22, 10.0.14 and 11.0.0. The libraries compared are System.Text.Json and the BCL, Macross.Json.Extensions 3.0.0, Humanizer.Core 3.0.10 and CSharpVitamins.ShortGuid 2.0.0. All three packages are MIT. The CSE code is the local `integ/all` branch (all open pull requests merged, commit `fe91b26a`).

## Framing

- Working title: "Is a helper library still worth it once the BCL caught up?"
- Most of what CSE.Json and CSE.Core do now has a BCL answer: `JsonStringEnumConverter(policy)`, `[JsonPolymorphic]`, `JsonNamingPolicy.SnakeCaseLower`, `Base64Url`, `RandomNumberGenerator.Shuffle`, `Task.WaitAsync`, `Guid.CreateVersion7`. The benchmark puts each CSE helper next to that answer and next to the library people used before the BCL had one.
- CSE's case is behaviour the BCL does not have (fallback members, strict number reads, many date formats, automatic polymorphism, in-process ordered GUIDs). The numbers show what that behaviour costs.
- The measured answer: the BCL is faster than CSE in most like-for-like rows. CSE is faster than Macross on enum writes and faster than Humanizer and ShortGuid on every row.

## What each library provides

- System.Text.Json and the BCL:
  - String enums with a naming policy, `[JsonStringEnumMemberName]` on STJ 9+, and an AOT-friendly generic converter.
  - Polymorphism through a declared `[JsonDerivedType]` list.
  - ISO 8601-1:2019 dates only, plus a documented `DateTime.Parse` fallback converter.
  - Snake, kebab and camel naming policies; `Base64Url` on .NET 9+; `RandomNumberGenerator.Shuffle`, `Random.Shuffle` and `Random.GetItems`; `Task.WaitAsync`; `Guid.CreateVersion7` on .NET 9+.
- Macross.Json.Extensions 3.0.0:
  - `JsonStringEnumMemberConverter` with `[EnumMember]` names and a fallback value for failed reads.
  - `/Date()/` dates (not benchmarked).
  - No trim or AOT annotations; it uses `DynamicMethod`.
- Humanizer.Core 3.0.10: `Underscore`, `Kebaberize` and `Camelize` over regex-based inflectors. Trimmable and annotated.
- CSharpVitamins.ShortGuid 2.0.0: a `ShortGuid` struct with implicit casts, the RFC 4648 URL-safe alphabet and a strict decode.
- CSE:
  - Json: `[StringEnum]` conventions with generated metadata, Data and Input read modes, `[EnumFallback]`, flags as a JSON array; `PolymorphicJsonConverterFactory` with no derived-type list; `MultiFormatDateTimeConverterFactory` over a fixed list of 34 formats plus Unix seconds.
  - Core: `ToSnakeCase`/`ToKebabCase`/`ToCamelCase`, `Guider` text helpers on the RFC 4648 alphabet, `Guider.NewGuid`/`NewMonotonicGuid` (version 7), `GetRandomItems` on a cryptographic RNG, and `WithCancellation` for Task and ValueTask.

## Feature matrix

Json:

| Feature | CSE | STJ built-in | Macross 3.0.0 |
|---|---|---|---|
| Enum as string with a naming policy | Yes | Yes | Yes |
| Custom member names | `[JsonStringEnumMemberName]` or `[EnumMember]` | `[JsonStringEnumMemberName]` (STJ 9+) | `[EnumMember]` or `[JsonPropertyName]` |
| Reads the member name in any case | Yes | Yes | No |
| Unknown string to a fallback member | `[EnumFallback]` (Data mode) | No | `deserializationFailureFallbackValue` |
| Strict mode rejects undefined numbers | Yes (Input mode) | No; `99` is accepted by default, and `allowIntegerValues: false` rejects every number, defined ones too | No; `99` is accepted by default, and `allowIntegerValues: false` rejects every number, defined ones too |
| Flags wire format | JSON array | Comma string | Comma string |
| AOT-friendly enum path | Yes | `JsonStringEnumConverter<T>` only | No |
| Polymorphism discriminator | Automatic, CLR full name | Declared | None |
| Polymorphism needs a derived-type list | No | Yes | n/a |
| Polymorphism under AOT | No | Yes | n/a |
| Multi-format DateTime read | 34 formats plus Unix seconds | ISO 8601-1:2019 only | `/Date()/` only |
| 7-digit ISO fraction | No (bug) | Yes | n/a |

Core:

| Feature | CSE | BCL | Humanizer 3.0.10 | ShortGuid 2.0.0 |
|---|---|---|---|---|
| snake / kebab / camel | Yes | `JsonNamingPolicy` (STJ 8+) | `Underscore`/`Kebaberize`/`Camelize` | n/a |
| Acronym split (`HTTPStatus`) | No | Yes | Yes | n/a |
| URL-safe GUID text | Yes, RFC alphabet | `Base64Url` (net9+) | n/a | Yes, RFC alphabet, strict decode |
| Unique random sample | Yes, cryptographic RNG | `RandomNumberGenerator.Shuffle` + slice | n/a | n/a |
| Await with cancellation | Task and ValueTask | `Task.WaitAsync`, Task only | n/a | n/a |
| Version 7 GUID | Every target, plus an in-process ordered variant | `Guid.CreateVersion7` (net9+) | n/a | n/a |

## Methodology

- Eight BenchmarkDotNet comparison classes in `benchmarks/CSharpEssentials.Json.Comparison.Benchmarks/` and two GUID creation classes in `benchmarks/CSharpEssentials.Core.Benchmarks/`, each with `[MemoryDiagnoser]`. CSE is the baseline in every comparison category that has a CSE row; `Guid.NewGuid` is the baseline of the GUID creation classes.
- Every comparison class checks parity in `[GlobalSetup]`, and `--self-check` runs the same checks without BenchmarkDotNet. A row that gives a different result stops the run.
- Rows that do different work are labelled, not compared as if they were the same: flags (array against comma string), the `Random.Shared` rows (non-cryptographic), `GetItems` (with replacement), and the GUID creation rows (version 4 against version 7, and the monotonic variant's shared state).
- Every competitor row follows its documented usage and carries a link to that documentation.
- Every `JsonSerializerOptions` is built once. The enum and polymorphic rows all start from `JsonSerializerDefaults.Web`.
- The CSE polymorphic row adds `PolymorphicJsonConverterFactory` to plain Web options, not the Readme's `ConvertToJson()`/`DefaultOptions`, which also turns on IgnoreCycles, relaxed escaping and ignore-nulls. Both rows then share the same base options, and the number is the factory's cost alone.
- On net8 CSE runs its netstandard2.1 builds, and every JSON row runs the System.Text.Json 9.0.4 package. On net9 and later the framework's STJ is used.
- Full default-job runs, one suite at a time. The 1-minute load average was 1.43 to 1.74 during the json suite (classes in the Json comparison project) and 3.35 to 4.87 during the core suite (GUID creation), sampled at start and end only. No row in either suite kept a CV above 5%, and no mean is near 0 ns.

## The numbers that matter

Format: mean (ratio against the CSE row), allocated. A ratio below 1.00 means the other row took less time than CSE.

1. Enum Write, 16 values. CSE against STJ and Macross:
   - net8.0: CSE 388.36 ns, STJ 368.09 ns (0.95), Macross 582.15 ns (1.50); all 384 B.
   - net10.0: CSE 394.33 ns, STJ 322.93 ns (0.82), Macross 572.93 ns (1.45); all 384 B.
   - net11.0: CSE 372.48 ns, STJ 321.64 ns (0.86), Macross 488.50 ns (1.31); all 384 B.
2. Enum Read, 16 values. net10.0: CSE 1,143.03 ns (304 B), STJ 1,112.77 ns (0.97, 304 B), Macross 910.53 ns (0.80, 968 B). net8.0: CSE 1,196.57 ns at 304 B against 968 B for both others.
3. Enum InvalidFallback, CSE against Macross: net10.0 80.49 ns (0 B) against 88.53 ns (1.10, 32 B); net11.0 84.08 ns against 83.54 ns (0.99).
4. Polymorphic, CSE against `[JsonPolymorphic]`, net10.0: Serialize 763.5 ns (640 B) against 296.2 ns (0.39, 528 B); Deserialize 668.7 ns against 378.5 ns (0.57), both 232 B.
5. DateTime LateList (`"14-Mar-2024"`), CSE's format walk against `DateTime.Parse`: net10.0 1,249.50 ns against 288.28 ns (0.23), both 48 B. Iso: CSE 94.65 ns, built-in 82.41 ns (0.87), fallback 84.04 ns (0.89), all 0 B.
6. Case conversion, 280 characters, net10.0:
   - Snake: CSE 1,017.55 ns, BCL 735.74 ns (0.72), Humanizer 6,470.99 ns (6.36, 1344 B against 672 B).
   - Camel: CSE 1,047.50 ns, BCL 34.73 ns (0.03), Humanizer 3,890.26 ns (3.71).
7. Guider, net10.0: Encode CSE 21.96 ns, ShortGuid 65.01 ns (2.96, 256 B against 72 B), `Base64Url` 19.63 ns (0.89, 72 B). Decode CSE 35.82 ns (0 B), ShortGuid 258.74 ns (7.22, 696 B), `Base64Url` 22.56 ns (0.63, 0 B).
8. RandomItems Unique, net10.0, cryptographic rows: K = 10 CSE 9,759.15 ns against `RandomNumberGenerator.Shuffle` 1,308,046.47 ns (134.04); K = 500 657,812.08 ns against 1,286,621.72 ns (1.956); K = 990 1,303,631.05 ns against 1,282,952.20 ns (0.984). The non-cryptographic `Random.Shared.Shuffle` row at K = 990 took 5,325.28 ns (0.004).
9. WithCancellation Pending: net10.0 CSE 169.72 ns (456 B) against `WaitAsync` 77.17 ns (0.45, 248 B); net8.0 283.00 ns (584 B) against 105.36 ns (0.37, 272 B).
10. GUID creation, one thread: net10.0 `Guider.NewGuid` 59.39 ns, `NewMonotonicGuid` 71.34 ns, `Guid.NewGuid` 752.17 ns, `Guid.CreateVersion7` 773.69 ns. net11.0: 52.95 ns, 57.92 ns, 57.94 ns and 90.15 ns. `Guid.NewGuid` itself dropped from 752.17 ns on net10.0 to 57.94 ns on net11.0 on this machine.
11. GUID creation, 6 threads, per GUID, net11.0: `Guider.NewGuid` 12.61 ns, `Guid.NewGuid` 12.58 ns, `Guid.CreateVersion7` 18.15 ns, `NewMonotonicGuid` 50.73 ns (4.03).

## Where CSE loses (state it, do not soften it)

Measured, with the reason from the code:

- Polymorphism against `[JsonPolymorphic]`, every TFM (Serialize ratio 0.39 to 0.43, Deserialize 0.56 to 0.60; Serialize also allocates 624 B or 640 B against 528 B). CSE writes by serializing the value to a `JsonElement` and copying every property behind `$type` (`CSharpEssentials.Json/PolymorphicJsonConverterFactory.cs:116`, `:126`). It reads by scanning the object once with a copy of the reader to find `$type` (`:82`) and then deserializing the derived type (`:74`). STJ reads and writes once.
- The late-list DateTime, every TFM (ratio 0.22 to 0.26). CSE gets the string (`MultiFormatDateTimeConverter.cs:128`) and calls `TryParseExact` once per format until one matches (`:136`); `dd-MMM-yyyy` is entry 27 of 34.
- DateTime Iso against the built-in converter, every TFM (ratio 0.83 to 0.87), even with CSE's byte-level ISO fast path (`:157`).
- Enum Write, Read and ReadNumber against STJ, every TFM (Write 0.82 to 0.95, Read 0.86 to 0.97, ReadNumber 0.85 to 0.87). Enum Read against Macross on net8.0 (0.96), net10.0 (0.80) and net11.0 (0.95). Enum InvalidThrow against Macross, every TFM (0.97 to 0.98). InvalidFallback against Macross on net11.0 (0.99).
- Flags, every TFM: STJ and Macross ratio 0.54 to 0.86, and CSE FlagsWrite allocates 56 B against 48 B. The payloads differ (array against comma string).
- Case conversion against `JsonNamingPolicy`, every length and TFM: snake and kebab 0.60 to 0.97, camel 0.03 to 0.27, with the same output and the same bytes. CSE runs one pass with an ASCII fast path for the character category (`CSharpEssentials.Core/Extensions/StringExtensions.cs:137`), so the gap is not explained by a second pass.
- Guider against `Base64Url` on net9.0 to net11.0: Encode 0.89 to 0.97, so close to level on net9.0 and net11.0, and Decode 0.55 to 0.93, for the same text. On net9.0 and later CSE calls `Base64Url.EncodeToString` and `Base64Url.DecodeFromChars` itself (`CSharpEssentials.Core/Guids/Guider.cs:34,68`); decode also checks the length first.
- WithCancellation Pending, every TFM: ratio 0.37 to 0.48, and CSE allocates 456 B against 248 B on net9.0 and later. On net9.0 and later CSE calls `WaitAsync` too (`CSharpEssentials.Core/Extensions/GeneralExtensions.cs:111`, `:117`); the extra cost is the fault-observing continuation it registers first (`:121`). On net8.0 the netstandard2.1 build allocates a `TaskCompletionSource`, a token registration, a `WhenAny` task and an async state machine.
- RandomItems against `Random.Shared` and the hand-written row, every K: these are not cryptographic, so this is the price of the cryptographic RNG, not a like-for-like loss. At K = 990 `RandomNumberGenerator.Shuffle` is also slightly faster than CSE on net10.0 (0.984) and net11.0 (0.989).
- GUID creation on net11.0: `NewMonotonicGuid` is level with `Guid.NewGuid` on one thread (57.92 ns against 57.94 ns) and 4.03 times its time on 6 threads (50.73 ns against 12.58 ns). The 6 threads contend on its one shared compare-and-swap value, which is what keeps the IDs in order.

## Where CSE wins or ties

Measured:
- Enum Write against Macross on every TFM (ratio 1.31 to 1.57), at the same 384 B.
- Enum ReadNumber and InvalidFallback against Macross on net8.0 to net10.0, with 0 B against 24 B and 32 B.
- Enum InvalidThrow against STJ (1.06 to 1.09), with the fewest bytes of the three.
- Enum Read allocation on net8.0: 304 B against 968 B for STJ 9.0.4 and Macross.
- Case conversion against Humanizer on every row (3.26 to 9.59), and Humanizer's Alloc Ratio is 2.00 to 10.50.
- Guider against ShortGuid on every row (Encode 1.20 to 3.75, Decode 2.66 to 8.66); CSE decodes with 0 B, ShortGuid allocates 696 B.
- RandomItems against `RandomNumberGenerator.Shuffle` at K = 10 (129.46 to 134.04) and K = 500 (1.956 to 2.053). CSE allocates the least of all Unique rows.
- `Guider.NewGuid` against `Guid.NewGuid` and `Guid.CreateVersion7` on net8.0 to net10.0 (ratio 0.08 to 0.10 on one thread), and on net11.0 one thread (52.95 ns against 57.94 ns and 90.15 ns). On net11.0 with 6 threads it is level with `Guid.NewGuid` (12.61 ns against 12.58 ns).
- WithCancellation on a completed task is level with `WaitAsync` (ratio 0.99 to 1.05, same 72 B).

Features only CSE ships (not a speed claim):
- Strict reads that reject undefined numbers.
- Polymorphism without a derived-type list.
- Many date formats without a custom converter.
- `WithCancellation` for `ValueTask`.
- Version 7 GUIDs on .NET 8 and an in-process ordered variant.

## Caveats to keep in the post

- The benchmark found a CSE bug. `MultiFormatDateTimeConverter` has no 7-digit fraction format (`CSharpEssentials.Json/MultiFormatDateTimeConverter.cs:37-90`), and its ISO fast path does not take that length, so the `O` form STJ writes for a `DateTime` throws on read. The Iso input uses a 3-digit fraction for that reason.
- Flags rows write different payloads and do not do the same work.
- The GUID text parity is a round trip within each library, even though all three now write the same RFC 4648 text.
- On net8 every JSON row uses the STJ 9.0.4 package, so those are not STJ 8 numbers.
- `Guid.NewGuid` took 750.46 ns to 759.69 ns per call on net8.0 to net10.0 on this machine and 57.94 ns on net11.0, so the GUID ratios swing by TFM. Quote means.
- The core suite (GUID creation) ran at a 1-minute load average of 3.35 to 4.87 on a 12-thread machine; the 6-thread class shared the CPU with that load.
- Macross 3.0.0 is from 2022 and targets net6.0. It is still the latest stable release.

## Possible angles

- "The BCL caught up": most rows have a BCL answer now, and the BCL answer is faster in most like-for-like rows. The post is about the behaviour CSE adds, not about speed.
- "Convenience has a price": automatic polymorphism (ratio 0.39 to 0.60) and the format list (0.22 to 0.26 on a late entry) are the two places where CSE trades speed for less setup.
- "The bug the benchmark found": the 7-digit fraction, which STJ itself writes.
- "Crypto randomness is not free": at K = 990 on net10.0 the two cryptographic rows took 1,303,631.05 ns (CSE) and 1,282,952.20 ns, against 5,325.28 ns for `Random.Shared.Shuffle`.
- "Faster than the old library, slower than the BCL": Humanizer and ShortGuid against `JsonNamingPolicy` and `Base64Url`.
