---
name: package-conventions
description: NuGet package and project file conventions for CSharpEssentials. Use when modifying .csproj, Directory.Packages.props, or Directory.Build.props.
---

## Package and project conventions

- All package versions live in `Directory.Packages.props`. Never set `Version=` directly in a `.csproj`.
- Use `<PackageReference Include="Foo" />` (no version) in projects; the version is resolved centrally. `VersionOverride` is only for pins that differ per project or target framework, such as the EF Core major per target and the Roslyn 4.8.0 pin in `build/Generators.props`.
- Target framework changes require updating the .NET badge in README.MD and the target framework summary in AGENTS.md.
- `<IsPackable>true</IsPackable>` is the default (set in Directory.Build.props). Override it to `false` only for test, example, benchmark and source generator or code fix projects (`build/Generators.props` already does it for the last two).
- New packages follow the existing metadata pattern in Directory.Build.props (Authors, Copyright, PackageLicenseExpression, etc.) and ship an agent skill at `.well-known/agent-skills/<name>/SKILL.md`; Directory.Build.targets packs it at `skills/<name>/SKILL.md`, and CI fails when a skill is not packed.
- `GeneratePackageOnBuild` is false. Packing is done explicitly through `dotnet pack` or the publish script.
- Keep `<IncludeSymbols>true</IncludeSymbols>` and `<SymbolPackageFormat>snupkg</SymbolPackageFormat>` on all packable projects.
