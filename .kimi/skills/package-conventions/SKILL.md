---
name: package-conventions
description: NuGet package and project file conventions for CSharpEssentials. Use when modifying .csproj, Directory.Packages.props, or Directory.Build.props.
---

## Package & Project Conventions

- All package versions live in `Directory.Packages.props`. Never set `Version=` directly in a `.csproj`.
- Use `<PackageReference Include="Foo" />` (no version) in projects; version is resolved centrally.
- Target framework changes require updating the README.MD framework support table.
- `<IsPackable>true</IsPackable>` is the default (set in Directory.Build.props); only override to `false` for test/example projects.
- New packages must follow the existing metadata pattern in Directory.Build.props (Authors, Copyright, PackageLicenseExpression, etc.).
- `GeneratePackageOnBuild` is false — packing is done explicitly via `dotnet pack` or the publish script.
- Keep `<IncludeSymbols>true</IncludeSymbols>` and `<SymbolPackageFormat>snupkg</SymbolPackageFormat>` on all packable projects.
