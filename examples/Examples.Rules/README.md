# CSharpEssentials.Rules example

Console application for the rule engine in `CSharpEssentials.Rules`.

| Feature | What the sample does |
|---------|----------------------|
| Simple rules | Builds rules with `Func<T, Result>.ToRule()` and runs them with `IRule<T>.Evaluate` |
| And | `RuleEngine.Evaluate(rules.And(), value)`: all rules must pass; a `Result` rule array stops at the first failure and returns its errors |
| Or | `rules.Or()`: at least one rule must pass |
| Conditional | `RuleEngine.If(condition, success, failure, value)` branches on a rule |
| Linear | `RuleEngine.Linear(rules, value)` runs rules in order and stops at the first failure |
| Rules with values | `IRule<TContext, TResult>` returns a `Result<TResult>` (a grade from a score) |
| Async rules | `IAsyncRule<T>` from a `Func<T, CancellationToken, ValueTask<Result>>` |
| Next | `rule.Next(other)` runs `other` only after `rule` passes |

## Running

```bash
cd examples/Examples.Rules
dotnet run
```
