using System.Runtime.CompilerServices;
using CSharpEssentials.Results;

namespace CSharpEssentials.Rules;

public static partial class Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext> ToRule<TContext>(this Func<TContext, Result> rule) =>
        SimpleRuleAdapter<TContext>.From(rule);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext> ToRule<TContext>(this Func<TContext, CancellationToken, Result> rule) =>
        SimpleRuleWithCancellationTokenAdapter<TContext>.From(rule);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext> ToRule<TContext>(this Func<TContext, CancellationToken, ValueTask<Result>> rule) =>
        SimpleAsyncRuleAdapter<TContext>.From(rule);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext, TResult> ToRule<TContext, TResult>(this Func<TContext, Result<TResult>> rule) =>
        SimpleRuleAdapter<TContext, TResult>.From(rule);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext, TResult> ToRule<TContext, TResult>(this Func<TContext, CancellationToken, Result<TResult>> rule) =>
        SimpleRuleWithCancellationTokenAdapter<TContext, TResult>.From(rule);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext, TResult> ToRule<TContext, TResult>(this Func<TContext, CancellationToken, ValueTask<Result<TResult>>> rule) =>
        SimpleAsyncRuleAdapter<TContext, TResult>.From(rule);
}
