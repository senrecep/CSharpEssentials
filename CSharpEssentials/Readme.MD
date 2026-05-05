# CSharpEssentials

Meta-package that bundles the core functional modules of the ecosystem. Install this to get the most commonly used packages in one reference.

## Included Packages

- `CSharpEssentials.Core`
- `CSharpEssentials.Errors`
- `CSharpEssentials.Results`
- `CSharpEssentials.Maybe`
- `CSharpEssentials.Json`
- `CSharpEssentials.Any`
- `CSharpEssentials.Rules`
- `CSharpEssentials.Entity`
- `CSharpEssentials.Http`
- `CSharpEssentials.Time`
- `CSharpEssentials.Clone`
- `CSharpEssentials.Enums`

## Installation

```bash
dotnet add package CSharpEssentials
```

## Usage

This meta-package is the easiest way to start. For ASP.NET Core, EF Core, Mediator, or GCP Secret Manager support, install the specific packages separately.

```csharp
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;

Maybe<User> maybe = FindUser(1);
Result<User> result = maybe.ToResult(Error.NotFound("User.Missing", "User not found."));
```
