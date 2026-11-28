# CSharpEssentials.AspNetCore.Swashbuckle example

Serves versioned Swagger documents with `AddSwagger` and `UseVersionableSwagger` from [CSharpEssentials.AspNetCore.Swashbuckle](../../CSharpEssentials.AspNetCore.Swashbuckle/Readme.MD), then prints both documents.

## Run

```bash
cd examples/Examples.AspNetCore.Swashbuckle
dotnet run              # prints /swagger/v1/swagger.json and /swagger/v2/swagger.json, then exits
dotnet run -- --serve   # keeps the app running; open http://localhost:5000/swagger
```

## What to look for

| Call in `Program.cs` | In the documents |
|---|---|
| `AddAndConfigureApiVersioning()`, `MapVersionedGroup(1)` and `(2)` | One document per version: `v1` has the orders endpoints, `v2` has `/v2/health` |
| `AddSwagger<DefaultConfigureSwaggerOptions>()` with `Swagger:Title` and `Swagger:Description` in `appsettings.json` | `info.title` is `Orders API`, `info.description` is `[Production] 1 Swashbuckle with the CSharpEssentials conventions.` |
| `AddSwagger` (schema ids) | Namespace-qualified ids such as `Examples.AspNetCore.Swashbuckle.OrderStatus` |
| `AddSwagger` (enum conventions) | The `OrderStatus` schema uses the wire names |
| `AddSwagger` (idempotency conventions), `AddIdempotency(o => o.RequireKey = true)` | `POST /v1/orders` has a required `Idempotency-Key` header and 400, 409 and 422 responses |
| `AddSwagger` (optional route parameters) | `/v1/orders/archive` (`GetArchiveWithoutYear`) and `/v1/orders/archive/{year}` (`GetArchive`) |
| `AddBearerSecurityScheme(...)` | `Bearer` scheme; only `POST` and `PUT /v1/orders` require it |
| `WithIfMatch(required: true)` | `PUT /v1/orders/{id}` lists 400, 412 and 428 |
