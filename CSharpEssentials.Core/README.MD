# CSharpEssentials.Core

Foundational utilities and extensions for everyday .NET development. Zero internal dependencies.

## Features

- **String Conversions** — Pascal, Camel, Snake, Kebab, Macro, Train, Title, UnderscoreCamel.
- **Collection Helpers** — Conditional add, null filtering, random selection, set comparison.
- **Functional Extensions** — Null-safe execution (`IfNotNull`, `IfNull`, `IfTrue`, `IfFalse`).
- **Task Helpers** — `WithCancellation`, `AsTask`, `AsValueTask`.
- **GUID Utilities** — URL-safe Base64 encoding, Version 7 GUID generation on .NET 9+.
- **Exception Helpers** — Flatten inner exceptions and messages.
- **HTTP Constants** — Common status codes as typed constants.

## Installation

```bash
dotnet add package CSharpEssentials.Core
```

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

list.ForEach(Console.WriteLine);

var filtered = list.WhereIf(true, x => x != "a");
var clean = new List<string?> { "a", null, "b" }.WithoutNulls();

var numbers = new[] { 1, 2, 3 };
numbers.GetRandomItem();
numbers.GetRandomItems(2);
```

### Functional Extensions

```csharp
object? data = GetData();

if (data.IsNull()) { /* ... */ }
if (data.IsNotNull()) { /* ... */ }

data.IfNotNull(d => Process(d));
data.IfNull(() => Initialize());

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

### GUID Utilities

```csharp
Guid id = Guider.NewGuid();          // Version 7 on .NET 9+
string shortId = id.ToStringFromGuid(); // URL-safe Base64 (22 chars)
Guid parsed = shortId.ToGuidFromString();
```

### Exception Helpers

```csharp
try { /* ... */ }
catch (Exception ex)
{
    var innerExceptions = ex.GetInnerExceptions();
    var messages = ex.GetInnerExceptionsMessages();
}
```

### HTTP Status Codes

```csharp
int ok = HttpCodes.Ok;
int notFound = HttpCodes.NotFound;
int badRequest = HttpCodes.BadRequest;
```
