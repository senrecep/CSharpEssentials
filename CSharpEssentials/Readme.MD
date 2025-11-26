# CSharpEssentials

`CSharpEssentials` is the core meta-package that brings together the entire CSharpEssentials ecosystem. It provides a unified entry point to the library's most popular features and includes exclusive extension methods that bridge different modules (e.g., connecting `Maybe` with `Result`, or `Json` with `Result`).

## 🚀 Features

- **Unified Access**: references key packages like `Core`, `Errors`, `Result`, `Maybe`, and `Json`.
- **Bridge Extensions**: exclusive extensions that make different modules work together seamlessly.
  - Convert `Maybe<T>` to `Result<T>`.
  - Safe JSON property navigation returning `Result<JsonElement>`.
  - String manipulation returning `Result<string>`.

## 📦 Installation

```bash
dotnet add package CSharpEssentials
```

## 🛠 Usage

### 1. Maybe to Result Conversion

Seamlessly transition from optional values to success/failure results.

```csharp
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;

Maybe<User> maybeUser = GetUser();

// If Maybe has value -> Result.Success(value)
// If Maybe is None   -> Result.Failure(Error.NotFound(...))
Result<User> result = maybeUser.ToMaybeResult(Error.NotFound("User.Missing"));
```

### 2. Safe JSON Navigation

Avoid exceptions when traversing complex JSON structures.

```csharp
using CSharpEssentials.Json;
using System.Text.Json;

JsonDocument doc = JsonDocument.Parse(jsonString);

// Returns Result<JsonElement?>
var element = doc.TryGetNestedProperty("users", "0", "address", "city");

if (element.IsSuccess)
{
    Console.WriteLine(element.Value?.GetString());
}
```

### 3. String Validations

Perform string operations that might fail (like trimming when empty) and get a Result back.

```csharp
using CSharpEssentials.Core;

Result<string> clean = input.TrimStart("prefix_");
```
