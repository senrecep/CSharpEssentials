---
name: csharpessentials-endpoints
description: Use when organizing ASP.NET Core Minimal API endpoints into classes — IEndpoint/IEndpointGroup with a static Map, [EndpointGroup<T>] nesting, the source-generated Map{Assembly}Endpoints/MapAllEndpoints registry (AOT-safe), EndpointMappingOptions, RouteOf<T>, RequireRoles/RequirePolicies/RequireAuthSchemes, the MapEndpointsFromAssemblies reflection fallback and analyzers CSE1001–CSE1011.
---

# CSharpEssentials.Endpoints

Source-generated endpoint organization for Minimal APIs. Route calls (`MapGet`, `MapPost`, …) stay in your code, so binding, filters, `IResult`, OpenAPI and the Request Delegate Generator work unchanged. A generator emits one registry per assembly; no reflection runs at startup, so it is trim and Native AOT safe.

## Installation

```bash
dotnet add package CSharpEssentials.Endpoints
```

Target frameworks: `net11.0`, `net10.0`, `net9.0`, `net8.0`. The source generator, analyzer and code fix ship in the package.

## Namespace

```csharp
using CSharpEssentials.Endpoints;
```

Generated registries live in `Microsoft.AspNetCore.Builder`, so `app.Map…Endpoints()` needs no extra using.

---

## Endpoints and Groups

```csharp
public sealed class AppsGroup : IEndpointGroup
{
    public static string Prefix => "apps";

    public static void Configure(RouteGroupBuilder group) => group.WithTags("Apps").RequireAuthorization();
}

[EndpointGroup<AppsGroup>]
public sealed class GetApp : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id:int}", (int id, IAppService service) => service.GetAsync(id));
}

[EndpointGroup(typeof(AppsGroup))]
public sealed class CreateApp : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", (CreateAppRequest request, IAppService service) => service.CreateAsync(request))
           .WithValidation<CreateAppRequest>()   // CSharpEssentials.AspNetCore + CSharpEssentials.Validation
           .RequireRoles("admin", "editor");
}
```

| Member | Rule |
|---|---|
| `IEndpoint.Map(IEndpointRouteBuilder)` | `static abstract`. Endpoint types are never instantiated; take dependencies as handler parameters. One type may map several routes. |
| `IEndpointGroup.Prefix` | `static abstract string`, passed to `MapGroup`. An empty string adds no segment. |
| `IEndpointGroup.Configure(RouteGroupBuilder)` | `static virtual`, optional. Group-wide tags, authorization, filters. |
| `[EndpointGroup(typeof(T))]` / `[EndpointGroup<T>]` | Places an endpoint or a group under a group. Groups nest. One group attribute per type. |
| `[ExcludeFromMapping]` | On an endpoint, a group (and everything under it) or an assembly. |

---

## Mapping

```csharp
app.MapAppsEndpoints();   // one assembly's registry
app.MapAllEndpoints();    // own registry + every referenced assembly's registry; call once per route builder, a repeat maps every route twice

IReadOnlyList<Type> mapped = AppsEndpointRegistry.EndpointTypes;   // mapping order
```

- `Map{Asm}Endpoints(Action<EndpointMappingOptions>? configure = null)` — `{Asm}` is the sanitized assembly name (`MyCompany.Apps.Api` → `MyCompanyAppsApi`) or the value of `[assembly: EndpointRegistryName("Apps")]`.
- `MapAllEndpoints` is `internal`, generated automatically in `Exe`/`WinExe` projects that are not test projects. `[assembly: GenerateEndpointAggregate]` opts in from a library or test project; `[assembly: DisableEndpointAggregate]` opts out. Own registry first, then referenced registries by assembly name, each once.
- `[assembly: EndpointModule(typeof(...))]` is emitted by the generator; do not write it yourself.
- Each endpoint type is mapped inside its own `MapGroup("")`, so routes, OpenAPI metadata, filters and authorization match direct mapping.

## Mapping Options

```csharp
app.MapAppsEndpoints(options =>
{
    options.Filter(type => type.Namespace != "Sample.Internal");   // AND-combined predicates
    options.OperationNaming = OperationNaming.TypeName;            // None (default), TypeName, Custom(...)
    options.AutoTagFromGroup = true;                               // UsersGroup -> "Users" when no tags
    options.LogDiscovered = true;                                  // Debug log, category CSharpEssentials.Endpoints
    options.ConfigureEach((endpoint, type) => endpoint.WithMetadata(new AuditedEndpoint(type)));
});
```

Every endpoint gets `EndpointTypeMetadata` (`EndpointType` property) so middleware and tests can identify the endpoint type at runtime. An explicit `WithName(...)` always wins over `OperationNaming`. `TypeName` names are unique per application: containing types are part of the name (`Orders_Endpoint`), a type with several routes gets the HTTP method as a suffix (`Items_Get`, `Items_Post`), colliding names are qualified with the namespace, and two types with the same full name throw at map time. Generated names can change when a same-named endpoint or type is added (an existing name gets the namespace or a numeric suffix), which renames NSwag/Kiota client methods; use `WithName(...)` for endpoints that need stable names. Constant explicit names (`WithName`, or `EndpointNameAttribute`/`EndpointNameMetadata`/`RouteNameMetadata` in `WithMetadata`) anywhere in the project, including plain `app.MapGet(...).WithName("Items")`, are reserved at build time by the generated registry and `MapAllEndpoints`, so a colliding `TypeName` name gets a suffix (`Items_2`) and the explicit name never changes. Non-constant names (CSE1011) and the `MapEndpointsFromAssemblies` fallback are not reserved. Reservation and CSE1010 are compilation-wide, not per application: two `WebApplication`s in one project that use the same `WithName` get CSE1010 and each reserves the other's names (a matching generated name gets an unneeded `_2`); suppress CSE1010 in `.editorconfig` (`dotnet_diagnostic.CSE1010.severity = none`) when that is intended. A host without endpoint types that calls `Map{Asm}Endpoints` of a library directly does not reserve its own explicit names; use `MapAllEndpoints` in the host so they are reserved.

---

## Typed Routes, Authorization, Versioning

```csharp
string path = app.RouteOf<GetApp>(new { id = 42 });                 // "/apps/42"
string remove = app.RouteOf<AppCommands>("DELETE", new { id = 42 }); // select by HTTP method or WithName(...)

RouteGroupBuilder admin = app.MapGroup("admin")
    .RequireRoles("admin")              // any listed role
    .RequirePolicies("CanRead")         // every listed policy
    .RequireAuthSchemes("Bearer");      // any listed scheme

app.MapVersionedGroup(2).MapAppsEndpoints();   // CSharpEssentials.AspNetCore: routes under /v2/...
```

- `RouteOf<TEndpoint>` reads `EndpointTypeMetadata`, so group prefixes and versioned groups are included; extra values become query string entries. Throws `InvalidOperationException` for no route, an ambiguous route without a selector, or a missing/invalid route value. Marked `[RequiresUnreferencedCode]`.
- `RequireRoles` / `RequirePolicies` / `RequireAuthSchemes` work on any `IEndpointConventionBuilder` and add `AuthorizeAttribute` metadata. Empty lists or null, blank or comma-separated entries throw `ArgumentException`.

---

## Reflection Fallback

For assemblies without a generated registry (for example plugins loaded at runtime):

```csharp
app.MapEndpointsFromAssemblies(typeof(Program).Assembly);
app.MapEndpointsFromAssemblies(options => options.LogDiscovered = true, pluginAssembly);
```

Same discovery rules, ordering and options as the generated registries. Types the analyzer reports as errors are skipped and logged at `Warning`. Marked `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]` — not for trimmed or Native AOT apps.

---

## Diagnostics

| ID | Severity | Rule |
|---|---|---|
| CSE1001 | Error | Endpoint or group type is not accessible from generated code. |
| CSE1002 | Error | Group nesting cycle. |
| CSE1003 | Error | More than one group attribute on one type. |
| CSE1004 | Warning | Endpoint declares instance state (fields, auto-properties, constructor parameters). Code fix removes unused state. |
| CSE1005 | Warning | Two endpoints in the same group map the same HTTP method and constant route. |
| CSE1006 | Info | Abstract or open-generic endpoint or group type is skipped. |
| CSE1007 | Error | `[EndpointGroup(typeof(X))]` target does not implement `IEndpointGroup`, or is abstract, open-generic or a ref struct. |
| CSE1008 | Error | Endpoint or group type is a `ref struct`. Generated code passes it as a generic type argument, which ref structs cannot be, so it is not mapped. |
| CSE1009 | Warning | Two referenced assemblies produce the same registry name, for example `Foo.Api` and `FooApi` both produce `FooApiEndpointRegistry`. `MapAllEndpoints` skips both registries so the project still compiles. The project's own registry counts too: when it has the same name as a referenced one, only the own registry is mapped. Give one of them a distinct name with `[assembly: EndpointRegistryName("...")]`. Reported only in projects that generate the aggregate. |
| CSE1010 | Warning | The same constant endpoint name is set at two call sites (`WithName`, or `EndpointNameAttribute`/`EndpointNameMetadata`/`RouteNameMetadata` in `WithMetadata`). Endpoint names and route names are compared separately. |
| CSE1011 | Info | An explicit endpoint name outside an `IEndpoint`/`IEndpointGroup` type is not a constant, so it cannot be reserved at build time. |

---

## Best Practices

- Keep endpoint types stateless: dependencies go in handler parameters, never in fields or constructors (CSE1004).
- Put cross-cutting conventions (tags, auth, filters) in `IEndpointGroup.Configure`, not in every `Map`.
- Use the generated `Map{Asm}Endpoints`/`MapAllEndpoints` in production; reserve `MapEndpointsFromAssemblies` for runtime-loaded plugins.
- Use `RouteOf<T>` in integration tests instead of hard-coded route strings.
- Migrating from Carter: a `CarterModule` becomes an `IEndpointGroup`, each route an `IEndpoint`; replace `AddCarter()`/`MapCarter()` with `app.MapAllEndpoints()`.
