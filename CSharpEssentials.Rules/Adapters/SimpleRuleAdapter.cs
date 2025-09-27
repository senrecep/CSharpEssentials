
using System.Runtime.CompilerServices;
using CSharpEssentials.Results;

namespace CSharpEssentials.Rules;

internal readonly record struct SimpleRuleAdapter<TContext>(
    Func<TContext, Result> Rule
) : IRule<TContext>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result Evaluate(TContext context, CancellationToken cancellationToken = default) => Rule(context);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SimpleRuleAdapter<TContext> From(Func<TContext, Result> rule) => new(rule);
}

internal readonly record struct SimpleRuleAdapter<TContext, TResult>(
    Func<TContext, Result<TResult>> Rule
) : IRule<TContext, TResult>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TResult> Evaluate(TContext context, CancellationToken cancellationToken = default) => Rule(context);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SimpleRuleAdapter<TContext, TResult> From(Func<TContext, Result<TResult>> rule) => new(rule);
}
