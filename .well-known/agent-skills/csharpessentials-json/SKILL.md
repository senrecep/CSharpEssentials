---
name: csharpessentials-json
description: Use when configuring System.Text.Json with CSharpEssentials.Json. EnhancedJsonSerializerOptions presets (DefaultOptions, StrictOptions, Create, ApplyTo), ConvertToJson/ConvertFromJson, AddEnumConventions for [StringEnum] enums, polymorphism with [JsonPolymorphic] + [JsonTypeDiscriminator] and AddPolymorphism (AOT safe) or AddPolymorphismWithReflection, [JsonPolymorphicProperty], RequireValueTypeConstructorParameters, MultiFormatDateTimeConverterFactory, JsonElement.ToClrObject and analyzers CSE4001-CSE4012.
---

# CSharpEssentials.Json

```bash
dotnet add package CSharpEssentials.Json
```

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;            // [JsonPolymorphic], JsonSerializerContext
using System.Text.Json.Serialization.Metadata;   // DefaultJsonTypeInfoResolver
using CSharpEssentials.Json;                     // presets, extensions, [JsonTypeDiscriminator]
using CSharpEssentials.Enums;                    // [StringEnum], EnumConventions, EnumReadMode
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Json
using System.Text.Json;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;

var item = new Item("Pen", ItemState.OutOfStock, null);
string json = item.ConvertToJson();               // DefaultOptions
Console.WriteLine(json);                          // {"name":"Pen","state":"out_of_stock"}

Item? back = json.ConvertFromJson<Item>();        // nullable result
Console.WriteLine(back!.State);                   // OutOfStock

string pretty = JsonSerializer.Serialize(item, EnhancedJsonSerializerOptions.DefaultOptions.Create(o => o.WriteIndented = true));
Console.WriteLine(pretty.Contains('\n'));         // True

[StringEnum]
public enum ItemState { InStock, OutOfStock }

public sealed record Item(string Name, ItemState State, string? Note);
```

## Choosing options

| Need | Use |
|---|---|
| General app JSON | `EnhancedJsonSerializerOptions.DefaultOptions` |
| Reject unknown properties, comments, trailing commas, missing value-type ctor params | `EnhancedJsonSerializerOptions.StrictOptions` |
| A modified copy | `preset.Create(o => ...)` (presets are read-only after first use) |
| Copy settings into ASP.NET Core options | `EnhancedJsonSerializerOptions.DefaultOptions.ApplyTo(o.SerializerOptions)` |
| Trimmed or Native AOT app | Your own options over a `JsonSerializerContext`, then `AddPolymorphism()` |

```csharp
builder.Services.ConfigureHttpJsonOptions(o => EnhancedJsonSerializerOptions.DefaultOptions.ApplyTo(o.SerializerOptions));
```

## Serializing [StringEnum] enums

```csharp Program.cs
// dotnet add package CSharpEssentials.Json
using System.Text.Json;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;

JsonSerializerOptions data = new JsonSerializerOptions(JsonSerializerDefaults.Web).AddEnumConventions(EnumConventions.Default);
JsonSerializerOptions input = new JsonSerializerOptions(JsonSerializerDefaults.Web).AddEnumConventions(EnumConventions.Default, EnumReadMode.Input);

Console.WriteLine(JsonSerializer.Serialize(Color.DarkRed, data));       // "dark_red"
Console.WriteLine(JsonSerializer.Deserialize<Color>("\"purple\"", data)); // Unknown (fallback, Data mode)

try
{
    JsonSerializer.Deserialize<Color>("\"purple\"", input);
}
catch (EnumValueJsonException ex)
{
    Console.WriteLine(string.Join(",", ex.Error.AllowedValues));        // dark_red,blue
}

[StringEnum]
public enum Color { DarkRed, Blue, [EnumFallback] Unknown }
```

- Use `EnumReadMode.Input` for request bodies and `EnumReadMode.Data` (default) for stored or trusted data.
- Only enums with generated metadata (`[StringEnum]`) are handled. A plain enum stays a number.
- `writeAs: EnumWireFormat.Number` writes numbers. `[Flags]` enums are JSON arrays.

## Adding derived types from any assembly

```csharp Program.cs
// dotnet add package CSharpEssentials.Json
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using CSharpEssentials.Json;

JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    TypeInfoResolver = new DefaultJsonTypeInfoResolver(),   // required before AddPolymorphism
}.AddPolymorphism();

string json = JsonSerializer.Serialize<INotification>(new EmailSent("a@b.c"), options);
Console.WriteLine(json);                                     // {"kind":"email.sent.v1","to":"a@b.c"}
INotification? back = JsonSerializer.Deserialize<INotification>("""{"to":"x","kind":"sms.sent.v1"}""", options);
Console.WriteLine(back);                                     // SmsSent { To = x }

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
public interface INotification;

[JsonTypeDiscriminator("email.sent.v1")]
public sealed record EmailSent(string To) : INotification;

[JsonTypeDiscriminator("sms.sent.v1")]
public sealed record SmsSent(string To) : INotification;
```

For Native AOT, start from a context, list every derived type and call `AddPolymorphism` last:

```csharp Program.cs
// dotnet add package CSharpEssentials.Json
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using CSharpEssentials.Json;

JsonSerializerOptions options = new JsonSerializerOptions(AppJsonContext.Default.Options).AddPolymorphism();
var typeInfo = (JsonTypeInfo<INotification>)options.GetTypeInfo(typeof(INotification));

Console.WriteLine(JsonSerializer.Serialize<INotification>(new EmailSent("a@b.c"), typeInfo)); // {"kind":"email.sent.v1","to":"a@b.c"}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
public interface INotification;

[JsonTypeDiscriminator("email.sent.v1")]
public sealed record EmailSent(string To) : INotification;

[JsonTypeDiscriminator("sms.sent.v1")]
public sealed record SmsSent(string To) : INotification;

[JsonSerializable(typeof(INotification))]
[JsonSerializable(typeof(EmailSent))]
[JsonSerializable(typeof(SmsSent))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, AllowOutOfOrderMetadataProperties = true)]
public partial class AppJsonContext : JsonSerializerContext;
```

A derived type missing from the context is left out and writing it throws `NotSupportedException` (CSE4006 reports it).

- `[JsonPolymorphicProperty("name")]` on a property or field renames the discriminator for the values of that member only.
- `[JsonAcceptClrTypeNames]` on a base keeps reading old `$type` payloads that hold CLR full names (needs `AddPolymorphismWithReflection`, which the presets include).
- An abstract base with no `[JsonPolymorphic]`/`[JsonDerivedType]` uses `PolymorphicJsonConverterFactory` from `DefaultOptions`: `$type` with the CLR full name, reflection only.

## Requiring value-type constructor parameters

```csharp
JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
}.RequireValueTypeConstructorParameters();    // missing int/enum/struct ctor param -> JsonException
```

## Reading dates and untyped JSON

```csharp
var options = new JsonSerializerOptions();
options.Converters.Add(new MultiFormatDateTimeConverterFactory("dd.MM.yyyy HH-mm")); // extra formats on top of 35 built-in

using JsonDocument document = JsonDocument.Parse("""{"id":1,"tags":["a"]}""");
object? plain = document.RootElement.ToClrObject();  // Dictionary<string, object?>, List<object?>, int/long/decimal/double, string, bool, null
```

`MultiFormatDateTimeConverterFactory` returns `Kind = Utc` for every value and reads `dd/MM/yyyy` before `MM/dd/yyyy`.

## Rules

- `AddPolymorphism()` throws `InvalidOperationException` when `TypeInfoResolver` is null. Set a `DefaultJsonTypeInfoResolver` or a context first, and call `AddPolymorphism` last. Call `RequireValueTypeConstructorParameters` before it.
- The attribute is `[JsonTypeDiscriminator("value")]` on the derived type, plus `[JsonPolymorphic]` on the base. Without `[JsonPolymorphic]` on the base it has no effect (CSE4008).
- Use stable, versioned values (`"order.placed.v1"`), not CLR names.
- `ConvertToJson`, `ConvertFromJson` and the presets use reflection (`[RequiresUnreferencedCode]`); do not use them in Native AOT code.
- Modify presets only through `Create`; mutating `DefaultOptions` after first use throws.
- Catch `EnumValueJsonException` (or `JsonException`) for bad enum input; read `ex.Error.AllowedValues` for the allowed names.
- `ConditionalStringEnumConverter` and `StringEnumNaming` are obsolete. Use `AddEnumConventions`, `ToWireName()` and `TryParseWire`.

## Diagnostics

| Id | Severity | Meaning |
|---|---|---|
| CSE4001 | Error | Two derived types share a discriminator value under one base |
| CSE4002 | Warning | `[JsonTypeDiscriminator]` ignored: the base lists the type in `[JsonDerivedType]` |
| CSE4003 | Error | `[JsonTypeDiscriminator]` on an abstract type, interface or generic definition |
| CSE4004 | Warning | Discriminator name equals a member's JSON name |
| CSE4005 | Warning | Derived type of an open base has no `[JsonTypeDiscriminator]` |
| CSE4006 | Warning | `JsonSerializerContext` lists the base but not a derived type (code fix) |
| CSE4007 | Warning | Derived type has its own `[JsonConverter]` and is left out |
| CSE4008 | Warning | `[JsonTypeDiscriminator]` on a type with no polymorphic base |
| CSE4009 | Warning | `[JsonPolymorphicProperty]` on a member with no polymorphic base |
| CSE4010 | Warning | Type is private, protected, file-local or generic; generated code cannot register it |
| CSE4011 | Info | Derived type of a closed base is neither listed nor valued; writing it throws |
| CSE4012 | Warning | `[property: JsonPolymorphicProperty]` on a positional record parameter is not seen |
