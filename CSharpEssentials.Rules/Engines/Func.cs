
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

public static partial class RuleEngine
{
    public static Result Evaluate<TContext>(Func<TContext, Result> rule, TContext context, CancellationToken cancellationToken = default) => Evaluate(rule.ToRule(), context, cancellationToken);

    public static Result Evaluate<TContext>(Func<TContext, CancellationToken, Result> rule, TContext context, CancellationToken cancellationToken = default) => Evaluate(rule.ToRule(), context, cancellationToken);

    public static Result<TResult> Evaluate<TContext, TResult>(Func<TContext, Result<TResult>> rule, TContext context, CancellationToken cancellationToken = default) => Evaluate(rule.ToRule(), context, cancellationToken);

    public static Result<TResult> Evaluate<TContext, TResult>(Func<TContext, CancellationToken, Result<TResult>> rule, TContext context, CancellationToken cancellationToken = default) => Evaluate(rule.ToRule(), context, cancellationToken);

    public static ValueTask<Result> Evaluate<TContext>(Func<TContext, CancellationToken, ValueTask<Result>> rule, TContext context, CancellationToken cancellationToken = default) => Evaluate(rule.ToRule(), context, cancellationToken);

    public static ValueTask<Result<TResult>> Evaluate<TContext, TResult>(Func<TContext, CancellationToken, ValueTask<Result<TResult>>> rule, TContext context, CancellationToken cancellationToken = default) => Evaluate(rule.ToRule(), context, cancellationToken);

    public static IRule<TContext> FromPredicate<TContext>(
        Func<TContext, bool> predicate,
        Error error)
        => new DelegateRuleAdapter<TContext>(predicate, _ => error);

    public static IRule<TContext> FromPredicate<TContext>(
        Func<TContext, bool> predicate,
        Func<TContext, Error> errorFactory)
        => new DelegateRuleAdapter<TContext>(predicate, errorFactory);

    public static IAsyncRule<TContext> FromPredicateAsync<TContext>(
        Func<TContext, Task<bool>> predicate,
        Error error)
        => new AsyncDelegateRuleAdapter<TContext>(predicate, _ => error);

    public static IAsyncRule<TContext> FromPredicateAsync<TContext>(
        Func<TContext, Task<bool>> predicate,
        Func<TContext, Error> errorFactory)
        => new AsyncDelegateRuleAdapter<TContext>(predicate, errorFactory);

}
