# Blog notes: CSharpEssentials.Any, OneOf and Dunet (draft)

These are notes for a post, not the post. The numbers will come from `CSharpEssentials.Any/BENCHMARKS.md` once the full default-job run is committed under `benchmarks/results/any/{tfm}/results/`. There are no numbers yet.

The suite runs on .NET 8, 9, 10 and 11 with BenchmarkDotNet 0.14.0. The libraries compared are OneOf 3.0.271, the third-party OneOf.Serialization.SystemTextJson 1.1.1 add-on (for JSON only) and Dunet 1.16.2. These has no library competitor and is compared with a hand-written record (see `CSharpEssentials.These/BENCHMARKS.md`).

## Framing

- Working title: "Where your union keeps its value decides what it costs."
- The three libraries keep the value in three different places:
  - CSE stores it as `object`.
  - OneOf stores it in typed fields.
  - Dunet makes it a class hierarchy.
- Building, matching and serializing a union each follow from that choice. The benchmark measures the same operations on the same inputs, and every row is checked to give the same result.
- CSE Any is the inline, ad-hoc option like OneOf, but it boxes. Unlike OneOf, it ships Partition/Traverse and built-in System.Text.Json. Whether those features are worth the boxing is the question the numbers should answer.

## What each library provides

- **OneOf 3.0.271:**
  - `OneOf<T0..T8>` structs with typed fields. `Match`, `Switch`, `TryPickTx` and `MapTx`.
  - `OneOfBase` lets you declare named unions.
  - No JSON support of its own. The System.Text.Json column uses OneOf.Serialization.SystemTextJson, a separate MIT package from a different author, which writes `{"Int32":42}`.
- **Dunet 1.16.2:**
  - A source generator. You declare `[Union] partial record` with nested variant records, and it generates `Match`, a void `Match`, a stateful `Match(state, ...)`, `MatchX`, implicit conversions and `UnwrapX`.
  - JSON goes through STJ's `[JsonDerivedType]` polymorphism.
  - Every value is a heap object.
- **CSE Any:**
  - `Any<T0..T7>` structs with an index and an `object` value. Implicit conversions, optional-handler `Match`/`Switch`, `IsX`/`GetX`, `TryAs<T>`, and `Partition`/`Traverse` over sequences.
  - Built-in JSON as `{"Index":0,"Value":42}`.

## The numbers that matter

Pending the full run. These are the rows to pull:

1. Construct, int case: allocated bytes and time for all three. This shows the cost of boxing against the cost of allocating a class.
2. Construct, text case: the control, where CSE does not box.
3. Match/Switch, int case: the cost of unboxing in CSE against a typed read in OneOf and a virtual call in Dunet.
4. Partition over 1000 items: CSE `Partition` against the same loop written by hand over CSE. This isolates the per-item delegate cost.
5. JSON Deserialize: CSE's double parse against the add-on's reflection invoke against STJ polymorphism.

## Where CSE is expected to lose (state it, do not soften it)

These predictions come from reading the code. Nothing has been measured yet.

- **Boxing.** `Any<T0, T1>` stores `object? Value` (`CSharpEssentials.Any/AnyT2.cs:20-21`), so every int case allocates 24 B when built and is unboxed on every read. OneOf does not allocate.
- **JSON deserialize.** The value is read as a `JsonElement` and parsed a second time from `GetRawText()` (`CSharpEssentials.Any/Any.cs:72`). That second parse uses `EnhancedJsonSerializerOptions.DefaultOptions`, not the caller's options. A naming policy on the outer call is therefore not applied to the inner value. That is a correctness bug as well as a speed cost.
- **Partition.** `item.Switch(first: first.Add, second: second.Add)` (`CSharpEssentials.Any/AnyCollectionExtensions.cs:39`) allocates two delegates per item. The hand-written CSE loop is the reference that shows this cost.
- **No exhaustiveness.** Handlers are optional and an unmatched case returns `NotExecuted`. Nothing enforces this at compile time, so callers have to check for it. OneOf and Dunet both refuse to compile a match that misses a case.
- **ToString** serializes the value through reflection-based STJ (`CSharpEssentials.Any/AnyT2.cs:70`). The outputs differ from the competitors', so this is not ranked, but it is not AOT-friendly.
- **These Partition** allocates three capturing delegates per item (plus one closure per call) (`CSharpEssentials.These/TheseCollectionExtensions.cs:14-17`).

## Where CSE may win or tie

To be confirmed by the run:
- Text-case construct and match, where no boxing happens.
- Possibly JSON serialize.

Features only CSE ships (not a speed claim):
- Partition and Traverse, which neither competitor ships.
- Built-in JSON, where OneOf needs a third-party package. The deserialize path is still expected to lose (see above).

## Caveats to keep in the post

- The OneOf JSON column is a third-party add-on, not OneOf.
- Dunet allocating on every construct is by design: it is a class hierarchy. Don't present that as overhead.
- The OneOf and Dunet partition rows are hand-written, because neither library has a partition.
- The wire formats differ, so the JSON payload sizes differ.
- ToString outputs differ and are not a ranking.
- On net8 CSE runs its netstandard2.1 build, and System.Text.Json is the 9.0.4 package for every column.
- The These pipeline baseline copies CSE's lossy FlatMap-on-Both rule so parity holds: errors are never combined, so either the mapper's left (on Right or Both) or the original left (on Left) is dropped.

## Possible angles

- "Boxing is the whole story": compare the int and text cases side by side.
- "A struct is not automatically free": a struct union that holds `object` still allocates.
- "Batteries included against a minimal core": CSE ships Partition and JSON, OneOf keeps a small core and leaves JSON to add-ons, and Dunet hands JSON to STJ.
- "The bug the benchmark found": the deserializer ignores the caller's options (`Any.cs:72`).
