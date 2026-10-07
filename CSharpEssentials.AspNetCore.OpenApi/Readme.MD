# CSharpEssentials.AspNetCore.OpenApi

`Microsoft.AspNetCore.OpenApi` integration for the CSharpEssentials enum conventions.

> One host, one OpenAPI package: reference either `CSharpEssentials.AspNetCore.OpenApi` (`Microsoft.AspNetCore.OpenApi`) or `CSharpEssentials.AspNetCore.Swashbuckle` (Swashbuckle), never both. Both use `Microsoft.OpenApi` 2.x.

## Installation

```bash
dotnet add package CSharpEssentials.AspNetCore.OpenApi
```

Targets net10.0 with `Microsoft.AspNetCore.OpenApi` 10.x and `Microsoft.OpenApi` 2.x. A net11.0 target with `Microsoft.AspNetCore.OpenApi` 11.x needs `Microsoft.OpenApi` 3.x and will come later as a non-breaking addition.

## Enum schemas

```csharp
builder.Services.AddEnumConventions();
builder.Services.AddOpenApi("v1", o => o.AddEnumConventions());
builder.Services.AddOpenApi("v2", o => o.AddEnumConventions());

app.MapOpenApi();
```

`AddEnumConventions` adds a schema, an operation and a document transformer. Enums handled by the enum conventions (`[StringEnum]` or generated metadata) are described the way they are written:

```json
"OrderStatus": {
  "type": "string",
  "description": "Order lifecycle state.\n\n| value | number | description |\n|---|---|---|\n| `pending` | 0 | Created, waiting for payment. |\n| `unknown` | 99 | Response only: a value this API version does not know. |",
  "enum": [ "pending", "unknown" ],
  "x-enum-varnames": [ "Pending", "Unknown" ],
  "x-enum-descriptions": [ "Created, waiting for payment.", "Response only: a value this API version does not know." ],
  "x-enum-numeric-values": [ 0, 99 ]
}
```

- One component per enum. Your description (XML `<summary>` or `[Description]`) is kept and the value table is appended. Obsolete members are marked `(deprecated)`, the `[EnumFallback]` member `Response only:`. Aliases are accepted but not listed.
- `default` is the wire name (`[DefaultValue]` on a property, a default parameter value).
- `[Flags]` enums are arrays with `uniqueItems: true`; collections are arrays of the component.
- Nullable is written where the enum is used, by the document's OpenAPI version: `allOf` + `nullable: true` for 3.0, `oneOf` with `{ "type": "null" }` for 3.1.
- A document whose operations all write numbers (`WithEnumWireFormat(EnumWireFormat.Number)` on the group, `[EnumWireFormat]` on the controller) describes the enums as `integer` (`int32`/`int64` by underlying type). A document that mixes formats describes strings and marks each number operation with `x-enum-wire-format: number` and a description note; it also logs one warning per enum used in both formats (once per document). One document per API version never mixes.
- Operations with a header selected format (`WithEnumWireFormat("X-Enum-Format", ...)`) show the format of a request without the header, plus `x-enum-wire-format-header` and a note. A selector that throws for such a request is described with the global `WriteAs` format and logged once as a warning.
- Enums without `[StringEnum]` or metadata keep the framework schema; nothing throws.

The Swashbuckle package produces the same enum schemas (both are compared with the same golden files).

## Optional route parameters

OpenAPI requires every path parameter to be `required: true`, so a route with an optional segment (`{id?}`, `{id:int?}`, `{page=1}`) cannot be described on one path. `AddOptionalRouteParameters` (from 6.2.0, opt-in) describes it on several paths, one per form:

```csharp
using CSharpEssentials.AspNetCore; // AddOptionalRouteParameters, OpenApiOptionalRouteParameterMode

builder.Services.AddOpenApi("v1", o => o.AddOptionalRouteParameters());
```

```csharp
[HttpGet("api/archive/{year?}/{month?}", Name = "GetArchive")]
```

| Path | operationId |
|------|-------------|
| `/api/archive` | `GetArchiveWithoutYearAndMonth` |
| `/api/archive/{year}` | `GetArchiveWithoutMonth` |
| `/api/archive/{year}/{month}` | `GetArchive` |

- Each form is a full copy of the operation (tags, security, request body, responses, extensions) without the omitted parameters; every path parameter is `required: true`, without `allowEmptyValue`, a `null` type or default, or a `{"type": "null"}` member in `oneOf` or `anyOf` (a nullable enum such as `{status?}` in OpenAPI 3.1).
- Only optional parameters at the end of the route are split, up to three. An optional parameter followed by a required segment (`api/{tenant?}/orders`), a route with more than three trailing optional parameters and a catch-all parameter (`{*rest}`, `{**path}`) keep one path with the parameters marked required.
- A route with more than three trailing optional parameters is logged as a warning (once per document, path and method) through the host's `ILoggerFactory`. Without an `ILoggerFactory` nothing is logged.
- A form whose path and method another operation already has (`[HttpGet("api/orders")]` next to `[HttpGet("api/orders/{id?}")]`) is skipped; the other operation stays.
- The full form keeps the operationId; a shorter form gets `{operationId}Without{Param}` (`And` between parameters), or none when the full form has none. A generated operationId that another operation already uses throws `InvalidOperationException` when the document is generated; rename one of them or pass a selector.
- Controller and Minimal API routes are handled the same way, on the paths `Microsoft.AspNetCore.OpenApi` writes (`/minimal/{id}` for `/minimal/{id:int?}`).

```csharp
// Split paths with your own operationIds for the shorter forms
builder.Services.AddOpenApi("v1", o => o.AddOptionalRouteParameters(
    operationIdSelector: (operationId, omitted) => $"{operationId}_{string.Join("_", omitted)}"));

// Or keep one path per operation and only mark the parameters required
builder.Services.AddOpenApi("v1", o => o.AddOptionalRouteParameters(OpenApiOptionalRouteParameterMode.RequiredOnly));
```

A later call on the same options replaces the mode and selector of an earlier one; the transformer keeps the pipeline position of the first call. Each document is described from its own operations, so documents split by `GroupName` (`[ApiExplorerSettings(GroupName = "v2")]`) only get the forms of their own routes. Without `AddOptionalRouteParameters`, the document keeps the framework output: one path per operation (`/api/archive/{year}/{month}`). The Swashbuckle package describes the same routes the same way.

## Scalar

No helper is needed; list the documents:

```csharp
app.MapOpenApi();
app.MapScalarApiReference(o => o.AddDocument("v1").AddDocument("v2"));
```

## Hosts without the Web SDK

`Microsoft.AspNetCore.OpenApi` intercepts `AddOpenApi` with generated code. A project on `Microsoft.NET.Sdk` (instead of `Microsoft.NET.Sdk.Web`) enables it:

```xml
<InterceptorsNamespaces>$(InterceptorsNamespaces);Microsoft.AspNetCore.OpenApi.Generated</InterceptorsNamespaces>
```
