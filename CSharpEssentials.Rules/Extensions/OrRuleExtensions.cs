
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

public static partial class Extensions
{
    public static IRuleBase<TContext> Or<TContext>(this IRuleBase<TContext>[] rules) =>
        OrRuleAdapter<TContext>.From(rules);

    public static IRuleBase<TContext, TResult> Or<TContext, TResult>(this IRuleBase<TContext, TResult>[] rules) =>
        OrRuleAdapter<TContext, TResult>.From(rules);

    public static IRuleBase<TContext> Or<TContext>(this Func<TContext, Result>[] rules) =>
        OrRuleAdapter<TContext>.From(rules.Select(item => item.ToRule()).ToArray());

    public static IRuleBase<TContext, TResult> Or<TContext, TResult>(this Func<TContext, Result<TResult>>[] rules) =>
#if NET8_0_OR_GREATER
        OrRuleAdapter<TContext, TResult>.From([.. rules.Select(item => item.ToRule())]);
#else
        OrRuleAdapter<TContext, TResult>.From(rules.Select(item => item.ToRule()).ToArray());
#endif

    public static IRuleBase<TContext> Or<TContext>(this Func<TContext, CancellationToken, Result>[] rules) =>
        OrRuleAdapter<TContext>.From(rules.Select(item => item.ToRule()).ToArray());

    public static IRuleBase<TContext, TResult> Or<TContext, TResult>(this Func<TContext, CancellationToken, Result<TResult>>[] rules) =>
#if NET8_0_OR_GREATER
        OrRuleAdapter<TContext, TResult>.From([.. rules.Select(item => item.ToRule())]);
#else
        OrRuleAdapter<TContext, TResult>.From(rules.Select(item => item.ToRule()).ToArray());
#endif

    public static IRuleBase<TContext> Or<TContext>(this Func<TContext, CancellationToken, ValueTask<Result>>[] rules) =>
        OrRuleAdapter<TContext>.From(rules.Select(item => item.ToRule()).ToArray());

    public static IRuleBase<TContext, TResult> Or<TContext, TResult>(this Func<TContext, CancellationToken, ValueTask<Result<TResult>>>[] rules) =>
#if NET8_0_OR_GREATER
        OrRuleAdapter<TContext, TResult>.From([.. rules.Select(item => item.ToRule())]);
#else
        OrRuleAdapter<TContext, TResult>.From(rules.Select(item => item.ToRule()).ToArray());
#endif
}
