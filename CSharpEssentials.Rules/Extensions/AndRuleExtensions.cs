using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

public static partial class Extensions
{
    public static IRuleBase<TContext> And<TContext>(this IRuleBase<TContext>[] rules) =>
        AndRuleAdapter<TContext>.From(rules);

    public static IRuleBase<TContext, TResult> And<TContext, TResult>(this IRuleBase<TContext, TResult>[] rules) =>
        AndRuleAdapter<TContext, TResult>.From(rules);

    public static IRuleBase<TContext> And<TContext>(this Func<TContext, Result>[] rules) =>
        AndRuleAdapter<TContext>.From(rules.Select(item => item.ToRule()).ToArray());

    public static IRuleBase<TContext, TResult> And<TContext, TResult>(this Func<TContext, Result<TResult>>[] rules) =>
#if NET8_0_OR_GREATER
        AndRuleAdapter<TContext, TResult>.From([.. rules.Select(item => item.ToRule())]);
#else
        AndRuleAdapter<TContext, TResult>.From(rules.Select(item => item.ToRule()).ToArray());
#endif

    public static IRuleBase<TContext> And<TContext>(this Func<TContext, CancellationToken, Result>[] rules) =>
        AndRuleAdapter<TContext>.From(rules.Select(item => item.ToRule()).ToArray());

    public static IRuleBase<TContext, TResult> And<TContext, TResult>(this Func<TContext, CancellationToken, Result<TResult>>[] rules) =>
#if NET8_0_OR_GREATER
        AndRuleAdapter<TContext, TResult>.From([.. rules.Select(item => item.ToRule())]);
#else
        AndRuleAdapter<TContext, TResult>.From(rules.Select(item => item.ToRule()).ToArray());
#endif

    public static IRuleBase<TContext> And<TContext>(this Func<TContext, CancellationToken, ValueTask<Result>>[] rules) =>
        AndRuleAdapter<TContext>.From(rules.Select(item => item.ToRule()).ToArray());

    public static IRuleBase<TContext, TResult> And<TContext, TResult>(this Func<TContext, CancellationToken, ValueTask<Result<TResult>>>[] rules) =>
#if NET8_0_OR_GREATER
        AndRuleAdapter<TContext, TResult>.From([.. rules.Select(item => item.ToRule())]);
#else
        AndRuleAdapter<TContext, TResult>.From(rules.Select(item => item.ToRule()).ToArray());
#endif
}
