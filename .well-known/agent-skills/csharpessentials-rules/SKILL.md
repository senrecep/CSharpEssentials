---
name: csharpessentials-rules
description: Use when composing business rules that return Result or Result<T>. Covers IRule<T>/IAsyncRule<T> classes, Func rules, RuleEngine.FromPredicate/FromPredicateAsync, .ToRule(), .And()/.Or()/.Linear()/.Next(), RuleEngine.If, the static RuleEngine.And/Or/Linear shortcuts, RuleEngine.Evaluate and EvaluateAsync, value rules (IRule<TContext,TResult>) and custom ILinearRule/IConditionalRule/IAndRule/IOrRule nodes.
---

# CSharpEssentials.Rules

Rules return `Result` (or `Result<T>`). You build a tree with And, Or, Linear and If, then evaluate it with `RuleEngine.Evaluate` (sync) or `RuleEngine.EvaluateAsync` (when the tree has async rules).

```bash
dotnet add package CSharpEssentials.Rules
```

```csharp
using CSharpEssentials.Rules;         // IRule<T>, IAsyncRule<T>, IRuleBase<T>, RuleEngine, extension methods
using CSharpEssentials.ResultPattern; // Result, Result<T>
using CSharpEssentials.Errors;        // Error
```

## Quick start

```csharp Program.cs
// dotnet add package CSharpEssentials.Rules
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Rules;

IRule<Order> hasItems = new HasItemsRule();
Func<Order, Result> positiveTotal = o => o.Total > 0
    ? Result.Success()
    : Error.Validation("Order.Total", "Total must be positive.");
IRule<Order> limit = RuleEngine.FromPredicate<Order>(
    o => o.Total <= 1_000,
    o => Error.Validation("Order.Limit", $"Total {o.Total} is over the limit."));

IRuleBase<Order> checkout = new IRuleBase<Order>[] { hasItems, positiveTotal.ToRule(), limit }.And();

IAsyncRule<Order> stock = RuleEngine.FromPredicateAsync<Order>(
    async o => { await Task.Yield(); return o.Items <= 5; },
    Error.Conflict("Order.Stock", "Not enough stock."));
IRule<Order> sequence = hasItems.Next(stock);

foreach (Order order in new[] { new Order(2, 50m), new Order(0, 0m), new Order(9, 2_000m) })
{
    Result sync = RuleEngine.Evaluate(checkout, order);
    Result async = await RuleEngine.EvaluateAsync(sequence, order);
    Console.WriteLine($"{order}: checkout={Describe(sync)}, stock={Describe(async)}");
}

static string Describe(Result r) => r.IsSuccess ? "ok" : string.Join(",", r.Errors.Select(e => e.Code));

internal sealed record Order(int Items, decimal Total);

internal sealed class HasItemsRule : IRule<Order>
{
    public Result Evaluate(Order context, CancellationToken cancellationToken = default) =>
        context.Items > 0 ? Result.Success() : Error.Validation("Order.Empty", "Order has no items.");
}
```

## Writing a rule

In the snippets below, `user` is a `User` record (`Name`, `Age`, `IsActive`, `Email`) and `store` is your own async service.

```csharp
// Class: implement IRule<TContext> (Evaluate) or IAsyncRule<TContext> (EvaluateAsync)
public sealed class MinimumAgeRule(int min) : IRule<User>
{
    public Result Evaluate(User context, CancellationToken cancellationToken = default) =>
        context.Age >= min ? Result.Success() : Error.Validation("User.Age", "Too young.");
}

public sealed class UniqueEmailRule(IUserStore store) : IAsyncRule<User>
{
    public async ValueTask<Result> EvaluateAsync(User context, CancellationToken cancellationToken = default) =>
        await store.EmailExistsAsync(context.Email, cancellationToken)
            ? Error.Conflict("User.Email", "Email is taken.")
            : Result.Success();
}

// Func: Func<T, Result>, Func<T, CancellationToken, Result>, Func<T, CancellationToken, ValueTask<Result>>
Func<User, Result> active = u => u.IsActive ? Result.Success() : Error.Forbidden("User.Inactive", "User is inactive.");

// Predicate
IRule<User> named = RuleEngine.FromPredicate<User>(u => u.Name.Length > 0, Error.Validation("User.Name", "Name is required."));
IAsyncRule<User> remote = RuleEngine.FromPredicateAsync<User>(u => store.IsAllowedAsync(u), u => Error.Forbidden("User.Blocked", $"{u.Name} is blocked."));

// Func -> IRule / IAsyncRule
IRule<User> activeRule = active.ToRule();
```

## Combining

```csharp
IRuleBase<User> all = new IRuleBase<User>[] { new MinimumAgeRule(18), activeRule, named }.And();  // all must pass
IRuleBase<User> any = new Func<User, Result>[] { active, u => Result.Success() }.Or();           // one must pass
IRuleBase<User> chain = new IRule<User>[] { new MinimumAgeRule(18), activeRule }.Linear();        // in order, stop at first failure
IRule<User> linked = new MinimumAgeRule(18).Next(active).Next(named);                             // same as Linear

Result r1 = RuleEngine.Evaluate(all, user);
Result r2 = RuleEngine.And(new Func<User, Result>[] { active }, user);   // static shortcuts: And, Or, Linear
Result r3 = RuleEngine.If(new MinimumAgeRule(18), success: activeRule, failure: named, user);
Result r4 = RuleEngine.If(user.IsActive, success: activeRule, failure: named, user);
```

## Value rules

```csharp
Func<int, Result<string>>[] graders =
[
    s => s >= 90 ? "A" : Error.Validation("Grade.NotA", "Below 90."),
    s => s >= 80 ? "B" : Error.Validation("Grade.NotB", "Below 80."),
];
Result<string> grade = RuleEngine.Or(graders, 85);    // "B": first success
Result<string> both = RuleEngine.And(graders, 95);    // "A": first child's value when all pass
```

## Async

```csharp
IRuleBase<User> tree = new IRuleBase<User>[] { named, new UniqueEmailRule(store) }.And();
Result result = await RuleEngine.EvaluateAsync(tree, user, cancellationToken);
```

## Custom composite nodes

`ILinearRule<T>` (`Next`), `IConditionalRule<T>` (`Success`, `Failure`), `IAndRule<T>` and `IOrRule<T>` (`Rules` array) plus their `Async` and `<TContext, TResult>` forms are public. Their `Evaluate` is the first step (linear), the condition (conditional) or a gate (and/or). The engine walks the rest, so evaluate them through `RuleEngine`, not by calling `Evaluate` directly.

## Rules

- Use `RuleEngine.EvaluateAsync` when the tree contains any `IAsyncRule`. `Evaluate` blocks on them and can deadlock.
- The async rule method is `EvaluateAsync(TContext, CancellationToken)` returning `ValueTask<Result>`. An `IAsyncRule` has no `Evaluate`.
- `.Linear()` exists only on `IRule<T>[]` and `IAsyncRule<T>[]`. For a `Func[]`, call `RuleEngine.Linear(funcs, ctx)` or convert each with `.ToRule()`.
- Mixing classes and funcs in one array needs `IRuleBase<T>[]` and `.ToRule()` on each `Func`.
- `FromPredicateAsync` takes `Func<T, Task<bool>>` with no token.
- `Result` And stops at the first failure. `Result<T>` And runs every rule, returns all errors, and on success returns the first rule's value. Or returns the first success, else all errors.
- Empty arrays fail with `EMPTY_RULE_ARRAY`, they do not throw.
- A class that implements only `IRuleBase<T>` is never run: the engine returns `ErrorType.NotFound` with code `RuleEngine.Evaluate.<ClassName>`. Implement `IRule<T>`, `IAsyncRule<T>` or one of the composite interfaces.
- A throwing rule becomes `ErrorType.Unexpected` with code `RuleEngine.Evaluate.<RuleType>`. Cancellation in `EvaluateAsync` throws `OperationCanceledException`.
- Never return `default(Result)` as success. It is a failure in 9.0. Return `Result.Success()`.
- `RuleEngine.And/Or/Linear(array, ctx)` rebuild the tree per call. Build the tree once with `.And()`/`.Linear()` on hot paths.
- The namespace is `CSharpEssentials.Rules`; `Result` is in `CSharpEssentials.ResultPattern`, not `CSharpEssentials.Results`.

## Diagnostics

This package reports no analyzer diagnostics.
