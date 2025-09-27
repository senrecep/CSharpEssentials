
using System.Runtime.CompilerServices;
using CSharpEssentials.Results;

namespace CSharpEssentials.Rules;

internal readonly record struct LinearAsyncRuleAdapter<TContext>(
    IAsyncRule<TContext> Rule,
    IRuleBase<TContext> Next
) : ILinearAsyncRule<TContext>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<Result> EvaluateAsync(TContext context, CancellationToken cancellationToken = default) =>
        Rule.EvaluateAsync(context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<Result<TResult>> EvaluateAsync(TContext context, CancellationToken cancellationToken = default) =>
        Rule.EvaluateAsync(context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static LinearAsyncRuleAdapter<TContext, TResult> From(
        IAsyncRule<TContext, TResult> rule,
        IRuleBase<TContext, TResult> next
    ) => new(rule, next);
}