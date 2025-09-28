
using System.Runtime.CompilerServices;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

internal readonly record struct AndRuleAdapter<TContext>(
    IRuleBase<TContext>[] Rules
) : IAndRule<TContext>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result Evaluate(TContext context, CancellationToken cancellationToken = default) =>
        Rules.Length > 0 ? Result.Success() : RuleErrors.EmptyRuleArrayError;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static AndRuleAdapter<TContext> From(
        IRuleBase<TContext>[] rules
    ) => new(rules);
}

internal readonly record struct AndRuleAdapter<TContext, TResult>(
    IRuleBase<TContext, TResult>[] Rules
) : IAndRule<TContext, TResult>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TResult> Evaluate(TContext context, CancellationToken cancellationToken = default) =>
        Rules.Length > 0 ? default : RuleErrors.EmptyRuleArrayError;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static AndRuleAdapter<TContext, TResult> From(
        IRuleBase<TContext, TResult>[] rules
    ) => new(rules);
}
