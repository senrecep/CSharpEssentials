
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

internal readonly record struct OrRuleAdapter<TContext>(
    IRuleBase<TContext>[] Rules
) : IOrRule<TContext>
{
    public Result Evaluate(TContext context, CancellationToken cancellationToken = default) =>
        Rules.Length > 0 ? Result.Success() : RuleErrors.EmptyRuleArrayError;

    internal static OrRuleAdapter<TContext> From(
        IRuleBase<TContext>[] rules
    ) => new(rules);
}


internal readonly record struct OrRuleAdapter<TContext, TResult>(
    IRuleBase<TContext, TResult>[] Rules
) : IOrRule<TContext, TResult>
{
    public Result<TResult> Evaluate(TContext context, CancellationToken cancellationToken = default) =>
        Rules.Length > 0 ? default : RuleErrors.EmptyRuleArrayError;

    internal static OrRuleAdapter<TContext, TResult> From(
        IRuleBase<TContext, TResult>[] rules
    ) => new(rules);
}
