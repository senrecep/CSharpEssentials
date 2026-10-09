# Migrating from 5.x to 6.0

## Breaking changes

- `CSharpEssentials.AspNetCore.Swashbuckle` needs `Swashbuckle.AspNetCore` `[10.2.3, 11)` and `Microsoft.OpenApi` `[2.7.5, 3)` on every target.
- `AddSwagger<T>(securityScheme, assembly)` is now `AddSwagger<T>(securitySchemeName, securityScheme, assembly)`; `SecuritySchemes.JwtBearerTokenSecurity` no longer sets `Reference`.
- `EnumSchemaFilter` is `sealed`, and `EnumSchemaFilter.Apply` takes `IOpenApiSchema` instead of `OpenApiSchema`.
- `OpenApiInfo`, `OpenApiLicense` and the other model types used by `ConfigureSwaggerOptions` subclasses moved from `Microsoft.OpenApi.Models` to `Microsoft.OpenApi`.
- `ReApplyOptionalRouteParameterOperationFilter` wraps an optional route parameter whose schema is a `$ref` in `allOf`. From 6.1.0 `AddSwagger` no longer registers it ([Optional route parameters](v6-optional-route-parameters.md)); only `OptionalRouteParameterMode.LegacyNonCompliant` does.
- An enum property with its own XML `<summary>` is written as `allOf` + `description`.
- The single-column cursor `PaginateAsync<T, TCursor>` is `[Obsolete]` (CS0618); move to `KeysetPaginateAsync`.
- `IPaginationRequest.Normalize()` and the offset `PaginateAsync`/`Paginate` overloads cap `PageSize` at `PaginationDefaults.MaxPageSize` (100); the synchronous `Paginate` overloads gained a `maxPageSize` parameter, so assemblies compiled against 5.x must be recompiled.

## Swashbuckle 10 and Microsoft.OpenApi 2.x (`CSharpEssentials.AspNetCore.Swashbuckle`)

5.x pinned `Swashbuckle.AspNetCore` `[8.1.0, 10)` with `Microsoft.OpenApi` 1.x. 6.0 needs `Swashbuckle.AspNetCore` `[10.2.3, 11)` and `Microsoft.OpenApi` `[2.7.5, 3)` on every target (net8.0, net9.0, net10.0, net11.0). `CSharpEssentials.AspNetCore.Swashbuckle` and `CSharpEssentials.AspNetCore.OpenApi` now share one enum schema writer and one Microsoft.OpenApi major version; a host still references one of the two.

1. Update `Swashbuckle.AspNetCore` (and any `Swashbuckle.AspNetCore.*` package you reference) to 10.2.3 or later. Remove a direct `Microsoft.OpenApi` 1.x reference.
2. Pass the security scheme id to `AddSwagger`. Microsoft.OpenApi 2.x has no `Reference` on `OpenApiSecurityScheme`, so the id is an argument:

   ```csharp
   // 5.x
   builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(SecuritySchemes.JwtBearerTokenSecurity);

   // 6.0
   builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(
       SecuritySchemes.JwtBearerSchemeName, // "Bearer", the id 5.x used
       SecuritySchemes.JwtBearerTokenSecurity);
   ```

   A custom scheme drops its `Reference = new OpenApiReference { ... }` and passes its id as the first argument.
3. Move your own Swashbuckle code to the Microsoft.OpenApi 2.x model ([Swashbuckle 10 migration guide](https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/docs/migrating-to-v10.md)):

   | 5.x (Microsoft.OpenApi 1.x) | 6.0 (Microsoft.OpenApi 2.x) |
   |---|---|
   | `using Microsoft.OpenApi.Models;`, `.Any`, `.Writers` | `using Microsoft.OpenApi;` |
   | `ISchemaFilter.Apply(OpenApiSchema, ...)` | `ISchemaFilter.Apply(IOpenApiSchema, ...)`; cast to `OpenApiSchema` to change it |
   | `Type = "string"`, `Nullable = true` | `Type = JsonSchemaType.String`, `Type = (Type ?? JsonSchemaType.Null) \| JsonSchemaType.Null` |
   | `new OpenApiString("x")`, `OpenApiArray` | `JsonValue.Create("x")`, `JsonArray` (`System.Text.Json.Nodes`) |
   | `Extensions["x-..."] = new OpenApiString(...)` | `Extensions ??= new Dictionary<string, IOpenApiExtension>(); Extensions["x-..."] = new JsonNodeExtension(node)` |
   | `operation.Parameters[0].Schema` | `operation.Parameters` is `IList<IOpenApiParameter>?` |
   | `new OpenApiSecurityRequirement { { scheme, [] } }` | `AddSecurityRequirement(document => new() { { new OpenApiSecuritySchemeReference(id, document), [] } })` |

4. Subclasses of `ConfigureSwaggerOptions` replace `using Microsoft.OpenApi.Models;` with `using Microsoft.OpenApi;` for `OpenApiInfo` and `OpenApiLicense`. `EnumSchemaFilter` is `sealed`; wrap it instead of deriving from it.

What the documents change:

- The enum components and the shared golden files (OpenAPI 3.0) are unchanged.
- An enum property with its own XML `<summary>` is written as `allOf: [$ref]` + `description` (the summary of the property). A property without one stays a plain `$ref`; the summary of the enum type stays on the component.
- An optional route parameter (`{id?}`) whose schema is a `$ref` is written as `allOf: [$ref]` + `nullable: true` + `default: null`, because a `$ref` cannot carry siblings in OpenAPI 3.0. An inline schema gets `nullable: true` and `default: null` on itself. This is the 6.0.0 output. From 6.1.0 `AddSwagger` splits the route into one path per form with required parameters instead, and only `OptionalRouteParameterMode.LegacyNonCompliant` keeps this shape ([Optional route parameters](v6-optional-route-parameters.md)).

Keep OpenAPI 3.0, the Swashbuckle default. The enum filters always write the 3.0 shape (a filter cannot see the version `UseSwagger` serializes), and with `UseSwagger(o => o.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1)` a nullable enum usage, and (with `OptionalRouteParameterMode.LegacyNonCompliant`) an optional route parameter whose schema is a `$ref`, is serialized as `{"type": "null", "allOf": [{"$ref": ...}]}`: no value matches both `type: "null"` and the referenced component, so the schema is unsatisfiable and validators and client generators reject `null` and every enum value.

## `CSharpEssentials.AspNetCore.OpenApi`

No change: net10.0, `Microsoft.AspNetCore.OpenApi` 10.x, `Microsoft.OpenApi` 2.x. `Microsoft.AspNetCore.OpenApi` 11.x needs `Microsoft.OpenApi` 3.x, so a net11.0 target comes later.

## Pagination (`CSharpEssentials.EntityFrameworkCore`)

### Single-column cursor `PaginateAsync` is obsolete

`PaginateAsync<T, TCursor>(IQueryable<T>, ICursorPaginationRequest<TCursor>, Expression<Func<T, TCursor>>, ...)` is marked `[Obsolete]`. It still works, but every call now raises warning CS0618, which fails the build under `TreatWarningsAsErrors`. It will be removed in a later major version.

It filters by one column only, so a page that ends inside a group of equal values (such as a timestamp) skips the rest of that group, and it does not cap `Limit`. Move to `KeysetPaginateAsync` and add a unique tie-breaker to the key:

```csharp
// 5.x
CursorPaginationResponse<Log, DateTime> response = await db.Logs.PaginateAsync(
    new CursorPaginationRequest<DateTime> { Limit = 20, Cursor = lastCreatedAt },
    x => x.CreatedAt,
    isAscending: false);
// response.Items, response.Next, response.HasMore

// 6.0
Result<KeysetPaginationResponse<Log>> page = await db.Logs.KeysetPaginateAsync(
    new KeysetPaginationRequest { Limit = 20, After = nextCursor },
    k => k.Descending(x => x.CreatedAt).Descending(x => x.Id),
    ct);
// page.Value.Items, page.Value.NextCursor, page.Value.HasNext, PreviousCursor/HasPrevious for backward paging
```

| 5.x | 6.0 |
|---|---|
| `CursorPaginationRequest<TCursor>.Cursor` (raw value) | `KeysetPaginationRequest.After` / `Before` (opaque string from `NextCursor` / `PreviousCursor`) |
| `Limit` (no upper bound) | `Limit`, clamped to `KeysetPaginationOptions.MaxLimit` (default 100) |
| `isAscending` and `thenBy` | `KeysetOrdering<T>.Ascending(...)` / `.Descending(...)` per key column |
| `search` delegate | apply `.Where(...)` to the query before `KeysetPaginateAsync` |
| `CursorPaginationResponse<T, TCursor>.Next` / `HasMore` | `KeysetPaginationResponse<T>.NextCursor` / `HasNext` |

`CursorPaginationRequest<TCursor>`, `ICursorPaginationRequest<TCursor>` and `CursorPaginationResponse<T, TCursor>` are not marked `[Obsolete]`. They are plain DTOs that may appear in public API contracts, client models and serialized payloads, so obsoleting them would break builds that never call the obsolete method. They stay for DTO compatibility and are slated for removal, together with the obsolete overload, in a future major version.

Cursors issued by `PaginateAsync` are raw values and are not accepted by `KeysetPaginateAsync`; clients restart from the first page once. If you cannot migrate yet, keep the call inside a member or type marked `[Obsolete]`, which does not report CS0618 for its body.

### Offset page size is capped at 100

In 5.x, `IPaginationRequest.Normalize()` only raised `PageNumber` and `PageSize` to 1, so a client could ask for `pageSize=100000` and get it. In 6.0 it also lowers `PageSize` to `PaginationDefaults.MaxPageSize` (100). The offset `PaginateAsync` and `Paginate` overloads (request and `pageNumber`/`pageSize` variants) take a new optional `maxPageSize` parameter, default `PaginationDefaults.MaxPageSize`, and pass it to `Normalize(int maxPageSize)`.

```csharp
// 6.0 default: at most 100 rows
await db.Users.OrderBy(u => u.Id).PaginateAsync(request, cancellationToken: ct);

// A larger cap for one endpoint
await db.Users.OrderBy(u => u.Id).PaginateAsync(request, maxPageSize: 500, cancellationToken: ct);

// 5.x behavior: no cap
await db.Users.OrderBy(u => u.Id).PaginateAsync(request, maxPageSize: int.MaxValue, cancellationToken: ct);
```

`PaginateAsync` and `Paginate` normalize the request themselves, so pass `maxPageSize` to them; calling `Normalize` before them does not change their cap. Call `Normalize(int.MaxValue)` only when you normalize a request yourself without `PaginateAsync` or `Paginate`:

```csharp
// Only when you page the query yourself, without PaginateAsync/Paginate
IPaginationRequest manual = request;
manual.Normalize(int.MaxValue);
List<User> rows = await db.Users.OrderBy(u => u.Id).Skip(manual.SkipCount()).Take(manual.PageSize).ToListAsync(ct);
```

A `maxPageSize` below 1 throws `ArgumentOutOfRangeException`.

Compatibility notes:

- `maxPageSize` sits before `CancellationToken`, so `cancellationToken` stays last. Calls that pass the token by name keep compiling. The 5.x async signatures `PaginateAsync(query, request, search, includeTotalCount, cancellationToken)` and `PaginateAsync(query, pageNumber, pageSize, includeTotalCount, cancellationToken)` remain as hidden overloads, so positional calls and assemblies compiled against 5.x still work; they apply the default cap.
- The synchronous `Paginate` overloads gained a trailing parameter. Source code compiles unchanged, but assemblies compiled against 5.x must be recompiled.
- `Normalize()` now calls `Normalize(PaginationDefaults.MaxPageSize)`, and `Normalize(int)` no longer calls `Normalize()`. A custom `IPaginationRequest` that overrides `Normalize()` to add its own rules must also override `Normalize(int maxPageSize)`, because `PaginateAsync` and `Paginate` call the latter. The 5.x direction (`Normalize(int)` calling `Normalize()`) cannot be kept: `Normalize()` now applies the default cap of 100, so it would lower every larger `maxPageSize`, including `int.MaxValue`. `ICursorPaginationRequest<TCursor>` keeps the 5.x direction because its `Normalize()` does not cap.
- `ICursorPaginationRequest<TCursor>.Normalize()` is unchanged and still does not cap `Limit`. Its only consumer is the obsolete single-column cursor `PaginateAsync`; the replacement, `KeysetPaginateAsync`, already clamps `Limit` to `KeysetPaginationOptions.MaxLimit` (default 100). Changing the obsolete path would break callers who are about to move off it anyway.
