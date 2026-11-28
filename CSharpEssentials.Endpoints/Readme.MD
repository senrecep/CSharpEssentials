# CSharpEssentials.Endpoints

Organizes ASP.NET Core Minimal API endpoints into classes. A source generator finds the classes at compile time and writes one mapping method per assembly, so startup does no reflection and the app can publish as Native AOT.

Your `Map` method still calls `MapGet`, `MapPost` and the rest, so model binding, filters, `IResult`, OpenAPI and the Request Delegate Generator behave as in plain Minimal APIs.

## Installation

```bash
dotnet add package CSharpEssentials.Endpoints
```

Target frameworks: `net11.0`, `net10.0`, `net9.0`, `net8.0`. The source generator, analyzers and code fixes ship inside the package.

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Endpoints
using CSharpEssentials.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<TodoStore>();

var app = builder.Build();
app.MapAllEndpoints();
app.Run();

public sealed class TodosGroup : IEndpointGroup
{
    public static string Prefix => "todos";

    public static void Configure(RouteGroupBuilder group) => group.WithTags("Todos");
}

[EndpointGroup<TodosGroup>]
public sealed class ListTodos : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/", (TodoStore store) => TypedResults.Ok(store.All()));
}

[EndpointGroup<TodosGroup>]
public sealed class CreateTodo : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", (CreateTodoRequest request, TodoStore store) =>
        {
            Todo todo = store.Add(request.Title);
            return TypedResults.Created($"/todos/{todo.Id}", todo);
        });
}

public sealed record CreateTodoRequest(string Title);

public sealed record Todo(int Id, string Title);

public sealed class TodoStore
{
    private readonly List<Todo> _todos = [];

    public Todo[] All() => [.. _todos];

    public Todo Add(string title)
    {
        Todo todo = new(_todos.Count + 1, title);
        _todos.Add(todo);
        return todo;
    }
}
```

This maps `GET /todos` and `POST /todos`, both tagged `Todos`.

## Endpoints

An endpoint is a class or struct that implements `IEndpoint` and declares a `static void Map(IEndpointRouteBuilder app)`.

```csharp
public sealed class Health : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/health", () => TypedResults.Text("healthy"));
}
```

- Endpoint types are never instantiated. Take services as handler parameters, not through a constructor.
- One type may map several routes, for example `GET` and `DELETE` on the same resource.
- An endpoint without a group attribute is mapped at the root, inside an empty `MapGroup("")`.

## Groups

A group implements `IEndpointGroup`. `Prefix` goes to `MapGroup`; an empty string adds no route segment. `Configure` is optional and runs on the created `RouteGroupBuilder`, which is the place for tags, authorization and filters that apply to every endpoint in the group.

Put an endpoint or another group under a group with `[EndpointGroup<TGroup>]` or `[EndpointGroup(typeof(TGroup))]`. The two forms do the same thing; use one per type.

```csharp
public sealed class ApiGroup : IEndpointGroup
{
    public static string Prefix => "api";
}

[EndpointGroup<ApiGroup>]
public sealed class OrdersGroup : IEndpointGroup
{
    public static string Prefix => "orders";

    public static void Configure(RouteGroupBuilder group) => group.WithTags("Orders").RequireAuthorization();
}

[EndpointGroup<OrdersGroup>]
public sealed class GetOrder : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id:int}", (int id) => TypedResults.Ok(id));   // GET /api/orders/{id}
}
```

## Excluding types

`[ExcludeFromMapping]` leaves out an endpoint, a group together with everything under it, or a whole assembly (`[assembly: ExcludeFromMapping]`). It applies to the generated registry and to the reflection fallback.

## Mapping

The generator writes a static class in the `Microsoft.AspNetCore.Builder` namespace, so no extra `using` is needed:

```csharp
app.MapAppsEndpoints();   // the endpoints of one assembly
app.MapAllEndpoints();    // this assembly and every referenced assembly that has a registry
```

- `Map{Asm}Endpoints(Action<EndpointMappingOptions>? configure = null)` maps one assembly. `{Asm}` is the assembly name with non-identifier characters removed and each segment capitalized (`MyCompany.Apps.Api` gives `MapMyCompanyAppsApiEndpoints`).
- `[assembly: EndpointRegistryName("Apps")]` sets the name instead, giving `AppsEndpointRegistry.MapAppsEndpoints`.
- `{Asm}EndpointRegistry.EndpointTypes` is an `IReadOnlyList<Type>` of the endpoint types in mapping order.
- `MapAllEndpoints` is `internal` and generated in `Exe` and `WinExe` projects that are not test projects. It maps its own registry first, then referenced registries ordered by assembly name, each once. `[assembly: GenerateEndpointAggregate]` turns it on in a library or test project, `[assembly: DisableEndpointAggregate]` turns it off.
- `[assembly: EndpointModule(typeof(...))]` is written by the generator to mark an assembly that has a registry. Do not add it yourself.

A handler lambda that uses the generated registry type (for example `AppsEndpointRegistry.EndpointTypes`) needs an explicit return type, such as `Ok<string[]> () => ...`, when the Request Delegate Generator is on (it is under Native AOT). That generator cannot see types written by another generator and otherwise emits code that does not compile.

Mapping is not idempotent. Calling the same method twice on one route builder maps every route twice, and ASP.NET Core then reports an ambiguous match at request time. Mapping one registry onto two different builders, for example two version groups, is fine.

### API versioning

`MapVersionedGroup` from `CSharpEssentials.AspNetCore` returns a group with the `v{version:apiVersion}` prefix. Map a registry onto it:

```csharp
app.MapVersionedGroup(1).MapAppsEndpoints();   // /v1/...
app.MapVersionedGroup(2).MapAppsEndpoints();   // /v2/...
```

## Mapping options

Every mapping method takes an optional `Action<EndpointMappingOptions>`.

```csharp Program.cs
// dotnet add package CSharpEssentials.Endpoints
using CSharpEssentials.Endpoints;

var app = WebApplication.CreateBuilder(args).Build();

app.MapAllEndpoints(options =>
{
    options.Filter(type => type.Namespace != "Internal");
    options.OperationNaming = OperationNaming.TypeName;
    options.AutoTagFromGroup = true;
    options.LogDiscovered = true;
    options.ConfigureEach((endpoint, type) => endpoint.WithMetadata(new AuditedAttribute(type.Name)));
});

app.Run();

public sealed class UsersGroup : IEndpointGroup
{
    public static string Prefix => "users";
}

[EndpointGroup<UsersGroup>]
public sealed class ListUsers : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/", () => TypedResults.Ok(new[] { "ada", "linus" }));
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class AuditedAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
```

| Option | What it does |
|---|---|
| `Filter(Func<Type, bool>)` | Skips endpoint types for which the predicate returns `false`. Several predicates are AND-combined. A group whose endpoints are all skipped is not created. |
| `ConfigureEach(Action<IEndpointConventionBuilder, Type>)` | Runs once per endpoint type, after group `Configure`, in registration order. |
| `OperationNaming` | `None` (default), `TypeName`, or `Custom(...)`. See below. |
| `AutoTagFromGroup` | Tags an endpoint that has no explicit tags with its innermost group's type name, minus a trailing `Group` (`UsersGroup` gives `Users`). |
| `LogDiscovered` | Logs mapped and filtered endpoint types at `Debug`, category `CSharpEssentials.Endpoints`. |

Every mapped endpoint also gets `EndpointTypeMetadata`, whose `EndpointType` property tells middleware and tests which class mapped it.

### Operation names

The endpoint name becomes the OpenAPI `operationId` and the method name in generated clients. An explicit `WithName(...)` always wins over the policy.

- `OperationNaming.TypeName` uses the type name and its containing types (`Orders_Create`). A type that maps several routes gets the HTTP method as a suffix (`Items_Get`, `Items_Post`). Names that collide are qualified with the namespace; two types with the same full name throw when mapped.
- `OperationNaming.Custom(Func<Type, EndpointBuilder, string?>)` lets you build the name. Return `null` to leave an endpoint unnamed.

```csharp
options.OperationNaming = OperationNaming.Custom((type, endpoint) => "op_" + type.Name);
```

`TypeName` names can change when you add an endpoint or type with the same name: the existing name picks up a namespace or a number, which renames NSwag or Kiota client methods. Set `WithName(...)` on endpoints whose client names must stay fixed.

Explicit names are reserved at build time, so a generated name never takes one. The generator collects every constant name set with `WithName(...)`, or with `EndpointNameAttribute`, `EndpointNameMetadata` or `RouteNameMetadata` inside `WithMetadata(...)`, anywhere in the project, including plain `app.MapGet(...).WithName("Items")` calls. A generated name that would match one gets a numeric suffix (`Items_2`). These names are not reserved:

- names that are not constants (reported as CSE1011);
- `[EndpointName]` attributes on handlers outside endpoint types;
- endpoints mapped by `MapEndpointsFromAssemblies`;
- names in a project whose registry is never called. A host that has no endpoint types and calls a library's `Map{Asm}Endpoints` directly does not reserve its own names; call `MapAllEndpoints` in the host instead.

Reservation and CSE1010 work per project, not per running application. A project that builds two `WebApplication`s with the same `WithName("Items")` in each gets CSE1010, and a generated name that matches gets an unneeded `_2`. If the duplicate is intended, set `dotnet_diagnostic.CSE1010.severity = none` in `.editorconfig`.

## Typed routes

`RouteOf<TEndpoint>` builds a request path from an endpoint type, so tests do not repeat route strings.

```csharp Program.cs
// dotnet add package CSharpEssentials.Endpoints
using CSharpEssentials.Endpoints;

var app = WebApplication.CreateBuilder(args).Build();
app.MapAllEndpoints();

Console.WriteLine(app.RouteOf<GetApp>(new { id = 42 }));                        // /apps/42
Console.WriteLine(app.RouteOf<GetApp>(new { id = 42, expand = "owner" }));      // /apps/42?expand=owner
Console.WriteLine(app.RouteOf<AppCommands>("DELETE", new { id = 7 }));         // /apps/7

public sealed class AppsGroup : IEndpointGroup
{
    public static string Prefix => "apps";
}

[EndpointGroup<AppsGroup>]
public sealed class GetApp : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id:int}", (int id) => TypedResults.Ok(id));
}

[EndpointGroup<AppsGroup>]
public sealed class AppCommands : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:int}", (int id) => TypedResults.NoContent());
        app.MapDelete("/{id:int}", (int id) => TypedResults.NoContent());
    }
}
```

- The second overload, `RouteOf<TEndpoint>(string nameOrMethod, object? values = null)`, picks one route of a type that maps several: an endpoint name set with `WithName` (ordinal match) or an HTTP method (case-insensitive).
- Group prefixes and version groups are included. Values that are not route parameters become query string entries.
- Throws `InvalidOperationException` when the type has no mapped route, has several and no selector was given, or a route value is missing or fails its constraint.
- Reads values from object properties, so it is marked `[RequiresUnreferencedCode]`. Use it in tests, not in trimmed app code.

## Authorization shortcuts

Extensions over `RequireAuthorization` for any endpoint, group or route handler builder:

```csharp
group.RequireRoles("admin", "editor");        // the user needs one of the roles
group.RequirePolicies("CanRead", "CanWrite"); // the user must pass every policy
group.RequireAuthSchemes("Bearer");           // one of the schemes must authenticate the user
```

- Each call adds `AuthorizeAttribute` metadata, so it combines with `RequireAuthorization`, `[Authorize]` and `AllowAnonymous` as usual.
- An empty list, or a null, blank or comma-separated entry, throws `ArgumentException`.
- The app still needs `AddAuthentication`, `AddAuthorization` and the authorization middleware, as with `RequireAuthorization`.

## Returning typed results

A handler that returns `Result<T, E1>`, `Result<T, E1, E2>` or `Result<T, E1, E2, E3>` from `CSharpEssentials.Results` as is would be serialized as a 200 response with the error in the body, and OpenAPI would list only that 200. The analyzer reports this as CSE1013. Convert the result with an extension from `CSharpEssentials.AspNetCore`:

```csharp Program.cs
// dotnet add package CSharpEssentials.Endpoints
// dotnet add package CSharpEssentials.AspNetCore
// dotnet add package CSharpEssentials.Results
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Endpoints;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

var app = WebApplication.CreateBuilder(args).Build();
app.MapAllEndpoints();
app.Run();

public sealed class GetBook : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/books/{id:int}", (int id) => BookCatalog.Find(id).ToHttpResult());
}

public sealed class CreateBook : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/books", (Book book) => BookCatalog.Add(book).ToCreated(created => $"/books/{created.Id}"));
}

public sealed record Book(int Id, string Title);

public static class BookCatalog
{
    public static Result<Book, NotFoundError> Find(int id) =>
        id == 1 ? new Book(1, "Dune") : new NotFoundError("Book.NotFound", $"Book {id} was not found.");

    public static Result<Book, ConflictError> Add(Book book) =>
        book.Id == 1 ? new ConflictError("Book.Exists", "Book 1 already exists.") : book;
}
```

- `ToHttpResult()` and `ToOk()` send 200, `ToCreated(location)` sends 201, `ToNoContent()` sends 204. An error becomes its problem response (404 for `NotFoundError`, 409 for `ConflictError`), and OpenAPI lists each status.
- Every method has `Task` and `ValueTask` overloads, so `await`-free chains like `service.GetAsync(id).ToHttpResult()` work.
- The analyzer checks every `Map*` call with a lambda or method group, inside an endpoint type or not.

## Validation

`WithValidation<T>()` from `CSharpEssentials.AspNetCore` runs the registered `CSharpEssentials.Validation` validators for `T` before the handler and returns a 400 problem response on failure. Call it in `Map`:

```csharp
app.MapPost("/", (CreateAppRequest request, IAppService service) => service.CreateAsync(request))
   .WithValidation<CreateAppRequest>();
```

See the [AspNetCore Readme](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD) for registration.

## Reflection fallback

When no registry can be generated, for example for plugin assemblies loaded at runtime, scan assemblies instead:

```csharp
app.MapEndpointsFromAssemblies(pluginAssembly);
app.MapEndpointsFromAssemblies(options => options.LogDiscovered = true, pluginAssembly);
```

- Same discovery rules, ordering, conventions and options as the generated registries.
- Types the analyzer would report as errors (CSE1001, CSE1002, CSE1003, CSE1007, CSE1008) are skipped and logged at `Warning`, category `CSharpEssentials.Endpoints`. If some types of an assembly cannot be loaded, the rest are mapped and each loader exception is logged.
- Marked `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`. Under Native AOT it throws `PlatformNotSupportedException` instead of mapping a partial set of routes.

## Inheriting `Map`

The generator looks only at types that declare `static Map` themselves or are `partial`. A non-partial class that only inherits `Map` from a base class is not mapped, and the analyzer reports CSE1012:

```csharp
public abstract class CrudEndpoint<T> : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", () => TypedResults.Ok(typeof(T).Name));
}

public sealed class Users : CrudEndpoint<User>;   // CSE1012: not mapped
```

Fix it in one of two ways:

```csharp
// Option 1: make the class partial.
public sealed partial class Users : CrudEndpoint<User>;

// Option 2: declare Map and call the base.
public sealed class Users : CrudEndpoint<User>
{
    public static new void Map(IEndpointRouteBuilder app) => CrudEndpoint<User>.Map(app);
}
```

`new` is needed because the method hides the base `Map` (CS0108). Keep the body a call to the base: the registry calls `Map` through `IEndpoint`, which resolves to the base method unless the derived class lists `IEndpoint` itself. Before 8.0 such classes were mapped. With a concrete base class, 7.x mapped both the base and the derived class, so the routes were registered twice.

## Diagnostics

The analyzer reports types the generator cannot map and endpoint names that repeat. Invalid types are skipped, so the generated registry always compiles. Types and assemblies marked `[ExcludeFromMapping]` are not analyzed, except by CSE1010 and CSE1011.

| ID | Severity | Rule |
|---|---|---|
| CSE1001 | Error | Endpoint or group type is not accessible from generated code (private or protected nested, `file`-local, or inside an inaccessible type). |
| CSE1002 | Error | The `[EndpointGroup]` chain returns to the same group. Reported on each group in the cycle. |
| CSE1003 | Error | A type has both `[EndpointGroup(typeof(X))]` and `[EndpointGroup<Y>]`. |
| CSE1004 | Warning | Endpoint declares instance fields, auto-properties or a constructor with parameters. Has a code fix. |
| CSE1005 | Warning | Two endpoints in one group map the same HTTP method and constant route (`MapGet`, `MapPost`, `MapPut`, `MapDelete`, `MapPatch`, `MapMethods`). Ungrouped endpoints count as one group. |
| CSE1006 | Info | Abstract or open-generic endpoint or group type is skipped. |
| CSE1007 | Error | The `[EndpointGroup]` target does not implement `IEndpointGroup`, or is abstract, open-generic or a ref struct. |
| CSE1008 | Error | Endpoint or group type is a `ref struct`, which cannot be a generic type argument. |
| CSE1009 | Warning | Two referenced assemblies produce the same registry name (`Foo.Api` and `FooApi` both give `FooApiEndpointRegistry`). `MapAllEndpoints` skips both; when the own registry shares the name, only the own one is mapped. Fix with `[assembly: EndpointRegistryName("...")]`. Reported only where the aggregate is generated. |
| CSE1010 | Warning | The same constant endpoint name is set at two call sites. Endpoint names and route names are compared separately. Reported on each call site. |
| CSE1011 | Info | An explicit endpoint name outside an endpoint or group type is not a constant, so it cannot be reserved. |
| CSE1012 | Warning | A non-partial endpoint class inherits `Map` and does not declare it, so it is not mapped. See [Inheriting `Map`](#inheriting-map). |
| CSE1013 | Warning | A `Map*` handler returns `Result<T, E1..E3>`, or a `Task` or `ValueTask` of it, without converting it. Has a code fix. |

### Code fixes

- CSE1004, "Remove instance state from '{Type}'": deletes the instance fields, auto-properties and parameterized constructors. Offered only when nothing else in the solution uses them, and not for records.
- CSE1013, "Convert with ToHttpResult()": appends `.ToHttpResult()` to what a lambda returns, including every `return` of a block body, and wraps an `await` in parentheses (`(await GetAsync(id)).ToHttpResult()`). Adds `using CSharpEssentials.AspNetCore;` when needed. Offered only when the project references `CSharpEssentials.AspNetCore`, and not for method groups or lambdas with an explicit return type. Use `.ToOk()`, `.ToCreated(...)` or `.ToNoContent()` by hand for another success status.

## Migrating from Carter

Carter creates module instances at startup and scans assemblies. Here nothing is constructed or scanned: the registry calls static methods.

| Carter | CSharpEssentials.Endpoints |
|---|---|
| `builder.Services.AddCarter()` | Nothing to register |
| `app.MapCarter()` | `app.MapAllEndpoints()` or `app.Map{Asm}Endpoints()` |
| `ICarterModule.AddRoutes(app)` | `static void Map(IEndpointRouteBuilder app)` on an `IEndpoint` |
| `CarterModule` base path and conventions | An `IEndpointGroup` plus `[EndpointGroup<TGroup>]` on each endpoint |
| Constructor injection into the module | Handler parameters |
| Module lists in the configurator | `options.Filter(...)` or `[ExcludeFromMapping]` |
| FluentValidation helpers | `.WithValidation<T>()` with `CSharpEssentials.Validation` |
| Response negotiators | `IResult` and `TypedResults` |

A Carter module:

```csharp
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

becomes a group and one endpoint type per route (a type can also keep several related routes):

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

Replace `AddCarter()` and `MapCarter()` with `app.MapAllEndpoints()` and remove the Carter package. The next build reports state left on endpoint types (CSE1004) and duplicate routes in a group (CSE1005).

## Performance

Measured on net10 against plain Minimal APIs, Carter and FastEndpoints; full tables in [BENCHMARKS.md](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Endpoints/BENCHMARKS.md).

- Cold startup ties Carter at 10, 100 and 500 endpoints and ties plain Minimal APIs at 10; at 100 and 500 it takes 1.05× and 1.07× the plain time. Against FastEndpoints it takes 23% less time at 10 endpoints and 1.62× as long at 500.
- Per request it ties plain Minimal APIs on every route. Carter ties on GET and DELETE, takes 10% less time on a valid POST and 1.32× as long on an invalid POST. FastEndpoints takes 1.26× (GET), 1.27× (DELETE), 1.24× (valid POST) and 2.06× (invalid POST) the CSE time.
- The POST rows compare a DataAnnotations stack (Plain, CSE) with FluentValidation stacks (Carter, FastEndpoints). FastEndpoints defers per-endpoint setup to the first request, so only the cold startup figures are like-for-like.
- The generated path publishes as Native AOT with 0 IL warnings.

Design-time cost of the generator against Immediate.Apis is in [the generator BENCHMARKS.md](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Endpoints.Generators/BENCHMARKS.md).

## See also

- Agent skill: [csharpessentials-endpoints](https://github.com/senrecep/CSharpEssentials/blob/main/.well-known/agent-skills/csharpessentials-endpoints/SKILL.md)
- Example: [examples/Examples.Endpoints](https://github.com/senrecep/CSharpEssentials/tree/main/examples/Examples.Endpoints), a Native AOT app that also uses `CSharpEssentials.DependencyInjection`
- Related: [CSharpEssentials.AspNetCore](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.AspNetCore/Readme.MD) (typed result conversion, validation filter, versioning), [CSharpEssentials.DependencyInjection](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.DependencyInjection/Readme.MD)
