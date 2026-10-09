---
name: csharpessentials-json
description: Use when configuring System.Text.Json. Covers EnhancedJsonSerializerOptions.DefaultOptions (camelCase, no nulls, cycle-safe), ConvertToJson/ConvertFromJson helpers, AddEnumConventions for [StringEnum] enums (obsolete ConditionalStringEnumConverter and StringEnumNaming), MultiFormatDateTimeConverterFactory, PolymorphicJsonConverterFactory ($type discriminator) and JsonElement.ToClrObject().
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
| `DefaultOptionsWithoutConverters` | Web defaults, camelCase, case-insensitive reads, nulls ignored on write, `ReferenceHandler.IgnoreCycles`, relaxed escaping |
| `DefaultOptions` | The above plus enum conventions (`AddEnumConventions(EnumConventions.Default)`, Data mode), `MultiFormatDateTimeConverterFactory`, `PolymorphicJsonConverterFactory` |
| `DefaultOptionsWithDateTimeConverter` | `DefaultOptionsWithoutConverters` plus `MultiFormatDateTimeConverterFactory` |
| `StrictOptions` | Case-sensitive; rejects trailing commas, comments and unmapped members |
| `CreateOptionsWithConverters(params JsonConverter[])` | `DefaultOptionsWithoutConverters` plus the given converters |
| `options.Create(configure)` | Copies options and applies a configuration |
| `source.ApplyTo(target)` / `target.ApplyFrom(source)` | Copies settings and converters onto another instance; the target keeps its own `TypeInfoResolver` when the source has none |

The preset instances become read-only after first use; call `Create` for a modifiable copy. The class is `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]` because `DefaultOptions` includes the reflection-based polymorphic converter.

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

All helpers except `string.ConvertToJsonDocument()` use `DefaultOptions` when no options are passed.

```csharp
string json = order.ConvertToJson();
Order? back = json.ConvertFromJson<Order>();
object? untyped = json.ConvertFromJson(typeof(Order));
using JsonDocument document = order.ConvertToJsonDocument();

// JsonElement to plain CLR values (Dictionary<string, object?>, List<object?>, int/long/decimal/double, string, bool, null)
object? clr = document.RootElement.ToClrObject();
```

---

## Enum Conventions

`AddEnumConventions` adds the enum converter for enums marked `[StringEnum]` (from `CSharpEssentials.Enums`); other enums keep the converters already on the options, so a plain enum stays a number unless you add `JsonStringEnumConverter` yourself. `DefaultOptions` already applies it in Data mode.

```csharp
using CSharpEssentials.Enums;

JsonSerializerOptions data = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    .AddEnumConventions(EnumConventions.Default);                       // Data: tolerant, [EnumFallback] member for unknown values

JsonSerializerOptions input = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    .AddEnumConventions(EnumConventions.Default, EnumReadMode.Input);   // Input: strict, throws EnumValueJsonException

// [StringEnum] OrderStatus.Shipped -> "shipped"
// Plain enum value                 -> 2
```

Names are `[JsonStringEnumMemberName]` when present, otherwise `snake_case_lower`. `AddEnumConventions` takes an optional `writeAs: EnumWireFormat.Number`. `AddEnumConventionsWithReflection` also converts enums without generated metadata and is not trim or AOT safe. `ConditionalStringEnumConverter` is obsolete: it forwards to the same converter in `Input` mode and no longer has `AllowUndefinedValues`.

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

Reads `DateTime` and `DateTime?` from many input formats (ISO 8601 and common patterns) and from numeric Unix-seconds strings. Extra formats can be passed to the constructor. Values without an offset are read as UTC and returned as local time (`Kind = Local`), so date-only input shifts to the previous evening in time zones behind UTC; the converter writes `yyyy-MM-ddTHH:mm:ss.ffffffzzz`.

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

// {"$type":"MyApp.Shapes.Circle","Radius":5} → Circle : Shape (property names follow the options' naming policy)
```

`$type` is written first and is required on read; a missing or unknown value throws `JsonException`.

---

## Best Practices

- Configure options once and share them; prefer `DefaultOptions` over ad-hoc instances
- `AddEnumConventions` only converts enums marked `[StringEnum]` from `CSharpEssentials.Enums`; use `Input` mode for request bodies
- Since 4.0, string enums are written in snake_case (`"shipped"`); EF Core storage follows the same naming
- `PolymorphicJsonConverterFactory` discovers types by reflection; it is not trim/AOT safe
