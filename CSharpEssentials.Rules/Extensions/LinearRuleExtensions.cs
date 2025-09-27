using System.Runtime.CompilerServices;
using CSharpEssentials.Rules.Adapters;

namespace CSharpEssentials;

public static partial class Extensions
{
    #region Build Chain
    public static IRuleBase<TContext> Linear<TContext>(this IRule<TContext>[] rules)
    {
        IRule<TContext> current = rules[0];
        IRuleBase<TContext> chain = current;

        foreach (IRule<TContext> item in rules[1..])
        {
            chain = current.Next(item);
            current = item;
        }
        return chain;
    }
    public static IRuleBase<TContext> Linear<TContext>(this IAsyncRule<TContext>[] rules)
    {
        IAsyncRule<TContext> current = rules[0];
        IRuleBase<TContext> chain = current;

        foreach (IAsyncRule<TContext> item in rules[1..])
        {
            chain = current.Next(item);
            current = item;
        }
        return chain;
    }

    public static IRuleBase<TContext, TResult> Linear<TContext, TResult>(this IRule<TContext, TResult>[] rules)
    {
        IRule<TContext, TResult> current = rules[0];
        IRuleBase<TContext, TResult> chain = current;

        foreach (IRule<TContext, TResult> item in rules[1..])
        {
            chain = current.Next(item);
            current = item;
        }
        return chain;
    }

    public static IRuleBase<TContext, TResult> Linear<TContext, TResult>(this IAsyncRule<TContext, TResult>[] rules)
    {
        IAsyncRule<TContext, TResult> current = rules[0];
        IRuleBase<TContext, TResult> chain = current;

        foreach (IAsyncRule<TContext, TResult> item in rules[1..])
        {
            chain = current.Next(item);
            current = item;
        }
        return chain;
    }
    #endregion

    #region IRule<TContext> Next
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext> Next<TContext>(
        this IRule<TContext> rule,
        IRuleBase<TContext> next) =>
        LinearRuleAdapter<TContext>.From(rule, next);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext> Next<TContext>(
        this IRule<TContext> rule,
        Func<TContext, Result> next) =>
        LinearRuleAdapter<TContext>.From(rule, next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext> Next<TContext>(
        this IRule<TContext> rule,
        Func<TContext, CancellationToken, Result> next) =>
        LinearRuleAdapter<TContext>.From(rule, next.ToRule());
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext> Next<TContext>(
        this IRule<TContext> rule,
        Func<TContext, CancellationToken, ValueTask<Result>> next) =>
        LinearRuleAdapter<TContext>.From(rule, next.ToRule());
    #endregion

    #region IAsyncRule<TContext> Next
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext> Next<TContext>(
        this IAsyncRule<TContext> rule,
        IRuleBase<TContext> next) =>
        LinearAsyncRuleAdapter<TContext>.From(rule, next);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext> Next<TContext>(
        this IAsyncRule<TContext> rule,
        Func<TContext, Result> next) =>
        LinearAsyncRuleAdapter<TContext>.From(rule, next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext> Next<TContext>(
        this IAsyncRule<TContext> rule,
        Func<TContext, CancellationToken, Result> next) =>
        LinearAsyncRuleAdapter<TContext>.From(rule, next.ToRule());
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext> Next<TContext>(
        this IAsyncRule<TContext> rule,
        Func<TContext, CancellationToken, ValueTask<Result>> next) =>
        LinearAsyncRuleAdapter<TContext>.From(rule, next.ToRule());
    #endregion

    #region Func<TContext, Result> Next
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext> Next<TContext>(
        this Func<TContext, Result> rule,
        IRuleBase<TContext> next) =>
        LinearRuleAdapter<TContext>.From(rule.ToRule(), next);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext> Next<TContext>(
        this Func<TContext, Result> rule,
        Func<TContext, Result> next) =>
        LinearRuleAdapter<TContext>.From(rule.ToRule(), next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext> Next<TContext>(
        this Func<TContext, Result> rule,
        Func<TContext, CancellationToken, Result> next) =>
        LinearRuleWithCancellationTokenAdapter<TContext>.From(rule.ToRule(), next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext> Next<TContext>(
        this Func<TContext, Result> rule,
        Func<TContext, CancellationToken, ValueTask<Result>> next) =>
        LinearRuleAdapter<TContext>.From(rule.ToRule(), next.ToRule());
    #endregion

    #region Func<TContext, CancellationToken, Result> Next
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext> Next<TContext>(
        this Func<TContext, CancellationToken, Result> rule,
        IRuleBase<TContext> next) =>
        LinearRuleWithCancellationTokenAdapter<TContext>.From(rule.ToRule(), next);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext> Next<TContext>(
        this Func<TContext, CancellationToken, Result> rule,
        Func<TContext, Result> next) =>
        LinearRuleWithCancellationTokenAdapter<TContext>.From(rule.ToRule(), next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext> Next<TContext>(
        this Func<TContext, CancellationToken, Result> rule,
        Func<TContext, CancellationToken, Result> next) =>
        LinearRuleWithCancellationTokenAdapter<TContext>.From(rule.ToRule(), next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext> Next<TContext>(
        this Func<TContext, CancellationToken, Result> rule,
        Func<TContext, CancellationToken, ValueTask<Result>> next) =>
        LinearRuleWithCancellationTokenAdapter<TContext>.From(rule.ToRule(), next.ToRule());
    #endregion

    #region Func<TContext, CancellationToken, ValueTask<Result>> Next
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext> Next<TContext>(
        this Func<TContext, CancellationToken, ValueTask<Result>> rule,
        IRuleBase<TContext> next) =>
        LinearAsyncRuleAdapter<TContext>.From(rule.ToRule(), next);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext> Next<TContext>(
        this Func<TContext, CancellationToken, ValueTask<Result>> rule,
        Func<TContext, Result> next) =>
        LinearAsyncRuleAdapter<TContext>.From(rule.ToRule(), next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext> Next<TContext>(
        this Func<TContext, CancellationToken, ValueTask<Result>> rule,
        Func<TContext, CancellationToken, Result> next) =>
        LinearAsyncRuleAdapter<TContext>.From(rule.ToRule(), next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext> Next<TContext>(
        this Func<TContext, CancellationToken, ValueTask<Result>> rule,
        Func<TContext, CancellationToken, ValueTask<Result>> next) =>
        LinearAsyncRuleAdapter<TContext>.From(rule.ToRule(), next.ToRule());
    #endregion

    #region IRule<TContext,TResult> Next
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext, TResult> Next<TContext, TResult>(
        this IRule<TContext, TResult> rule,
        IRuleBase<TContext, TResult> next) =>
        LinearRuleAdapter<TContext, TResult>.From(rule, next);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext, TResult> Next<TContext, TResult>(
        this IRule<TContext, TResult> rule,
        Func<TContext, Result<TResult>> next) =>
        LinearRuleAdapter<TContext, TResult>.From(rule, next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext, TResult> Next<TContext, TResult>(
        this IRule<TContext, TResult> rule,
        Func<TContext, CancellationToken, Result<TResult>> next) =>
        LinearRuleAdapter<TContext, TResult>.From(rule, next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext, TResult> Next<TContext, TResult>(
        this IRule<TContext, TResult> rule,
        Func<TContext, CancellationToken, ValueTask<Result<TResult>>> next) =>
        LinearRuleAdapter<TContext, TResult>.From(rule, next.ToRule());
    #endregion

    #region IAsyncRule<TContext,TResult> Next
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext, TResult> Next<TContext, TResult>(
        this IAsyncRule<TContext, TResult> rule,
        IRuleBase<TContext, TResult> next) =>
        LinearAsyncRuleAdapter<TContext, TResult>.From(rule, next);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext, TResult> Next<TContext, TResult>(
        this IAsyncRule<TContext, TResult> rule,
        Func<TContext, CancellationToken, ValueTask<Result<TResult>>> next) =>
        LinearAsyncRuleAdapter<TContext, TResult>.From(rule, next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext, TResult> Next<TContext, TResult>(
        this IAsyncRule<TContext, TResult> rule,
        Func<TContext, CancellationToken, Result<TResult>> next) =>
        LinearAsyncRuleAdapter<TContext, TResult>.From(rule, next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext, TResult> Next<TContext, TResult>(
        this IAsyncRule<TContext, TResult> rule,
        Func<TContext, Result<TResult>> next) =>
        LinearAsyncRuleAdapter<TContext, TResult>.From(rule, next.ToRule());
    #endregion

    #region Func<TContext, Result<TResult>> Next
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext, TResult> Next<TContext, TResult>(
        this Func<TContext, Result<TResult>> rule,
        IRuleBase<TContext, TResult> next) =>
        LinearRuleAdapter<TContext, TResult>.From(rule.ToRule(), next);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext, TResult> Next<TContext, TResult>(
        this Func<TContext, Result<TResult>> rule,
        Func<TContext, Result<TResult>> next) =>
        LinearRuleAdapter<TContext, TResult>.From(rule.ToRule(), next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext, TResult> Next<TContext, TResult>(
        this Func<TContext, Result<TResult>> rule,
        Func<TContext, CancellationToken, Result<TResult>> next) =>
        LinearRuleAdapter<TContext, TResult>.From(rule.ToRule(), next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext, TResult> Next<TContext, TResult>(
        this Func<TContext, Result<TResult>> rule,
        Func<TContext, CancellationToken, ValueTask<Result<TResult>>> next) =>
        LinearRuleAdapter<TContext, TResult>.From(rule.ToRule(), next.ToRule());
    #endregion

    #region Func<TContext, CancellationToken, Result<TResult>> Next
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext, TResult> Next<TContext, TResult>(
        this Func<TContext, CancellationToken, Result<TResult>> rule,
        IRuleBase<TContext, TResult> next) =>
        LinearRuleWithCancellationTokenAdapter<TContext, TResult>.From(rule.ToRule(), next);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext, TResult> Next<TContext, TResult>(
        this Func<TContext, CancellationToken, Result<TResult>> rule,
        Func<TContext, Result<TResult>> next) =>
        LinearRuleWithCancellationTokenAdapter<TContext, TResult>.From(rule.ToRule(), next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext, TResult> Next<TContext, TResult>(
        this Func<TContext, CancellationToken, Result<TResult>> rule,
        Func<TContext, CancellationToken, Result<TResult>> next) =>
        LinearRuleWithCancellationTokenAdapter<TContext, TResult>.From(rule.ToRule(), next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IRule<TContext, TResult> Next<TContext, TResult>(
        this Func<TContext, CancellationToken, Result<TResult>> rule,
        Func<TContext, CancellationToken, ValueTask<Result<TResult>>> next) =>
        LinearRuleWithCancellationTokenAdapter<TContext, TResult>.From(rule.ToRule(), next.ToRule());
    #endregion

    #region Func<TContext, CancellationToken, ValueTask<Result<TResult>> Next
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext, TResult> Next<TContext, TResult>(
        this Func<TContext, CancellationToken, ValueTask<Result<TResult>>> rule,
        IRuleBase<TContext, TResult> next) =>
        LinearAsyncRuleAdapter<TContext, TResult>.From(rule.ToRule(), next);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext, TResult> Next<TContext, TResult>(
        this Func<TContext, CancellationToken, ValueTask<Result<TResult>>> rule,
        Func<TContext, CancellationToken, ValueTask<Result<TResult>>> next) =>
        LinearAsyncRuleAdapter<TContext, TResult>.From(rule.ToRule(), next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext, TResult> Next<TContext, TResult>(
        this Func<TContext, CancellationToken, ValueTask<Result<TResult>>> rule,
        Func<TContext, Result<TResult>> next) =>
        LinearAsyncRuleAdapter<TContext, TResult>.From(rule.ToRule(), next.ToRule());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IAsyncRule<TContext, TResult> Next<TContext, TResult>(
        this Func<TContext, CancellationToken, ValueTask<Result<TResult>>> rule,
        Func<TContext, CancellationToken, Result<TResult>> next) =>
        LinearAsyncRuleAdapter<TContext, TResult>.From(rule.ToRule(), next.ToRule());
    #endregion
}
