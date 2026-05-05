using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

public static partial class Extensions
{
    public static IRuleBase<TContext> And<TContext>(this IRuleBase<TContext>[] rules) =>
        AndRuleAdapter<TContext>.From(rules);

    public static IRuleBase<TContext, TResult> And<TContext, TResult>(this IRuleBase<TContext, TResult>[] rules) =>
        AndRuleAdapter<TContext, TResult>.From(rules);

    public static IRuleBase<TContext> And<TContext>(this Func<TContext, Result>[] rules) =>
        AndRuleAdapter<TContext>.From([.. rules.Select(item => item.ToRule())]);

    public static IRuleBase<TContext, TResult> And<TContext, TResult>(this Func<TContext, Result<TResult>>[] rules) =>
        AndRuleAdapter<TContext, TResult>.From([.. rules.Select(item => item.ToRule())]);

    public static IRuleBase<TContext> And<TContext>(this Func<TContext, CancellationToken, Result>[] rules) =>
        AndRuleAdapter<TContext>.From([.. rules.Select(item => item.ToRule())]);

    public static IRuleBase<TContext, TResult> And<TContext, TResult>(this Func<TContext, CancellationToken, Result<TResult>>[] rules) =>
        AndRuleAdapter<TContext, TResult>.From([.. rules.Select(item => item.ToRule())]);

    public static IRuleBase<TContext> And<TContext>(this Func<TContext, CancellationToken, ValueTask<Result>>[] rules) =>
        AndRuleAdapter<TContext>.From([.. rules.Select(item => item.ToRule())]);

    public static IRuleBase<TContext, TResult> And<TContext, TResult>(this Func<TContext, CancellationToken, ValueTask<Result<TResult>>>[] rules) =>
        AndRuleAdapter<TContext, TResult>.From([.. rules.Select(item => item.ToRule())]);
}
