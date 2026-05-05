# CSharpEssentials.Json

Enhanced JSON serialization utilities on top of `System.Text.Json`.

## Features

- **Conditional String Enums** — Enums marked with `[StringEnum]` serialize as strings; others stay as integers.
- **Multi-Format DateTime** — Deserialize from many date formats automatically.
- **Polymorphic Serialization** — Serialize abstract types and interfaces with a `$type` discriminator.
- **Pre-Configured Options** — CamelCase, ignore cycles, ignore nulls, relaxed escaping.

## Installation

```bash
dotnet add package CSharpEssentials.Json
```

## Usage

### Pre-Configured Options

```csharp
using CSharpEssentials.Json;
using System.Text.Json;

var options = EnhancedJsonSerializerOptions.DefaultOptions;
string json = JsonSerializer.Serialize(myObject, options);
```

### Quick Extensions

```csharp
var user = new { Name = "Alice", Age = 30 };

string json = user.ConvertToJson();
var parsed = json.ConvertFromJson<User>();
JsonDocument doc = json.ConvertToJsonDocument();
```

### String Enum Serialization

```csharp
using CSharpEssentials.Enums;

[StringEnum]
public enum Status { Active, Inactive }

public enum Level { Low, High }

var data = new { S = Status.Active, L = Level.High };
string json = data.ConvertToJson(); // {"s":"active","l":1}
```

### Polymorphic Serialization

```csharp
public abstract class Animal { public string Name { get; set; } = string.Empty; }
public class Dog : Animal { public string Breed { get; set; } = string.Empty; }

Animal dog = new Dog { Name = "Rex", Breed = "Labrador" };
string json = dog.ConvertToJson(); // {"$type":"Dog", "name":"Rex", "breed":"Labrador"}

Animal? restored = json.ConvertFromJson<Animal>();
```

### Multi-Format DateTime

```csharp
// All formats work automatically
string[] dates = ["2024-03-14T15:30:45Z", "14.03.2024", "March 14, 2024"];
foreach (var d in dates)
{
    DateTime dt = d.ConvertFromJson<DateTime>();
}
```
