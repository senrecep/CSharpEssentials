# CSharpEssentials.Errors: Performance benchmarks (draft)

This page compares the CSharpEssentials.Errors (CSE) `Error` type with ErrorOr 2.1.1 and FluentResults 4.0.0. CSharpFunctionalExtensions 3.7.0 appears only where it has a matching API. It covers creating an error, equality and hashing, building an error from an exception, metadata and record `ToString`, and System.Text.Json. The suite uses BenchmarkDotNet v0.14.0 and targets .NET 8, 9, 10 and 11.

**Status: draft.** The suite builds, and its parity self-check passes on every TFM. No full default-job run has been done, so this page has no numbers yet (see [Results](#results)).

The competitors are pinned exactly in `Directory.Packages.props`. Each pin is the latest stable release on NuGet as of 2026-11-03:

| Package | Version | License |
|---|---|---|
| ErrorOr | 2.1.1 | MIT. The nuspec has no license element; the repository `LICENSE.md` is MIT |
| FluentResults | 4.0.0 | MIT (nuspec expression) |
| CSharpFunctionalExtensions | 3.7.0 | MIT (nuspec expression) |

net8.0 runs CSE's netstandard2.1 build, because Errors has no net8.0 target.

Sequence, CombineAll, Try and TryAsync live in the same suite but measure Results APIs. They are described in [CSharpEssentials.Results/BENCHMARKS.md](../CSharpEssentials.Results/BENCHMARKS.md#pending-sequence-combineall-try-and-tryasync). The Maybe collection helpers are in [CSharpEssentials.Maybe/BENCHMARKS.md](../CSharpEssentials.Maybe/BENCHMARKS.md).

---

## What was measured

The code is in `benchmarks/CSharpEssentials.Results.Comparison.Benchmarks/`, in six classes.
- Every class has `[MemoryDiagnoser]`, and CSE is the baseline in each category.
- Each class checks parity in `[GlobalSetup]`. `--self-check` runs the same check for every parameter value without BenchmarkDotNet, for these classes and the nine older ones. A mismatch fails the run.
- Every row returns its result, so the JIT cannot drop the work. Inputs are read from instance fields and built in setup.
- Each library row has a comment above it with its README or source URL. CSE links point at main, and competitor links point at the pinned tag. Hand-written rows have none.
- Shared inputs: code `Order.Invalid`, message `Order is invalid`. Metadata has three entries, `OrderId = 42`, `Field = "Total"` and `Attempt = 3`, added in the same order for every library. The values are boxed once and shared.

| # | Class | Measured unit | Rows | Label |
|---|---|---|---|---|
| 1 | `ErrorFactoryComparison` | `Plain`: 16 errors from 16 prebuilt code and message pairs per op, written into a sink array (`OperationsPerInvoke = 16`). `WithMetadata`: one error with the three entries | CSE `Error.Validation`, ErrorOr `Error.Validation`, FluentResults `new Error(message)` and `WithMetadata` | CSE vs ErrorOr like-for-like. FluentResults labelled: no code or type |
| 2 | `ErrorEqualityComparison` | `Equals` on two equal errors built from distinct string instances, `GetHashCode`, and `new HashSet<Error>` over 100 errors (10 codes, 10 copies each). `WithMetadata` = false or true | CSE, ErrorOr | like-for-like |
| 3 | `ErrorExceptionComparison` | an error from an `InvalidOperationException("boom")`. `Thrown` = true (thrown and caught once in setup) or false (never thrown) | CSE `Error.Exception(ex)` (baseline), CSE `Error.Failure(type name, message)`, ErrorOr `Error.Failure(type name, message)`, FluentResults `new ExceptionalError(ex)` | labelled, see below |
| 4 | `ErrorMetadataToStringComparison` | `ErrorMetadata.ToString()` with `Entries` = 1, 8 or 32 | CSE (baseline), a hand-written `StringBuilder` loop over the same dictionary | like-for-like, exact text |
| 5 | `ErrorToStringComparison` | `ToString()` on an error, an error with metadata, and a success `Result<int>` of 42 | CSE, ErrorOr, FluentResults; CFE in the `Result` category | `Error`: CSE vs ErrorOr like-for-like. The rest labelled |
| 6 | `ErrorJsonComparison` | `Serialize` and `Deserialize` of an error without metadata, one shared `JsonSerializerOptions` with default settings | Serialize: CSE, ErrorOr. Deserialize: CSE only | Serialize like-for-like, exact text |

In `ErrorEqualityComparison` the two errors use distinct code and message strings, but their metadata shares the same key strings and the same boxed values. Key lookups and value comparisons can then succeed on a reference check, for both libraries alike, so the metadata rows are a best case.

CSharpFunctionalExtensions has no column in classes 1 to 4 and 6. Its default error is a `string`, so there is nothing to construct, compare or serialize.

### Parity keys

- Factory: code, type, message and metadata entries in order for CSE and ErrorOr; message and metadata for FluentResults. FluentResults always has an empty metadata dictionary, which the key treats as no metadata.
- Equality: `Equals` gives `True` for both libraries, each library gives the same hash for its two equal errors, and both HashSets hold 10 items.
- Exception: every row has the message `boom`. The CSE rows have code `InvalidOperationException` and type `Failure`. `Error.Exception` has the metadata keys `exceptionType`, `exceptionStackTrace` and `exceptionMessage`. Stack text is never compared.
- Metadata `ToString`: exact string equality, for example `[{ key0: 0 }]`.
- `ToString`: CSE and ErrorOr give the same text for an error without metadata. The other rows only have to contain the message, or `42` for the result rows.
- JSON: CSE and ErrorOr give the same text. CSE deserializes back to an equal error.

---

## Semantic differences

- **FluentResults errors are classes with no code or type.** `new Error(message)` allocates the error, an empty metadata dictionary and an empty reasons list.
- **No FluentResults equality column.** FluentResults `Error` uses reference equality, so `Equals` is false and a HashSet keeps all 100 items.
- **`Error.Exception` does more work than the other rows.** It builds a three-entry metadata dictionary with the type name, the stack trace string and the message. When the exception was never thrown, `StackTrace` is null and CSE falls back to `Environment.StackTrace`, a stack walk whose cost depends on the depth of the calling stack. On a thrown exception, reading `StackTrace` formats the trace again on every call.
- **ErrorOr has no exception bridge.** Its row maps the type name and message by hand, as the README's section on replacing exceptions suggests. The CSE `Error.Failure` row does the same, so it shows CSE without the exception metadata.
- **FluentResults `ExceptionalError` keeps a reference** to the exception and copies nothing.
- **`ToString` output differs.** CSE and ErrorOr `Error` are record structs, and without metadata both print `Error { Code = Order.Invalid, Description = Order is invalid, Type = Validation, NumericType = 2, Metadata =  }`. With metadata, CSE formats each entry, ErrorOr prints the dictionary type name, and FluentResults joins the entries with `; `. On a success result, CSE prints `Success: 42` through reflection-based JSON, and every other library prints its own text.
- **Deserialize is CSE only.** System.Text.Json does not throw. It silently returns a `default` ErrorOr `Error` with a null code. FluentResults throws `NotSupportedException`, and its README says not to serialize its result objects. The serialize rows use no metadata, because a CSE round trip with metadata does not give back an equal error.

---

## Feature matrix

This table comes from reading each library's source and docs. None of it was measured.

| Feature | CSE | ErrorOr 2.1.1 | FluentResults 4.0.0 | CSharpFunctionalExtensions 3.7.0 |
|---|---|---|---|---|
| Error kind | struct (code, description, type, metadata) | struct (same shape) | class (message, metadata, reasons) | `string` by default |
| Typed categories | 8 `ErrorType` values | 7, plus `Custom(int)` | none | none |
| Metadata | `ErrorMetadata : Dictionary<string, object?>` with fluent `AddMetadata`, which keeps the first value for a key | `Dictionary<string, object>` | `WithMetadata`, which throws on a duplicate key | none |
| Value equality | yes, including metadata | yes, including metadata | no (reference) | string equality |
| Hash with metadata | depends on insertion order | depends on insertion order | reference | n/a |
| Exception bridge | `Error.Exception(ex)` copies type, stack trace and message | none | `ExceptionalError` keeps a reference | `ex.Message` |
| `ToString` | record output with formatted metadata | record output with the dictionary type name | `Error with Message='...'` | the string |
| System.Text.Json serialize | yes | yes, same text without metadata | yes, but the README advises against serializing result objects | n/a |
| System.Text.Json deserialize | yes; metadata values come back as `JsonElement` | silently `default` | throws | n/a |
| License | MIT | MIT | MIT | MIT |

---

## Results

**Pending the full default-job run on net8.0-net11.0.** The reports will be committed under `benchmarks/results/results-comparison/{tfm}/results/`. Short-job (`--quick`) numbers are not published here.

---

## Caveats

- **Expected CSE costs and issues, from reading the code and from probes** (not yet measured):
  - `CSharpEssentials.Errors/ErrorMetadata.cs:77-79`: `ToString` calls `ElementAtOrDefault(i)` on the dictionary inside a `for` loop. Each call walks from the start, so the method is O(n²) in the entry count. `ErrorMetadataToStringComparison` shows the curve at 1, 8 and 32 entries.
  - `CSharpEssentials.Errors/Error.cs:232-236`: `GetHashCode` adds metadata entries in insertion order, while `Equals` compares them as a set. Two equal errors whose metadata was added in a different order get different hashes. ErrorOr 2.1.1 has the same behavior. The equality inputs always use the same order.
  - `CSharpEssentials.Errors/ErrorMetadata.cs:25`: a never-thrown exception falls back to `Environment.StackTrace`.
  - `CSharpEssentials.Errors/ErrorMetadata.cs:29` with `Error.cs:252`: for an exception with an inner exception, `Error.Exception(ex)` is not equal to itself, because the nested `ErrorMetadata` is compared by reference.
  - `CSharpEssentials.Errors/ErrorMetadata.cs:19`: an error with `CreateWithException` metadata throws `NotSupportedException` when System.Text.Json serializes it.
  - A JSON round trip of an error with metadata is not equal to the original, because the values come back as `JsonElement`.
- The CSE column is the working tree, which may not match the version published on NuGet.

---

## How to reproduce

```bash
cd benchmarks
./run-benchmarks.sh --project results-comparison                        # all TFMs (net8-net11), default job
./run-benchmarks.sh --project results-comparison --tfm net10.0          # one TFM
./run-benchmarks.sh --project results-comparison --quick                # net9.0, Short job, one filter per class
dotnet run -c Release -f net10.0 --project CSharpEssentials.Results.Comparison.Benchmarks -- --self-check
dotnet run -c Release -f net10.0 --project CSharpEssentials.Results.Comparison.Benchmarks -- --filter "*.ErrorFactoryComparison.*"
```

Short-job numbers cannot be compared with default-job numbers. Results are stored per framework under `benchmarks/results/results-comparison/{tfm}/results/`.
