# CSharpEssentials.Enums Example

This console application shows the helpers that the `CSharpEssentials.Enums` source generator writes for `[StringEnum]` enums. Full documentation: [CSharpEssentials.Enums Readme](../../CSharpEssentials.Enums/Readme.MD).

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **`[StringEnum]`** | Opts an enum into the generator and the enum conventions |
| **Wire names** | `ToWireName()`, the `{Member}WireName` constants, `TryParseWire()` and `ParseWire()` |
| **Other generated helpers** | `ToOptimizedString()`, `ToKebabCase()`, `ToLowerCase()`, `ToUpperCase()`, `IsDefined()`, `GetNames()`, `GetValues()` |
| **Runtime API** | `EnumValueFormatter` and `EnumMetadata` for `DeliveryStatus` |

## Package references

The project references the library projects and the generator project with `OutputItemType="Analyzer"`. That form only works inside this repository. In your own project reference the package, which contains the generator, the analyzers and the code fixes:

```xml
<ItemGroup>
  <PackageReference Include="CSharpEssentials.Enums" Version="5.0.0" />
  <PackageReference Include="CSharpEssentials.Json" Version="5.0.0" />
</ItemGroup>
```

Reference `CSharpEssentials.Enums` directly in every project that declares a `[StringEnum]` enum; the generator does not flow through other packages.

## Running

```bash
cd examples/Examples.Enums
dotnet run
```
