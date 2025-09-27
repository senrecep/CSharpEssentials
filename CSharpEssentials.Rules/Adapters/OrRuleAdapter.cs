
using System.Runtime.CompilerServices;
using CSharpEssentials.Results;

namespace CSharpEssentials.Rules.Adapters;

internal readonly record struct OrRuleAdapter<TContext>(
    IRuleBase<TContext>[] Rules
) : IOrRule<TContext>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result Evaluate(TContext context, CancellationToken cancellationToken = default) =>
        Rules.Length > 0 ? Result.Success() : RuleErrors.EmptyRuleArrayError;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static OrRuleAdapter<TContext> From(
        IRuleBase<TContext>[] rules
    ) => new(rules);
}


internal readonly record struct OrRuleAdapter<TContext, TResult>(
    IRuleBase<TContext, TResult>[] Rules
) : IOrRule<TContext, TResult>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TResult> Evaluate(TContext context, CancellationToken cancellationToken = default) =>
        Rules.Length > 0 ? default : RuleErrors.EmptyRuleArrayError;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static OrRuleAdapter<TContext, TResult> From(
        IRuleBase<TContext, TResult>[] rules
    ) => new(rules);
}
