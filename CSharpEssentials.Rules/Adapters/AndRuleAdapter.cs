
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

internal readonly record struct AndRuleAdapter<TContext>(
    IRuleBase<TContext>[] Rules
) : IAndRule<TContext>
{
    public Result Evaluate(TContext context, CancellationToken cancellationToken = default) =>
        Rules.Length > 0 ? Result.Success() : RuleErrors.EmptyRuleArrayError;

    internal static AndRuleAdapter<TContext> From(
        IRuleBase<TContext>[] rules
    ) => new(rules);
}

internal readonly record struct AndRuleAdapter<TContext, TResult>(
    IRuleBase<TContext, TResult>[] Rules
) : IAndRule<TContext, TResult>
{
    public Result<TResult> Evaluate(TContext context, CancellationToken cancellationToken = default) =>
        Rules.Length > 0 ? default(Result<TResult>) : RuleErrors.EmptyRuleArrayError;

    internal static AndRuleAdapter<TContext, TResult> From(
        IRuleBase<TContext, TResult>[] rules
    ) => new(rules);
}
