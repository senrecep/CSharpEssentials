# CSharpEssentials.AspNetCore

Production-ready ASP.NET Core utilities: global exception handling, structured ProblemDetails, API versioning, Swagger setup, and Result endpoint filters.

## Features

- **Enhanced Problem Details**: One configurable ProblemDetails pipeline for Minimal API, MVC, the exception handler and status code pages. It has secure defaults and the extension points `IProblemDetailsEnricher`, `IErrorStatusCodeMapper` and `IExceptionProblemMapper`.
- **Global Exception Handler**: Maps exceptions to ProblemDetails. Unknown exceptions return 500, and the exception message goes to the log, not to the response.
- **Result Endpoint Filter**: Converts `Result<T>` return values to `200 OK` or a ProblemDetails response.
- **Validation Endpoint Filter**: Runs `CSharpEssentials.Validation` validators for a handler argument and returns a 400 ProblemDetails response on failure.
- **Enum Binding**: Query and route values for `[StringEnum]` enums accept the same spellings as JSON. Invalid values return 400.
- **API Versioning**: Pre-configured URL segment and header-based versioning.
- **Swagger**: Setup with versioning support, enum schema filters, and custom schema IDs.

## Installation

```bash
dotnet add package CSharpEssentials.AspNetCore
```

Depends on `CSharpEssentials.Errors`, `CSharpEssentials.Json`, `CSharpEssentials.Results` and `CSharpEssentials.Validation` (for the validation endpoint filter). None of them references this package, so there is no cycle.

## Usage

### Problem Details

```csharp
builder.Services.AddEnhancedProblemDetails(o =>
{
    o.TraceId = TraceIdFormat.W3CTraceId;                 // default
    o.ErrorFields = ProblemErrorFields.Codes | ProblemErrorFields.ValidationErrors; // default
    o.ExposeExceptionDetails = builder.Environment.IsDevelopment();
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Optional extension points
builder.Services.AddProblemDetailsEnricher<TenantEnricher>();     // add fields to every problem response
builder.Services.AddErrorStatusCodeMapper<AuthAwareStatusMapper>(); // Error -> status code / title
builder.Services.AddExceptionProblemMapper<PaymentExceptionMapper>(); // Exception -> problem

var app = builder.Build();
app.UseEnhancedProblemDetails(); // UseExceptionHandler + UseStatusCodePages
```

| Option | Default | Notes |
|---|---|---|
| `TraceId` | `W3CTraceId` | `TraceparentHeader` or `None` |
| `IncludeRequestId` / `IncludeUser` / `IncludeSpanIds` | `false` | |
| `Instance` | `Path` | `MethodAndPath` or `None` |
| `ErrorFields` | `Codes \| ValidationErrors` | `Messages`, `AllErrors`, `All`, `None` |
| `ValidationErrorsFormat` | `List` | `Dictionary` groups descriptions by code |
| `TypeUriResolver` | `ProblemTypeUris.Rfc9110` | `ProblemTypeUris.Rfc7231` or a custom function |
| `ExposeExceptionDetails` | `false` | Enable only in Development |

`o.UseLegacyDefaults()` restores the 3.x output. See the migration notes in the repository README.

With no W3C `Activity` (tracing disabled), `W3CTraceId` falls back to `HttpContext.TraceIdentifier`. `errors` lists only `ErrorType.Validation` errors; every code is in `errorCodes`.

`services.ConfigureInvalidModelStateResponse()` writes the automatic 400 of `[ApiController]` (invalid model state) through the same pipeline, with the model state key as the error code. It can be called before or after `AddControllers()`. It has no effect together with `ConfigureModelValidatorResponse()`, which turns the automatic 400 off (`SuppressModelStateInvalidFilter = true`); use one or the other.

`errors.ToProblemResult()` (Minimal API) and `errors.ToActionResult(HttpContext)` (MVC) read these options when they execute, so both produce the same JSON.

### API Versioning

```csharp
builder.Services.AddAndConfigureApiVersioning();

var app = builder.Build();
var usersGroup = app.CreateVersionedGroup("users", version: 1);
usersGroup.MapGet("/{id}", (int id) => Results.Ok(new { Id = id }));
```

`MapVersionedGroup(version)` creates a group with only the `v{version:apiVersion}` prefix and the version set, so other route groups can be composed under it:

```csharp
var v2 = app.MapVersionedGroup(2);            // /v2, ApiExplorer group "v2"
v2.MapGet("/ping", () => "pong");             // GET /v2/ping
v2.MapAppsEndpoints();                        // CSharpEssentials.Endpoints registry under /v2
```

### Swagger with Versioning

```csharp
builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(securityScheme: SecuritySchemes.JwtBearerTokenSecurity);

var app = builder.Build();
app.UseVersionableSwagger();
```

### Result Endpoint Filter

```csharp
builder.Services.AddScoped<IResultErrorMapper, DomainErrorMapper>(); // optional, any lifetime

app.MapGet("/users/{id}", (int id) => GetUser(id))
   .AddEndpointFilter<ResultEndpointFilter>();

// Returns 200 with value on success; on failure the mapper's result (resolved per request),
// or a ProblemDetails response when no mapper is registered
```

### Validation Endpoint Filter

```csharp
builder.Services.AddValidator<CreateUserRequest, CreateUserRequestValidator>();

app.MapPost("/users", (CreateUserRequest request) => CreateUser(request))
   .WithValidation<CreateUserRequest>();
```

- Validates each non-null argument of type `T` with every registered `IValidator<T>`, in ascending `Order`. Errors are combined and returned with `ToProblemResult()` (validation errors → 400); the handler does not run.
- A `null` argument (optional body) is passed through.
- No registered `IValidator<T>` throws `InvalidOperationException` at request time, so unvalidated input never reaches the handler. A handler without a `T` parameter throws when the endpoint is built.
- `ValidationEndpointFilter<T>` can also be added directly with `AddEndpointFilter<ValidationEndpointFilter<T>>()`, for example on a route group.

### Enum Conventions: JSON, Binding and Wire Format

```csharp
builder.Services.AddEnumConventions(); // JSON options, binding rules, body errors, output filters
app.UseEnumBinding(); // after routing, UseAuthentication and UseAuthorization (otherwise a 400 hides the 401)

// [StringEnum] enum OrderStatus { Pending, PendingApproval }
// ?status=pending_approval, ?status=PENDING_APPROVAL, ?status=PendingApproval, ?status=1 all bind to OrderStatus.PendingApproval
app.MapGet("/orders", (OrderStatus? status) => Results.Ok(status));
```

`AddEnumConventions(c => c with { ... })` takes the `EnumConventions` of `CSharpEssentials.Enums` (`AcceptNumbers`, `CaseInsensitive`, `WriteAs`, `CanHandle`, ...). Route, query, header and form values follow the same accept rules as a JSON body. This works for Minimal API (including `[AsParameters]`) and MVC (`[FromQuery]`, `[FromRoute]`, `[FromHeader]`, `[FromForm]`, and DTO properties, nested up to 8 levels: `?filter.inner.status=…`). `UseEnumBinding()` throws at startup when `AddEnumConventions()` was not called.

- Nullable, array, `List<T>` and `[Flags]` parameters are supported. Arrays accept repeated keys and comma-separated values (`?s=a&s=b`, `?s=a,b`); flags values are a comma-separated list.
- These values return a 400 ProblemDetails response that lists the allowed values (`'99' is not a valid OrderStatus. Allowed values: pending, pending_approval.`):
  - undefined numbers;
  - unknown names, and values with leading or trailing whitespace;
  - commas on enums without `[Flags]`;
  - empty values for non-nullable parameters.
- A missing required value keeps the framework's handling.
- The error code is the key (`status`, `filter.status`, `X-Status`). Change the code or message once for binding and JSON bodies with `.ConfigureErrors((error, key) => Error.Validation($"validation.{key}", error.Message))`.
- Enums without generated metadata (no `[StringEnum]`) keep the framework's binding and output. `AddEnumConventionsWithReflection(c => c with { CanHandle = ... })` opts them in with reflection (not trimming or AOT safe).
- A JSON body error of a Minimal API endpoint (with `RouteHandlerOptions.ThrowOnBadRequest`) is mapped by `GlobalExceptionHandler` to the same 400. MVC reports body errors through model state.
- Empty values of nullable or collection parameters are dropped, so they bind as `null` or are skipped.
- Matched values are rewritten to the C# member name before binding. Link generation that reuses the current route values (ambient values, such as `CreatedAtRoute` without explicit values) therefore emits the member name (`/items/HttpStatus`). Pass the value explicitly to get the JSON spelling.

Legacy numeric output per group, controller or action, in both directions (`WriteAs` is the global default):

```csharp
app.MapGroup("/api/v1").WithEnumWireFormat(EnumWireFormat.Number);  // old clients: 1
app.MapGroup("/api/v2");                                              // "pending_approval"
app.MapGroup("/api/shared").WithEnumWireFormat("X-Enum-Format",       // per request, adds Vary: X-Enum-Format
    ctx => ctx.Request.Headers["X-Enum-Format"] == "string" ? EnumWireFormat.String : EnumWireFormat.Number);

[EnumWireFormat(EnumWireFormat.Number)] // MVC controller or action
public sealed class LegacyOrdersController : ControllerBase { ... }
```

Precedence: action attribute > controller attribute > endpoint or group (`MapControllers().WithEnumWireFormat(...)` for MVC) > `WriteAs`. The host's `JsonOptions` are never changed; the other format is written with a copy. Status codes and headers of the result (`Created` + `Location`, `Results<Ok<T>, NotFound>`) are kept. Reading is not affected.

`AddEnumBinding`/`EnumBindingOptions` are obsolete forwarders to `AddEnumConventions` (see the v4 to v5 migration guide).

### Structured Error Response

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Email format is incorrect.",
  "instance": "/users",
  "traceId": "0af7651916cd43dd8448eb211c80319c",
  "errorCodes": ["User.InvalidEmail"],
  "errors": [
    { "code": "User.InvalidEmail", "description": "Email format is incorrect." }
  ]
}
```
