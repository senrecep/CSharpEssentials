# CSharpEssentials.Json

Enhanced JSON serialization utilities on top of `System.Text.Json`.

## Features

- **Enum Conventions**: `[StringEnum]` enums read and write their wire names (`AddEnumConventions`); other enums keep the host's behavior.
- **Multi-Format DateTime**: Deserialize from many date formats automatically.
- **Polymorphic Serialization**: Serialize abstract types and interfaces with a `$type` discriminator.
- **Pre-Configured Options**: CamelCase, ignore cycles, ignore nulls, relaxed escaping.

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

`ConvertToJson`, `ConvertFromJson` and `ConvertToJsonDocument` use `EnhancedJsonSerializerOptions.DefaultOptions` (or the options you pass) through reflection-based serialization. They are annotated `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`; in trimmed or NativeAOT apps call `JsonSerializer` with a `JsonSerializerContext` instead.

```csharp
using System.Text.Json;
using CSharpEssentials.Json;

var user = new { Name = "Alice", Age = 30 };

string json = user.ConvertToJson();
var parsed = json.ConvertFromJson<User>();
JsonDocument doc = json.ConvertToJsonDocument();
```

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
- Undefined values (`"gone"`, `999`) are rejected with `EnumValueJsonException` (a `JsonException`). In `Data` mode only, they map to the `[EnumFallback]` member when the enum declares one (`EnumConventions.UnknownValue = UnknownEnumValueHandling.UseFallback`, the default).
- Writes are the wire name, or the number with `writeAs: EnumWireFormat.Number`. `[Flags]` values are JSON arrays.
- `EnhancedJsonSerializerOptions.DefaultOptions` already calls `AddEnumConventions(EnumConventions.Default)` (Data mode).
- A `[StringEnum]` enum without generated metadata (CSE0015) throws when its converter is created instead of falling back to numbers. `AddEnumConventionsWithReflection` also converts enums without metadata through reflection; it is not trim or AOT safe.

`ConditionalStringEnumConverter` (4.x) is obsolete and forwards to the same converter in `Input` mode. It no longer has `AllowUndefinedValues`: undefined numbers are always rejected. See [Migrating from 4.x to 5.0](../docs/migration/v4-to-v5.md).

### StringEnumNaming

`StringEnumNaming` is obsolete in 5.0 and stays as a working facade over `EnumMetadata`. In new code use `EnumMetadata`, `EnumValueFormatter`, `EnumValueParser` or the generated `ToWireName()` and `TryParseWire` helpers: they are the naming source that JSON, EF Core storage, Swagger schemas and query/route binding all use, so a member is spelled the same everywhere. Names are `[JsonStringEnumMemberName]` when present, otherwise `snake_case_lower`.

```csharp
using CSharpEssentials.Json;

[StringEnum]
public enum Http { OK, HTTPStatus }

StringEnumNaming.GetName(Http.HTTPStatus);                    // "http_status"
StringEnumNaming.GetNames<Http>();                            // ["ok", "http_status"]
StringEnumNaming.TryParse<Http>("HTTPStatus", out var value); // true (policy name, member name or number)
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
