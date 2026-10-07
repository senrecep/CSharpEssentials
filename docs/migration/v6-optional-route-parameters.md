# Optional route parameters in Swashbuckle documents (6.1.0)

`CSharpEssentials.AspNetCore.Swashbuckle` changes the default document output for routes with optional parameters (issue #101). No code change is needed; the generated OpenAPI document and the clients generated from it change.

## What changes

Up to 6.0.0, `AddSwagger` registered `ReApplyOptionalRouteParameterOperationFilter`, which wrote an optional parameter (`{id?}`) of an MVC action as `required: false`, `allowEmptyValue: true` and a nullable schema with `default: null`. OpenAPI requires path parameters to be `required: true` and allows `allowEmptyValue` only on query parameters, so validators rejected the document.

From 6.1.0, `AddSwagger` uses `OptionalRouteParameterMode.SplitPaths`: the operation is described once per form, without the trailing optional segments and with each of them.

```text
6.0.0:  GET /api/orders/{id}         operationId GetOrder   id: required false, allowEmptyValue, nullable, default null
6.1.0:  GET /api/orders              operationId GetOrderWithoutId
        GET /api/orders/{id}         operationId GetOrder   id: required true
```

- Every path parameter is `required: true`, without `allowEmptyValue` or a `null` default. This also applies to `{id:int?}`, `{page=1}`, optional parameters of a controller-level `[Route]` and Minimal API routes, which 6.0.0 left untouched or only partly handled.
- Only trailing optional parameters are split, up to three; other routes keep one path with the parameters marked required. A catch-all parameter is never split.
- A shorter form whose path and method another operation already has is skipped.
- The shorter forms get the operationId `{operationId}Without{Param}` (`And` between parameters), or none when the full form has none. A generated operationId that is already used throws `InvalidOperationException` when the document is generated.

Clients generated from the document get a method per form (`GetOrderWithoutId`, `GetOrder`).

## Opting out

Call `AddOptionalRouteParameters` after `AddSwagger`; the last call wins.

```csharp
using CSharpEssentials.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;

// One path per operation, parameters required (valid OpenAPI, no new paths or operationIds)
builder.Services.Configure<SwaggerGenOptions>(o => o.AddOptionalRouteParameters(OptionalRouteParameterMode.RequiredOnly));

// Your own operationIds for the shorter forms
builder.Services.Configure<SwaggerGenOptions>(o => o.AddOptionalRouteParameters(
    operationIdSelector: (operationId, omitted) => $"{operationId}_{string.Join("_", omitted)}"));

// The 6.0.0 output (invalid OpenAPI); the member is [Obsolete]
builder.Services.Configure<SwaggerGenOptions>(o => o.AddOptionalRouteParameters(OptionalRouteParameterMode.LegacyNonCompliant));
```

`ReApplyOptionalRouteParameterOperationFilter` stays public and unchanged; `LegacyNonCompliant` registers it. A `ConfigureSwaggerOptions` subclass that registers it itself keeps the old output for those operations, next to the `SplitPaths` document filter that `AddSwagger` adds; call `AddOptionalRouteParameters(OptionalRouteParameterMode.LegacyNonCompliant)` instead.

## Warning for routes that are not split (6.2.0)

From 6.2.0, a route with more than three trailing optional parameters (`api/deep/{a?}/{b?}/{c?}/{d?}`) is logged as a warning in `SplitPaths` mode, once per document, path and method, through the host's `ILoggerFactory`. The route is still described on one path with every parameter required; the warning tells you the shorter forms are missing from the document.

## Microsoft.AspNetCore.OpenApi (6.2.0)

`CSharpEssentials.AspNetCore.OpenApi` gets the same behavior in 6.2.0 as an opt-in document transformer; documents that do not call it keep the framework output.

```csharp
using CSharpEssentials.AspNetCore;

builder.Services.AddOpenApi("v1", o => o.AddOptionalRouteParameters());
builder.Services.AddOpenApi("v2", o => o.AddOptionalRouteParameters(OpenApiOptionalRouteParameterMode.RequiredOnly));
```

The forms, operationIds, collision rules, skipped forms and the warning are the same as in the Swashbuckle package, on the paths `Microsoft.AspNetCore.OpenApi` writes (`/orders/{id}` for `orders/{id:int?}`). The framework already writes path parameters as `required: true`; the transformer adds the shorter forms and removes a `null` type or default from path parameter schemas. There is no legacy mode.
