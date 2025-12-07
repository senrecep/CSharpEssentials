using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

public static partial class RuleEngine
{
    public static Result Linear<TContext>(IRule<TContext>[] rules, TContext context, CancellationToken cancellationToken = default) =>
        Evaluate(rules.Linear(), context, cancellationToken);

    public static Result Linear<TContext>(IAsyncRule<TContext>[] rules, TContext context, CancellationToken cancellationToken = default) =>
        Evaluate(rules.Linear(), context, cancellationToken);

    public static Result<TResult> Linear<TContext, TResult>(IRule<TContext, TResult>[] rules, TContext context, CancellationToken cancellationToken = default) =>
        Evaluate(rules.Linear(), context, cancellationToken);

    public static Result<TResult> Linear<TContext, TResult>(IAsyncRule<TContext, TResult>[] rules, TContext context, CancellationToken cancellationToken = default) =>
        Evaluate(rules.Linear(), context, cancellationToken);

    public static Result Linear<TContext>(Func<TContext, Result>[] rules, TContext context, CancellationToken cancellationToken = default)
    {
        IRuleBase<TContext> chain = rules.Select(item => item.ToRule()).ToArray().Linear();
        return Evaluate(chain, context, cancellationToken);
    }

    public static Result Linear<TContext>(Func<TContext, CancellationToken, Result>[] rules, TContext context, CancellationToken cancellationToken = default)
    {
        IRuleBase<TContext> chain = rules.Select(item => item.ToRule()).ToArray().Linear();
        return Evaluate(chain, context, cancellationToken);
    }


    public static Result Linear<TContext>(Func<TContext, CancellationToken, ValueTask<Result>>[] rules, TContext context, CancellationToken cancellationToken = default)
    {
        IRuleBase<TContext> chain = rules.Select(item => item.ToRule()).ToArray().Linear();
        return Evaluate(chain, context, cancellationToken);
    }

    public static Result<TResult> Linear<TContext, TResult>(Func<TContext, Result<TResult>>[] rules, TContext context, CancellationToken cancellationToken = default)
    {
        IRuleBase<TContext, TResult> chain = rules.Select(item => item.ToRule()).ToArray().Linear();
        return Evaluate(chain, context, cancellationToken);
    }

    public static Result<TResult> Linear<TContext, TResult>(Func<TContext, CancellationToken, Result<TResult>>[] rules, TContext context, CancellationToken cancellationToken = default)
    {
        IRuleBase<TContext, TResult> chain = rules.Select(item => item.ToRule()).ToArray().Linear();
        return Evaluate(chain, context, cancellationToken);
    }

    public static Result<TResult> Linear<TContext, TResult>(Func<TContext, CancellationToken, ValueTask<Result<TResult>>>[] rules, TContext context, CancellationToken cancellationToken = default)
    {
        IRuleBase<TContext, TResult> chain = rules.Select(item => item.ToRule()).ToArray().Linear();
        return Evaluate(chain, context, cancellationToken);
    }
}
