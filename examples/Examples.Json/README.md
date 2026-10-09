# CSharpEssentials.Json Example

This console application demonstrates JSON utilities from `CSharpEssentials.Json`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **Default Options** | `EnhancedJsonSerializerOptions.DefaultOptions` and `DefaultOptionsWithoutConverters`, with `Create`, `ApplyTo` and `ApplyFrom` |
| **Polymorphic Serialization** | Serialize/deserialize derived types, collections and nested objects with the `$type` discriminator, plus missing and unknown `$type` errors |
| **Json Extensions** | `ConvertToJson<T>()`, `ConvertFromJson<T>()`, `ConvertToJsonDocument<T>()` |
| **DateTime Conversion** | Multi-format date parsing with `MultiFormatDateTimeConverterFactory`, Unix timestamps and `DateTime?` |
| **Enum Conventions** | `[StringEnum]` enums written as strings, plain enums written as numbers |
| **JsonDocument Navigation** | `TryGetNestedProperty`, which comes from the `CSharpEssentials` meta-package (`Result<JsonElement?>`), not from `CSharpEssentials.Json` |
| **Malformed JSON** | `JsonException` from `ConvertFromJson` for invalid input |
| **JsonElement Extraction** | Reading values with `GetProperty`, `GetInt32`, `EnumerateArray` and related members |

## Running

```bash
cd examples/Examples.Json
dotnet run
```
