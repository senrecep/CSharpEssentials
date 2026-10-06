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

The project references the library projects and the generator project with `OutputItemType="Analyzer"`. That form only works inside this repository: a `ProjectReference` never carries analyzers. In your own project reference the package:

```xml
<ItemGroup>
  <PackageReference Include="CSharpEssentials.Json" Version="5.0.0" />
</ItemGroup>
```

`CSharpEssentials.Json` brings `CSharpEssentials.Enums` with its generator, analyzers and code fixes, as do `.EntityFrameworkCore`, `.AspNetCore`, `.Http` and the `CSharpEssentials` meta-package. Reference `CSharpEssentials.Enums` directly only in a project that uses none of them.

## Running

```bash
cd examples/Examples.Enums
dotnet run
```
