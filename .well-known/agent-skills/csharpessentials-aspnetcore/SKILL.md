---
name: csharpessentials-aspnetcore
description: Use when wiring CSharpEssentials into ASP.NET Core. Covers AddEnhancedProblemDetails/UseEnhancedProblemDetails with GlobalExceptionHandler, ToProblemResult/ToActionResult/ToProblemDetails, typed ToHttpResult/ToCreated, ResultEndpointFilter, WithValidation, AddIdempotency/UseIdempotency/WithIdempotency/[Idempotent], WithConditionalGet/WithIfMatch, AddEnumConventions/UseEnumBinding/WithEnumWireFormat, AddAndConfigureApiVersioning/MapVersionedGroup, and the OpenAPI helpers AddEnumConventions, AddIdempotencyConventions and AddBearerSecurityScheme.
---

# CSharpEssentials.AspNetCore

Turns `Error`/`Result` values and exceptions into RFC 9457 problem responses, and adds typed HTTP results, request validation, `Idempotency-Key` handling, `ETag`/`If-Match`, `[StringEnum]` binding and API versioning.

## Install

```bash
dotnet add package CSharpEssentials.AspNetCore
# OpenAPI, pick one:
dotnet add package CSharpEssentials.AspNetCore.OpenApi        # Microsoft.AspNetCore.OpenApi, net10.0
dotnet add package CSharpEssentials.AspNetCore.Swashbuckle    # Swashbuckle 10
```

## Namespaces

| Types | Namespace |
|---|---|
| Every AspNetCore, OpenApi and Swashbuckle extension and type below | `CSharpEssentials.AspNetCore` |
| `Error`, `ErrorType`, `ErrorMetadata`, `NotFoundError`, `ConflictError`, `ValidationError` | `CSharpEssentials.Errors` |
| `Result`, `Result<T>`, `Result<T, E1>` | `CSharpEssentials.ResultPattern` |
| `IdempotencyMode`, `IdempotencyScopes`, `UnscopedKeyBehavior`, `IdempotencyErrorCodes`, `IIdempotencyRecordStore` | `CSharpEssentials.Idempotency` |
| `[StringEnum]`, `EnumWireFormat`, `EnumConventions` | `CSharpEssentials.Enums` |
| `Validator<T>`, `RuleContext<T>` / `NotEmpty()` etc. / `AddValidator` | `CSharpEssentials.Validation` / `.Validators` / `.Extensions` |
| `EntityTagHeaderValue` | `Microsoft.Net.Http.Headers` |
| `SwashbuckleSchemaIdFactory`, `EnumSchemaFilter` | `CSharpEssentials.AspNetCore.Swagger.Filters` |
| `SwaggerGenOptions` | `Swashbuckle.AspNetCore.SwaggerGen` |

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.AspNetCore
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEnhancedProblemDetails(o => o.ExposeExceptionDetails = builder.Environment.IsDevelopment());
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

WebApplication app = builder.Build();
app.UseEnhancedProblemDetails();

app.MapGet("/users/{id:int}", (int id) => FindUser(id).ToHttpResult());          // 200 or 404 problem

RouteGroupBuilder api = app.MapGroup("/api").AddEndpointFilter<ResultEndpointFilter>();
api.MapGet("/orders/{id:int}", Result<string> (int id) =>
    id > 0 ? $"order {id}" : Error.Validation("order.id", "The id must be positive."));  // 200 or 400 problem

app.MapGet("/legacy/{id:int}", (int id) =>
    Error.NotFound("item.not_found", $"Item {id} was not found.").ToProblemResult());

app.Run();

static Result<User, NotFoundError> FindUser(int id) =>
    id == 1 ? new User(1, "Ada") : new NotFoundError("user.not_found", $"User {id} was not found.");

public sealed record User(int Id, string Name);
```

Problem JSON outside a request (default options, no `instance` or `traceId`):

```csharp Program.cs
// dotnet add package CSharpEssentials.AspNetCore
using System.Text.Json;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

Result<int> failed = Error.Domain("order.closed", "The order is closed.");
EnhancedProblemDetails problem = failed.ToProblemDetails();
Console.WriteLine(JsonSerializer.Serialize(problem));
// {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.21","title":"Domain Error","status":422,"detail":"The order is closed.","errorCodes":["order.closed"]}
```

## Problem details

```csharp
builder.Services.AddEnhancedProblemDetails(o =>
{
    o.ExposeExceptionDetails = builder.Environment.IsDevelopment();  // default false
    o.ValidationErrorsFormat = ValidationErrorsFormat.ByField;       // default List
    o.ErrorFields = ProblemErrorFields.Codes | ProblemErrorFields.ValidationErrors; // default
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
app.UseEnhancedProblemDetails();   // = UseExceptionHandler() + UseStatusCodePages()
```

Options: `TraceId` (`TraceIdFormat.W3CTraceId`), `IncludeRequestId`/`IncludeUser`/`IncludeSpanIds` (false), `Instance` (`ProblemInstanceFormat.Path`), `ErrorFields`, `ValidationErrorsFormat` (`List`, `Dictionary`, `ByField`), `TypeUriResolver` (`ProblemTypeUris.Rfc9110`), `ExposeExceptionDetails`. `o.UseLegacyDefaults()` turns on the older verbose output: `TraceparentHeader`, every `Include*` flag, `MethodAndPath`, `ProblemErrorFields.All` and `ProblemTypeUris.Rfc7231`.

Status by `ErrorType`: Validation 400, Unauthorized 401, Forbidden 403, NotFound 404, Conflict 409, Domain 422, Failure/Unexpected/Unknown 500. The highest status among several errors wins. `Http.PreconditionFailed` maps to 412 and the `Idempotency.*` codes to 400/409/422 whatever their type.

Responses with status 500 or more hide the description and exception metadata unless `ExposeExceptionDetails` is true.

### Returning problems

| Call | Returns | Use in |
|---|---|---|
| `error.ToProblemResult(extensions?, statusCode?)` | `IResult` | Minimal API |
| `error.ToActionResult(httpContext?, extensions?, statusCode?)` | `IActionResult` | MVC |
| `this.Problem(error)` | `IActionResult` | `ControllerBase` |
| `error.ToProblemDetails(extensions?, statusCode?)` | `EnhancedProblemDetails` | Anywhere (default options) |
| `.ProducesProblem(404)` / `.ProducesProblem<TProblem>(404)` | `RouteHandlerBuilder` | OpenAPI metadata |

Each works on `Error`, `Error[]` and a failed `Result`/`Result<T>`. Calling them on a successful result throws `InvalidOperationException`.

### Extension points

```csharp
builder.Services.AddProblemDetailsEnricher<TenantEnricher>();          // IProblemDetailsEnricher.Enrich(ProblemDetailsContext)
builder.Services.AddErrorStatusCodeMapper<AuthAwareStatusMapper>();    // replaces IErrorStatusCodeMapper; derive from DefaultErrorStatusCodeMapper
builder.Services.AddExceptionProblemMapper<TimeoutExceptionMapper>();  // IExceptionProblemMapper.TryMap(HttpContext, Exception, out ExceptionProblem?)
builder.Services.AddEnhancedProblemDetails((problem, http) => problem.Extensions["region"] = "eu");  // delegate enricher
```

Each `Add*` takes an optional `ServiceLifetime` (default `Singleton`). `DefaultErrorStatusCodeMapper` virtuals: `GetStatusCode`, `SelectPrimaryError`, `GetTitle`, `GetDeclaredStatusCodes`. `ExceptionProblem(StatusCode?, Title?, Detail?, Errors?)`.

### MVC model state

```csharp
builder.Services.AddControllers();
builder.Services.ConfigureInvalidModelStateResponse();   // [ApiController] 400 as an enhanced problem, code = model state key
```

`ConfigureModelValidatorResponse()` is the alternative: it disables the automatic 400 and adds `[ValidateModel]` (code `validation.{key}`). Never call both.

## Typed results

```csharp
app.MapGet("/orders/{id:int}", async (int id, IOrderService orders, CancellationToken ct) =>
    await orders.GetAsync(id, ct).ToHttpResult());               // Task<Result<OrderDto, NotFoundError>>

app.MapPost("/orders", (CreateOrder command, IOrderService orders) =>
    orders.Create(command).ToCreated(o => $"/orders/{o.Id}"));
```

`ToHttpResult()`, `ToOk()`, `ToHttpResult(v => TypedResults.Accepted(uri, v))`, `ToCreated(v => location)`, `ToNoContent()`. They exist only for `Result<T, E1>`, `Result<T, E1, E2>` and `Result<T, E1, E2, E3>` (and their `Task`/`ValueTask`); a plain `Result<T>` uses `ResultEndpointFilter` or `ToProblemResult()`. Error slots must implement `IError`. The OpenAPI document gets one problem response per slot with `[ErrorKind]` (`NotFoundError`, `ConflictError`, ...).

## ResultEndpointFilter

```csharp
RouteGroupBuilder api = app.MapGroup("/api").AddEndpointFilter<ResultEndpointFilter>();
api.MapGet("/users/{id:guid}", Result<User> (Guid id) => Error.NotFound("user.not_found", "User not found."));
```

Success writes 200 with the value (an empty 200 for a plain `Result`); failure writes a problem. Register `IResultErrorMapper` (`IResult Map(Error[] errors)`, any lifetime) to replace the problem response.

## Validation

```csharp
builder.Services.AddValidator<CreateUser, CreateUserValidator>();   // CSharpEssentials.Validation.Extensions
app.MapPost("/users", (CreateUser request) => Results.Ok(request)).WithValidation<CreateUser>();
// group: group.AddEndpointFilter<ValidationEndpointFilter<CreateUser>>()
```

Runs every `IValidator<T>`; failure returns 400 before the handler. If no `IValidator<T>` is registered, the request throws `InvalidOperationException`.

## Idempotency

```csharp
builder.Services.AddIdempotency(o => { o.RequireKey = true; o.ScopeResolver = IdempotencyScopes.User; });

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseIdempotency();        // after auth

app.MapPost("/orders", CreateOrder).WithIdempotency();
app.MapPost("/payments", Pay).WithIdempotency("payments");                       // profile: o.AddProfile("payments", p => ...)
app.MapPost("/imports", Import).WithIdempotency(o => o.InFlightTimeout = TimeSpan.FromMinutes(10));
// MVC: [HttpPost, Idempotent(Profile = "payments", LeaseSeconds = 300)]
```

- Only endpoints with `WithIdempotency()`/`[Idempotent]` and a method in `Methods` (POST, PATCH by default) take part. PUT and DELETE need `o.Methods.Add("PUT")`.
- Replay: same key and request returns the stored response with `Idempotency-Replayed: true`. In flight: 409 `Idempotency.InFlight`. Other request body: 422 `Idempotency.KeyReused`. Missing key with `RequireKey`: 400 `Idempotency.KeyMissing`.
- Anonymous callers have no scope. With the default `UnscopedKeyBehavior.Skip` they run without idempotency. Use `IdempotencyScopes.Client` for client-credential callers, `UnscopedKeyBehavior.Global` or `Reject` otherwise.
- Stores: in-memory by default, `o.UseDistributedCacheStore()`, `o.UseStore<TStore>()`. Use an atomic store with more than one instance.
- Modes: `IdempotencyMode.Enforce` (default), `Observe` (stores and logs, never blocks), `Off`.
- Parse the header yourself with `IdempotencyKeyHeader.TryParse(header, out string? key)`.

## Conditional requests

```csharp
builder.Services.AddConditionalRequests();                       // o => o.UseBodyHashFallback = true for non-IVersioned values
builder.Services.AddETagSource<Article, ArticleETagSource>();    // optional IETagSource<T>

app.MapGet("/articles/{id:int}", GetArticle).AddEndpointFilter<ResultEndpointFilter>().WithConditionalGet();
app.MapPut("/articles/{id:int}", UpdateArticle).AddEndpointFilter<ResultEndpointFilter>().WithIfMatch(required: true);

// In the PUT handler:
if (!http.GetPreconditions().Matches(current))       // current : IVersioned
    return ConditionalRequestErrors.PreconditionFailed;   // 412
```

- `IVersioned { string Version { get; } }` gives a strong ETag `"{Version}"`.
- Missing `If-Match` with `required: true`: 428. Malformed `If-Match`: 400. Matching `If-None-Match` on GET: 304.
- MVC: `[ConditionalGet]`, `[IfMatch(Required = true)]`.
- OpenAPI: `WithIfMatch`/`[IfMatch]` declare 400, 412 and (when required) 428.

## Enums

```csharp
builder.Services.AddEnumConventions()                                   // required by UseEnumBinding
    .ConfigureErrors((error, key) => Error.Validation($"validation.{key}", error.Message));
app.UseEnumBinding();                                                   // after UseRouting and auth
app.MapGroup("/api/v1").WithEnumWireFormat(EnumWireFormat.Number);      // output as numbers for this group
```

`[StringEnum]` enums bind from route, query, header and form with the JSON spellings (`pending_approval`, `PendingApproval`, `1`). Bad values return 400 listing allowed values. The enum must have `[StringEnum]` and the CSharpEssentials.Enums generator must run.

## API versioning

```csharp
builder.Services.AddAndConfigureApiVersioning();             // default 1.0, URL segment or x-api-version header
RouteGroupBuilder v1 = app.MapVersionedGroup(1);             // /v1
RouteGroupBuilder users = app.CreateVersionedGroup("users", version: 2);   // /v2/users
```

## OpenAPI

```csharp
// CSharpEssentials.AspNetCore.OpenApi (also add a direct Microsoft.AspNetCore.OpenApi reference, else CS9137)
builder.Services.AddOpenApi("v1", o => o
    .AddEnumConventions()
    .AddIdempotencyConventions()
    .AddOptionalRouteParameters()
    .AddBearerSecurityScheme());

// CSharpEssentials.AspNetCore.Swashbuckle
builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>();           // enum + idempotency conventions included
builder.Services.Configure<SwaggerGenOptions>(o => o.AddBearerSecurityScheme());
app.UseVersionableSwagger();
```

`AddBearerSecurityScheme()` marks only operations with `[Authorize]`/`RequireAuthorization()` and no `AllowAnonymous`. `AddSwagger<T>(SecuritySchemes.JwtBearerSchemeName, SecuritySchemes.JwtBearerTokenSecurity)` instead marks every operation.

## Rules

- Use `AddEnhancedProblemDetails()`, not only `AddProblemDetails()`, and call `app.UseEnhancedProblemDetails()`.
- `Error` has `Code` and `Description`. There is no `Error.Message`.
- `Error.NotFound(code, description)` takes the code first. `Error.NotFound("User not found")` sets the code, not the description, and the response shows a default description.
- `ToActionResult` reads the registered options when MVC executes it. Pass `HttpContext` (`error.ToActionResult(HttpContext)`) so the result's `Value` is already the final problem before execution, for example in filters or tests.
- `ToHttpResult()` does not exist on `Result<T>`. Return `Result<T>` with `ResultEndpointFilter`, or use a typed `Result<T, NotFoundError>`.
- Return a typed `Result<T, E>` through `ToHttpResult()`. Without it and without `ResultEndpointFilter`, writing it throws at runtime and the Endpoints analyzer reports `CSE1013`. With the filter it works, but OpenAPI only sees the return type.
- `UseEnumBinding()` throws without `AddEnumConventions()`. `UseIdempotency()` and `UseEnumBinding()` go after `UseRouting()` and auth.
- `WithValidation<T>()` throws when no `IValidator<T>` is registered.
- Reference one OpenAPI package, never both.
- Keep `ExposeExceptionDetails` off outside development.

## Diagnostics

This package reports no diagnostics of its own. `CSE1013` (an endpoint returns a typed result without `ToHttpResult()`) comes from CSharpEssentials.Endpoints.
