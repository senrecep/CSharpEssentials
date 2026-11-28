# CSharpEssentials.AspNetCore.Swashbuckle

Swashbuckle setup for CSharpEssentials.AspNetCore: one Swagger document per API version, stable schema ids, XML comments, and filters that describe `[StringEnum]` enums, idempotent endpoints, optional route parameters and bearer security. Use it when your app serves Swagger with Swashbuckle.

A host uses this package or [CSharpEssentials.AspNetCore.OpenApi](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.OpenApi/Readme.MD), never both. Both write the same schemas (they are tested against the same golden files).

Document generation benchmarks (50 Minimal API endpoints, 30 enums, plain `AddSwaggerGen` against the CSE conventions and the full `AddSwagger` setup) are in [BENCHMARKS.md](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore.Swashbuckle/BENCHMARKS.md).

## Installation

```bash
dotnet add package CSharpEssentials.AspNetCore.Swashbuckle
```

Targets net8.0 to net11.0 with `Swashbuckle.AspNetCore` 10.2.3 or later (below 11) and `Microsoft.OpenApi` 2.7.5 or later (below 3). Your own filters use the Microsoft.OpenApi 2.x model (`using Microsoft.OpenApi;`, `IOpenApiSchema`, `JsonSchemaType`, `JsonNode`).

Swashbuckle builds documents with reflection, so this package is not Native AOT compatible.

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.AspNetCore.Swashbuckle
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;
using Swashbuckle.AspNetCore.SwaggerGen;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddEnumConventions();
builder.Services.AddIdempotency();
builder.Services.AddAndConfigureApiVersioning();

builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>();
builder.Services.Configure<SwaggerGenOptions>(o => o.AddBearerSecurityScheme());

WebApplication app = builder.Build();

app.UseRouting();
app.UseEnumBinding();
app.UseIdempotency();
app.UseVersionableSwagger();                         // /swagger and /swagger/v1/swagger.json

RouteGroupBuilder v1 = app.MapVersionedGroup(1);
v1.MapGet("/orders", (OrderStatus? status) => Array.Empty<Order>());
v1.MapPost("/orders", (Order order) => Results.Created($"/v1/orders/{order.Id}", order))
    .WithIdempotency()
    .RequireAuthorization();

app.Run();

[StringEnum]
public enum OrderStatus { Pending, PendingApproval }

public sealed record Order(int Id, OrderStatus Status);
```

## AddSwagger

```csharp
services.AddSwagger<TConfigureSwaggerOptions>(Assembly? assembly = null);
services.AddSwagger<TConfigureSwaggerOptions>(string securitySchemeName, OpenApiSecurityScheme securityScheme, Assembly? assembly = null);
```

Both overloads call `AddEndpointsApiExplorer()` and `AddSwaggerGen()` and then:

- build schema ids with `SwashbuckleSchemaIdFactory` (namespace-qualified, see below);
- add the enum conventions and the undefined-default fix (`AddSwaggerEnumConventions()`);
- add the idempotency conventions (`AddIdempotencyConventions()`);
- include the XML documentation file of the entry assembly, of every assembly that declares an endpoint, and of `assembly`. Missing files are skipped;
- apply `TConfigureSwaggerOptions`.

The second overload also registers the given scheme and a document-level requirement for it, so every operation requires it, anonymous ones included. `SecuritySchemes.JwtBearerSchemeName` (`"Bearer"`) and `SecuritySchemes.JwtBearerTokenSecurity` (an HTTP bearer scheme) are ready-made arguments:

```csharp
builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(
    SecuritySchemes.JwtBearerSchemeName,
    SecuritySchemes.JwtBearerTokenSecurity);
```

To require the token only where the endpoint needs authorization, use the first overload with [AddBearerSecurityScheme](#bearer-security) instead. Do not combine the two.

## Versioned documents and Swagger UI

`DefaultConfigureSwaggerOptions` creates one `SwaggerDoc` per API version reported by `Asp.Versioning` (`AddAndConfigureApiVersioning` from CSharpEssentials.AspNetCore), or a single `v1` without versioning. It reads configuration:

| Key | Default |
|---|---|
| `Swagger:Title` | `API` |
| `Swagger:Description` | `API Description`. With versioning: `[{environment}] {version} {description}`, plus `This API version has been deprecated` for a deprecated version |
| `Swagger:License` | `API License` |
| `Swagger:LicenseUrl` | `https://opensource.org/license/mit` |

It also keeps the first action when two actions conflict (`ResolveConflictingActions`), calls `AddOptionalRouteParameters()`, and maps `TimeSpan` and `TimeOnly` to `type: string` with the example `00:00:00`.

`app.UseVersionableSwagger(Action<SwaggerOptions>? options = null, Action<SwaggerUIOptions>? uiOptions = null)` serves `/swagger/{documentName}/swagger.json` and a Swagger UI with one entry per version, titled `{Swagger:Title} {GROUP}`.

To change the documents, derive from the abstract `ConfigureSwaggerOptions` and pass your class to `AddSwagger<T>`. Its constructor takes `IServiceProvider`, `IHostEnvironment` and `IConfiguration`; `Configure(SwaggerGenOptions)` and `Configure(string?, SwaggerGenOptions)` are virtual.

```csharp
using CSharpEssentials.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

public sealed class MySwaggerOptions(IServiceProvider services, IHostEnvironment environment, IConfiguration configuration)
    : ConfigureSwaggerOptions(services, environment, configuration)
{
    public override void Configure(SwaggerGenOptions options)
    {
        base.Configure(options);
        options.SupportNonNullableReferenceTypes();
    }
}
```

## Schema ids

`SwashbuckleSchemaIdFactory` (namespace `CSharpEssentials.AspNetCore.Swagger.Filters`) builds ids from the namespace-qualified type name with generic arguments in front: `NsB.Order`, and `NsB.OrderSystem.Collections.Generic.List` for `List<NsB.Order>`. The ids do not depend on the order in which types are seen.

For short ids (the 7.x output), set `QualifyWithNamespace = false` after `AddSwagger`:

```csharp
builder.Services.Configure<SwaggerGenOptions>(o =>
    o.CustomSchemaIds(new SwashbuckleSchemaIdFactory { QualifyWithNamespace = false }.GetSchemaId));
```

Short ids use the type name (`Box<Dto>` becomes `DtoBox`) and add a numeric suffix when two types share a name. That suffix follows discovery order, so `Order` and `Order1` can swap between runs.

## Enum schemas

`SwaggerGenOptions.AddEnumConventions()` registers the public `EnumSchemaFilter` and an operation filter. Enums that the enum conventions handle (`[StringEnum]` or generator metadata) get one component each with the wire names in `enum`, plus `x-enum-varnames`, `x-enum-descriptions`, `x-enum-numeric-values` and a value table appended to your description. Obsolete members are marked `(deprecated)`, the `[EnumFallback]` member `Response only:`. `default` is the wire name, `[Flags]` enums are arrays with `uniqueItems: true`, and nullable is written where the enum is used (`allOf` plus `nullable: true`).

```csharp
builder.Services.AddEnumConventions();              // runtime: JSON and binding
builder.Services.AddSwaggerGen(o => o.AddEnumConventions());
// or, without AddSwagger, also removing undefined enum defaults:
builder.Services.AddSwaggerEnumConventions();
```

- Keep OpenAPI 3.0, the Swashbuckle default. The filters write the 3.0 nullable shape. Serialized as 3.1 (`UseSwagger(o => o.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1)`), a nullable enum becomes `{"type": "null", "allOf": [{"$ref": ...}]}`, which no value matches.
- An undefined default such as `[DefaultValue((OrderStatus)42)]` is left out by `AddSwagger` and by `services.AddSwaggerEnumConventions()`, which wrap the registered `ISerializerDataContractResolver`. Call `AddSwaggerEnumConventions()` after `AddSwaggerGen` and after registering your own resolver; calling it twice registers once. A resolver registered by type (`AddSingleton<ISerializerDataContractResolver, MyResolver>()`) is not wrapped; register it as an instance or a factory.
- A document whose operations all write numbers describes the enums as `integer`. A mixed document uses strings, marks number operations with `x-enum-wire-format: number` and logs one warning per enum used in both formats. Header-selected operations get `x-enum-wire-format-header`.
- Enums without `[StringEnum]` or metadata keep the Swashbuckle schema.
- The schemas match those of CSharpEssentials.AspNetCore.OpenApi. Its schema ids are short, so the `$ref` values match only with `QualifyWithNamespace = false`.

## Idempotency

`SwaggerGenOptions.AddIdempotencyConventions()` (called by `AddSwagger`) describes endpoints marked with `.WithIdempotency()` or `[Idempotent]` whose method is in `IdempotencyOptions.Methods`. In `Enforce` mode each gets:

- the `Idempotency-Key` header parameter, `required` when the endpoint's `RequireKey` resolves to true;
- the `Idempotency-Replayed` header on every 2xx response;
- `400` (`Idempotency.KeyMissing`, `Idempotency.KeyInvalid`, `Idempotency.ScopeRequired`), `409` (`Idempotency.InFlight`, with `Retry-After`) and `422` (`Idempotency.KeyReused`) as `application/problem+json` with a `code` enum.

`Observe` endpoints get only the key header with `required: false`. `Off` endpoints and other methods are unchanged. An existing 400, 409 or 422 keeps its description; the idempotency text is appended and the content becomes a `oneOf`.

## Optional route parameters

OpenAPI requires every path parameter to be required. `AddSwagger` (through `DefaultConfigureSwaggerOptions`) writes a route with optional segments as one path per form. For `api/archive/{year?}/{month?}` named `GetArchive`:

| Path | operationId |
|------|-------------|
| `/api/archive` | `GetArchiveWithoutYearAndMonth` |
| `/api/archive/{year}` | `GetArchiveWithoutMonth` |
| `/api/archive/{year}/{month}` | `GetArchive` |

```csharp
// Your own operationIds for the shorter forms
builder.Services.Configure<SwaggerGenOptions>(o => o.AddOptionalRouteParameters(
    operationIdSelector: (operationId, omitted) => $"{operationId}_{string.Join("_", omitted)}"));

// One path per operation; the parameters are only marked required
builder.Services.Configure<SwaggerGenOptions>(o => o.AddOptionalRouteParameters(OptionalRouteParameterMode.RequiredOnly));
```

- `OptionalRouteParameterMode`: `SplitPaths` (default), `RequiredOnly`, and the obsolete `LegacyNonCompliant`, which writes `required: false` and `allowEmptyValue: true` on path parameters (invalid OpenAPI, the 6.0.0 output; it uses the public `ReApplyOptionalRouteParameterOperationFilter`).
- `AddOptionalRouteParameters` replaces the earlier setting, so call it after `AddSwagger`.
- Only trailing optional parameters are split, up to three. Optional parameters before a required segment, more than three, or a catch-all keep one path with the parameters marked required.
- A form whose path and method another operation already has is skipped. A generated operationId that is already used throws `InvalidOperationException` at document generation.
- Forms are found by matching the trailing `{name}` segments of the path from `SwaggerGenOptions.PathGroupSelector`. A custom selector whose paths do not end with them falls back to required-only.

## Bearer security

`SwaggerGenOptions.AddBearerSecurityScheme(schemeName = "Bearer", bearerFormat = "JWT", description = null)` adds an HTTP bearer scheme and a requirement on the operations that need authorization.

```csharp
builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>();
builder.Services.Configure<SwaggerGenOptions>(o => o.AddBearerSecurityScheme());
// without AddSwagger:
builder.Services.AddSwaggerGen(o => o.AddBearerSecurityScheme());

app.MapGet("/orders", () => "...").RequireAuthorization();                 // security: [{ "Bearer": [] }]
app.MapGet("/health", () => "ok");                                         // no security
app.MapGet("/status", () => "ok").RequireAuthorization().AllowAnonymous(); // no security
```

- An operation gets the requirement when its endpoint metadata has `IAuthorizeData` (`[Authorize]`, any `RequireAuthorization` overload on the endpoint or group) and no `IAllowAnonymous`.
- `AuthenticationSchemes` is not checked: an endpoint that accepts only an API key or a cookie is still described as needing the bearer token.
- Endpoints protected only by the authorization `FallbackPolicy` carry no metadata and are not marked.
- A scheme with the same name, also one added with `AddSecurityDefinition`, is replaced. An operation is never given the requirement twice.

## Upgrading

- 8.0: schema ids are namespace-qualified by default (`MyApp.Contracts.Order` instead of `Order`). Regenerate clients, or switch back as shown in [Schema ids](#schema-ids).
- 6.0: Swashbuckle 10 and Microsoft.OpenApi 2.x. `AddSwagger<T>(securityScheme, assembly)` became `AddSwagger<T>(securitySchemeName, securityScheme, assembly)`. See [v5-to-v6.md](https://github.com/senrecep/CSharpEssentials/blob/main/docs/migration/v5-to-v6.md) and [v6-optional-route-parameters.md](https://github.com/senrecep/CSharpEssentials/blob/main/docs/migration/v6-optional-route-parameters.md).
- 5.0: the Swagger code moved here from CSharpEssentials.AspNetCore with the same namespaces. Register enum schemas with `AddEnumConventions()` instead of `SchemaFilter<EnumSchemaFilter>()`. See [v4-to-v5.md](https://github.com/senrecep/CSharpEssentials/blob/main/docs/migration/v4-to-v5.md).

## See also

- Example: [examples/Examples.AspNetCore.Swashbuckle](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.AspNetCore.Swashbuckle) prints the generated documents.
- Agent skill: [csharpessentials-aspnetcore](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-aspnetcore/SKILL.md)
- [CSharpEssentials.AspNetCore](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD)
