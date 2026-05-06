---
name: csharpessentials-any
description: Use when a method can return one of several distinct types — Any<T1,T2> as a type-safe discriminated union, implicit assignment from any branch type, exhaustive Match() to handle all cases, and Is<T>/As<T> for type inspection.
---

# CSharpEssentials.Any

`Any<T1,T2,...>` is a discriminated union — a value that is exactly one of several possible types at runtime. Replaces `object`-typed returns and eliminates unsafe casting.

## Installation

```bash
dotnet add package CSharpEssentials.Any
```

## Namespace

```csharp
using CSharpEssentials.Any;
```

## Creating Any

```csharp
// Implicit assignment — just assign the value
Any<User, NotFoundError> result = user;
Any<User, NotFoundError> result = new NotFoundError("User not found");

// Up to Any<T1,T2,T3,T4> supported
Any<Order, ValidationError, NotFoundError> outcome = order;
```

## Exhaustive Match

```csharp
// All branches must be handled — compile error if one is missing
IResult response = result.Match(
    whenT0: u   => Ok(u),
    whenT1: err => NotFound(err.Message));

// Async match
IResult response = await result.MatchAsync(
    whenT0: async u   => await BuildOkResponseAsync(u),
    whenT1: async err => await BuildErrorResponseAsync(err));
```

## Type Inspection

```csharp
if (result.Is<User>())
{
    User user = result.As<User>();  // safe after Is<T>() check
}
```

## Typical Usage — service return type

```csharp
public Any<Order, ValidationErrors, NotFoundError> PlaceOrder(PlaceOrderRequest request)
{
    if (!_validator.IsValid(request))
        return new ValidationErrors(request.Errors);

    var cart = _repo.FindCart(request.CartId);
    if (cart is null)
        return new NotFoundError("Cart not found");

    return _orderFactory.Create(cart);
}

// At API boundary
var result = _service.PlaceOrder(request);
return result.Match(
    whenT0: order => Created($"/orders/{order.Id}", order),
    whenT1: errs  => BadRequest(errs),
    whenT2: err   => NotFound(err.Message));
```

## Best Practices

- Use `Any<T1,T2>` over `Result<T>` when the error branches carry distinct, typed data
- Always use `Match()` — it enforces exhaustiveness at compile time
- `Is<T>()` + `As<T>()` is the escape hatch for cases where `Match()` is too verbose
- Avoid `object`-typed union members — defeats the purpose
