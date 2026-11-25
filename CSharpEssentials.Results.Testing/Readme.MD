# CSharpEssentials.Results.Testing

Assertion helpers for `Result`, `Result<T>`, the typed `Result<T, E1..E3>`, `Maybe<T>` and problem details HTTP responses. A failed assertion throws `ResultAssertionException`, and its message lists every error with its type, code, description and metadata. The package references no test framework, so it works from xUnit (v2 or v3), NUnit, MSTest or a console check, next to any assertion library.

## Installation

```bash
dotnet add package CSharpEssentials.Results.Testing
```

Targets `net11.0`, `net10.0`, `net9.0` and `netstandard2.1`. It depends only on `CSharpEssentials.Results` and `CSharpEssentials.Maybe`. The problem details helpers read the body with `System.Text.Json`, so they need no ASP.NET Core reference.

```csharp
using CSharpEssentials.ResultPattern.Testing;
```

## Results

```csharp
OrderDto order = (await service.GetAsync(id)).ShouldBeSuccess();

Error[] errors = service.Create(invalid).ShouldBeFailure();

Error error = service.Create(invalid).ShouldHaveError("Order.Name.Required");
service.Create(invalid).ShouldHaveError(ErrorType.Validation);
service.Create(invalid).ShouldHaveError(ErrorType.Validation, "Order.Name.Required");
```

When a result is not what the test expected, the message shows every error:

```text
Expected a success, but the result failed with 2 errors:
  [0] Validation Name.Required: Name is required. metadata [{ field: Name }]
  [1] NotFound User.NotFound: User 7 was not found.
```

| Member | What it does |
|--------|-------------|
| `ShouldBeSuccess()` | On `Result`: passes on success. On `Result<T>` and the typed results: returns the value |
| `ShouldBeFailure()` | Returns the errors. A typed result gives its one error converted to an `Error`. On success, the message shows the value of `Result<T>` and the typed results |
| `ShouldHaveError(code)` / `(type)` / `(type, code)` | Returns the first error that matches. The code is compared ordinally |
| `ShouldBeFailureOf<TError>()` | Typed results only: returns the error when the result holds a `TError` (a slot type or a base type of one) |

`default(Result<T>)` fails with the `Result.Default` error, as at runtime. A `default` typed result holds no case, and the message says so.

## Typed results

```csharp
Result<OrderDto, NotFoundError, ConflictError> result = await service.ReserveAsync(id);

ConflictError conflict = result.ShouldBeFailureOf<ConflictError>();
```

When the result holds another case, the message names it:

```text
Expected an error of type NotFoundError, but the result holds the error ConflictError { Code = Order.Taken, ... }.
```

`Result` and `Result<T>` have no `ShouldBeFailureOf`. They hold `Error` values, never `NotFoundError` or another typed error, so a type check could only match `Error` itself. Use `ShouldHaveError(type)` or `ShouldHaveError(code)` on them instead.

## Maybe

```csharp
Customer customer = repository.Find(id).ShouldHaveValue();
repository.Find(missingId).ShouldBeNone();
```

## Problem details responses

```csharp
using HttpResponseMessage response = await client.PostAsJsonAsync("/orders", new CreateOrder(""));

JsonElement problem = await response.ShouldBeValidationProblemAsync("Name.NotEmpty");

using HttpResponseMessage missing = await client.GetAsync("/orders/42");
await missing.ShouldBeProblemAsync(404, "Order.NotFound");
```

| Member | What it does |
|--------|-------------|
| `ShouldBeProblemAsync(status?, code?)` | Checks the `application/problem+json` content type, the status and an error code, and returns the body as a `JsonElement`. The code is looked up in `errorCodes` and in `errors` |
| `ShouldBeValidationProblemAsync(params keys)` | Checks for a 400 problem whose `errors` has every key: the codes of a list (`ValidationErrorsFormat.List`) or the property names of an object (`Dictionary` or `ByField`) |

The message shows the request method and URI, the status and the body:

```text
Expected status 404, but it was 400.
POST http://localhost/orders returned 400 BadRequest with body:
{"status":400,"title":"Validation Error","errors":[{"code":"Name.NotEmpty","description":"'Name' must not be empty."}]}
```

The helpers read the field names that `CSharpEssentials.AspNetCore` writes (`errorCodes`, `errors` and the `code` of each list entry), so they also work for any RFC 9457 producer that uses the same names.
