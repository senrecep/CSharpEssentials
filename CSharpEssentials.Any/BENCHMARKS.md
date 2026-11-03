# CSharpEssentials.Any: Performance benchmarks (draft)

This page compares CSharpEssentials.Any (CSE) `Any<int, string>` with OneOf 3.0.271 `OneOf<int, string>` and with a Dunet 1.16.2 union (`IntOrText`). The JSON rows for OneOf use the third-party OneOf.Serialization.SystemTextJson 1.1.1 add-on. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11.

**Status: draft.** The suite builds, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet (see [Results](#results)).

All three packages are pinned exactly in `Directory.Packages.props`, and each pin is the latest stable release on NuGet as of 2026-11-03:

| Package | Version | Published | License |
|---|---|---|---|
| OneOf | 3.0.271 | 2024-05-16 | MIT. The nuspec has only a `licenseUrl`, which points to [licence.md](https://github.com/mcintyre321/OneOf/blob/6e02dbe75d0f20f198c640d4c04190a85c5ac9e6/licence.md) (MIT text). |
| OneOf.Serialization.SystemTextJson | 1.1.1 (its only version) | 2023-11-05 | MIT (nuspec expression). It comes from a different author than OneOf. OneOf has no System.Text.Json support of its own, so this add-on is the only option for the OneOf JSON column. |
| Dunet | 1.16.2 | 2026-04-25 | MIT (nuspec expression). A source generator plus a small `Dunet.dll` |

OneOf.SourceGenerator is not included. All it does is generate `OneOfBase` subclass boilerplate, and the suite uses inline `OneOf<int, string>`, which the generator never touches.

The three libraries store a union in different ways, and most of the results follow from that:
- CSE `Any<T0, T1>` is a `readonly struct` holding an `int Index` and an `object? Value`. A value-type case is boxed when the union is built and unboxed when it is read.
- OneOf `OneOf<T0, T1>` is a `readonly struct` with one typed field per case and an index, so it does not box.
- A Dunet union is a user-declared `[Union] partial record` with nested variant records. Dunet makes the base abstract and generates the match members. The suite declares its variants `sealed`. Every union value is a heap object.

---

## What was measured

The code is in `benchmarks/CSharpEssentials.Any.Comparison.Benchmarks/`, in six BenchmarkDotNet classes. The same project also holds the four These classes, which are documented in [CSharpEssentials.These/BENCHMARKS.md](../CSharpEssentials.These/BENCHMARKS.md).
- Every class has `[MemoryDiagnoser]`.
- CSE is the baseline in every category that has a like-for-like CSE row.
- Each class checks parity in `[GlobalSetup]`, and `--self-check` runs the same check. Every row in a category must give the same result for the same input, or the run fails.
- Rows that do different work go in a category of their own and are not compared with anything.
- Each competitor row has a comment above it that links to the documentation it follows.

| # | Class | Measured unit | Rows |
|---|---|---|---|
| 1 | `ConstructComparison` | Build the union from an int (42) or a string ("forty-two") read from a field, through the implicit conversion | CSE, OneOf, Dunet, in categories FromInt and FromText |
| 2 | `MatchComparison` | Fold to an int: the int case returns the value (42), the text case returns the length (9). `[Params]` picks the active case | ToInt: CSE `Match(...).Result`, OneOf `Match`, Dunet `Match`. Switch: CSE `Switch`, OneOf `Switch`, Dunet void `Match`, each writing to one static sink |
| 3 | `TryPickComparison` | Get the int, or -1 when the union holds text | ByCase: CSE `IsFirst ? GetFirst()`, OneOf `TryPickT0`, Dunet `is IntOrText.Number n`. ByRuntimeType: CSE `TryAs<int>` only |
| 4 | `PartitionComparison` | Split 1000 unions (500 ints, 500 strings) into `(int[], string[])` | CSE `Partition()`. Hand-written loops: CSE `IsFirst`/`GetFirst` (reference row), OneOf `TryPickT0`, Dunet stateful `Match` |
| 5 | `JsonComparison` | System.Text.Json `Serialize` of the union, and `Deserialize` of each library's own payload | CSE built-in, OneOf through the add-on, Dunet through `[JsonDerivedType]` |
| 6 | `ToStringComparison` | Each library's default `ToString()` | One category per library and no baseline. Not a ranking |

Inputs: `IntValue = 42` and `TextValue = "forty-two"`. `Texts[i] = "t" + i` is preallocated for i in 0 to 999. In the partition input, even indexes hold the int `i` and odd indexes hold `Texts[i]`. The benchmark methods do not build the unions they read: setup builds them and stores them in fields. Only `ConstructComparison` builds unions inside the measured method. Every lambda is `static`.

On net8.0, CSE runs its netstandard2.1 build, because CSharpEssentials.Any, These and Json have no net8 target. On net9.0 and later, CSE runs the build for that TFM.

---

## Semantic differences

The parity checks use inputs on which every row agrees. The libraries still differ in ways the numbers alone do not show.

- **Storage.** CSE keeps the value in `object? Value`, so building an int case boxes it (24 B on 64-bit) and every read unboxes it. A string case is stored as a reference and is not boxed, which is why FromText is the control row. OneOf keeps typed fields and does not allocate. A Dunet union is a record class, so every value allocates, whichever case it holds.
- **Exhaustiveness.** CSE `Match` and `Switch` handlers are optional. When no handler applies, CSE returns `AnyActionStatus.NotExecuted`, so nothing checks at compile time that every case is handled. OneOf and Dunet need a handler for every case. CSE `Match` returns `AnyActionResult<T>`, and its value is read through `.Result`.
- **Null value.** CSE `Match` and `Switch` throw `InvalidOperationException` when `Value` is null. That is the state `default(Any<...>)` is in.
- **TryPick.** OneOf `TryPickT0` also writes the remainder (for two cases, the bare other value `T1`; for more, a `OneOf` of the rest). CSE `TryAs<T>` tests the runtime type of `Value`, not the active case. The two differ for a union such as `Any<int, int>`, so `TryAs` sits in its own category.
- **Partition.** Only CSE ships one. The OneOf and Dunet rows are hand-written loops that use each library's documented access API. The Dunet row passes a value tuple of two lists as the state of the stateful `Match`, so it needs no extra holder object. Every row fills unsized lists and copies them to arrays, as CSE `Partition` does.
- **JSON wire format.** The payloads differ, so the payload lengths differ too. The parity check compares the decoded case and value, never the JSON text.

  | Library | Int payload | Text payload |
  |---|---|---|
  | CSE | `{"Index":0,"Value":42}` | `{"Index":1,"Value":"forty-two"}` |
  | OneOf add-on | `{"Int32":42}` | `{"String":"forty-two"}` |
  | Dunet | `{"$type":"Number","Value":42}` | `{"$type":"Text","Value":"forty-two"}` |

  - CSE deserializes the value as a `JsonElement` and then parses its raw text a second time against the case type (`CSharpEssentials.Any/Any.cs:72`).
  - The OneOf add-on builds its converter with `MakeGenericType` and `Activator.CreateInstance`, and calls a factory through `MethodInfo.Invoke` on every read.
  - Dunet uses System.Text.Json's built-in polymorphism.
  - The add-on README registers `OneOfJsonConverter` and `OneOfBaseJsonConverter`. It also says the second one can be left out when no `OneOfBase` types are serialized, and the suite leaves it out.
- **ToString.** The outputs differ, so these rows measure each library's default `ToString` and are not a ranking:
  - CSE returns the JSON of the value, `42` or `"forty-two"` (with the quotes), through reflection-based System.Text.Json.
  - OneOf returns `System.Int32: 42`.
  - Dunet returns `Number { Value = 42 }`.

---

## Feature matrix

This table comes from reading source, docs and nuspecs. None of it was measured. OneOf links point at commit `6e02dbe` (3.0.271) and Dunet links at commit `945c38e` (1.16.2).

| Feature | CSE Any | OneOf 3.0.271 | Dunet 1.16.2 |
|---|---|---|---|
| Shape | `readonly struct`, `int Index` + `object? Value` | `readonly struct`, one typed field per case + index | user-declared `partial record` hierarchy (class, one allocation per value) |
| Value-type payload boxing | Yes, when built (and unboxed on read) | No | No, but the record itself allocates |
| Arity | 2-8 | 1-9 (OneOf.Extended goes up to 32) | Any number of named variants |
| Ad-hoc inline use (no type declaration) | Yes | Yes | No, a declared type is required |
| Exhaustive Match (compile time) | No. Handlers are optional and an unmatched case gives `NotExecuted` | Yes | Yes |
| Switch / Action form | `Switch`, returns `AnyActionStatus` | `Switch` | void `Match` |
| Stateful (closure-free) match | No | No | Yes, [`Match(state, ...)`](https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#stateful-matching) |
| Try-get per case | `IsX`/`GetX`, `TryAs<T>` (runtime type) | [`TryPickTx(out value, out remainder)`](https://github.com/mcintyre321/OneOf/blob/6e02dbe75d0f20f198c640d4c04190a85c5ac9e6/README.md?plain=1#L107), `IsTx`/`AsTx` | C# pattern `is Variant v`, `UnwrapX` |
| Map per case | No | `MapTx` | No |
| Partition / Traverse over sequences | Yes, all arities | No | No |
| JSON (System.Text.Json) | Built in, `{"Index","Value"}` | None. Third-party add-on, `{"Int32":42}` | Through STJ [`[JsonDerivedType]`](https://github.com/domn1995/dunet/blob/945c38e48c048d7062edc392e19196b3cef1227f/Readme.md#serializationdeserialization), `{"$type",...}` |
| ToString | JSON of the value (reflection) | `"<Type.FullName>: <value>"` | Record `ToString` |
| `default(T)` | `IsFirst` is true but `Value` is null. `GetFirst()` throws a `NullReferenceException` for a value-type first case (unboxing null, `AnyT2.cs:28`) and returns null for a reference type. `Match`/`Switch` throw | Index 0 with the default value | n/a (class, `null`) |
| Native AOT / trim | Not annotated. `ToString` and deserialize call reflection-based STJ (`Any.cs:72`, `AnyT2.cs:70`) | The core uses no reflection. The JSON add-on uses `MakeGenericType`, `Activator` and `MethodInfo.Invoke` | Generated code, no reflection. JSON needs an STJ source-gen context for AOT (not verified) |
| License | MIT | MIT (licence.md) | MIT |

---

## Results

**Pending.** No full run has been done yet. The numbers will come from the default BenchmarkDotNet job on net8.0, net9.0, net10.0 and net11.0, and the reports will be committed under `benchmarks/results/any/{tfm}/results/`. Short-job (`--quick`) numbers are not published here.

---

## Caveats

- The OneOf JSON column measures OneOf.Serialization.SystemTextJson, a third-party add-on, not OneOf itself. OneOf ships no System.Text.Json converter.
- On net8.0 CSE runs its netstandard2.1 build. The repo's System.Text.Json pin (`[9.0.4,)`) means every column on net8 uses the STJ 9.0.4 package. On net9.0 and later the framework's copy wins.
- Dunet has to allocate for every union value, because that is how it is designed. Its Construct rows measure that design, not overhead from the library.
- The OneOf and Dunet Partition rows are hand-written loops, because neither library has a partition helper.
- The Dunet JSON rows use reflection-based STJ with `[JsonDerivedType]`. AOT with a source-gen context was not tried.
- The CSE column is the 7.0.0 working tree on branch `perf/bench-any`, which may not match the version published on NuGet.
- The feature matrix comes from reading source and docs.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project any                       # all TFMs (net8-net11), Any and These classes, default job
./run-benchmarks.sh --project any --tfm net10.0         # one TFM
./run-benchmarks.sh --project any --quick --tfm net10.0 # one TFM, Short job, one filter per class
dotnet run -c Release -f net10.0 --project CSharpEssentials.Any.Comparison.Benchmarks -- --self-check
```

Without `--tfm`, `--quick` runs on net9.0, the script default. Short-job numbers cannot be compared with default-job numbers. Results are stored per framework under `benchmarks/results/any/{tfm}/results/`.
