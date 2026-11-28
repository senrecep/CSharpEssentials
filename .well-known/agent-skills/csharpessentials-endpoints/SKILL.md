---
name: csharpessentials-endpoints
description: Use when organizing ASP.NET Core Minimal API endpoints into classes with CSharpEssentials.Endpoints. Covers IEndpoint and IEndpointGroup with static members, [EndpointGroup<T>] nesting, [ExcludeFromMapping], the generated app.MapAllEndpoints() and app.Map{Asm}Endpoints() registry, EndpointMappingOptions (Filter, ConfigureEach, OperationNaming, AutoTagFromGroup), RouteOf<T>, RequireRoles, RequirePolicies, RequireAuthSchemes, MapEndpointsFromAssemblies, and diagnostics CSE1001 to CSE1013.
---

# CSharpEssentials.Endpoints

Endpoint classes for Minimal APIs. A source generator finds every `IEndpoint` at compile time and writes one mapping method per assembly. No reflection at startup, Native AOT safe. Route calls (`MapGet`, `MapPost`) stay in your code.

## Install

```bash
dotnet add package CSharpEssentials.Endpoints
```

Targets `net11.0`, `net10.0`, `net9.0`, `net8.0`. Generator, analyzers and code fixes are in the package.

## Namespaces

```csharp
using CSharpEssentials.Endpoints;   // IEndpoint, IEndpointGroup, attributes, options, RouteOf, RequireRoles
```

The generated `MapAllEndpoints` and `Map{Asm}Endpoints` live in `Microsoft.AspNetCore.Builder`, which web projects import implicitly.

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Endpoints
using CSharpEssentials.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ProductStore>();

var app = builder.Build();
app.MapAllEndpoints();
app.Run();

public sealed class ProductsGroup : IEndpointGroup
{
    public static string Prefix => "products";

    public static void Configure(RouteGroupBuilder group) => group.WithTags("Products");
}

[EndpointGroup<ProductsGroup>]
public sealed class GetProduct : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id:int}", (int id, ProductStore store) =>
            store.Find(id) is { } product ? Results.Ok(product) : Results.NotFound());
}

[EndpointGroup<ProductsGroup>]
public sealed class CreateProduct : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", (Product product, ProductStore store) =>
        {
            store.Add(product);
            return TypedResults.Created($"/products/{product.Id}", product);
        });
}

public sealed record Product(int Id, string Name);

public sealed class ProductStore
{
    private readonly Dictionary<int, Product> _items = [];

    public Product? Find(int id) => _items.GetValueOrDefault(id);

    public void Add(Product product) => _items[product.Id] = product;
}
```

## Creating an endpoint

- Implement `IEndpoint` and declare `public static void Map(IEndpointRouteBuilder app)`. `Map` is `static abstract` on the interface.
- The type is never instantiated. Services come in as handler parameters: `(int id, ProductStore store) => ...`.
- One type may map several routes (`MapPut` and `MapDelete` on `/{id:int}`).
- Without a group attribute the endpoint maps at the root.

## Creating a group

```csharp
public sealed class ApiGroup : IEndpointGroup
{
    public static string Prefix => "api";            // required, "" adds no segment
}

[EndpointGroup<ApiGroup>]                            // groups nest
public sealed class AdminGroup : IEndpointGroup
{
    public static string Prefix => "admin";

    public static void Configure(RouteGroupBuilder group) =>   // optional
        group.WithTags("Admin").RequireRoles("admin");
}

[EndpointGroup(typeof(AdminGroup))]                  // same as [EndpointGroup<AdminGroup>]
public sealed class GetStats : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/stats", () => TypedResults.Ok(42));   // GET /api/admin/stats
}
```

- `Prefix` is `static abstract string`. `Configure(RouteGroupBuilder)` is `static virtual` with an empty default.
- One group attribute per type. Generic and `typeof` forms are equivalent.
- `[ExcludeFromMapping]` on an endpoint, a group (and everything under it) or the assembly leaves it out.

## Mapping

```csharp
app.MapAllEndpoints();                      // own registry, then referenced registries by assembly name
app.MapMyCompanyAppsApiEndpoints();         // one assembly; name from the sanitized assembly name
IReadOnlyList<Type> types = MyCompanyAppsApiEndpointRegistry.EndpointTypes;   // mapping order
```

- `Map{Asm}Endpoints(Action<EndpointMappingOptions>? configure = null)`. `{Asm}`: non-identifier characters removed, each segment capitalized (`MyCompany.Apps.Api` gives `MyCompanyAppsApi`).
- `[assembly: EndpointRegistryName("Apps")]` renames it to `MapAppsEndpoints` and `AppsEndpointRegistry`.
- `MapAllEndpoints(Action<EndpointMappingOptions>? configure = null)` is `internal`, generated only in `Exe`/`WinExe` projects that are not test projects. `[assembly: GenerateEndpointAggregate]` opts a library or test project in; `[assembly: DisableEndpointAggregate]` opts out.
- Call each mapping method once per route builder. A second call maps every route twice and requests fail with an ambiguous match.
- Versioning (`CSharpEssentials.AspNetCore`): `app.MapVersionedGroup(2).MapAppsEndpoints();` gives `/v2/...`.

## Configuring mapping

```csharp
app.MapAllEndpoints(options =>
{
    options.Filter(type => type.Namespace != "Internal");          // AND-combined
    options.OperationNaming = OperationNaming.TypeName;            // None (default) | TypeName | Custom(...)
    options.AutoTagFromGroup = true;                               // UsersGroup -> tag "Users" when no tags set
    options.LogDiscovered = true;                                  // Debug, category CSharpEssentials.Endpoints
    options.ConfigureEach((endpoint, type) => endpoint.WithMetadata(type));   // after group Configure
});

options.OperationNaming = OperationNaming.Custom((type, endpoint) => "op_" + type.Name);   // null = unnamed
```

- Every mapped endpoint carries `EndpointTypeMetadata` (`EndpointType` property).
- `TypeName`: type plus containing types (`Orders_Create`), HTTP method suffix when a type maps several routes (`Items_Get`), namespace on collision, throws when two types share a full name. Explicit `WithName(...)` always wins.
- Constant names from `WithName(...)` (or `EndpointNameAttribute`, `EndpointNameMetadata`, `RouteNameMetadata` in `WithMetadata`) anywhere in the project are reserved at build time; a colliding generated name gets `_2`. Non-constant names (CSE1011) and `MapEndpointsFromAssemblies` endpoints are not reserved.
- Use `WithName(...)` for endpoints whose client method names must not change; `TypeName` output can shift when same-named types are added.

## Building paths in tests

```csharp
string path = app.RouteOf<GetProduct>(new { id = 42 });                  // "/products/42"
string withQuery = app.RouteOf<GetProduct>(new { id = 42, view = "full" }); // "/products/42?view=full"
string byMethod = app.RouteOf<GetProduct>("GET", new { id = 42 });       // pick by HTTP method or WithName
```

- `RouteOf<TEndpoint>(object? values = null)` and `RouteOf<TEndpoint>(string nameOrMethod, object? values = null)`, extensions on `IEndpointRouteBuilder`. Call after mapping. Use the selector when the type maps several routes.
- Extra values become query string entries. Throws `InvalidOperationException` for no route, several routes without a selector, or a missing or invalid route value.
- `[RequiresUnreferencedCode]`: tests only, not trimmed app code.

## Requiring authorization

```csharp
group.RequireRoles("admin", "editor");          // any one role
group.RequirePolicies("CanRead", "CanWrite");   // every policy
group.RequireAuthSchemes("Bearer");             // any one scheme
```

Work on any `IEndpointConventionBuilder` (group, route handler, `ConfigureEach` builder) and return the same builder. An empty list or a null, blank or comma-containing value throws `ArgumentException`. The app still needs `AddAuthentication()`, `AddAuthorization()` and the auth middleware.

## Returning Result<T, E..> from a handler

Needs `CSharpEssentials.AspNetCore` (conversions) and `CSharpEssentials.Results` (the result type).

```csharp Program.cs
// dotnet add package CSharpEssentials.Endpoints
// dotnet add package CSharpEssentials.AspNetCore
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.AspNetCore;    // ToHttpResult, ToOk, ToCreated, ToNoContent
using CSharpEssentials.Endpoints;
using CSharpEssentials.Errors;        // NotFoundError, ConflictError
using CSharpEssentials.ResultPattern; // Result<T, E1>

var app = WebApplication.CreateBuilder(args).Build();
app.MapAllEndpoints();
app.Run();

public sealed class GetBook : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/books/{id:int}", (int id) => Find(id).ToHttpResult());   // 200 or 404 problem

    private static Result<string, NotFoundError> Find(int id) =>
        id == 1 ? "Dune" : new NotFoundError("Book.NotFound", "No such book.");
}
```

Returning the raw `Result<T, E1..E3>` (or a `Task`/`ValueTask` of it) is CSE1013: it would serialize as 200 with the error in the body. `ToCreated(value => location)` gives 201, `ToNoContent()` 204. Each has `Task` and `ValueTask` overloads.

## Plugins: reflection fallback

```csharp
app.MapEndpointsFromAssemblies(pluginAssembly);
app.MapEndpointsFromAssemblies(options => options.LogDiscovered = true, pluginAssembly);
```

Same rules and options as the generated registry. `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`; throws `PlatformNotSupportedException` under Native AOT. Use only for assemblies loaded at runtime.

## Rules

- `Map` must be `static`. Do not add instance fields, properties or constructor parameters to an endpoint (CSE1004).
- A non-partial class that only inherits `Map` is not mapped (CSE1012). Make it `partial`, or add `public static new void Map(IEndpointRouteBuilder app) => Base.Map(app);`.
- Use `MapAllEndpoints()` in the app's `Program.cs`. In a class library it does not exist unless the library has `[assembly: GenerateEndpointAggregate]`.
- Never write `[assembly: EndpointModule(...)]`; the generator emits it.
- Do not call `EndpointMapper` or the registry's `MapEndpoints(app, options)`; they are generated-code plumbing.
- Convert `Result<T, E..>` with `.ToHttpResult()` and friends before returning it (CSE1013).
- Do not use `RouteOf` or `MapEndpointsFromAssemblies` in AOT-published app code.
- Under Native AOT (Request Delegate Generator on), a handler lambda that references a generated `*EndpointRegistry` type needs an explicit return type: `Ok<string[]> () => ...`.
- Group conventions (tags, auth, filters) go in `IEndpointGroup.Configure`, not in every `Map`.

## Diagnostics

| ID | Severity | Meaning | Fix |
|---|---|---|---|
| CSE1001 | Error | Endpoint or group not accessible from generated code | Make it and its containing types `public` or `internal`, not `file` |
| CSE1002 | Error | Group nesting cycle | Break the `[EndpointGroup]` chain |
| CSE1003 | Error | Both group attribute forms on one type | Keep one |
| CSE1004 | Warning | Endpoint has instance state | Move dependencies to handler parameters; code fix removes unused state |
| CSE1005 | Warning | Same HTTP method and constant route twice in one group | Change one route |
| CSE1006 | Info | Abstract or open-generic endpoint or group skipped | Expected for base classes |
| CSE1007 | Error | Group target does not implement `IEndpointGroup`, or is abstract, open-generic or a ref struct | Point the attribute at a concrete group |
| CSE1008 | Error | Endpoint or group is a `ref struct` | Use a class or plain struct |
| CSE1009 | Warning | Two referenced assemblies give the same registry name; `MapAllEndpoints` skips them | `[assembly: EndpointRegistryName("...")]` |
| CSE1010 | Warning | Same constant endpoint name at two call sites | Rename one, or set severity `none` if intended |
| CSE1011 | Info | Explicit name outside endpoint types is not a constant | Use a `const` |
| CSE1012 | Warning | Non-partial class inherits `Map` without declaring it | `partial`, or declare `static new Map` calling the base |
| CSE1013 | Warning | Handler returns `Result<T, E1..E3>` unconverted | `.ToHttpResult()`; code fix appends it to lambdas |
