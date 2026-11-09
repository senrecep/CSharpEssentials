# CSharpEssentials.Json

JSON serialization helpers on top of `System.Text.Json`.

> Benchmarks against System.Text.Json built-ins and Macross.Json.Extensions 3.0.0 (enums, flags, polymorphism and DateTime reads) are in
> [BENCHMARKS.md](./BENCHMARKS.md). CSE is slower than System.Text.Json in every timed category except the invalid-input rows
> and the net8.0 enum Write and Read rows (polymorphic serialize takes 402.6 ns against 276.3 ns on net10.0), and faster than Macross on enum writes.

## Features

- Enum conventions: `[StringEnum]` enums read and write their wire names (`AddEnumConventions`); other enums keep the host's behavior.
- Multi-format DateTime: reads `DateTime` and `DateTime?` from many date formats automatically.
- Polymorphism: derived types from any assembly join a `[JsonPolymorphic]` base through `[JsonTypeDiscriminator]`, with a source generator for native AOT; bases without attributes keep a `$type` discriminator.
- Pre-configured options: camelCase, ignore cycles, ignore nulls, relaxed escaping, plus a strict preset.
- JsonElement helpers: `ToClrObject` turns a `JsonElement` into plain CLR values.

## Installation

```bash
dotnet add package CSharpEssentials.Json
```

Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1`. It depends on `CSharpEssentials.Enums`.

## Usage

### Pre-configured options

```csharp
using CSharpEssentials.Json;
using System.Text.Json;

var options = EnhancedJsonSerializerOptions.DefaultOptions;
string json = JsonSerializer.Serialize(myObject, options);
```

| Member | Contents |
|---|---|
| `DefaultOptionsWithoutConverters` | Web defaults, camelCase, case-insensitive reads, nulls ignored on write, `ReferenceHandler.IgnoreCycles`, relaxed escaping |
| `DefaultOptions` | The above plus enum conventions (Data mode), `MultiFormatDateTimeConverterFactory`, `PolymorphicJsonConverterFactory` and `AddPolymorphismWithReflection()` |
| `DefaultOptionsWithDateTimeConverter` | `DefaultOptionsWithoutConverters` plus `MultiFormatDateTimeConverterFactory` |
| `StrictOptions` | Case-sensitive; rejects trailing commas, comments and unmapped members; `AddPolymorphismWithReflection()` |
| `CreateOptionsWithConverters(params JsonConverter[])` | `DefaultOptionsWithoutConverters` plus the given converters |
| `options.Create(configure)` | Copies the options and applies `configure` to the copy |
| `source.ApplyTo(target)` / `target.ApplyFrom(source)` | Copies settings and converters onto another instance. The target keeps its own `TypeInfoResolver` when it has one; if the source used `AddPolymorphism` or `AddPolymorphismWithReflection`, the target's resolver gets the same polymorphism added |

The preset instances are shared and become read-only after first use, so call `Create` to get a modifiable copy. `EnhancedJsonSerializerOptions` is annotated `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]` because its presets find polymorphic derived types with reflection.

```csharp
JsonSerializerOptions indented = EnhancedJsonSerializerOptions.DefaultOptions.Create(o => o.WriteIndented = true);

var target = new JsonSerializerOptions();
EnhancedJsonSerializerOptions.DefaultOptions.ApplyTo(target);
```

### Quick extensions

`ConvertToJson`, `ConvertFromJson` and `ConvertToJsonDocument<T>` use `EnhancedJsonSerializerOptions.DefaultOptions` (or the options you pass) through reflection-based serialization. They are annotated `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`; in trimmed or NativeAOT apps call `JsonSerializer` with a `JsonSerializerContext` instead.

```csharp
using System.Text.Json;
using CSharpEssentials.Json;

var user = new User("Alice", 30);

string json = user.ConvertToJson();                    // {"name":"Alice","age":30}
User? parsed = json.ConvertFromJson<User>();
object? untyped = json.ConvertFromJson(typeof(User));
JsonDocument doc = json.ConvertToJsonDocument();

public record User(string Name, int Age);
```

`ConvertToJsonDocument()` on a `string` takes no options: it parses the text as JSON, and text that is not valid JSON becomes a JSON string value in the document. It returns `null` only when every strategy fails. `ConvertToJsonDocument<T>(options)` on any other value serializes it to a document.

### JsonElement to CLR values

`Dictionary<string, object>` and `object` deserialize to `JsonElement`. `ToClrObject` turns them into plain values: objects become `Dictionary<string, object?>`, arrays `List<object?>`, numbers `int`, `long`, `decimal` or `double` (first that fits).

```csharp
using System.Text.Json;
using CSharpEssentials.Json;

using JsonDocument doc = JsonDocument.Parse("""{"id":1,"price":9.99,"tags":["a"]}""");

var data = (Dictionary<string, object?>)doc.RootElement.ToClrObject()!;
// data["id"] is int, data["price"] is decimal, data["tags"] is List<object?>
```

### Enum conventions

`AddEnumConventions` adds the enum converter for every enum with generated metadata (`[StringEnum]`, see [CSharpEssentials.Enums](../CSharpEssentials.Enums/Readme.MD)). Other enums are left to the converters already on the options, so a plain enum stays a number unless you add `JsonStringEnumConverter` yourself.

```csharp
using System.Text.Json;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;

[StringEnum]
public enum Status { Active, Inactive, [EnumFallback] Unknown }

public enum Level { Low, High }

public sealed record Account(Status Status, Level Level);

// Data mode (the default): tolerant, for values you stored or received from a trusted service
JsonSerializerOptions data = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    .AddEnumConventions(EnumConventions.Default);

// Input mode: strict, for request bodies (AddEnumConventions() in CSharpEssentials.AspNetCore does this for you)
JsonSerializerOptions input = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    .AddEnumConventions(EnumConventions.Default, EnumReadMode.Input);

JsonSerializer.Serialize(new Account(Status.Active, Level.High), data); // {"status":"active","level":1}
JsonSerializer.Deserialize<Account>("""{"status":"gone","level":0}""", data);  // Status.Unknown (fallback)
JsonSerializer.Deserialize<Account>("""{"status":"gone","level":0}""", input); // throws EnumValueJsonException
```

- Reads accept the wire name, the member name in any casing, aliases and the number of a defined member. `Input` mode applies `EnumConventions.AcceptNumbers`, `AcceptMemberNames` and `CaseInsensitive`.
- Undefined values (`"gone"`, `999`) are rejected with `EnumValueJsonException` (a `JsonException` whose `Error` property carries the `EnumValueError`). In `Data` mode only, they map to the `[EnumFallback]` member when the enum declares one (`EnumConventions.UnknownValue = UnknownEnumValueHandling.UseFallback`, the default).
- Writes are the wire name, or the number with `writeAs: EnumWireFormat.Number`. `[Flags]` values are JSON arrays.
- An undefined value created in code (`(Status)42`, or a `[Flags]` value with a bit no member covers) is written as its number, `42`, in both write formats; as a dictionary key it is `"42"`. Writes do not throw. Reads stay strict, so this is one-way: `Input` mode rejects the `42` it just wrote, and `Data` mode maps it to the `[EnumFallback]` member or rejects it.
- `EnhancedJsonSerializerOptions.DefaultOptions` already calls `AddEnumConventions(EnumConventions.Default)` (Data mode).
- A `[StringEnum]` enum without generated metadata (CSE0015) throws when its converter is created instead of falling back to numbers. `AddEnumConventionsWithReflection` also converts enums without metadata through reflection; it is not trim or AOT safe.

`ConditionalStringEnumConverter` (4.x) is obsolete and forwards to the same converter in `Input` mode. It no longer has `AllowUndefinedValues`: undefined numbers are always rejected. See [Migrating from 4.x to 5.0](../docs/migration/v4-to-v5.md).

### StringEnumNaming

`StringEnumNaming` is obsolete since 5.0 and stays as a working facade over `EnumMetadata`. New code uses `EnumMetadata`, `EnumValueFormatter`, `EnumValueParser` or the generated `ToWireName()` and `TryParseWire` helpers, the naming source that JSON, EF Core storage, OpenAPI schemas and query/route binding share. Names are `[JsonStringEnumMemberName]` when present, otherwise `snake_case_lower`.

```csharp
using CSharpEssentials.Enums;
using CSharpEssentials.Json;

[StringEnum]
public enum Http { OK, HTTPStatus }

StringEnumNaming.GetName(Http.HTTPStatus);                    // "http_status"
StringEnumNaming.GetNames<Http>();                            // ["ok", "http_status"]
StringEnumNaming.TryParse<Http>("HTTPStatus", out var value); // true (policy name, member name or number)
```

### Polymorphism

There are two paths, and the base type picks one:

- A base with System.Text.Json's `[JsonPolymorphic]` or `[JsonDerivedType]` uses System.Text.Json's own polymorphism. CSharpEssentials adds the derived types the base does not list, so they can live in any assembly.
- A base with neither attribute keeps `PolymorphicJsonConverterFactory`: a `$type` property with the CLR full name, found by scanning the loaded assemblies. See [Bases without attributes](#bases-without-attributes).

#### Events owned by each service

A shared Core project defines only the base. Each service defines its own events, with stable versioned values, in its own assembly. Adding an event never touches Core.

```csharp
// Core
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$event")]
public interface IEvent;

// Orders service
[JsonTypeDiscriminator("order.placed.v1")]
public sealed record OrderPlaced(Guid Id, decimal Total) : IEvent;

[JsonTypeDiscriminator("order.placed.v2")]
public sealed record OrderPlacedV2(Guid Id, decimal Total, string Currency) : IEvent;

// Users service
[JsonTypeDiscriminator("user.created.v1")]
public sealed record UserCreated(Guid Id, string Name) : IEvent;
```

```csharp
var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() }
    .AddPolymorphism();

string json = JsonSerializer.Serialize<IEvent>(new UserCreated(id, "Ada"), options);
// {"$event":"user.created.v1","id":"...","name":"Ada"}

IEvent? back = JsonSerializer.Deserialize<IEvent>("""{"id":"...","name":"Ada","$event":"user.created.v1"}""", options);
```

The bundled source generator runs in every project that references `CSharpEssentials.Json`. Each assembly gets a registry that its module initializer runs: it registers the assembly's `[JsonTypeDiscriminator]` types, after the registries of the assemblies it references. The app's own registry therefore covers every service assembly it references, even one that is not loaded yet. Renaming or moving a class does not change its value. Integer values (`[JsonTypeDiscriminator(7)]`) work too.

`AddPolymorphism` and `AddPolymorphismWithReflection` turn on `AllowOutOfOrderMetadataProperties`, so the discriminator does not have to be the first property on read.

#### Resolution order

For a base with `[JsonPolymorphic]` or `[JsonDerivedType]`, the derived types come from:

1. `[JsonDerivedType(typeof(X), "x")]` on the base. System.Text.Json reads these; CSharpEssentials never changes them. When a listed type also has `[JsonTypeDiscriminator]`, the base value wins (CSE4002).
2. Generated registrations from `[JsonTypeDiscriminator]` on derived types, in any assembly. This step uses no reflection.
3. With `AddPolymorphismWithReflection` only, and only for a base with no `[JsonDerivedType]`: a scan of the given assemblies, or of every loaded assembly except .NET framework assemblies when none are given. A type with `[JsonTypeDiscriminator]` gets its value; a type without one gets its CLR full name (CSE4005 warns about it). Abstract types, open generic types, by-ref-like types, types with their own `[JsonConverter]` and types System.Text.Json cannot configure are skipped.

```csharp
options.AddPolymorphismWithReflection(typeof(OrderPlaced).Assembly, typeof(UserCreated).Assembly);
```

Two derived types with the same value under one base throw `InvalidOperationException` the first time the base is used. The message names both types and their assemblies. A base that ends up with no derived types throws too. An unknown value on read throws `JsonException`.

#### Discriminator name per property

`[JsonPolymorphicProperty]` renames the discriminator for the values of one property. The property type, or its element type for arrays, collections, dictionary values and `Nullable<T>`, must be a base with `[JsonPolymorphic]` or `[JsonDerivedType]`.

```csharp
[JsonPolymorphic]
public interface IValue;

[JsonTypeDiscriminator("text")]   public sealed class TextValue : IValue { public string Text { get; set; } = ""; }
[JsonTypeDiscriminator("number")] public sealed class NumberValue : IValue { public decimal Number { get; set; } }

public class Data
{
    [JsonPolymorphicProperty("values")]
    public IValue? Value { get; set; }

    [JsonPolymorphicProperty("values")]
    public List<IValue> Items { get; set; } = [];

    public IValue? Other { get; set; }
}

// {"value":{"values":"text","text":"a"},"items":[{"values":"number","number":2}],"other":{"$type":"number","number":1}}
```

The name applies to every value of that base inside the property, nested values included.

#### Source-generated contexts and native AOT

`AddPolymorphism` uses no reflection, so it is trim and native AOT safe. Copy the context's options so the `[JsonSourceGenerationOptions]` values stay, list every derived type with `[JsonSerializable]`, and call `AddPolymorphism` last:

```csharp
[JsonSerializable(typeof(IEvent))]
[JsonSerializable(typeof(OrderPlaced))]
[JsonSerializable(typeof(UserCreated))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web, AllowOutOfOrderMetadataProperties = true)]
public partial class AppJsonContext : JsonSerializerContext;

var options = new JsonSerializerOptions(AppJsonContext.Default.Options).AddPolymorphism();
var events = (JsonTypeInfo<IEvent>)options.GetTypeInfo(typeof(IEvent));
string json = JsonSerializer.Serialize(new OrderPlaced(id, 10m), events);
```

A derived type missing from the context is left out, so writing it throws `NotSupportedException`; CSE4006 reports it and its code fix adds the `[JsonSerializable]` line. A resolver inserted into `TypeInfoResolverChain` after `AddPolymorphism` bypasses it. `AddPolymorphismWithReflection`, `[JsonAcceptClrTypeNames]` and `PolymorphicJsonConverterFactory` use reflection and are not AOT safe. `benchmarks/json-aot` publishes a sample with native AOT and no warnings.

#### Reading old CLR names

Payloads written by `PolymorphicJsonConverterFactory` carry the CLR full name in `$type`. After the base moves to `[JsonPolymorphic]` and values, mark it `[JsonAcceptClrTypeNames]` to keep reading those payloads. A CLR name is read from the base's discriminator property and, when the base uses another name such as `$event`, from `$type` too. Old names nested inside a current payload read as well. Writes use the new value, except for values typed as `object`, which get no discriminator (see Limits). Every payload of such a base is read through a `JsonObject` copy, so it is slower than a base without the attribute. It needs `AddPolymorphismWithReflection`, which `DefaultOptions` and `StrictOptions` include.

```csharp
[JsonPolymorphic]
[JsonAcceptClrTypeNames]
public interface IEvent;
```

#### Analyzers

| Id | Severity | Reports |
|---|---|---|
| CSE4001 | Error | Two derived types of one base use the same value (from `[JsonTypeDiscriminator]`, `[JsonDerivedType]` or a CLR full name) |
| CSE4002 | Warning | `[JsonTypeDiscriminator]` on a type the base already lists; the base value wins |
| CSE4003 | Error | `[JsonTypeDiscriminator]` on an abstract type, an interface or a generic type |
| CSE4004 | Warning | The discriminator name equals a member's JSON name (ignoring case) |
| CSE4005 | Warning | A derived type of a base without `[JsonDerivedType]` has no `[JsonTypeDiscriminator]`. `AddPolymorphismWithReflection` writes its CLR name, which breaks when the type is renamed, and `AddPolymorphism` leaves it out |
| CSE4006 | Warning | A `JsonSerializerContext` lists a base but not one of its `[JsonTypeDiscriminator]` types declared in the same project (code fix) |
| CSE4007 | Warning | A derived type has its own `[JsonConverter]` and is left out |
| CSE4008 | Warning | `[JsonTypeDiscriminator]` on a type whose bases have no System.Text.Json polymorphism attribute |
| CSE4009 | Warning | `[JsonPolymorphicProperty]` on a member that holds no polymorphic base |
| CSE4010 | Warning | A type generated code cannot reach (private, protected, file-local or generic) |
| CSE4011 | Info | A derived type of a base with `[JsonDerivedType]` is neither listed nor valued, so writing it throws |
| CSE4012 | Warning | `[property: JsonPolymorphicProperty]` on a positional record parameter, which generated code does not see; declare the property or use `AddPolymorphismWithReflection` |

#### Limits

- System.Text.Json caches a base's contract per options instance. A derived type registered after the base was first used, for example from a plugin loaded later, is not added to that instance.
- A value typed as `object` whose runtime type derives from two unrelated polymorphic bases gets no discriminator (System.Text.Json's ambiguity rule).
- Generic bases are not registered by the generator; `AddPolymorphismWithReflection` still finds their derived types.
- A value typed as `object` whose runtime type derives from a `[JsonAcceptClrTypeNames]` base is written without a discriminator, so it cannot be read back as that base. Declare the member as the base type.
- `[JsonPolymorphicProperty]` and `[JsonAcceptClrTypeNames]` serialize their values in a nested call. With `ReferenceHandler.Preserve` or a custom `ReferenceHandler` they throw `NotSupportedException`, because the nested call cannot share `$id` references. With `ReferenceHandler.IgnoreCycles` a cycle through such a value is cut one level later than usual.

#### Bases without attributes

```csharp
namespace Zoo;

public abstract class Animal { public string Name { get; set; } = string.Empty; }
public class Dog : Animal { public string Breed { get; set; } = string.Empty; }
```

```csharp
using Zoo;

Animal dog = new Dog { Name = "Rex", Breed = "Labrador" };
string json = dog.ConvertToJson(); // {"$type":"Zoo.Dog","breed":"Labrador","name":"Rex"}

Animal? restored = json.ConvertFromJson<Animal>();
```

`DefaultOptions` applies `PolymorphicJsonConverterFactory` to abstract classes and interfaces; collections and dictionaries keep the built-in array and object serialization. The `$type` value is the concrete type's full name (namespace included) and is written first. On read, `$type` is required and is matched against the non-abstract types of the loaded assemblies that derive from the declared type; a missing or unknown value throws `JsonException`. The scan runs once per declared type, on first use, so assemblies loaded later are not seen. Generic derived types are not discovered: a value such as `Cage<Dog>` is written with its full generic name in `$type`, but reading it back throws `JsonException`. When two loaded types share a full name, the first one found wins. Discovery uses reflection, so this is not trim or AOT safe: use `[JsonPolymorphic]` with `AddPolymorphism` there. .NET framework types are never candidates.

A type marked with `[JsonPolymorphic]` or `[JsonDerivedType]` is left to System.Text.Json and the path above. Async streams (any type that implements `IAsyncEnumerable<T>`) and `JsonNode` values keep their built-in serialization too.

### Multi-format DateTime

`MultiFormatDateTimeConverterFactory` (part of `DefaultOptions` and `DefaultOptionsWithDateTimeConverter`) reads `DateTime` and `DateTime?` from JSON strings in ISO 8601 (with or without `Z` or an offset), `yyyy-MM-dd`, `dd.MM.yyyy`, `dd/MM/yyyy`, `MM/dd/yyyy`, `March 14, 2024`, `14 March 2024`, `yyyyMMdd`, RFC 1123 (`Thu, 14 Mar 2024 15:30:45 GMT`) and other patterns, and from a numeric string of Unix seconds. Unrecognized text throws `JsonException` listing the supported formats; `null` is allowed only for `DateTime?`. Values are written as `yyyy-MM-ddTHH:mm:ss.ffffffzzz`; a `DateTime` with `Kind` `Unspecified` is written as UTC (`+00:00`), so it reads back unchanged.

```csharp
string[] dates = ["\"2024-03-14T15:30:45Z\"", "\"14.03.2024\"", "\"March 14, 2024\""];
foreach (var d in dates)
{
    DateTime dt = d.ConvertFromJson<DateTime>();
}

var options = new JsonSerializerOptions();
options.Converters.Add(new MultiFormatDateTimeConverterFactory("dd.MM.yyyy HH-mm"));
DateTime custom = JsonSerializer.Deserialize<DateTime>("\"14.03.2024 10-20\"", options);
```

Constructor arguments add formats to the built-in list. Every value is returned as UTC (`DateTime.Kind` is `Utc`) regardless of the host time zone: input without an offset is read as UTC, input with an offset or `Z` is converted to UTC, and a Unix-seconds string becomes the matching UTC instant. A date-only value such as `"14.03.2024"` is midnight UTC on that date. Slash dates are tried as `dd/MM/yyyy` before `MM/dd/yyyy`, so `"03/04/2024"` reads as 3 April.

## Upgrading to 8.0

- Members declared as `JsonNode` or `JsonValue` are written without a `$type` wrapper. A `JsonObject` stored by 7.x in such a member carries a `"$type"` key and now reads back with that key inside it; strip `$type` from those stored objects.
- Abstract types and interfaces marked with `[JsonPolymorphic]` or `[JsonDerivedType]` are no longer handled by `PolymorphicJsonConverterFactory`. They are written with the discriminators from the attributes, and payloads stored with a full CLR name in `$type` no longer read. Mark the base `[JsonAcceptClrTypeNames]` to keep reading them, rewrite stored `$type` values to the attribute discriminators, or remove the attributes to keep the old format.
- Abstract and interface properties nested inside a polymorphic value now get their own `$type` and read back as the derived type. Before, they were written without `$type` and could not be read. Payloads written by 7.x that hold such nested members need a `$type` added to each nested object before they can be read.
- `MultiFormatDateTimeConverter` throws `JsonException` for a Unix-seconds string outside the `DateTime` range. Before, it threw `ArgumentOutOfRangeException`. Catch `JsonException` instead.
- `MultiFormatDateTimeConverter` writes a `DateTime` with `Kind` `Unspecified` as UTC (`+00:00`). Before, it used the host's local offset, so a round trip on a non-UTC host shifted the value. Set `Kind` to `Local` before serializing if the value is local time.
- The enum converter writes an undefined value as its underlying number (`42`, dictionary key `"42"`) instead of throwing `EnumValueException`. A `[Flags]` value with undefined bits is written as one number, not an array. Reads still reject undefined numbers in `Input` mode. `HasJsonConversion` properties save such a value too, and loading it gives the `[EnumFallback]` member or throws when the enum has none. Check `EnumInfo<TEnum>.IsDefined` before serializing if an undefined value must still fail.
- `PolymorphicJsonConverterFactory` no longer picks derived types from .NET framework assemblies (assemblies signed with a .NET framework public key). A payload on an `IDisposable` or `ICloneable` member could name any loaded framework type with a public parameterless constructor; such a `$type` now throws `JsonException`. Your own types that implement those interfaces still read. If you stored a framework type behind an interface, wrap it in your own type.
- `DefaultOptions` and `StrictOptions` call `AddPolymorphismWithReflection()`, and so does the `HttpRequestBuilder` in `CSharpEssentials.Http`. Their `TypeInfoResolver` is no longer `null` but a `DefaultJsonTypeInfoResolver` with the polymorphism modifier, and `AllowOutOfOrderMetadataProperties` is `true`. `ApplyTo` sets `AllowOutOfOrderMetadataProperties` on the target and adds the polymorphism to the target's own resolver (or uses the source's when the target has none), so ASP.NET Core options set up with `ConfigureSystemTextJson` keep their contexts and get it too. If you set your own `TypeInfoResolver` after `ApplyTo`, call `AddPolymorphism()` or `AddPolymorphismWithReflection()` again. A `[JsonPolymorphic]` base with no `[JsonDerivedType]` used to throw; it now finds its derived types.
