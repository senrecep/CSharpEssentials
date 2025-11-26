# CSharpEssentials.Core

`CSharpEssentials.Core` is the foundational package of the **CSharpEssentials** ecosystem. It provides a rich set of extension methods, utilities, and constants designed to simplify common programming tasks in .NET applications. This library focuses on improving code readability, reducing boilerplate, and providing high-performance utilities for daily development.

## 🚀 Features

- **Advanced Collection Extensions**: Fluent methods for conditional additions, filtering, and null handling.
- **String Manipulations**: Comprehensive case conversion utilities (Pascal, Camel, Kebab, Snake, etc.).
- **Functional-Style Extensions**: `IfTrue`, `IfNull`, `IfNotNull` extensions to write cleaner, more expressive code.
- **Task Extensions**: Simplified `WithCancellation` support for `Task` and `ValueTask`.
- **Optimized GUID Utilities**: High-performance, URL-friendly Base64 string conversions for GUIDs and Version 7 GUID generation.
- **Randomization**: Extensions for retrieving random items from collections and arrays.
- **Exception Handling**: Utilities to flatten and retrieve inner exceptions.
- **Constants**: Standard HTTP status codes available as constants.

## 📦 Installation

```bash
dotnet add package CSharpEssentials.Core
```

## 🛠 Usage

### 1. String Extensions (Case Conversion)

Convert strings between various naming conventions easily.

```csharp
using CSharpEssentials.Core;

string text = "helloWorld";

string pascal = text.ToPascalCase();          // "HelloWorld"
string kebab = text.ToKebabCase();            // "hello-world"
string snake = text.ToSnakeCase();            // "hello_world"
string macro = text.ToMacroCase();            // "HELLO_WORLD"
string title = text.ToTitleCase();            // "Hello World"
string underscoreCamel = text.ToUnderscoreCamelCase(); // "_helloWorld"
```

### 2. Collection Extensions

Simplify list and enumerable operations.

```csharp
using CSharpEssentials.Core;

var list = new List<string> { "Apple", "Banana" };

// Conditional Add
list.IfAdd(condition: true, "Cherry");

// Conditional AddRange
list.IfAddRange(condition: true, "Date", "Elderberry");

// Fluent ForEach
list.ForEach(item => Console.WriteLine(item));

// Conditional Where (Clean LINQ)
var filtered = list.WhereIf(condition: true, item => item.StartsWith("A"));

// Remove Nulls safely
var nullableList = new List<string?> { "A", null, "B" };
var cleanList = nullableList.WithoutNulls(); // ["A", "B"]
```

### 3. Functional & Logical Extensions

Reduce `if` statements and improve flow control visibility.

```csharp
using CSharpEssentials.Core;

object? data = GetSomeData();

// Null Checks
if (data.IsNull()) { /* handle null */ }
if (data.IsNotNull()) { /* handle data */ }

// Functional 'If' Execution
data.IfNotNull(d => Process(d));
data.IfNull(() => InitializeDefaults());

data.IfNotNull(
    action: d => Log("Data exists"),
    elseAction: () => Log("Data is missing")
);

bool isValid = true;
isValid.IfTrue(() => Console.WriteLine("Valid!"));
isValid.IfFalse(() => Console.WriteLine("Invalid!"));
```

### 4. Task & Async Extensions

Easily add cancellation support to tasks.

```csharp
using CSharpEssentials.Core;

CancellationTokenSource cts = new();

// Safe cancellation handling
await SomeLongRunningTask().WithCancellation(cts.Token);
```

### 5. GUID Utilities

Work with GUIDs more efficiently, especially for APIs and URLs.

```csharp
using CSharpEssentials.Core;

// Generate New Version 7 GUID (Time-ordered)
Guid newId = Guider.NewGuid();

// Convert to URL-safe Base64 string (Shorter than standard Guid string)
string urlFriendlyId = newId.ToStringFromGuid(); 

// Convert back from URL-safe string
Guid parsedId = urlFriendlyId.ToGuidFromString();
```

### 6. Random Extensions

Pick random items from collections or arrays efficiently.

```csharp
using CSharpEssentials.Core;

var numbers = new [] { 1, 2, 3, 4, 5 };

int randomItem = numbers.GetRandomItem();
int[] randomSubset = numbers.GetRandomItems(3);
```

### 7. Exception Extensions

Flatten nested exceptions to get to the root cause or collect all messages.

```csharp
using CSharpEssentials.Core;

try 
{
    // ... code that throws nested exceptions
}
catch (Exception ex)
{
    // Get all inner exceptions
    IEnumerable<Exception> allExceptions = ex.GetInnerExceptions();

    // Get all messages
    IEnumerable<string?> messages = ex.GetInnerExceptionsMessages();
}
```

## 🧩 Dependencies

`CSharpEssentials.Core` has **zero dependencies** on other packages, making it extremely lightweight and safe to include in any project.
