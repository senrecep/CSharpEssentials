
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

internal readonly record struct LinearAsyncRuleAdapter<TContext>(
    IAsyncRule<TContext> Rule,
    IRuleBase<TContext> Next
) : ILinearAsyncRule<TContext>
{
    public ValueTask<Result> EvaluateAsync(TContext context, CancellationToken cancellationToken = default) =>
        Rule.EvaluateAsync(context, cancellationToken);

    internal static LinearAsyncRuleAdapter<TContext> From(
        IAsyncRule<TContext> rule,
        IRuleBase<TContext> next
    ) => new(rule, next);
}


internal readonly record struct LinearAsyncRuleAdapter<TContext, TResult>(
    IAsyncRule<TContext, TResult> Rule,
    IRuleBase<TContext, TResult> Next
) : ILinearAsyncRule<TContext, TResult>
{
    public ValueTask<Result<TResult>> EvaluateAsync(TContext context, CancellationToken cancellationToken = default) =>
        Rule.EvaluateAsync(context, cancellationToken);

    internal static LinearAsyncRuleAdapter<TContext, TResult> From(
        IAsyncRule<TContext, TResult> rule,
        IRuleBase<TContext, TResult> next
    ) => new(rule, next);
}
