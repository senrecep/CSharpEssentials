
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

internal readonly record struct LinearRuleWithCancellationTokenAdapter<TContext>(
    IRule<TContext> Rule,
    IRuleBase<TContext> Next
) : ILinearRule<TContext>
{
    public Result Evaluate(TContext context, CancellationToken cancellationToken = default) =>
        Rule.Evaluate(context, cancellationToken);

    internal static LinearRuleWithCancellationTokenAdapter<TContext> From(
        IRule<TContext> rule,
        IRuleBase<TContext> next
    ) => new(rule, next);
}


internal readonly record struct LinearRuleWithCancellationTokenAdapter<TContext, TResult>(
    IRule<TContext, TResult> Rule,
    IRuleBase<TContext, TResult> Next
) : ILinearRule<TContext, TResult>
{
    public Result<TResult> Evaluate(TContext context, CancellationToken cancellationToken = default) =>
        Rule.Evaluate(context, cancellationToken);

    internal static LinearRuleWithCancellationTokenAdapter<TContext, TResult> From(
        IRule<TContext, TResult> rule,
        IRuleBase<TContext, TResult> next
    ) => new(rule, next);
}