
using System.Runtime.CompilerServices;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

internal readonly record struct SimpleAsyncRuleAdapter<TContext>(
    Func<TContext, CancellationToken, ValueTask<Result>> Rule
) : IAsyncRule<TContext>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<Result> EvaluateAsync(TContext context, CancellationToken cancellationToken = default) => Rule(context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SimpleAsyncRuleAdapter<TContext> From(Func<TContext, CancellationToken, ValueTask<Result>> rule) => new(rule);
}


internal readonly record struct SimpleAsyncRuleAdapter<TContext, TResult>(
    Func<TContext, CancellationToken, ValueTask<Result<TResult>>> Rule
) : IAsyncRule<TContext, TResult>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<Result<TResult>> EvaluateAsync(TContext context, CancellationToken cancellationToken = default) => Rule(context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SimpleAsyncRuleAdapter<TContext, TResult> From(Func<TContext, CancellationToken, ValueTask<Result<TResult>>> rule) => new(rule);
}