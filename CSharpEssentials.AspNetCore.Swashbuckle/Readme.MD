# CSharpEssentials.AspNetCore.Swashbuckle

Swashbuckle (Swagger) integration for `CSharpEssentials.AspNetCore`: versioned Swagger setup, schema ids and security schemes.

> One host, one OpenAPI package: reference either `CSharpEssentials.AspNetCore.Swashbuckle` (Swashbuckle) or `CSharpEssentials.AspNetCore.OpenApi` (`Microsoft.AspNetCore.OpenApi`), never both. Both use `Microsoft.OpenApi` 2.x.

## Installation

```bash
dotnet add package CSharpEssentials.AspNetCore.Swashbuckle
```

Targets net8.0, net9.0, net10.0 and net11.0 with `Swashbuckle.AspNetCore` 10.2.3 or later (below 11) and `Microsoft.OpenApi` 2.x (2.7.5 or later) on every target. Filters and `MapType` factories you add next to these use the Microsoft.OpenApi 2.x model (`using Microsoft.OpenApi;`, `IOpenApiSchema`, `JsonSchemaType`, `JsonNode`).

## Swagger with Versioning

```csharp
using CSharpEssentials.AspNetCore; // AddSwagger, DefaultConfigureSwaggerOptions, SecuritySchemes, UseVersionableSwagger

builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(
    SecuritySchemes.JwtBearerSchemeName,
    SecuritySchemes.JwtBearerTokenSecurity);

var app = builder.Build();
app.UseVersionableSwagger();
```

The first argument is the id of the security scheme in `components.securitySchemes`; every operation requires it. `AddSwagger` includes the XML documentation file of every assembly that declares an endpoint (controllers and Minimal API handlers), plus the optional `assembly` argument. Missing files are skipped.

## Optional route parameters

OpenAPI requires every path parameter to be `required: true`, so a route with an optional segment (`{id?}`, `{id:int?}`, `{page=1}`) cannot be described on one path. `AddSwagger` describes it on several paths, one per form:

```csharp
[HttpGet("api/archive/{year?}/{month?}", Name = "GetArchive")]
```

| Path | operationId |
|------|-------------|
| `/api/archive` | `GetArchiveWithoutYearAndMonth` |
| `/api/archive/{year}` | `GetArchiveWithoutMonth` |
| `/api/archive/{year}/{month}` | `GetArchive` |

- Each form is a full copy of the operation (tags, security, request body, responses, extensions) without the omitted parameters; every path parameter is `required: true`, without `allowEmptyValue` or a `null` default.
- Only optional parameters at the end of the route are split, up to three. An optional parameter followed by a required segment (`api/{tenant?}/orders`), a route with more than three trailing optional parameters and a catch-all parameter (`{*rest}`, `{**path}`) keep one path with the parameters marked required.
- A route with more than three trailing optional parameters is logged as a warning (once per document, path and method) through the host's `ILoggerFactory`, so the missing shorter forms do not go unnoticed. Without an `ILoggerFactory` nothing is logged.
- A form whose path and method another operation already has (`[HttpGet("api/orders")]` next to `[HttpGet("api/orders/{id?}")]`) is skipped; the other operation stays.
- The full form keeps the operationId; a shorter form gets `{operationId}Without{Param}` (`And` between parameters), or none when the full form has none. A generated operationId that another operation already uses throws `InvalidOperationException` when the document is generated; rename one of them or pass a selector.
- Controller and Minimal API routes are handled the same way.
- Forms are found by matching the trailing `{name}` segments of the path that `SwaggerGenOptions.PathGroupSelector` returns; a custom selector whose paths do not end with those segments falls back to required-only.

```csharp
using CSharpEssentials.AspNetCore; // AddOptionalRouteParameters, OptionalRouteParameterMode
using Swashbuckle.AspNetCore.SwaggerGen; // SwaggerGenOptions

// Split paths with your own operationIds for the shorter forms
builder.Services.Configure<SwaggerGenOptions>(o => o.AddOptionalRouteParameters(
    operationIdSelector: (operationId, omitted) => $"{operationId}_{string.Join("_", omitted)}"));

// Or keep one path per operation and only mark the parameters required
builder.Services.Configure<SwaggerGenOptions>(o =>
    o.AddOptionalRouteParameters(OptionalRouteParameterMode.RequiredOnly));
```

`AddOptionalRouteParameters` replaces the earlier setting, so call it after `AddSwagger`. `OptionalRouteParameterMode.LegacyNonCompliant` (obsolete) restores the output of 6.0.0 and earlier: `required: false`, `allowEmptyValue: true` and a nullable schema with a `null` default on the path parameter, which is not valid OpenAPI. See [docs/migration/v6-optional-route-parameters.md](../docs/migration/v6-optional-route-parameters.md).

## Enum schemas

```csharp
using CSharpEssentials.AspNetCore; // AddEnumConventions for IServiceCollection and SwaggerGenOptions

builder.Services.AddEnumConventions();
builder.Services.AddSwaggerGen(o => o.AddEnumConventions()); // AddSwagger calls it
```

Enums handled by the enum conventions (`[StringEnum]` or generated metadata) are described the way they are written: one component per enum with the wire names in `enum`, `x-enum-varnames`, `x-enum-descriptions`, `x-enum-numeric-values` and a value table appended to your description (obsolete members `(deprecated)`, the `[EnumFallback]` member `Response only:`). `default` is the wire name, `[Flags]` enums are arrays with `uniqueItems: true`, and nullable is written where the enum is used (`allOf` + `nullable: true`, OpenAPI 3.0). Keep OpenAPI 3.0, the Swashbuckle default. The enum filters always write the 3.0 shape (a filter cannot see the version `UseSwagger` serializes), and with `UseSwagger(o => o.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1)` a nullable enum usage, and (with `OptionalRouteParameterMode.LegacyNonCompliant`) an optional route parameter whose schema is a `$ref`, is serialized as `{"type": "null", "allOf": [{"$ref": ...}]}`: no value matches both `type: "null"` and the referenced component, so the schema is unsatisfiable and validators and client generators reject `null` and every enum value.

A document whose operations all write numbers describes the enums as `integer`; a document that mixes formats describes strings and marks each number operation with `x-enum-wire-format: number` and a note; it also logs one warning per enum used in both formats (once per document). Header selected operations get `x-enum-wire-format-header` and a note; a selector that throws for a request without the header is described with the global `WriteAs` format and logged once as a warning. Enums without `[StringEnum]` or metadata keep the Swashbuckle schema.

The schemas match those of `CSharpEssentials.AspNetCore.OpenApi` (both are compared with the same golden files).

### From 5.x

5.x pinned `Swashbuckle.AspNetCore` 8.x/9.x and `Microsoft.OpenApi` 1.x. 6.0 needs `Swashbuckle.AspNetCore` 10 and `Microsoft.OpenApi` 2.x on every target:

- `AddSwagger<T>(securityScheme, assembly)` is now `AddSwagger<T>(securitySchemeName, securityScheme, assembly)`: Microsoft.OpenApi 2.x has no `Reference` on `OpenApiSecurityScheme`, so the scheme id is an argument. `SecuritySchemes.JwtBearerSchemeName` is `"Bearer"`, the id 5.x used.
- `SecuritySchemes.JwtBearerTokenSecurity` no longer sets `Reference`.
- Your own Swashbuckle filters move to the Swashbuckle 10 / Microsoft.OpenApi 2.x API: `using Microsoft.OpenApi;` (no `.Models`/`.Any`), `ISchemaFilter.Apply(IOpenApiSchema, ...)`, `JsonSchemaType` instead of type strings, `JsonNode` instead of `IOpenApiAny`, `JsonNodeExtension` for extensions.
- `EnumSchemaFilter` is `sealed` and its `Apply` takes `IOpenApiSchema`.
- A `ConfigureSwaggerOptions` subclass imports `OpenApiInfo` and `OpenApiLicense` from `Microsoft.OpenApi` (was `Microsoft.OpenApi.Models`).
- An optional route parameter (`{id?}`) whose schema is a `$ref` is written as `allOf: [$ref]` + `nullable: true`, since a `$ref` cannot carry siblings in OpenAPI 3.0. From 6.1.0 this applies to `OptionalRouteParameterMode.LegacyNonCompliant` only; see [Optional route parameters](#optional-route-parameters).
- An enum property with its own XML `<summary>` is written as `allOf: [$ref]` + `description`; a property without one stays a plain `$ref`.

See [docs/migration/v5-to-v6.md](../docs/migration/v5-to-v6.md) for the full list.

### From 4.x

The Swagger code moved here from `CSharpEssentials.AspNetCore`; namespaces and the `AddSwagger` API are unchanged. `EnumSchemaFilter` now takes an `IServiceProvider` (Swashbuckle creates it); register it with `AddEnumConventions()` instead of `SchemaFilter<EnumSchemaFilter>()`.
