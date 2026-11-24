# Migrating from 8.x to 9.0

## Breaking changes

- `Error.Exception(...)` defaults to `ErrorType.Unexpected` instead of `ErrorType.Failure`, in all three overloads.
- `ErrorType` has a new member, `Domain` (numeric value 8), which maps to HTTP 422. The values of the existing members do not change.
- `CSharpEssentials.Http` maps a 422 response to `ErrorType.Domain` instead of `ErrorType.Validation`.
- `ErrorType.Unknown` is the `[EnumFallback]` member: data reads map a type name or number they do not know to `Unknown`.

## Exceptions become `Unexpected` (`CSharpEssentials.Errors`)

`Error.Exception(ex)`, `Error.Exception(code, ex)` and `Error.Exception(code, description, ex)` now give `ErrorType.Unexpected` when you do not pass a type. The status stays 500, so the HTTP response does not change. What changes is everything that reads the type:

- The problem title becomes `Unexpected Error` instead of `Server Failure`.
- `Result.TryCatch`, the Mediator `ExceptionHandlingBehavior`, a throwing validator in `ValidationBehavior` and a throwing rule in `CSharpEssentials.Rules` return `Unexpected` errors.
- The Mediator `IdempotencyBehavior` keeps a stored failure whose first error is `Unexpected` for the failure retention (`IdempotencyExecutorOptions.FailureRetention`), not for the success retention. An exception caught by `ExceptionHandlingBehavior` now falls under that shorter retention, so the request can be retried sooner.
- Code that tested `error.Type == ErrorType.Failure` to find exceptions should test `ErrorType.Unexpected`, or check the `exceptionType` metadata key.

The default is a compile-time value. An assembly compiled against 8.x keeps passing `Failure` until it is recompiled. Pass the type explicitly to keep the old behavior:

```csharp
Error error = Error.Exception(ex, ErrorType.Failure);
```

## `ErrorType.Domain` and HTTP 422

`Domain` means a business rule rejected a well-formed request: a declined payment, an order that can no longer be cancelled. Create it with `Error.Domain(code, description, metadata)`.

| Where | 8.x | 9.0 |
|---|---|---|
| `ErrorType.Domain.ToHttpStatusCode()` | (no member) | 422 |
| `422.ToErrorType()` | `Unexpected` | `Domain` |
| `HttpStatusCodeMapper.ToErrorType(HttpStatusCode.UnprocessableEntity)` | `Validation` | `Domain` |
| `HttpStatusCodeMapper.ToError(HttpStatusCode.UnprocessableEntity)` | `Error.Validation` | `Error.Domain` |
| Problem title | | `Domain Error` |
| Resilience retry | | not retried, like `Validation` |

5xx descriptions are hidden by the AspNetCore problem details unless `ExposeExceptionDetails` is on. Business-rule rejections that used `Failure` so far reached the client as a 500 with a generic message. Move them to `Error.Domain` if the client should read the description.

When a response carries several errors, the default `DefaultErrorStatusCodeMapper.SelectPrimaryError` picks the one with the highest status, so a `Domain` error (422) wins over `Conflict` (409) and the 4xx below it, and loses to any 5xx.

An HTTP client that handled a 422 as `ErrorType.Validation` must match `ErrorType.Domain` instead. `IdempotentRetry` only retries a 409 with the code `Idempotency.InFlight`, so it is not affected.

## Payloads written by another version

`ErrorType` is a `[StringEnum]`, and an `Error` carries both `Type` and `NumericType`.

- 8.x payloads read by 9.0: every 8.x name and number still exists, so nothing changes.
- 9.0 payloads read by 8.x: 8.x does not know `Domain` or the number 8. Readers that use the enum conventions of `CSharpEssentials.Json` in `Data` mode reject the unknown name, because 8.x has no fallback member. A reader that takes the plain number gets the undefined value 8, which the 8.x `ToHttpStatusCode` maps to 500. Roll the 9.0 packages out to every reader of cached, stored or idempotency payloads before a writer starts producing `Domain` errors.
- From 9.0, a name or number a reader does not know maps to `Unknown` (HTTP 500) in `Data` mode, so the next new member will not break readers.
