# CSharpEssentials.AspNetCore.OpenApi

Transformers for `Microsoft.AspNetCore.OpenApi` that describe the CSharpEssentials.AspNetCore conventions in the OpenAPI document: `[StringEnum]` enums by their wire names, idempotent endpoints, optional route parameters and a bearer security scheme. Use it when your app builds its document with `AddOpenApi` and uses CSharpEssentials.AspNetCore.

A host uses this package or [CSharpEssentials.AspNetCore.Swashbuckle](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.Swashbuckle/Readme.MD), never both. Both write the same schemas (they are tested against the same golden files).

Document generation benchmarks (50 Minimal API endpoints, 30 enums, plain `AddOpenApi` against the CSE conventions) are in [BENCHMARKS.md](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.OpenApi/BENCHMARKS.md).

## Installation

```bash
dotnet add package CSharpEssentials.AspNetCore.OpenApi
```

Targets net10.0 with `Microsoft.AspNetCore.OpenApi` 10.x and `Microsoft.OpenApi` 2.x.

`Microsoft.AspNetCore.OpenApi` generates interceptors for XML comments, and its build file turns that on only in a project that references it directly. Since this package brings it in transitively, a host without a direct reference fails with `CS9137`. Add a direct reference, or enable the namespace in the host project:

```xml
<InterceptorsNamespaces>$(InterceptorsNamespaces);Microsoft.AspNetCore.OpenApi.Generated</InterceptorsNamespaces>
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.AspNetCore.OpenApi
// dotnet add package Microsoft.AspNetCore.OpenApi
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddEnumConventions();
builder.Services.AddIdempotency();

builder.Services.AddOpenApi("v1", o => o
    .AddEnumConventions()
    .AddIdempotencyConventions()
    .AddOptionalRouteParameters()
    .AddBearerSecurityScheme());

WebApplication app = builder.Build();

app.UseRouting();
app.UseEnumBinding();
app.UseIdempotency();
app.MapOpenApi();                                    // GET /openapi/v1.json

app.MapGet("/orders", (OrderStatus? status) => Array.Empty<Order>());
app.MapGet("/orders/archive/{year:int?}", (int? year) => Array.Empty<Order>()).WithName("GetArchive");
app.MapPost("/orders", (Order order) => Results.Created($"/orders/{order.Id}", order))
    .WithIdempotency()
    .RequireAuthorization();

app.Run();

[StringEnum]
public enum OrderStatus { Pending, PendingApproval }

public sealed record Order(int Id, OrderStatus Status);
```

All four methods are extension methods on `OpenApiOptions` in the `CSharpEssentials.AspNetCore` namespace and return the options, so they chain. Call them once per document.

## Enum schemas

`o.AddEnumConventions()` adds a schema, an operation and a document transformer. Enums that the enum conventions handle (`[StringEnum]`, or metadata from the generator) are described the way the API writes them. Register `builder.Services.AddEnumConventions()` as well; it is what makes the API read and write the wire names.

```json
"OrderStatus": {
  "type": "string",
  "description": "Order lifecycle state.\n\n| value | number | description |\n|---|---|---|\n| `pending` | 0 | Created, waiting for payment. |\n| `unknown` | 99 | Response only: A value this API version does not know. |",
  "enum": [ "pending", "unknown" ],
  "x-enum-varnames": [ "Pending", "Unknown" ],
  "x-enum-descriptions": [ "Created, waiting for payment.", "Response only: A value this API version does not know." ],
  "x-enum-numeric-values": [ 0, 99 ]
}
```

- One component per enum. Your description (XML `<summary>` or `[Description]`) is kept and a value table is appended. Obsolete members are marked `(deprecated)`, the `[EnumFallback]` member `Response only:`. Aliases are accepted on input but not listed.
- `default` is written as the wire name (`[DefaultValue]` on a property, a parameter default). An undefined default such as `(OrderStatus)42` is serialized by the framework before any transformer runs; with CSharpEssentials.Json it comes out as a number and the transformer then leaves the default out.
- `[Flags]` enums are arrays with `uniqueItems: true`. Collections are arrays of the component.
- Nullable is written where the enum is used: `allOf` plus `nullable: true` in OpenAPI 3.0, `oneOf` with `{ "type": "null" }` in 3.1.
- A document whose operations all write numbers (`WithEnumWireFormat(EnumWireFormat.Number)` on the group, `[EnumWireFormat]` on the controller) describes the enums as `integer` (`int32` or `int64` by underlying type). A document that mixes formats uses strings, marks each number operation with `x-enum-wire-format: number` and a note, and logs one warning per enum used in both formats. One document per API version avoids the mix.
- Operations whose format comes from a header (`WithEnumWireFormat("X-Enum-Format", ...)`) show the format of a request without the header, plus `x-enum-wire-format-header` and a note. If the selector throws for such a request, the global `WriteAs` format is shown and a warning is logged once.
- Enums without `[StringEnum]` or metadata keep the framework schema.

## Idempotency

`o.AddIdempotencyConventions()` describes endpoints marked with `.WithIdempotency()` or `[Idempotent]` whose method is in `IdempotencyOptions.Methods` (POST and PATCH by default). In `Enforce` mode each such operation gets:

- the `Idempotency-Key` header parameter (`IdempotencyOptions.HeaderName`), `required` when the endpoint's `RequireKey` resolves to true;
- the `Idempotency-Replayed` header (`ReplayedHeaderName`) on every 2xx response;
- `400` (`Idempotency.KeyMissing` when the key is required, `Idempotency.KeyInvalid`, and `Idempotency.ScopeRequired` when unscoped keys are rejected), `409` (`Idempotency.InFlight`, with a `Retry-After` header) and `422` (`Idempotency.KeyReused`), as `application/problem+json` with a `code` enum.

An `Observe` endpoint gets only the key header with `required: false`, so clients can start sending keys before you switch to `Enforce`. `Off` endpoints, other methods and endpoints without idempotency are unchanged. If an endpoint already declares 400, 409 or 422, its description is kept and the idempotency text is appended; the content becomes a `oneOf` of both schemas. Header names are read from the options when the document is generated.

## Optional route parameters

OpenAPI requires every path parameter to be required, so `{id?}`, `{id:int?}` or `{page=1}` cannot be described on one path. `o.AddOptionalRouteParameters()` writes one path per form. For `api/archive/{year?}/{month?}` named `GetArchive`:

| Path | operationId |
|------|-------------|
| `/api/archive` | `GetArchiveWithoutYearAndMonth` |
| `/api/archive/{year}` | `GetArchiveWithoutMonth` |
| `/api/archive/{year}/{month}` | `GetArchive` |

```csharp
// Your own operationIds for the shorter forms
o.AddOptionalRouteParameters(operationIdSelector: (operationId, omitted) => $"{operationId}_{string.Join("_", omitted)}");

// One path per operation; the optional parameters are only marked required
o.AddOptionalRouteParameters(OpenApiOptionalRouteParameterMode.RequiredOnly);
```

- `OpenApiOptionalRouteParameterMode`: `SplitPaths` (default) or `RequiredOnly`.
- Each form is a full copy of the operation without the omitted parameters. Every path parameter is `required: true`, with no `allowEmptyValue`, no `null` type or default, and no `{"type": "null"}` member in `oneOf`/`anyOf`.
- Only trailing optional parameters are split, up to three. A route with an optional parameter before a required segment (`api/{tenant?}/orders`), more than three trailing optional parameters, or a catch-all (`{*rest}`) keeps one path with the parameters marked required. More than three also logs a warning once per document, path and method.
- A form whose path and method another operation already has is skipped.
- The full form keeps the operationId. A shorter form gets `{operationId}Without{Param}` (`And` between names), or none when the full form has none. A generated id that another operation already uses throws `InvalidOperationException` when the document is generated.
- A second call on the same options replaces the mode and selector.
- Without this call the document keeps the framework output (`/api/archive/{year}/{month}` only).

## Bearer security

`o.AddBearerSecurityScheme(schemeName = "Bearer", bearerFormat = "JWT", description = null)` adds an HTTP bearer scheme to `components.securitySchemes` and a requirement for it on operations that need authorization.

```csharp
app.MapGet("/orders", () => "...").RequireAuthorization();                 // security: [{ "Bearer": [] }]
app.MapGet("/health", () => "ok");                                         // no security
app.MapGet("/status", () => "ok").RequireAuthorization().AllowAnonymous(); // no security
```

```json
"securitySchemes": {
  "Bearer": { "type": "http", "scheme": "bearer", "bearerFormat": "JWT" }
}
```

- An operation gets the requirement when its endpoint metadata has `IAuthorizeData` (`[Authorize]` on the action, controller or a base class, or any `RequireAuthorization` overload on the endpoint or its group) and no `IAllowAnonymous`.
- `IAuthorizeData.AuthenticationSchemes` is not checked. An endpoint that accepts only an API key or a cookie is still described as needing the bearer token, so use this only when bearer is the scheme your protected endpoints accept.
- The authorization `FallbackPolicy` adds no endpoint metadata. Endpoints protected only by it are not marked; add `RequireAuthorization()` to them.
- A scheme already registered under the same name is replaced. An operation that already requires the scheme does not get a second requirement.

## Scalar

Scalar (the `Scalar.AspNetCore` package) needs no helper from this package. List the documents:

```csharp
app.MapOpenApi();
app.MapScalarApiReference(o => o.AddDocument("v1").AddDocument("v2"));
```

## See also

- Example: [examples/Examples.AspNetCore.OpenApi](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.AspNetCore.OpenApi) prints the generated document.
- Agent skill: [csharpessentials-aspnetcore](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-aspnetcore/SKILL.md)
- [CSharpEssentials.AspNetCore](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD) for the runtime side: enum binding, idempotency, conditional requests.
- [CSharpEssentials.AspNetCore.Swashbuckle](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.Swashbuckle/Readme.MD), the same conventions for Swashbuckle.
