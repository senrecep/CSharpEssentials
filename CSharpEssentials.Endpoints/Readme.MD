# CSharpEssentials.Endpoints

Source-generated endpoint organization for ASP.NET Core Minimal APIs. A thin layer over Minimal APIs: route calls stay in your code, so binding, filters, `IResult`, OpenAPI and the Request Delegate Generator work unchanged.

## Installation

```bash
dotnet add package CSharpEssentials.Endpoints
```

Target frameworks: `net11.0`, `net10.0`, `net9.0`, `net8.0`. The source generator, analyzer and code fix are bundled in the package.

## Endpoints and groups

```csharp
using CSharpEssentials.Endpoints;

public sealed class AppsGroup : IEndpointGroup
{
    public static string Prefix => "apps";

    public static void Configure(RouteGroupBuilder group) => group.WithTags("Apps").RequireAuthorization();
}

[EndpointGroup(typeof(AppsGroup))]
public sealed class CreateApp : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", (CreateAppRequest request, IAppService service) => service.CreateAsync(request));
}
```

- `IEndpoint.Map` is static. Endpoint types are never instantiated; dependencies come from handler parameters.
- `IEndpointGroup.Prefix` is passed to `MapGroup`. `Configure` applies group-wide conventions.
- `[EndpointGroup(typeof(TGroup))]` or `[EndpointGroup<TGroup>]` places an endpoint or a group under a group. Groups can nest.
- `[ExcludeFromMapping]` excludes an endpoint, a group (with everything under it) or an assembly.

## Mapping

The bundled source generator creates one registry per assembly in the `Microsoft.AspNetCore.Builder` namespace:

```csharp
var app = builder.Build();

app.MapAppsEndpoints();   // one assembly's registry
// or
app.MapAllEndpoints();    // own registry + every referenced assembly's registry
```

- `Map{Asm}Endpoints` maps the endpoints of one assembly. `{Asm}` is the sanitized assembly name (`MyCompany.Apps.Api` → `MyCompanyAppsApi`) or the `[assembly: EndpointRegistryName("Apps")]` value.
- `{Asm}EndpointRegistry.EndpointTypes` lists the endpoint types in mapping order.
- `MapAllEndpoints` is `internal` and generated automatically in `Exe`/`WinExe` projects that are not test projects. Use `[assembly: GenerateEndpointAggregate]` to opt in from a library or test project, `[assembly: DisableEndpointAggregate]` to opt out. Each registry is mapped once, own registry first, then referenced registries by assembly name.
- Mapping is not idempotent: calling `Map{Asm}Endpoints` or `MapAllEndpoints` twice on the same route builder maps every route twice, and ASP.NET Core reports an ambiguous match at request time. Mapping the same registry onto different route builders (for example two version groups) is legitimate, so call each method once per builder.
- Each endpoint type is mapped inside its own `MapGroup("")`, so route patterns, OpenAPI metadata, filters and authorization are the same as direct mapping. Route calls such as `MapGet` stay in your `Map` method.

## Reflection fallback

When a registry cannot be generated (for example, plugin assemblies loaded at runtime), map endpoints by scanning assemblies:

```csharp
app.MapEndpointsFromAssemblies(typeof(CreateApp).Assembly);
app.MapEndpointsFromAssemblies(options => options.LogDiscovered = true, pluginAssembly);
```

- Uses the same discovery rules, ordering, conventions and options as the generated registries.
- Types the analyzer would report as errors (CSE1001, CSE1002, CSE1003, CSE1007, CSE1008) are skipped and logged at `Warning`, category `CSharpEssentials.Endpoints`. If some types of an assembly cannot be loaded, the loadable types are mapped and each loader exception is logged at `Warning`.
- Marked `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`. Prefer the generated methods in trimmed and Native AOT apps.

## API versioning

`CSharpEssentials.AspNetCore` provides `MapVersionedGroup`, which composes with any registry:

```csharp
app.MapVersionedGroup(2).MapAppsEndpoints();   // routes under /v2/...
```

## Mapping options

```csharp
app.MapAppsEndpoints(options =>
{
    options.Filter(type => type.Namespace != "Sample.Internal");
    options.OperationNaming = OperationNaming.TypeName;
    options.AutoTagFromGroup = true;
    options.LogDiscovered = true;
    options.ConfigureEach((builder, type) => builder.WithMetadata(new MyMetadata(type)));
});
```

| Option | Behavior |
|---|---|
| `Filter` | Skips endpoint types for which a predicate returns `false`. Multiple predicates are AND-combined. Groups whose endpoints are all filtered out are not created. |
| `ConfigureEach` | Runs once per endpoint type after group `Configure` and per-type metadata, in registration order. |
| `OperationNaming` | `None` (default), `TypeName` or `Custom(...)`. `TypeName` builds unique names from the type and its containing types (`Orders_Endpoint`), adds the HTTP method when a type maps several routes (`Items_Get`, `Items_Post`), qualifies colliding names with the namespace and throws when two types share a full name. An explicit `WithName(...)` always wins. |
| `AutoTagFromGroup` | Tags endpoints without explicit tags with the innermost group name (`UsersGroup` → `Users`). |
| `LogDiscovered` | Logs mapped and filtered endpoint types at `Debug`, category `CSharpEssentials.Endpoints`. |

Every endpoint gets `EndpointTypeMetadata`, which identifies the endpoint type at runtime.

`TypeName` names are not stable across changes to the application. Adding an endpoint or type with the same name can qualify an existing name with its namespace or give it a numeric suffix, which renames the generated operationId and the method names of NSwag or Kiota clients. Set the name with `WithName(...)` in `Map` for endpoints whose client method names must not change.

## Typed routes

`RouteOf<TEndpoint>` builds a request path from the endpoint type, which keeps tests independent of route strings:

```csharp
string path = app.RouteOf<GetApp>(new { id = 42 });                  // "/apps/42"
string remove = app.RouteOf<AppCommands>("DELETE", new { id = 42 }); // select by HTTP method or WithName(...)
```

- Uses `EndpointTypeMetadata` on the mapped endpoints, so group prefixes and versioned groups are included. Extra values become query string entries.
- Throws `InvalidOperationException` when the type has no route, has more than one route and no selector is given, or a route value is missing or fails its constraint.
- Reads values from object properties, so it is marked `[RequiresUnreferencedCode]`.

## Authorization shortcuts

Thin wrappers over `RequireAuthorization` for any endpoint, group or route handler builder:

```csharp
group.RequireRoles("admin", "editor");       // any listed role
group.RequirePolicies("CanRead", "CanWrite"); // every listed policy
group.RequireAuthSchemes("Bearer");           // any listed scheme
```

- Each call adds `AuthorizeAttribute` metadata, so it combines with `RequireAuthorization`, `[Authorize]` and `AllowAnonymous` as usual.
- Empty lists and null, blank or comma-separated entries throw `ArgumentException`.

## Validation

`CSharpEssentials.AspNetCore` adds a validation endpoint filter for `CSharpEssentials.Validation` validators. Call it on the route handler inside `Map`:

```csharp
[EndpointGroup<AppsGroup>]
public sealed class CreateApp : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", (CreateAppRequest request, IAppService service) => service.CreateAsync(request))
           .WithValidation<CreateAppRequest>();
}
```

Failed validation returns a 400 problem response and the handler does not run. See the [AspNetCore Readme](../CSharpEssentials.AspNetCore/Readme.MD) for the details.

## Diagnostics

The bundled analyzer reports types the generator cannot map. Invalid types are skipped, so the generated registry always compiles. Types and assemblies marked `[ExcludeFromMapping]` are not analyzed.

| ID | Severity | Rule |
|---|---|---|
| CSE1001 | Error | Endpoint or group type is not accessible from generated code (private/protected nested, `file`-local, or nested in an inaccessible type). |
| CSE1002 | Error | Group nesting cycle: the `[EndpointGroup]` chain returns to the same group. Reported on each group in the cycle. |
| CSE1003 | Error | More than one group attribute on one type (`[EndpointGroup(typeof(X))]` and `[EndpointGroup<Y>]`). |
| CSE1004 | Warning | Endpoint declares instance fields, auto-properties or a constructor with parameters. Endpoint types are never instantiated; take dependencies as handler parameters. Code fix: removes the state when nothing else uses it. |
| CSE1005 | Warning | Two endpoints in the same group map the same HTTP method and constant route (`MapGet`/`MapPost`/`MapPut`/`MapDelete`/`MapPatch`/`MapMethods` on the `Map` parameter). Ungrouped endpoints count as one group. |
| CSE1006 | Info | Abstract or open-generic endpoint or group type is skipped. |
| CSE1007 | Error | `[EndpointGroup(typeof(X))]` target does not implement `IEndpointGroup`, or is abstract, open-generic or a ref struct. |
| CSE1008 | Error | Endpoint or group type is a `ref struct`. Generated code passes it as a generic type argument, which ref structs cannot be, so it is not mapped. |
| CSE1009 | Warning | Two referenced assemblies produce the same registry name, for example `Foo.Api` and `FooApi` both produce `FooApiEndpointRegistry`. `MapAllEndpoints` skips both registries so the project still compiles. The project's own registry counts too: when it has the same name as a referenced one, only the own registry is mapped. Give one of them a distinct name with `[assembly: EndpointRegistryName("...")]`. Reported only in projects that generate the aggregate. |

The bundled code fix for CSE1004 ("Remove instance state") deletes the instance fields, auto-properties and parameterized constructors of the endpoint. It is offered only when nothing else in the solution uses them, and not for records.

## Migrating from Carter

Carter modules are instances created at startup; this package maps static methods from a generated registry, so no module is constructed and no reflection runs.

| Carter | CSharpEssentials.Endpoints |
|---|---|
| `builder.Services.AddCarter()` | Nothing to register |
| `app.MapCarter()` | `app.Map{Assembly}Endpoints()` or `app.MapAllEndpoints()` |
| `ICarterModule.AddRoutes(IEndpointRouteBuilder app)` | `static void Map(IEndpointRouteBuilder app)` on an `IEndpoint` |
| `CarterModule` base class with a base path and fluent conventions in the constructor | An `IEndpointGroup` (`Prefix`, `Configure`) and `[EndpointGroup<TGroup>]` on each endpoint |
| Constructor injection into the module | Handler parameters (`(IAppService service) => ...`) |
| Explicit module lists in the Carter configurator | `options.Filter(...)` or `[ExcludeFromMapping]` |
| FluentValidation helpers | `.WithValidation<T>()` from `CSharpEssentials.AspNetCore` with `CSharpEssentials.Validation` |
| Response negotiators | `IResult` and `TypedResults` |

Before:

```csharp
using Carter;

public sealed class AppsModule : CarterModule
{
    public AppsModule() : base("/apps") => WithTags("Apps");

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:int}", (int id, IAppService service) => service.GetAsync(id));
        app.MapPost("/", (CreateAppRequest request, IAppService service) => service.CreateAsync(request));
    }
}
```

After: the module becomes a group, and each route becomes an endpoint type. One endpoint type can also keep several related routes.

```csharp
public sealed class AppsGroup : IEndpointGroup
{
    public static string Prefix => "apps";

    public static void Configure(RouteGroupBuilder group) => group.WithTags("Apps");
}

[EndpointGroup<AppsGroup>]
public sealed class GetApp : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id:int}", (int id, IAppService service) => service.GetAsync(id));
}

[EndpointGroup<AppsGroup>]
public sealed class CreateApp : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", (CreateAppRequest request, IAppService service) => service.CreateAsync(request));
}
```

Then replace `AddCarter()` and `MapCarter()` with `app.MapAllEndpoints()` and remove the Carter package. Build: the analyzer reports module state left on endpoint types (CSE1004) and duplicate routes in a group (CSE1005).
