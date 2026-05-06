---
name: csharpessentials-json
description: Use when configuring System.Text.Json for ASP.NET Core — JsonOptions.Default with camelCase/no-nulls/no-cycles, ConditionalStringEnumConverter for [StringEnum] enums, MultiFormatDateTimeConverter for flexible date parsing, and PolymorphicJsonConverterFactory for $type discriminator.
---

# CSharpEssentials.Json

Pre-configured `System.Text.Json` options and converters for common ASP.NET Core patterns.

## Installation

```bash
dotnet add package CSharpEssentials.Json
```

## Namespace

```csharp
using CSharpEssentials.Json;
```

---

## JsonOptions.Default

Pre-configured profile: camelCase property names, ignore null values, handle circular references.

```csharp
// In ASP.NET Core
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.ApplyDefaults());

// Standalone serialization
var json = JsonSerializer.Serialize(obj, JsonOptions.Default);
var obj  = JsonSerializer.Deserialize<MyType>(json, JsonOptions.Default);
```

---

## ConditionalStringEnumConverter

Serializes enums marked with `[StringEnum]` (from `CSharpEssentials.Enums`) as strings, and all other enums as integers.

```csharp
// In setup
options.Converters.Add(new ConditionalStringEnumConverter());

// [StringEnum] enum → "Shipped" in JSON
// Regular enum     → 2 in JSON
```

---

## MultiFormatDateTimeConverter

Deserializes `DateTime` / `DateTimeOffset` from multiple input formats (ISO 8601, custom patterns). Useful when consuming third-party APIs with inconsistent date formats.

```csharp
options.Converters.Add(new MultiFormatDateTimeConverter());
```

---

## PolymorphicJsonConverterFactory

Enables polymorphic deserialization using a `$type` discriminator field.

```csharp
options.Converters.Add(new PolymorphicJsonConverterFactory());

// JSON: { "$type": "Circle", "radius": 5 }
// Deserializes to Circle : Shape
```

---

## Best Practices

- Call `ApplyDefaults()` in one place — do not configure `JsonSerializerOptions` in multiple locations
- `ConditionalStringEnumConverter` requires enums to be decorated with `[StringEnum]` from `CSharpEssentials.Enums`
- `PolymorphicJsonConverterFactory` requires the discriminator field to be named `$type`
