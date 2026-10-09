# CSharpEssentials

Meta-package that bundles the core functional modules of the ecosystem. Install this to get the most commonly used packages in one reference.

## Included Packages

- `CSharpEssentials.Core`
- `CSharpEssentials.Errors`
- `CSharpEssentials.Results`
- `CSharpEssentials.Maybe`
- `CSharpEssentials.Json`
- `CSharpEssentials.Any`
- `CSharpEssentials.These`
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

This meta-package is the easiest way to start. It targets net11.0, net10.0, net9.0 and netstandard2.1. The other packages are installed separately: `CSharpEssentials.Validation`, `CSharpEssentials.Mediator`, `CSharpEssentials.Resilience`, `CSharpEssentials.Endpoints`, `CSharpEssentials.DependencyInjection`, `CSharpEssentials.EntityFrameworkCore`, `CSharpEssentials.AspNetCore` (with `CSharpEssentials.AspNetCore.OpenApi` or `CSharpEssentials.AspNetCore.Swashbuckle`), `CSharpEssentials.RequestResponseLogging` and `CSharpEssentials.GcpSecretManager`.

```csharp
using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;

Maybe<User> maybe = FindUser(1);
Result<User> result = maybe.ToResult(Error.NotFound("User.Missing", "User not found."));
```

## Extensions in the meta-package

The assembly itself adds a few helpers that combine the bundled packages:

```csharp
using System.Text.Json;
using CSharpEssentials.Json;          // JsonExtensions
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Time;          // TimeExtensions

using JsonDocument doc = JsonDocument.Parse("""{"a":{"b":5},"userName":"x"}""");

Result<JsonElement> name = doc.RootElement.TryGetProperty("user_name", "userName"); // the first name that exists
Result<JsonElement?> five = doc.TryGetNestedProperty("a", "b");                     // walks the names as a path

long? timestamp = 1_700_000_000_000;
Maybe<DateTime> date = timestamp.MsToDateTime();                                    // None for null, or the default you pass
```

`TryGetProperty` and `TryGetNestedProperty` return a failed `Result` (`PropertyNotFound` as the error code when no name matches, `NoPropertyNames` when you pass none, `DocumentIsNull` for a null document) instead of throwing.
