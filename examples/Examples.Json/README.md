# CSharpEssentials.Json example

A console application that runs the `CSharpEssentials.Json` helpers. Full documentation: [CSharpEssentials.Json Readme](../../CSharpEssentials.Json/Readme.MD).

## Features demonstrated

| Feature | Description |
|---------|-------------|
| Default options | `EnhancedJsonSerializerOptions.DefaultOptions` and `DefaultOptionsWithoutConverters`, with `Create`, `ApplyTo` and `ApplyFrom` |
| Polymorphic serialization | Serialize/deserialize derived types, collections and nested objects with the `$type` discriminator, plus missing and unknown `$type` errors |
| JSON extensions | `ConvertToJson<T>()`, `ConvertFromJson<T>()`, `ConvertToJsonDocument<T>()` |
| DateTime conversion | Multi-format date parsing with `MultiFormatDateTimeConverterFactory`, Unix timestamps and `DateTime?` |
| Enum conventions | `[StringEnum]` enums written as strings, plain enums written as numbers |
| JsonDocument navigation | `TryGetNestedProperty`, which comes from the `CSharpEssentials` meta-package (`Result<JsonElement?>`), not from `CSharpEssentials.Json` |
| Malformed JSON | `JsonException` from `ConvertFromJson` for invalid input |
| JsonElement extraction | Reading values with `GetProperty`, `GetInt32`, `EnumerateArray` and related members |

## Running

```bash
cd examples/Examples.Json
dotnet run
```
