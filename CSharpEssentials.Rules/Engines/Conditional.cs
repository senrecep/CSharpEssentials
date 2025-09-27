using System.Runtime.CompilerServices;
using CSharpEssentials.Rules.Adapters;

namespace CSharpEssentials;

public static partial class RuleEngine
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result If<TContext>(bool condition, IRuleBase<TContext> success, IRuleBase<TContext> failure, TContext context, CancellationToken cancellationToken = default)
    {
        Func<TContext, Result> ruleFunc = _ => condition ? Result.Success() : Error.False;
        return Evaluate(ConditionalRuleAdapter<TContext>.From(ruleFunc.ToRule(), success, failure), context, cancellationToken);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result If<TContext>(IRuleBase<TContext> rule, IRuleBase<TContext> success, IRuleBase<TContext> failure, TContext context, CancellationToken cancellationToken = default) =>
        Evaluate(ConditionalRuleAdapter<TContext>.From(rule, success, failure), context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult> If<TContext, TResult>(IRuleBase<TContext, TResult> rule, IRuleBase<TContext, TResult> success, IRuleBase<TContext, TResult> failure, TContext context, CancellationToken cancellationToken = default) =>
        Evaluate(ConditionalRuleAdapter<TContext, TResult>.From(rule, success, failure), context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result If<TContext>(Func<TContext, Result> rule, Func<TContext, Result> success, Func<TContext, Result> failure, TContext context, CancellationToken cancellationToken = default) =>
        Evaluate(ConditionalRuleAdapter<TContext>.From(rule.ToRule(), success.ToRule(), failure.ToRule()), context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult> If<TContext, TResult>(Func<TContext, Result<TResult>> rule, Func<TContext, Result<TResult>> success, Func<TContext, Result<TResult>> failure, TContext context, CancellationToken cancellationToken = default) =>
        Evaluate(ConditionalRuleAdapter<TContext, TResult>.From(rule.ToRule(), success.ToRule(), failure.ToRule()), context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result If<TContext>(Func<TContext, CancellationToken, Result> rule, Func<TContext, CancellationToken, Result> success, Func<TContext, CancellationToken, Result> failure, TContext context, CancellationToken cancellationToken = default) =>
        Evaluate(ConditionalRuleAdapter<TContext>.From(rule.ToRule(), success.ToRule(), failure.ToRule()), context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult> If<TContext, TResult>(Func<TContext, CancellationToken, Result<TResult>> rule, Func<TContext, CancellationToken, Result<TResult>> success, Func<TContext, CancellationToken, Result<TResult>> failure, TContext context, CancellationToken cancellationToken = default) =>
        Evaluate(ConditionalRuleAdapter<TContext, TResult>.From(rule.ToRule(), success.ToRule(), failure.ToRule()), context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result If<TContext>(Func<TContext, CancellationToken, ValueTask<Result>> rule, Func<TContext, CancellationToken, ValueTask<Result>> success, Func<TContext, CancellationToken, ValueTask<Result>> failure, TContext context, CancellationToken cancellationToken = default) =>
        Evaluate(ConditionalRuleAdapter<TContext>.From(rule.ToRule(), success.ToRule(), failure.ToRule()), context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult> If<TContext, TResult>(Func<TContext, CancellationToken, ValueTask<Result<TResult>>> rule, Func<TContext, CancellationToken, ValueTask<Result<TResult>>> success, Func<TContext, CancellationToken, ValueTask<Result<TResult>>> failure, TContext context, CancellationToken cancellationToken = default) =>
        Evaluate(ConditionalRuleAdapter<TContext, TResult>.From(rule.ToRule(), success.ToRule(), failure.ToRule()), context, cancellationToken);
}