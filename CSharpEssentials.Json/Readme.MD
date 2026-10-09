# CSharpEssentials.Json

Enhanced JSON serialization utilities on top of `System.Text.Json`.

## Features

- **Enum Conventions**: `[StringEnum]` enums read and write their wire names (`AddEnumConventions`); other enums keep the host's behavior.
- **Multi-Format DateTime**: Read `DateTime` and `DateTime?` from many date formats automatically.
- **Polymorphic Serialization**: Serialize abstract types and interfaces with a `$type` discriminator.
- **Pre-Configured Options**: CamelCase, ignore cycles, ignore nulls, relaxed escaping, plus a strict preset.
- **JsonElement Helpers**: `ToClrObject` turns a `JsonElement` into plain CLR values.

## Installation

```bash
dotnet add package CSharpEssentials.Json
```

Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1`. It depends on `CSharpEssentials.Enums`.

## Usage

### Pre-Configured Options

```csharp
using CSharpEssentials.Json;
using System.Text.Json;

var options = EnhancedJsonSerializerOptions.DefaultOptions;
string json = JsonSerializer.Serialize(myObject, options);
```

| Member | Contents |
|---|---|
| `DefaultOptionsWithoutConverters` | Web defaults, camelCase, case-insensitive reads, nulls ignored on write, `ReferenceHandler.IgnoreCycles`, relaxed escaping |
| `DefaultOptions` | The above plus enum conventions (Data mode), `MultiFormatDateTimeConverterFactory` and `PolymorphicJsonConverterFactory` |
| `DefaultOptionsWithDateTimeConverter` | `DefaultOptionsWithoutConverters` plus `MultiFormatDateTimeConverterFactory` |
| `StrictOptions` | Case-sensitive; rejects trailing commas, comments and unmapped members |
| `CreateOptionsWithConverters(params JsonConverter[])` | `DefaultOptionsWithoutConverters` plus the given converters |
| `options.Create(configure)` | Copies the options and applies `configure` to the copy |
| `source.ApplyTo(target)` / `target.ApplyFrom(source)` | Copies settings and converters onto another instance; the target keeps its own `TypeInfoResolver` when the source has none |

The preset instances are shared and become read-only after first use, so call `Create` to get a modifiable copy. `EnhancedJsonSerializerOptions` is annotated `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]` because `DefaultOptions` includes the reflection-based polymorphic converter.

```csharp
JsonSerializerOptions indented = EnhancedJsonSerializerOptions.DefaultOptions.Create(o => o.WriteIndented = true);

var target = new JsonSerializerOptions();
EnhancedJsonSerializerOptions.DefaultOptions.ApplyTo(target);
```

### Quick Extensions

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

### JsonElement to CLR Values

`Dictionary<string, object>` and `object` deserialize to `JsonElement`. `ToClrObject` turns them into plain values: objects become `Dictionary<string, object?>`, arrays `List<object?>`, numbers `int`, `long`, `decimal` or `double` (first that fits).

```csharp
using System.Text.Json;
using CSharpEssentials.Json;

using JsonDocument doc = JsonDocument.Parse("""{"id":1,"price":9.99,"tags":["a"]}""");

var data = (Dictionary<string, object?>)doc.RootElement.ToClrObject()!;
// data["id"] is int, data["price"] is decimal, data["tags"] is List<object?>
```

### Enum Conventions

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
- `EnhancedJsonSerializerOptions.DefaultOptions` already calls `AddEnumConventions(EnumConventions.Default)` (Data mode).
- A `[StringEnum]` enum without generated metadata (CSE0015) throws when its converter is created instead of falling back to numbers. `AddEnumConventionsWithReflection` also converts enums without metadata through reflection; it is not trim or AOT safe.

`ConditionalStringEnumConverter` (4.x) is obsolete and forwards to the same converter in `Input` mode. It no longer has `AllowUndefinedValues`: undefined numbers are always rejected. See [Migrating from 4.x to 5.0](../docs/migration/v4-to-v5.md).

### StringEnumNaming

`StringEnumNaming` is obsolete in 5.0 and stays as a working facade over `EnumMetadata`. In new code use `EnumMetadata`, `EnumValueFormatter`, `EnumValueParser` or the generated `ToWireName()` and `TryParseWire` helpers: they are the naming source that JSON, EF Core storage, Swagger schemas and query/route binding all use, so a member is spelled the same everywhere. Names are `[JsonStringEnumMemberName]` when present, otherwise `snake_case_lower`.

```csharp
using CSharpEssentials.Enums;
using CSharpEssentials.Json;

[StringEnum]
public enum Http { OK, HTTPStatus }

StringEnumNaming.GetName(Http.HTTPStatus);                    // "http_status"
StringEnumNaming.GetNames<Http>();                            // ["ok", "http_status"]
StringEnumNaming.TryParse<Http>("HTTPStatus", out var value); // true (policy name, member name or number)
```

### Polymorphic Serialization

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

`DefaultOptions` applies `PolymorphicJsonConverterFactory` to abstract classes and interfaces; collections and dictionaries keep the built-in array and object serialization. The `$type` value is the concrete type's full name (namespace included) and is written first. On read, `$type` is required and is matched against the non-abstract types of the loaded assemblies that derive from the declared type; a missing or unknown value throws `JsonException`. Discovery uses reflection, so this is not trim or AOT safe: use `[JsonPolymorphic]` with source generation there.

### Multi-Format DateTime

`MultiFormatDateTimeConverterFactory` (part of `DefaultOptions` and `DefaultOptionsWithDateTimeConverter`) reads `DateTime` and `DateTime?` from JSON strings in ISO 8601 (with or without `Z` or an offset), `yyyy-MM-dd`, `dd.MM.yyyy`, `dd/MM/yyyy`, `MM/dd/yyyy`, `March 14, 2024`, `14 March 2024`, `yyyyMMdd`, RFC 1123 (`Thu, 14 Mar 2024 15:30:45 GMT`) and other patterns, and from a numeric string of Unix seconds. Unrecognized text throws `JsonException` listing the supported formats; `null` is allowed only for `DateTime?`. Values are written as `yyyy-MM-ddTHH:mm:ss.ffffffzzz`.

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

Constructor arguments add formats to the built-in list. Input without an offset is read as UTC and the result is converted to the host's local time (`DateTime.Kind` is `Local`), so a date-only value such as `"14.03.2024"` is midnight UTC shown in local time, which in a time zone behind UTC is the previous evening. Slash dates are tried as `dd/MM/yyyy` before `MM/dd/yyyy`, so `"03/04/2024"` reads as 3 April. A Unix-seconds string becomes an `Unspecified` `DateTime` holding the UTC clock reading.
