---
name: csharpessentials-json
description: Use when configuring System.Text.Json. Covers EnhancedJsonSerializerOptions presets (DefaultOptions, StrictOptions, ApplyTo), ConvertToJson/ConvertFromJson helpers, AddEnumConventions for [StringEnum] enums (obsolete ConditionalStringEnumConverter and StringEnumNaming), polymorphism with [JsonTypeDiscriminator] and AddPolymorphism (source-generated, AOT safe) or AddPolymorphismWithReflection, the $type PolymorphicJsonConverterFactory for bases without attributes, MultiFormatDateTimeConverterFactory, JsonElement.ToClrObject() and the CSE4001-CSE4012 analyzers.
---

# CSharpEssentials.Json

Pre-configured `System.Text.Json` options, converters and serialization helpers. Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1`; depends on `CSharpEssentials.Enums`.

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
| `DefaultOptions` | The above plus enum conventions (`AddEnumConventions(EnumConventions.Default)`, Data mode), `MultiFormatDateTimeConverterFactory`, `PolymorphicJsonConverterFactory` and `AddPolymorphismWithReflection()` |
| `DefaultOptionsWithDateTimeConverter` | `DefaultOptionsWithoutConverters` plus `MultiFormatDateTimeConverterFactory` |
| `StrictOptions` | Case-sensitive; rejects trailing commas, comments and unmapped members; `AddPolymorphismWithReflection()` |
| `CreateOptionsWithConverters(params JsonConverter[])` | `DefaultOptionsWithoutConverters` plus the given converters |
| `options.Create(configure)` | Copies the options and applies `configure` to the copy |
| `source.ApplyTo(target)` / `target.ApplyFrom(source)` | Copies settings and converters onto another instance. The target keeps its own `TypeInfoResolver` when it has one, and gets the source's polymorphism added to it |

The preset instances become read-only after first use; call `Create` for a modifiable copy. The class is `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]` because its presets find polymorphic derived types with reflection.

```csharp
// ASP.NET Core Minimal APIs
builder.Services.ConfigureHttpJsonOptions(o =>
    EnhancedJsonSerializerOptions.DefaultOptions.ApplyTo(o.SerializerOptions));

string json = JsonSerializer.Serialize(order, EnhancedJsonSerializerOptions.DefaultOptions);
```

---

## Serialization helpers

All helpers except `string.ConvertToJsonDocument()` use `DefaultOptions` when no options are passed. They use reflection-based serialization; in trimmed or AOT apps call `JsonSerializer` with a `JsonSerializerContext`.

```csharp
string json = order.ConvertToJson();
Order? back = json.ConvertFromJson<Order>();
object? untyped = json.ConvertFromJson(typeof(Order));
using JsonDocument document = order.ConvertToJsonDocument();

// JsonElement to plain CLR values (Dictionary<string, object?>, List<object?>, int/long/decimal/double, string, bool, null)
object? clr = document.RootElement.ToClrObject();
```

---

## Enum conventions

`AddEnumConventions` adds the enum converter for enums with generated metadata (`[StringEnum]` from `CSharpEssentials.Enums`). Other enums keep the converters already on the options, so a plain enum stays a number unless you add `JsonStringEnumConverter`. `DefaultOptions` already applies it in Data mode.

```csharp
using CSharpEssentials.Enums;

JsonSerializerOptions data = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    .AddEnumConventions(EnumConventions.Default);                       // Data: tolerant, [EnumFallback] member for unknown values

JsonSerializerOptions input = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    .AddEnumConventions(EnumConventions.Default, EnumReadMode.Input);   // Input: strict, throws EnumValueJsonException

// [StringEnum] OrderStatus.Shipped -> "shipped"
// Plain enum value                 -> 2
```

- Names are `[JsonStringEnumMemberName]` when present, otherwise `snake_case_lower`. `[Flags]` values are JSON arrays.
- `writeAs: EnumWireFormat.Number` writes numbers. An undefined value created in code is written as its number (`42`) in both formats; Input mode still rejects it on read.
- `AddEnumConventionsWithReflection` also converts enums without generated metadata and is not trim or AOT safe.
- `ConditionalStringEnumConverter` is obsolete: it forwards to the same converter in Input mode and has no `AllowUndefinedValues`.
- `StringEnumNaming` (`GetName`, `GetNames<T>`, `TryParse`) is obsolete since 5.0, a facade over `EnumMetadata`. Use `EnumMetadata`, `EnumValueFormatter` or the generated `ToWireName()` and `TryParseWire` helpers.

---

## Polymorphism

The base type picks the path:

- A base with `[JsonPolymorphic]` or `[JsonDerivedType]` uses System.Text.Json polymorphism. Derived types in any assembly join it through `[JsonTypeDiscriminator("value")]` (or an integer). A bundled source generator registers them per assembly, so `AddPolymorphism()` uses no reflection and is trim and AOT safe.
- A base with neither attribute uses `PolymorphicJsonConverterFactory`: a `$type` property with the CLR full name, found by scanning loaded assemblies (reflection, not AOT safe).

```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$event")]
public interface IEvent;

[JsonTypeDiscriminator("order.placed.v1")]
public sealed record OrderPlaced(Guid Id, decimal Total) : IEvent;

var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() }
    .AddPolymorphism();
// {"$event":"order.placed.v1","id":"...","total":10}

// Source-generated context: copy its options, list every derived type with [JsonSerializable], call AddPolymorphism last
var aot = new JsonSerializerOptions(AppJsonContext.Default.Options).AddPolymorphism();
```

- Resolution order: `[JsonDerivedType]` on the base, then generated `[JsonTypeDiscriminator]` registrations, then (only with `AddPolymorphismWithReflection` and a base without `[JsonDerivedType]`) an assembly scan that uses the CLR full name for types without a value.
- `AddPolymorphism` and `AddPolymorphismWithReflection` turn on `AllowOutOfOrderMetadataProperties`.
- `[JsonPolymorphicProperty("name")]` renames the discriminator for the values of one property.
- `[JsonAcceptClrTypeNames]` on a base keeps reading old `$type` CLR-name payloads (needs `AddPolymorphismWithReflection`; slower).
- Duplicate values under one base throw `InvalidOperationException` on first use; an unknown value on read throws `JsonException`.
- Analyzers CSE4001 to CSE4012 report duplicate values, unreachable types, derived types missing from a `JsonSerializerContext` (CSE4006, with a code fix) and similar mistakes.

---

## MultiFormatDateTimeConverterFactory

Reads `DateTime` and `DateTime?` from a fixed list of 35 formats (ISO 8601, `dd.MM.yyyy`, `MM/dd/yyyy`, `March 14, 2024`, RFC 1123 and more) and from numeric Unix-seconds strings. Constructor arguments add formats. Every value is returned as UTC (`Kind = Utc`); input without an offset is read as UTC. Slash dates try `dd/MM/yyyy` before `MM/dd/yyyy`. Writes use `yyyy-MM-ddTHH:mm:ss.ffffffzzz`, and an `Unspecified` value is written as UTC. ISO text with 1 to 7 fraction digits is read, including the `O` round-trip form System.Text.Json writes.

```csharp
var options = new JsonSerializerOptions();
options.Converters.Add(new MultiFormatDateTimeConverterFactory("dd.MM.yyyy HH-mm"));
```

---

## Best practices

- Configure options once and share them; prefer `DefaultOptions` over ad-hoc instances.
- Use Input mode for request bodies and Data mode for stored or received data.
- In AOT apps use `[JsonPolymorphic]` + `[JsonTypeDiscriminator]` with `AddPolymorphism` and a `JsonSerializerContext`, not the reflection presets.
