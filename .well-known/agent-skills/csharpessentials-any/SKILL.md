---
name: csharpessentials-any
description: Use when a method can return one of several distinct types. Covers Any<T0,T1> through Any<T0,…,T7> as a type-safe discriminated union, implicit assignment from any branch type, Match()/Switch() with one handler per branch, IsFirst/GetFirst, Is/As/TryAs, and Partition/Traverse for sequences.
---

# CSharpEssentials.Any

`Any<T0,T1,...>` is a discriminated union: a value that is exactly one of several possible types at runtime. Replaces `object`-typed returns and eliminates unsafe casting.

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
// Implicit assignment from any branch type
Any<User, NotFoundError> found = user;
Any<User, NotFoundError> missing = new NotFoundError("User not found");

// Explicit factories: First, Second, ... (unions of 2 to 8 types)
Any<Order, ValidationFailure, NotFoundError> outcome = Any<Order, ValidationFailure, NotFoundError>.First(order);
```

## Match and Switch

```csharp
// Match returns AnyActionResult<TResult>: Status says whether a handler ran, Result holds its value.
// Handlers are optional parameters, so a missing handler returns Status = NotExecuted.
AnyActionResult<IResult> response = found.Match(
    first: u => Results.Ok(u),
    second: err => Results.NotFound(err.Message));
IResult http = response.Result!;

// Switch runs side effects and returns AnyActionStatus
AnyActionStatus status = found.Switch(
    first: u => Console.WriteLine(u.Name),
    second: err => Console.WriteLine(err.Message));
```

## Type Inspection

```csharp
if (found.IsFirst)
{
    User u = found.GetFirst(); // throws InvalidOperationException for the wrong branch
}

// Generic helpers take the target type followed by the union's type arguments
bool isUser = found.Is<User, User, NotFoundError>();
User? asUser = found.As<User, User, NotFoundError>();
bool ok = found.TryAs<NotFoundError, User, NotFoundError>(out NotFoundError? error);

// Deconstruct / ToTuple: the inactive branches are default
var (maybeUser, maybeError) = found;
```

## Collections

```csharp
// Split a sequence of unions into one typed array per branch
(User[] users, NotFoundError[] notFound) = lookups.Partition();

// Or project and split in one pass
(User[] loaded, NotFoundError[] failed) = ids.Traverse(id => FindUser(id));
```

## Typical Usage: service return type

```csharp
public Any<Order, ValidationFailure, NotFoundError> PlaceOrder(PlaceOrderRequest request)
{
    if (request.Quantity <= 0)
        return new ValidationFailure("Quantity must be positive");

    Cart? cart = _repo.FindCart(request.UserId);
    if (cart is null)
        return new NotFoundError("Cart not found");

    return new Order { Id = Guid.NewGuid() };
}

// At the API boundary
public IResult PlaceOrderEndpoint(PlaceOrderRequest request) =>
    PlaceOrder(request).Match(
        first: order => Results.Created($"/orders/{order.Id}", order),
        second: failure => Results.BadRequest(failure.Message),
        third: err => Results.NotFound(err.Message)).Result!;
```

## Best Practices

- Use `Any<T0,T1>` over `Result<T>` when the error branches carry distinct, typed data
- Pass a handler for every branch to `Match()`/`Switch()`; a missing handler is not a compile error, it returns `NotExecuted`
- `IsFirst`/`GetFirst()` or `TryAs<…>()` are the escape hatch for cases where `Match()` is too verbose
- Avoid `object`-typed union members; they defeat the purpose
