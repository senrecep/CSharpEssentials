# CSharpEssentials.Results.Testing example

An xUnit project that tests a small `OrderService` with every helper of `CSharpEssentials.Results.Testing`. It needs no database, network or web host: the problem details tests build their `HttpResponseMessage` by hand.

## Run

```bash
cd examples/Examples.Results.Testing
dotnet test
```

The project targets net10.0.

## Files

| File | What it holds |
|---|---|
| `OrderService.cs` | The code under test. `Create` returns `Result<Order>`, `Delete` returns `Result`, `Find` returns `Maybe<Order>`, and `Reserve` returns the typed `Result<Order, NotFoundError, ConflictError>`. |
| `OrderServiceTests.cs` | One test per helper. |

## What the tests show

| Test | Helpers |
|---|---|
| `Create_returns_the_order` | `ShouldBeSuccess()` on `Result<T>` returns the value |
| `Create_without_a_name_fails_with_a_validation_error` | `ShouldBeFailure()`, `ShouldHaveError(type, code)` |
| `Delete_of_a_missing_order_fails_with_not_found` | `ShouldBeSuccess()` on `Result`, `ShouldHaveError(type)`, `ShouldHaveError(code)` |
| `Find_returns_a_maybe` | `ShouldHaveValue()`, `ShouldBeNone()` |
| `Reserve_returns_typed_errors` | `ShouldBeSuccess()` and `ShouldBeFailureOf<TError>()` on a typed result |
| `A_wrong_case_throws_with_the_case_in_the_message` | the `ResultAssertionException` message for the wrong typed case |
| `A_failed_success_assertion_lists_every_error` | the message that lists each error |
| `Validation_problem_has_every_error_key` | `ShouldBeValidationProblemAsync(keys)` |
| `Problem_has_status_and_code` | `ShouldBeProblemAsync(status, code)` with `errorCodes` |
| `Wrong_status_throws_with_request_and_body` | the message with the request, status and body |

See the [package Readme](https://github.com/senrecep/CSharpEssentials/blob/main/CSharpEssentials.Results.Testing/Readme.MD).
