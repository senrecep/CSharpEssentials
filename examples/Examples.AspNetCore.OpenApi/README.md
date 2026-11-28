# CSharpEssentials.AspNetCore.OpenApi example

Builds an OpenAPI document with `Microsoft.AspNetCore.OpenApi` and the transformers from [CSharpEssentials.AspNetCore.OpenApi](../../CSharpEssentials.AspNetCore.OpenApi/Readme.MD), then prints it.

## Run

```bash
cd examples/Examples.AspNetCore.OpenApi
dotnet run              # prints /openapi/v1.json and exits
dotnet run -- --serve   # keeps the app running; open http://localhost:5000/openapi/v1.json
```

The project file enables `Microsoft.AspNetCore.OpenApi.Generated` in `InterceptorsNamespaces`. Without it the build fails with `CS9137`, because `Microsoft.AspNetCore.OpenApi` is only a transitive reference.

## What to look for in the document

| Call in `Program.cs` | Endpoint | In the document |
|---|---|---|
| `AddEnumConventions()` | `GET /orders?status=` | `OrderStatus` component with `pending`, `pending_approval`, `unknown`, `x-enum-varnames` and a value table; `unknown` is marked `Response only:` |
| `AddIdempotencyConventions()` with `RequireKey = true` | `POST /orders` | Required `Idempotency-Key` header, 400, 409 and 422 problem responses |
| `AddOptionalRouteParameters()` | `GET /orders/archive/{year:int?}` named `GetArchive` | Two paths: `/orders/archive` (`GetArchiveWithoutYear`) and `/orders/archive/{year}` (`GetArchive`) |
| `AddBearerSecurityScheme(description: ...)` | `POST` and `PUT /orders` with `RequireAuthorization()` | `components.securitySchemes.Bearer` and `security: [{ "Bearer": [] }]` on those two only |
| `WithIfMatch(required: true)` from CSharpEssentials.AspNetCore | `PUT /orders/{id}` | 400, 412 and 428 problem responses |

The enum types are in `Orders.cs`.
