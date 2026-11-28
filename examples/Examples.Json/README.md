# CSharpEssentials.Json example

A console app for [CSharpEssentials.Json](../../CSharpEssentials.Json/Readme.MD). Each section prints the JSON it writes or the value it reads.

## Run

```bash
dotnet run --project examples/Examples.Json
```

## What each section shows

| Section | Shows |
|---|---|
| Option presets | `DefaultOptions`, `DefaultOptionsWithoutConverters`, `Create`, `CreateOptionsWithConverters`, `ApplyTo`, `ApplyFrom`, and `StrictOptions` rejecting an unknown property |
| Extensions | `ConvertToJson`, both `ConvertFromJson` overloads, both `ConvertToJsonDocument` overloads, `JsonException` for malformed input |
| `JsonElement.ToClrObject` | The CLR type each JSON value becomes |
| Required value-type constructor parameters | `RequireValueTypeConstructorParameters` turning a missing `quantity` into a `JsonException` |
| Enum conventions | `AddEnumConventions` in Data and Input mode, `writeAs: EnumWireFormat.Number`, enum dictionary keys, a plain enum staying a number, `EnumValueJsonException` |
| Polymorphism with `[JsonTypeDiscriminator]` | `AddPolymorphism`, a discriminator that is not the first property, `[JsonPolymorphicProperty]`, an unknown value |
| Source-generated context | `AddPolymorphism` over a `JsonSerializerContext`, the Native AOT path |
| `[JsonAcceptClrTypeNames]` | Reading a `$type` CLR name and writing the new value |
| Bases without attributes | `$type` with the CLR name from `PolymorphicJsonConverterFactory`, and the error for a missing `$type` |
| Multi-format DateTime | Several input formats, all returned as UTC, `DateTime?`, a custom format, an unknown format |

## Project references

Inside this repository the project references `CSharpEssentials.Json.Generators` and `CSharpEssentials.Enums.Generators` with `OutputItemType="Analyzer"`, because a `ProjectReference` does not carry a project's generator. A project that uses the NuGet package needs only:

```xml
<PackageReference Include="CSharpEssentials.Json" Version="9.0.0" />
```
