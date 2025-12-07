
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

internal readonly record struct ConditionalRuleAdapter<TContext>(
    IRuleBase<TContext> Rule,
    IRuleBase<TContext> Success,
    IRuleBase<TContext> Failure) : IConditionalRule<TContext>
{
    public Result Evaluate(TContext context, CancellationToken cancellationToken = default) =>
        RuleEngine.Evaluate(Rule, context, cancellationToken);

    internal static ConditionalRuleAdapter<TContext> From(
        IRuleBase<TContext> rule,
        IRuleBase<TContext> success,
        IRuleBase<TContext> failure
    ) => new(rule, success, failure);
}


internal readonly record struct ConditionalRuleAdapter<TContext, TResult>(
    IRuleBase<TContext, TResult> Rule,
    IRuleBase<TContext, TResult> Success,
    IRuleBase<TContext, TResult> Failure) : IConditionalRule<TContext, TResult>
{
    public Result<TResult> Evaluate(TContext context, CancellationToken cancellationToken = default) =>
        RuleEngine.Evaluate(Rule, context, cancellationToken);

    internal static ConditionalRuleAdapter<TContext, TResult> From(
        IRuleBase<TContext, TResult> rule,
        IRuleBase<TContext, TResult> success,
        IRuleBase<TContext, TResult> failure
    ) => new(rule, success, failure);
}
