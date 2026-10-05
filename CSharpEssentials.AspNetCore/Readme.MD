# CSharpEssentials.AspNetCore

Production-ready ASP.NET Core utilities: global exception handling, structured ProblemDetails, API versioning, Swagger setup, and Result endpoint filters.

## Features

- **Enhanced Problem Details** — One configurable ProblemDetails pipeline for Minimal API, MVC, the exception handler and status code pages. It has secure defaults and the extension points `IProblemDetailsEnricher`, `IErrorStatusCodeMapper` and `IExceptionProblemMapper`.
- **Global Exception Handler** — Maps exceptions to ProblemDetails. Unknown exceptions return 500, and the exception message goes to the log, not to the response.
- **Result Endpoint Filter** — Converts `Result<T>` return values to `200 OK` or a ProblemDetails response.
- **Enum Binding** — Query and route values for `[StringEnum]` enums accept the same spellings as JSON. Invalid values return 400.
- **API Versioning** — Pre-configured URL segment and header-based versioning.
- **Swagger** — Easy setup with versioning support, enum schema filters, and custom schema IDs.

## Installation

```bash
dotnet add package CSharpEssentials.AspNetCore
```

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

`services.ConfigureInvalidModelStateResponse()` writes the automatic 400 of `[ApiController]` (invalid model state) through the same pipeline, with the model state key as the error code.

`errors.ToProblemResult()` (Minimal API) and `errors.ToActionResult(HttpContext)` (MVC) read these options when they execute, so both produce the same JSON.

### API Versioning

```csharp
builder.Services.AddAndConfigureApiVersioning();

var app = builder.Build();
var usersGroup = app.CreateVersionedGroup("users", version: 1);
usersGroup.MapGet("/{id}", (int id) => Results.Ok(new { Id = id }));
```

### Swagger with Versioning

```csharp
builder.Services.AddSwagger<ConfigureSwaggerOptions>(securityScheme: SecuritySchemes.Bearer);

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

### Enum Query/Route Binding

```csharp
builder.Services.AddEnumBinding(o => o.AllowIntegerValues = true); // optional
app.UseEnumBinding(); // after routing, UseAuthentication and UseAuthorization (otherwise a 400 hides the 401)

// [StringEnum] enum OrderStatus { InProgress, Done }
// ?status=in_progress, ?status=InProgress, ?status=inprogress, ?status=0 all bind to OrderStatus.InProgress
app.MapGet("/orders", (OrderStatus? status) => ...);
```

This works for Minimal API (including `[AsParameters]`) and MVC (`[FromQuery]`, `[FromRoute]`, and query DTO properties, nested up to 8 levels: `?filter.inner.status=…`).

- Nullable, array, `List<T>` and `[Flags]` parameters are supported. Flags values are a comma-separated list.
- These values return a 400 ProblemDetails response:
  - undefined numbers;
  - unknown names;
  - commas on enums without `[Flags]`;
  - empty values for non-nullable parameters.
- The error code is the query/route key (`status`, `filter.status`). Change the code or message with `o.ErrorFactory = (key, enumType, names) => Error.Validation($"validation.{key}", ...)`.
- Empty values of nullable or collection parameters are dropped, so they bind as `null` or are skipped.
- Matched values are rewritten to the C# member name before binding. Link generation that reuses the current route values (ambient values, such as `CreatedAtRoute` without explicit values) therefore emits the member name (`/items/HttpStatus`). Pass the value explicitly to get the JSON spelling.

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
