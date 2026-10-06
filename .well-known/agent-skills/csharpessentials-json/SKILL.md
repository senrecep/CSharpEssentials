---
name: csharpessentials-json
description: Use when configuring System.Text.Json. Covers EnhancedJsonSerializerOptions.DefaultOptions (camelCase, no nulls, cycle-safe), ConvertToJson/ConvertFromJson helpers, ConditionalStringEnumConverter and StringEnumNaming for [StringEnum] enums, MultiFormatDateTimeConverterFactory, PolymorphicJsonConverterFactory ($type discriminator) and JsonElement.ToClrObject().
---

# CSharpEssentials.Json

Pre-configured `System.Text.Json` options, converters and serialization helpers.

## Installation

```bash
dotnet add package CSharpEssentials.Json
```

## Namespace

```csharp
using System.Text.Json;
using CSharpEssentials.Json;
```

---

## EnhancedJsonSerializerOptions

| Member | Contents |
|---|---|
| `DefaultOptionsWithoutConverters` | Web defaults, camelCase, case-insensitive reads, nulls ignored on write, `ReferenceHandler.IgnoreCycles` |
| `DefaultOptions` | The above plus `ConditionalStringEnumConverter`, `MultiFormatDateTimeConverterFactory`, `PolymorphicJsonConverterFactory` |
| `DefaultOptionsWithDateTimeConverter` | The base options plus `MultiFormatDateTimeConverterFactory` |
| `StrictOptions` | Case-sensitive; rejects trailing commas, comments and unmapped members |
| `CreateOptionsWithConverters(params JsonConverter[])` | Base options plus the given converters |
| `options.Create(configure)` | Copies options and applies a configuration |
| `source.ApplyTo(target)` / `ApplyFrom` | Copies settings and converters onto another instance |

```csharp
// ASP.NET Core Minimal APIs
builder.Services.ConfigureHttpJsonOptions(o =>
    EnhancedJsonSerializerOptions.DefaultOptions.ApplyTo(o.SerializerOptions));

// Standalone serialization
string json = JsonSerializer.Serialize(order, EnhancedJsonSerializerOptions.DefaultOptions);
Order? copy = JsonSerializer.Deserialize<Order>(json, EnhancedJsonSerializerOptions.DefaultOptions);
```

---

## Serialization Helpers

All helpers use `DefaultOptions` when no options are passed.

```csharp
string json = order.ConvertToJson();
Order? back = json.ConvertFromJson<Order>();
object? untyped = json.ConvertFromJson(typeof(Order));
using JsonDocument document = order.ConvertToJsonDocument();

// 4.1: JsonElement to plain CLR values (Dictionary<string, object?>, List<object?>, int/long/decimal/double, string, bool, null)
object? clr = document.RootElement.ToClrObject();
```

---

## ConditionalStringEnumConverter

Serializes enums marked with `[StringEnum]` (from `CSharpEssentials.Enums`) as strings and all other enums as numbers. Names use `StringEnumNaming.DefaultPolicy` (`JsonNamingPolicy.SnakeCaseLower`); `[JsonStringEnumMemberName]` wins.

```csharp
var options = new JsonSerializerOptions();
options.Converters.Add(new ConditionalStringEnumConverter { AllowUndefinedValues = false });

// [StringEnum] OrderStatus.Shipped → "shipped"
// Plain enum value              → 2
```

`AllowUndefinedValues` (default `true`) controls whether undefined numeric values are accepted on read. The constructor also takes a naming policy, `allowIntegerValues` and a custom `canConvert` predicate.

## StringEnumNaming

Obsolete in 5.0 (a working facade over `EnumMetadata`): use `EnumMetadata`, `EnumValueFormatter` or the generated `ToWireName()` and `TryParseWire` helpers in new code.

The same names are used by JSON, EF Core storage, OpenAPI schemas and route/query binding.

```csharp
string name = StringEnumNaming.GetName(OrderStatus.Shipped);             // "shipped"
IReadOnlyList<string> names = StringEnumNaming.GetNames<OrderStatus>();
bool ok = StringEnumNaming.TryParse("shipped", out OrderStatus status);
```

---

## MultiFormatDateTimeConverterFactory

Reads `DateTime` and `DateTime?` from many input formats (ISO 8601 and common patterns). Extra formats can be passed to the constructor.

```csharp
// Accepts the built-in formats plus "dd.MM.yyyy"
var options = new JsonSerializerOptions();
options.Converters.Add(new MultiFormatDateTimeConverterFactory("dd.MM.yyyy"));
```

---

## PolymorphicJsonConverterFactory

Handles abstract classes and interfaces (collections excluded) with a `$type` discriminator that holds the concrete type's full name.

```csharp
// Abstract and interface types round-trip through "$type"
var options = new JsonSerializerOptions();
options.Converters.Add(new PolymorphicJsonConverterFactory());

// { "$type": "MyApp.Shapes.Circle", "radius": 5 } → Circle : Shape
```

---

## Best Practices

- Configure options once and share them; prefer `DefaultOptions` over ad-hoc instances
- `ConditionalStringEnumConverter` only converts enums marked `[StringEnum]` from `CSharpEssentials.Enums`
- Since 4.0, string enums are written in snake_case (`"shipped"`); EF Core storage follows the same naming
- `PolymorphicJsonConverterFactory` discovers types by reflection; it is not trim/AOT safe
