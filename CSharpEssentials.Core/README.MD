# CSharpEssentials.Core

Utilities and extensions for everyday .NET development, with no internal dependencies.

> Benchmarks against the BCL, Humanizer.Core 3.0.10 and CSharpVitamins.ShortGuid 2.0.0 (case conversion, GUID text, random items and WithCancellation) are in
> [BENCHMARKS.md](./BENCHMARKS.md). That page is a draft: results are pending the full run.

## Features

- String case conversions: Pascal, Camel, Snake, Kebab, Macro, Train, Title, UnderscoreCamel.
- Collection helpers for conditional add and filter, null filtering, random selection, set comparison and `AllTrue` / `AllFalse`.
- `IsBetween` / `IsBetweenExclusive` range checks for any `IComparable<T>`.
- Null-safe execution (`IfNotNull`, `IfNull`, `IfTrue`, `IfFalse`), `IsNull` / `IsNotNull`, and string `IsEmpty` / `IsNotEmpty`.
- `WithCancellation` (for `Task`, `Task<T>`, `ValueTask`, `ValueTask<T>`), `AsTask` and `AsValueTask`.
- URL-safe Base64 encoding of GUIDs, and version 7 and strictly increasing version 7 GUIDs on every target.
- Helpers that flatten inner exceptions and their messages.
- Common HTTP status codes as `int` constants (`HttpCodes`).
- `ITransactionRunner`, a transaction abstraction that data-access packages implement.

## Installation

```bash
dotnet add package CSharpEssentials.Core
```

Targets `net11.0`, `net10.0`, `net9.0`, `netstandard2.1` and `netstandard2.0`.

## Usage

### String case conversions

```csharp
using CSharpEssentials.Core;

string text = "helloWorld";

text.ToPascalCase();          // "HelloWorld"
text.ToCamelCase();           // "helloWorld"
text.ToSnakeCase();           // "hello_world"
text.ToKebabCase();           // "hello-world"
text.ToMacroCase();           // "HELLO_WORLD"
text.ToTrainCase();           // "Hello-World"
text.ToTitleCase();           // "Hello World"
text.ToUnderscoreCamelCase(); // "_helloWorld"
```

The converters keep letters of every script, combining marks and decimal digits (Unicode categories `Lu`, `Ll`, `Lt`, `Lm`, `Lo`, `Mn`, `Mc` and `Nd`); any other character starts a new word and is dropped. A word starts at an uppercase or titlecase letter, at a digit, and after a dropped character. Letters without case, such as CJK, Arabic, Hebrew or Thai, are kept as they are and continue the current word, so `"用户名".ToSnakeCase()` is `"用户名"` and `"user用户Name".ToSnakeCase()` is `"user用户_name"`. Titlecase letters such as `ǅ` are lowercased like uppercase letters. Letters outside the Basic Multilingual Plane (surrogate pairs) are still dropped. EF Core `LegacySnakeCase` storage and the Enums generator's `SnakeCase` and `KebabCase` output use the same rule.

### Collection extensions

```csharp
var list = new List<string> { "a", "b" };

list.IfAdd(true, "c");
list.IfAddRange(true, ["d", "e"]);

IEnumerable<string> source = list;
source.ForEach(Console.WriteLine).ToList();

var filtered = list.WhereIf(true, x => x != "a");
var clean = new List<string?> { "a", null, "b" }.WithoutNulls();

var numbers = new[] { 1, 2, 3 };
numbers.GetRandomItem();
numbers.GetRandomItems(2);

new List<string> { "a", "b" }.HasSameElements(["b", "a"]);  // true (order and duplicates ignored)
new[] { true, true }.AllTrue();                                // true
new[] { false, false }.AllFalse();                             // true
```

`ForEach` on `IEnumerable<T>` is lazy: it runs the action for each item while the result is enumerated, so nothing happens until you iterate it (for example with `ToList()`). On a `List<T>` the built-in `List<T>.ForEach` is chosen instead. `GetRandomItem` and `GetRandomItems` use `RandomNumberGenerator`, pick unique items and work on `T[]`, `List<T>` and `Span<T>`.

### Range checks

```csharp
5.IsBetween(1, 10);            // true  (inclusive)
10.IsBetweenExclusive(1, 10);  // false (exclusive)
orderDate.IsBetween(start, end);
```

### Functional extensions

```csharp
object? data = GetData();

if (data.IsNull()) { /* ... */ }
if (data.IsNotNull()) { /* ... */ }

data.IfNotNull(d => Process(d));
data.IfNull(() => Initialize());

string? name = null;
name.IsEmpty();      // true for null, "" and whitespace
name.IsNotEmpty();   // false
```

Each `IfNotNull` / `IfNull` overload returns a `bool`: whether the value was not null (`IfNotNull`) or null (`IfNull`). `IfNotNull` and `IfNull` also take an else action:

```csharp
data.IfNotNull(d => Process(d), () => Initialize());
data.IfNull(() => Initialize(), d => Process(d));

true.IfTrue(() => Console.WriteLine("yes"));
false.IfFalse(() => Console.WriteLine("no"));
```

### Task helpers

```csharp
await SomeTask().WithCancellation(cts.Token);

string value = "hello";
Task<string> task = value.AsTask();
ValueTask<string> vt = value.AsValueTask();
```

`WithCancellation` stops waiting and throws `OperationCanceledException` when the token is cancelled. On .NET 9 and later it is `Task.WaitAsync`, so the exception is the `TaskCanceledException` subclass and carries the token. It does not cancel the underlying task, and it returns the original task when the token cannot be cancelled.

### GUID utilities

```csharp
Guid id = Guider.NewGuid();          // Version 7 on every target
Guid ordered = Guider.NewMonotonicGuid(); // Version 7, strictly increasing in this process
Guid sqlKey = Guider.NewSqlServerGuid();  // Version 8, ordered for SQL Server uniqueidentifier
string shortId = id.ToStringFromGuid(); // URL-safe, 22 chars
Guid parsed = shortId.ToGuidFromString();
```

The string is RFC 4648 base64url without padding: standard Base64 with `+` replaced by `-` and `/` replaced by `_`. Any base64url codec, such as `System.Buffers.Text.Base64Url` on .NET 9+, decodes it to the same GUID, and `ToGuidFromString` accepts strings that codec produces.

`Guider.ToStringFromGuid` and `Guider.ToGuidFromString` are the static forms of the same two conversions. See [Guids](Guids/Readme.MD).

### Exception helpers

```csharp
try { /* ... */ }
catch (Exception ex)
{
    var innerExceptions = ex.GetInnerExceptions();
    var messages = ex.GetInnerExceptionsMessages();
}
```

Both return lazy sequences that start with `ex` itself, followed by each inner exception.

### HTTP status codes

```csharp
int ok = HttpCodes.Ok;
int notFound = HttpCodes.NotFound;
int badRequest = HttpCodes.BadRequest;
```

### Other helpers

```csharp
object boxed = 42;
int number = boxed.ExplicitCast<int>();

DateTime when = 1700000000000L.MsToDateTime();   // Unix milliseconds, Kind is Utc

HttpGroup group = HttpStatus.BadRequest.GetTypeGroup<HttpGroup, HttpStatus>();  // HttpGroup.ClientError

public enum HttpStatus { Ok = 200, BadRequest = 400 }
public enum HttpGroup { Success = 200, ClientError = 400 }
```

`GetTypeGroup` rounds the enum value down to a multiple of `group` (default 100) and converts it to the target enum.

On .NET 10 and later, `using CSharpEssentials;` also brings the C# 14 extension members `IEnumerable<T>.IsEmpty` (property, `!source.Any()`) and `string.IsPalindrome` (property).

### Transaction runner abstraction

`ITransactionRunner` (namespace `CSharpEssentials.Transactions`) runs a unit of work in a transaction and commits only when `shouldCommit` returns `true`. It has no dependencies, so data-access packages implement it (`EfCoreTransactionRunner<TDbContext>` in `CSharpEssentials.EntityFrameworkCore`) and pipelines consume it (`TransactionBehavior` in `CSharpEssentials.Mediator`).

```csharp
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Transactions;

Result result = await transactionRunner.ExecuteAsync(
    ct => handler.Handle(command, ct),
    outcome => outcome.IsSuccess,
    cancellationToken);
```

Here `handler.Handle` returns `ValueTask<Result>`; `Result` comes from [CSharpEssentials.Results](../CSharpEssentials.Results/Readme.MD).

## Upgrading to 8.0

- `GetRandomItems` throws `ArgumentOutOfRangeException` (parameter `count`) for a negative `count`. It threw `OverflowException` before; update any `catch` that relied on that.
- `MsToDateTime` returns a `DateTime` with `Kind = Utc`. It returned `Kind = Unspecified` before, with the same ticks. `ToUniversalTime()` no longer shifts it by the local offset, System.Text.Json now writes a trailing `Z`, and Npgsql maps it to `timestamp with time zone`. If you stored or compared the old value, call `DateTime.SpecifyKind(value, DateTimeKind.Unspecified)` to keep the old behaviour.
- `ToSnakeCase`, `ToKebabCase` and the other case converters keep letters without case (CJK, Arabic, Hebrew, Thai), titlecase letters and combining marks. 7.x dropped them, so `"用户名".ToSnakeCase()` was `""` and `"user用户Name"` was `"user_name"`; they are now `"用户名"` and `"user用户_name"`. Output for names written only in Latin letters and digits does not change. If you stored converted values of non-Latin names, for example through EF Core `LegacySnakeCase` enum storage, rewrite them with the new output.
- `ToStringFromGuid` and `ToGuidFromString` use the RFC 4648 base64url alphabet (`+` as `-`, `/` as `_`). 7.x used the reverse mapping, so a short ID stored by 7.x that contains `-` or `_` now decodes to a different GUID. Convert stored IDs once by swapping every `-` with `_` and every `_` with `-`, for example `id.Replace('-', '*').Replace('_', '-').Replace('*', '_')`.
- `Guider.NewGuid()` on the `netstandard2.0` and `netstandard2.1` assets, and so on .NET 8 and .NET Framework, returns RFC 9562 version 7 GUIDs instead of version 4. The first 6 bytes hold the creation time. Use `Guid.NewGuid()` where an ID must not reveal when it was created. On every target the random bits now come from a per-thread pool; apps that run in a cloned virtual machine or a checkpointed and restored process should read [Cloned processes](Guids/Readme.MD#cloned-processes).
- `WithCancellation` on the .NET 9, 10 and 11 targets throws `TaskCanceledException`, a subclass of `OperationCanceledException`, when the token is cancelled. 7.x threw `OperationCanceledException` itself. A `catch (OperationCanceledException)` still catches it, but an exact type check such as `ex.GetType() == typeof(OperationCanceledException)` or xUnit `Assert.ThrowsAsync<OperationCanceledException>` no longer matches. The `netstandard2.0` and `netstandard2.1` assets still throw `OperationCanceledException`.
