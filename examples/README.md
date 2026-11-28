# Examples

One project per package. Each folder has a README that says what the code shows. Run a console app with `dotnet run`, a test project with `dotnet test` and a web app with `dotnet run` (then open the URL it prints).

```bash
dotnet run --project examples/Examples.Results
dotnet test examples/Examples.Results.Testing
```

The projects reference the local package projects, so they match the source in this repository.

## Console apps

These run offline and exit when done.

| Project | Package |
|---|---|
| [Examples.Main](Examples.Main/README.md) | `CSharpEssentials` (meta-package) |
| [Examples.Results](Examples.Results/README.md) | `CSharpEssentials.Results` |
| [Examples.Errors](Examples.Errors/README.md) | `CSharpEssentials.Errors` |
| [Examples.Maybe](Examples.Maybe/README.md) | `CSharpEssentials.Maybe` |
| [Examples.Any](Examples.Any/README.md) | `CSharpEssentials.Any` |
| [Examples.These](Examples.These/README.md) | `CSharpEssentials.These` |
| [Examples.Rules](Examples.Rules/README.md) | `CSharpEssentials.Rules` |
| [Examples.Validation](Examples.Validation/README.md) | `CSharpEssentials.Validation` |
| [Examples.Core](Examples.Core/README.md) | `CSharpEssentials.Core` |
| [Examples.Enums](Examples.Enums/README.md) | `CSharpEssentials.Enums` |
| [Examples.Json](Examples.Json/README.md) | `CSharpEssentials.Json` |
| [Examples.Time](Examples.Time/README.md) | `CSharpEssentials.Time` |
| [Examples.Clone](Examples.Clone/README.md) | `CSharpEssentials.Clone` |
| [Examples.Entity](Examples.Entity/README.md) | `CSharpEssentials.Entity` |
| [Examples.EntityFrameworkCore](Examples.EntityFrameworkCore/README.md) | `CSharpEssentials.EntityFrameworkCore` (SQLite in memory) |
| [Examples.EntityFrameworkCore.Operations](Examples.EntityFrameworkCore.Operations/README.md) | `CSharpEssentials.EntityFrameworkCore.HealthChecks`, `.Hosting` and `.Locking` |
| [Examples.DependencyInjection](Examples.DependencyInjection/README.md) | `CSharpEssentials.DependencyInjection` |
| [Examples.Mediator](Examples.Mediator/README.md) | `CSharpEssentials.Mediator` |
| [Examples.Idempotency](Examples.Idempotency/README.md) | `CSharpEssentials.Idempotency` |
| [Examples.Http](Examples.Http/README.md) | `CSharpEssentials.Http` (a fake handler answers in memory) |
| [Examples.Resilience](Examples.Resilience/README.md) | `CSharpEssentials.Resilience` |
| [Examples.GcpSecretManager](Examples.GcpSecretManager/README.md) | `CSharpEssentials.GcpSecretManager` (the last section needs GCP credentials and runs only when you set `CSE_GCP_PROJECT`) |

## Web apps

| Project | Package |
|---|---|
| [Examples.AspNetCore](Examples.AspNetCore/README.md) | `CSharpEssentials.AspNetCore` |
| [Examples.AspNetCore.OpenApi](Examples.AspNetCore.OpenApi/README.md) | `CSharpEssentials.AspNetCore.OpenApi` |
| [Examples.AspNetCore.Swashbuckle](Examples.AspNetCore.Swashbuckle/README.md) | `CSharpEssentials.AspNetCore.Swashbuckle` |
| [Examples.Endpoints](Examples.Endpoints/README.md) | `CSharpEssentials.Endpoints` and `.DependencyInjection`, published with Native AOT |
| [Examples.RequestResponseLogging](Examples.RequestResponseLogging/README.md) | `CSharpEssentials.RequestResponseLogging` |
| [Examples.Enums.EndToEnd](Examples.Enums.EndToEnd/README.md) | Enum conventions across JSON, binding, OpenAPI and EF Core (needs PostgreSQL, for example in Docker) |

## Test projects

| Project | Package |
|---|---|
| [Examples.Results.Testing](Examples.Results.Testing/README.md) | `CSharpEssentials.Results.Testing` |
| [Examples.Mediator.Testing](Examples.Mediator.Testing/README.md) | `CSharpEssentials.Mediator.Testing` |
| [Examples.Idempotency.Testing](Examples.Idempotency.Testing/README.md) | `CSharpEssentials.Idempotency.Testing` |
| [Examples.Idempotency.EndToEnd](Examples.Idempotency.EndToEnd/README.md) | Idempotency across HTTP, Mediator, messaging and EF Core (needs Docker) |
| [Examples.Idempotency.Redis](Examples.Idempotency.Redis/README.md) | A Redis `IIdempotencyRecordStore` checked with the contract tests (needs Docker) |
