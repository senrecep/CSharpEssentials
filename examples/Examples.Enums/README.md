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

The project references the library projects and the generator project with `OutputItemType="Analyzer"`. That form only works inside this repository: a `ProjectReference` does not carry the referenced project's own generator output, but analyzers from that project's NuGet packages do flow, so the generator project is referenced explicitly. In your own project reference the package:

```xml
<ItemGroup>
  <PackageReference Include="CSharpEssentials.Json" Version="5.0.0" />
</ItemGroup>
```

Any CSharpEssentials package brings the generator, the analyzers and the code fixes: every package that depends on Enums, directly or through another CSharpEssentials package, passes them on. Reference `CSharpEssentials.Enums` directly only in a project that uses no other CSharpEssentials package that depends on it (`.Core`, `.Clone`, `.Time`, `.DependencyInjection`, `.Endpoints` and `.RequestResponseLogging` do not depend on it).

## Running

```bash
cd examples/Examples.Enums
dotnet run
```
