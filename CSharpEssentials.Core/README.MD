# CSharpEssentials.Core

Foundational utilities and extensions for everyday .NET development. Zero internal dependencies.

## Features

- **String Conversions**: Pascal, Camel, Snake, Kebab, Macro, Train, Title, UnderscoreCamel.
- **Collection Helpers**: Conditional add and filter, null filtering, random selection, set comparison, `AllTrue` / `AllFalse`.
- **Range Checks**: `IsBetween` / `IsBetweenExclusive` for any `IComparable<T>`.
- **Functional Extensions**: Null-safe execution (`IfNotNull`, `IfNull`, `IfTrue`, `IfFalse`), `IsNull` / `IsNotNull`, string `IsEmpty` / `IsNotEmpty`.
- **Task Helpers**: `WithCancellation` (for `Task`, `Task<T>`, `ValueTask`, `ValueTask<T>`), `AsTask`, `AsValueTask`.
- **GUID Utilities**: URL-safe Base64 encoding, Version 7 GUID generation on .NET 9+.
- **Exception Helpers**: Flatten inner exceptions and messages.
- **HTTP Constants**: Common status codes as `int` constants (`HttpCodes`).
- **Transaction Abstraction**: `ITransactionRunner`, implemented by data-access packages.

## Installation

```bash
dotnet add package CSharpEssentials.Core
```

Targets `net11.0`, `net10.0`, `net9.0`, `netstandard2.1` and `netstandard2.0`.

## Usage

### String Case Conversions

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

### Collection Extensions

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

### Range Checks

```csharp
5.IsBetween(1, 10);            // true  (inclusive)
10.IsBetweenExclusive(1, 10);  // false (exclusive)
orderDate.IsBetween(start, end);
```

### Functional Extensions

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

### Task Helpers

```csharp
await SomeTask().WithCancellation(cts.Token);

string value = "hello";
Task<string> task = value.AsTask();
ValueTask<string> vt = value.AsValueTask();
```

`WithCancellation` stops waiting and throws `OperationCanceledException` when the token is cancelled. It does not cancel the underlying task, and it returns the original task when the token cannot be cancelled.

### GUID Utilities

```csharp
Guid id = Guider.NewGuid();          // Version 7 on .NET 9+
string shortId = id.ToStringFromGuid(); // URL-safe Base64 (22 chars)
Guid parsed = shortId.ToGuidFromString();
```

`Guider.ToStringFromGuid` and `Guider.ToGuidFromString` are the static forms of the same two conversions. See [Guids](Guids/Readme.MD).

### Exception Helpers

```csharp
try { /* ... */ }
catch (Exception ex)
{
    var innerExceptions = ex.GetInnerExceptions();
    var messages = ex.GetInnerExceptionsMessages();
}
```

Both return lazy sequences that start with `ex` itself, followed by each inner exception.

### HTTP Status Codes

```csharp
int ok = HttpCodes.Ok;
int notFound = HttpCodes.NotFound;
int badRequest = HttpCodes.BadRequest;
```

### Other Helpers

```csharp
object boxed = 42;
int number = boxed.ExplicitCast<int>();

DateTime when = 1700000000000L.MsToDateTime();   // Unix milliseconds

HttpGroup group = HttpStatus.BadRequest.GetTypeGroup<HttpGroup, HttpStatus>();  // HttpGroup.ClientError

public enum HttpStatus { Ok = 200, BadRequest = 400 }
public enum HttpGroup { Success = 200, ClientError = 400 }
```

`GetTypeGroup` rounds the enum value down to a multiple of `group` (default 100) and converts it to the target enum.

On .NET 10 and later, `using CSharpEssentials;` also brings the C# 14 extension members `IEnumerable<T>.IsEmpty` (property, `!source.Any()`) and `string.IsPalindrome` (property).

### Transaction Runner Abstraction

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
