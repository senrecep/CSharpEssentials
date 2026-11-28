# CSharpEssentials.AspNetCore example

A Minimal API and MVC app that uses the main features of [CSharpEssentials.AspNetCore](../../CSharpEssentials.AspNetCore/Readme.MD). Running it starts the app on a free local port, sends a request to each feature, prints the status and body, and exits.

## Run

```bash
cd examples/Examples.AspNetCore
dotnet run              # tour: prints each request and response, then exits
dotnet run -- --serve   # keeps the app running on http://localhost:5000
```

The `/boom` request logs the exception with its message; the response does not contain it.

## Files

| File | Contents |
|---|---|
| `Program.cs` | Service registration, middleware order, the endpoints and the tour |
| `Orders.cs` | `OrderStatus` (`[StringEnum]`), `Order` (`IVersioned`), `CreateOrderValidator`, an in-memory `OrderStore` returning `Result<Order, NotFoundError>` and `Result<Order>` |
| `Controllers/MvcOrdersController.cs` | MVC action with `[ConditionalGet]` and `this.Problem(error)`, and a model that fails `[Required]`/`[Range]` |

## What each request shows

| Request | Feature | Result |
|---|---|---|
| `GET /v1/orders?status=PENDING_APPROVAL` | `AddEnumConventions`, `UseEnumBinding`, `MapVersionedGroup(1)` | 200, any spelling binds |
| `GET /v1/orders?status=lost` | Enum binding error | 400 listing the allowed values |
| `GET /v1/orders/{id}` | `ToHttpResult()` on `Result<Order, NotFoundError>`, `WithConditionalGet()` | 200 with `ETag: "1"` |
| same, `If-None-Match: "1"` | Conditional GET | 304 |
| `GET /v1/orders/{empty guid}` | Typed error to problem | 404 problem |
| `PUT /v1/orders/{id}` without `If-Match` | `WithIfMatch(required: true)` | 428 |
| same, `If-Match: "7"` | `GetPreconditions().Matches(order)`, `ConditionalRequestErrors.PreconditionFailed` | 412 |
| same, `If-Match: "1"` | `ResultEndpointFilter` on `Result<Order>` | 200, revision 2 |
| `POST /v1/orders` with an empty customer | `WithValidation<CreateOrder>()` | 400 with both validation errors |
| `POST /v1/orders` with `Idempotency-Key: order-1`, three times | `AddIdempotency`, `UseIdempotency`, `WithIdempotency(...)` | 201, then 201 replayed with `Idempotency-Replayed: true`, then 422 for a different body |
| `GET /legacy/orders` | `WithEnumWireFormat(EnumWireFormat.Number)` | `"status": 2` instead of `"shipped"` |
| `GET /boom` | `GlobalExceptionHandler`, `UseEnhancedProblemDetails` | 500 problem without the exception message |
| `GET /nothing-here` | Status code pages | 404 problem |
| `GET /mvc/orders/{empty guid}` | `this.Problem(error)` | 404 problem |
| `POST /mvc/orders` | `ConfigureInvalidModelStateResponse()` | 400 problem with `errorCodes: ["Customer", "Quantity"]` |

The idempotent endpoint sets `UnscopedKeyBehavior.Global` because the example has no authentication. With the default `Skip`, anonymous requests run without idempotency.

## Sample output

```text
# Idempotency: same key, same body: stored response replayed
POST /v1/orders -> 201 Idempotency-Replayed: true
{"id":"34435dcb-4580-42b4-86b8-6b7bce923ab3","customer":"bob","quantity":1,"status":"pending","revision":1,"version":"1"}

# If-Match stale: 412
PUT /v1/orders/11111111-1111-1111-1111-111111111111 -> 412
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.13","title":"Precondition Failed","status":412,"detail":"The resource has changed since it was read. Fetch it again and retry with its current ETag.","instance":"/v1/orders/11111111-1111-1111-1111-111111111111","errorCodes":["Http.PreconditionFailed"],"traceId":"fa6b1730a99c8861d7d4829592d267bc"}
```

OpenAPI for the same features is shown in [Examples.AspNetCore.OpenApi](../Examples.AspNetCore.OpenApi) and [Examples.AspNetCore.Swashbuckle](../Examples.AspNetCore.Swashbuckle).
