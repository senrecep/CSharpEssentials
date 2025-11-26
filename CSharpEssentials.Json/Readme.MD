# CSharpEssentials.Json

`CSharpEssentials.Json` provides advanced JSON serialization and deserialization utilities for .NET. It builds on top of `System.Text.Json` to offer enhanced features like polymorphic serialization, multi-format date handling, and smarter enum conversion.

## 🚀 Features

- **Conditional String Enums**: Automatically serialize enums as strings if they are marked with `[StringEnum]`.
- **Multi-Format Date Conversion**: Deserialize dates from a wide variety of formats (ISO 8601, RFC, etc.) seamlessly.
- **Polymorphic Serialization**: Built-in support for handling abstract classes and interfaces using a `$type` discriminator.
- **Enhanced Options**: Pre-configured `JsonSerializerOptions` for modern web standards (CamelCase, IgnoreCycles, etc.).
- **Fluent Extensions**: Methods like `ConvertToJson()` and `ConvertFromJson()` for quick object-string conversions.

## 📦 Installation

```bash
dotnet add package CSharpEssentials.Json
```

## 🛠 Usage

### 1. Enhanced Serialization Options

Use `EnhancedJsonSerializerOptions.DefaultOptions` to get a configured set of options that includes all the custom converters.

```csharp
using CSharpEssentials.Json;
using System.Text.Json;

var options = EnhancedJsonSerializerOptions.DefaultOptions;
string json = JsonSerializer.Serialize(myObject, options);
```

### 2. Polymorphic Serialization

Serialize and deserialize class hierarchies automatically. The converter adds a `$type` property to the JSON to identify the concrete type.

```csharp
using CSharpEssentials.Json;

public abstract class Animal { public string Name { get; set; } }
public class Dog : Animal { public string Bark { get; set; } }
public class Cat : Animal { public string Meow { get; set; } }

var animals = new List<Animal> { new Dog(), new Cat() };

// Serializes with "$type": "Namespace.Dog"
string json = animals.ConvertToJson(); 

// Deserializes back to specific Dog/Cat types
List<Animal>? result = json.ConvertFromJson<List<Animal>>();
```

### 3. Multi-Format Date Handling

The library can deserialize date strings in many different formats without manual configuration.

```csharp
using CSharpEssentials.Json;

// All these formats work automatically
string[] jsonDates = [
    "\"2024-03-14T15:30:45Z\"",
    "\"14.03.2024\"",
    "\"March 14, 2024\""
];

foreach(var json in jsonDates)
{
    DateTime dt = json.ConvertFromJson<DateTime>();
}
```

### 4. String Enum Support

Mark your enums with `[StringEnum]` (from `CSharpEssentials.Enums`) to serialize them as strings. Unmarked enums remain as integers (or default behavior).

```csharp
using CSharpEssentials.Enums;
using CSharpEssentials.Json;

[StringEnum]
public enum Status { Active, Inactive }

public enum Level { Low, High }

var data = new { S = Status.Active, L = Level.High };

// Result: {"s": "Active", "l": 1}
string json = data.ConvertToJson(); 
```

### 5. Quick Extensions

Simplify your code with direct conversion methods.

```csharp
using CSharpEssentials.Json;

var user = new { Name = "Alice", Age = 30 };

// To JSON
string json = user.ConvertToJson();

// From JSON
var parsed = json.ConvertFromJson<dynamic>();

// To JsonDocument
JsonDocument doc = json.ConvertToJsonDocument();
```
