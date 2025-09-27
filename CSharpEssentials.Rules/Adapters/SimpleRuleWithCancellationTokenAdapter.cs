
using System.Runtime.CompilerServices;

namespace CSharpEssentials.Rules.Adapters;

internal readonly record struct SimpleRuleWithCancellationTokenAdapter<TContext>(
    Func<TContext, CancellationToken, Result> Rule
) : IRule<TContext>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result Evaluate(TContext context, CancellationToken cancellationToken = default) => Rule(context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SimpleRuleWithCancellationTokenAdapter<TContext> From(Func<TContext, CancellationToken, Result> rule) => new(rule);
}


internal readonly record struct SimpleRuleWithCancellationTokenAdapter<TContext, TResult>(
    Func<TContext, CancellationToken, Result<TResult>> Rule
) : IRule<TContext, TResult>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TResult> Evaluate(TContext context, CancellationToken cancellationToken = default) => Rule(context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SimpleRuleWithCancellationTokenAdapter<TContext, TResult> From(Func<TContext, CancellationToken, Result<TResult>> rule) => new(rule);
}

