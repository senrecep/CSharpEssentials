# CSharpEssentials.Enums example

A console app for [CSharpEssentials.Enums](../../CSharpEssentials.Enums/Readme.MD): the helpers the source generator writes for `[StringEnum]` enums, the runtime parser and metadata, flags, and JSON through CSharpEssentials.Json.

## Run

```bash
dotnet run --project examples/Examples.Enums
```

## What each section shows

| Section | Shows |
|---|---|
| Wire names | `ToWireName()`, `ToDescription()`, the `{Member}WireName` constant, `[JsonStringEnumMemberName]` and `EnumNaming.KebabCaseLower` |
| Parsing with the generated helpers | `TryParseWire` with a wire name, a member name, an `[EnumAlias]`, a number and an unknown value; `ParseWire` throwing `EnumValueException` |
| Input and Data read modes | `EnumValueParser.TryParse` rejecting an unknown value in `Input` mode (with `EnumValueError.AllowedValues`) and mapping it to `[EnumFallback]` in `Data` mode; a `with` variant of `EnumConventions` |
| Other generated helpers | `ToOptimizedString`, `ToKebabCase`, `ToLowerCase`, `ToUpperCase`, `AsUnderlyingType`, `IsDefined`, `Names`, `GetValues` |
| Metadata and formatter | `EnumMetadata.Get`, `WireNames`, `Fallback`, `TypedMembers` with their aliases, `EnumValueFormatter.Format`, `EnumMetadata.IsRegistered` |
| Flags | `ToWireName()` of a combination, `ParseWire` of a number, `IsDefined` on an unknown bit |
| JSON | `AddEnumConventions` writes wire names and flag arrays, and reads an alias back |

`Shipped` lists `shipped` as an alias although none is declared: the generator adds the 4.x snake case spelling when it differs from the wire name, so old stored values still read.

## Project references

Inside this repository the project references the library projects and `CSharpEssentials.Enums.Generators` with `OutputItemType="Analyzer"`, because a `ProjectReference` does not carry a project's generator. Your own project references the NuGet package instead, which brings the generator:

```xml
<PackageReference Include="CSharpEssentials.Json" Version="9.0.0" />
```

For an ASP.NET Core API with EF Core storage and OpenAPI, see [Examples.Enums.EndToEnd](../Examples.Enums.EndToEnd/README.md).
