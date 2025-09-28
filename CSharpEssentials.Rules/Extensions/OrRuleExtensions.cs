
using System.Runtime.CompilerServices;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

public static partial class Extensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRuleBase<TContext> Or<TContext>(this IRuleBase<TContext>[] rules) =>
        OrRuleAdapter<TContext>.From(rules);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRuleBase<TContext, TResult> Or<TContext, TResult>(this IRuleBase<TContext, TResult>[] rules) =>
        OrRuleAdapter<TContext, TResult>.From(rules);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRuleBase<TContext> Or<TContext>(this Func<TContext, Result>[] rules) =>
        OrRuleAdapter<TContext>.From([.. rules.Select(item => item.ToRule())]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRuleBase<TContext, TResult> Or<TContext, TResult>(this Func<TContext, Result<TResult>>[] rules) =>
        OrRuleAdapter<TContext, TResult>.From([.. rules.Select(item => item.ToRule())]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRuleBase<TContext> Or<TContext>(this Func<TContext, CancellationToken, Result>[] rules) =>
        OrRuleAdapter<TContext>.From([.. rules.Select(item => item.ToRule())]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRuleBase<TContext, TResult> Or<TContext, TResult>(this Func<TContext, CancellationToken, Result<TResult>>[] rules) =>
        OrRuleAdapter<TContext, TResult>.From([.. rules.Select(item => item.ToRule())]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRuleBase<TContext> Or<TContext>(this Func<TContext, CancellationToken, ValueTask<Result>>[] rules) =>
        OrRuleAdapter<TContext>.From([.. rules.Select(item => item.ToRule())]);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRuleBase<TContext, TResult> Or<TContext, TResult>(this Func<TContext, CancellationToken, ValueTask<Result<TResult>>>[] rules) =>
        OrRuleAdapter<TContext, TResult>.From([.. rules.Select(item => item.ToRule())]);
}
