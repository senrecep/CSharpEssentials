# Blog notes: CSharpEssentials.Json and Core against the BCL, Macross, Humanizer and ShortGuid (draft)

These are notes for a post, not the post. The numbers will come from `CSharpEssentials.Json/BENCHMARKS.md` and `CSharpEssentials.Core/BENCHMARKS.md` once the full default-job run is committed under `benchmarks/results/json/{tfm}/results/`. There are no numbers yet.

The suite runs on .NET 8, 9, 10 and 11 with BenchmarkDotNet 0.14.0. The libraries compared are System.Text.Json and the BCL, Macross.Json.Extensions 3.0.0, Humanizer.Core 3.0.10 and CSharpVitamins.ShortGuid 2.0.0. All three packages are MIT.

## Framing

- Working title: "Is a helper library still worth it once the BCL caught up?"
- Most of what CSE.Json and CSE.Core do now has a BCL answer: `JsonStringEnumConverter(policy)`, `[JsonPolymorphic]`, `JsonNamingPolicy.SnakeCaseLower`, `Base64Url`, `RandomNumberGenerator.Shuffle`, `Task.WaitAsync`. The benchmark puts each CSE helper next to that answer and next to the library people used before the BCL had one.
- CSE's case is behaviour the BCL does not have (fallback members, strict number reads, many date formats, automatic polymorphism). The numbers should show what that behaviour costs.

## What each library provides

- System.Text.Json and the BCL:
  - String enums with a naming policy, `[JsonStringEnumMemberName]` on STJ 9+, and an AOT-friendly generic converter.
  - Polymorphism through a declared `[JsonDerivedType]` list.
  - ISO 8601-1:2019 dates only, plus a documented `DateTime.Parse` fallback converter.
  - Snake, kebab and camel naming policies; `Base64Url` on .NET 9+; `RandomNumberGenerator.Shuffle`, `Random.Shuffle` and `Random.GetItems`; `Task.WaitAsync`.
- Macross.Json.Extensions 3.0.0:
  - `JsonStringEnumMemberConverter` with `[EnumMember]` names and a fallback value for failed reads.
  - Microsoft `/Date()/` dates (not benchmarked).
  - No trim or AOT annotations; it uses `DynamicMethod`.
- Humanizer.Core 3.0.10: `Underscore`, `Kebaberize` and `Camelize` over regex-based inflectors. Trimmable and annotated.
- CSharpVitamins.ShortGuid 2.0.0: a `ShortGuid` struct with implicit casts, the RFC 4648 URL-safe alphabet and a strict decode.
- CSE:
  - Json: `[StringEnum]` conventions with generated metadata, Data and Input read modes, `[EnumFallback]`, flags as a JSON array; `PolymorphicJsonConverterFactory` with no derived-type list; `MultiFormatDateTimeConverterFactory` over a fixed format list plus Unix seconds.
  - Core: `ToSnakeCase`/`ToKebabCase`/`ToCamelCase`, `Guider`, `GetRandomItems` on a cryptographic RNG, and `WithCancellation` for Task and ValueTask.

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
| Multi-format DateTime read | 39 formats (34 after the cleanup) plus Unix seconds | ISO 8601-1:2019 only | `/Date()/` only |
| 7-digit ISO fraction | No (bug) | Yes | n/a |

Core:

| Feature | CSE | BCL | Humanizer 3.0.10 | ShortGuid 2.0.0 |
|---|---|---|---|---|
| snake / kebab / camel | Yes | `JsonNamingPolicy` (STJ 8+) | `Underscore`/`Kebaberize`/`Camelize` | n/a |
| Acronym split (`HTTPStatus`) | No | Yes | Yes | n/a |
| URL-safe GUID text | Yes (swapped alphabet until the fix) | `Base64Url` (net9+) | n/a | Yes, RFC alphabet, strict decode |
| Unique random sample | Yes, cryptographic RNG | `RandomNumberGenerator.Shuffle` + slice | n/a | n/a |
| Await with cancellation | Task and ValueTask | `Task.WaitAsync`, Task only | n/a | n/a |

## Methodology

- Eight BenchmarkDotNet classes in `benchmarks/CSharpEssentials.Json.Comparison.Benchmarks/`, each with `[MemoryDiagnoser]`. CSE is the baseline in every category that has a CSE row.
- Every class checks parity in `[GlobalSetup]`, and `--self-check` runs the same checks without BenchmarkDotNet. A row that gives a different result stops the run.
- Rows that do different work are labelled, not compared as if they were the same: flags (array against comma string), the `Random.Shared` rows (non-cryptographic), `GetItems` (with replacement), and the GUID rows (different alphabets, so parity is a round trip per library).
- Every competitor row follows its documented usage and carries a link to that documentation.
- Every `JsonSerializerOptions` is built once. The enum and polymorphic rows all start from `JsonSerializerDefaults.Web`.
- The CSE polymorphic row adds `PolymorphicJsonConverterFactory` to plain Web options, not the Readme's `ConvertToJson()`/`DefaultOptions`, which also turns on IgnoreCycles, relaxed escaping and ignore-nulls. Both rows then share the same base options, and the number is the factory's cost alone.
- On net8 CSE runs its netstandard2.1 builds, and every JSON row runs the System.Text.Json 9.0.4 package. On net9 and later the framework's STJ is used.

## The numbers that matter

Pending the full run. These are the rows to pull:

1. Enum Write and Read: CSE generated metadata against the built-in converter and Macross's dictionary-based converter.
2. Enum InvalidFallback: CSE against Macross, the only two with a fallback.
3. Polymorphic Serialize and Deserialize: CSE's JsonDocument round trip against `[JsonPolymorphic]`.
4. DateTime LateList: CSE's format walk against `DateTime.Parse`.
5. Case conversion on the 280-character input, where CSE uses `ArrayPool`.
6. Guider Encode and Decode against `Base64Url` and ShortGuid.
7. RandomItems Unique at 99 percent, where CSE's rejection loop is at its worst.
8. WithCancellation Pending: allocated bytes against `WaitAsync`.

## Where CSE is expected to lose (state it, do not soften it)

These predictions come from reading the code. Nothing has been measured yet.

- Polymorphism against `[JsonPolymorphic]`. CSE reads into a `JsonDocument` and parses `GetRawText()` a second time (`CSharpEssentials.Json/PolymorphicJsonConverterFactory.cs:73,87`). It writes by serializing to a `JsonElement` and copying every property (`:102-105`). STJ reads and writes once.
- The late-list DateTime. CSE calls `TryParseExact` once per format until one matches (`MultiFormatDateTimeConverter.cs:136-140`), and `dd-MMM-yyyy` is entry 30 (entry 27 after the pending format-list cleanup). It also allocates the string first (`:128`) and boxes the result (`:139`).
- WithCancellation allocations. CSE allocates a `TaskCompletionSource`, a token registration, a `WhenAny` task and an async state machine for every pending task (`CSharpEssentials.Core/Extensions/GeneralExtensions.cs:77-105`). `WaitAsync` is built into the task.
- Cryptographic RNG against `Random.Shared`. CSE calls `RandomNumberGenerator.GetInt32` once per draw (`RandomItemsExtensions.cs:40,82`), and its rejection loop draws more often as K approaches n (`:38-45`). The `Random.Shared` rows are expected to be faster, but they do not do the same job.
- How case conversion compares with the BCL on long inputs is unknown. CSE makes two passes and calls `char.GetUnicodeCategory` per character in each (`StringExtensions.cs:133-159`, then `:65-81`).

## Where CSE may win or tie

To be confirmed by the run:
- Enum Write and Read against Macross. Macross reflects once when the converter is built, then does a dictionary lookup and `WriteStringValue(string)` per value, the same shape as CSE's write (`CSharpEssentials.Json/EnumConverter.cs:46`), so a tie is as likely as a win. Its Read also allocates a string per value (`reader.GetString()`).
- Enum fallback reads, where only CSE and Macross have the feature.
- Guider against ShortGuid, whose decode re-encodes to check the input.

Features only CSE ships (not a speed claim):
- Strict reads that reject undefined numbers.
- Polymorphism without a derived-type list.
- Many date formats without a custom converter.
- `WithCancellation` for `ValueTask`.

## Caveats to keep in the post

- The benchmark found a CSE bug. `MultiFormatDateTimeConverter` has no 7-digit fraction format (`CSharpEssentials.Json/MultiFormatDateTimeConverter.cs:37-98`, `:136-149`), so the `O` form STJ writes for a `DateTime` throws on read. The Iso input uses a 3-digit fraction for that reason.
- Flags rows write different payloads and do not do the same work.
- The GUID texts differ until CSE moves to the RFC alphabet; parity is a round trip within each library.
- On net8 every JSON row uses the STJ 9.0.4 package, so those are not STJ 8 numbers.
- Macross 3.0.0 is from 2022 and targets net6.0. It is still the latest stable release.
- Pending CSE changes affect enum writes, polymorphism, the DateTime list, the GUID alphabet and case conversion. The self-check has to pass again after they land, and the numbers have to come from after that.

## Possible angles

- "The BCL caught up": most rows have a BCL answer now, so the post is about the behaviour CSE adds, not about speed.
- "Convenience has a price": automatic polymorphism and the format list are the two places where CSE trades speed for less setup.
- "The bug the benchmark found": the 7-digit fraction, which STJ itself writes.
- "Crypto randomness is not free": the cost of `RandomNumberGenerator` against `Random.Shared`, and why CSE picks it.
