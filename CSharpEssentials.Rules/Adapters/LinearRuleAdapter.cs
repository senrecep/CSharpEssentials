
using System.Runtime.CompilerServices;

namespace CSharpEssentials.Rules.Adapters;

internal readonly record struct LinearRuleAdapter<TContext>(
    IRule<TContext> Rule,
    IRuleBase<TContext> Next
) : ILinearRule<TContext>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result Evaluate(TContext context, CancellationToken cancellationToken = default) =>
        Rule.Evaluate(context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static LinearRuleAdapter<TContext> From(
        IRule<TContext> rule,
        IRuleBase<TContext> next
    ) => new(rule, next);
}

internal readonly record struct LinearRuleAdapter<TContext, TResult>(
    IRule<TContext, TResult> Rule,
    IRuleBase<TContext, TResult> Next
) : ILinearRule<TContext, TResult>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TResult> Evaluate(TContext context, CancellationToken cancellationToken = default) =>
        Rule.Evaluate(context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static LinearRuleAdapter<TContext, TResult> From(
        IRule<TContext, TResult> rule,
        IRuleBase<TContext, TResult> next
    ) => new(rule, next);
}
