
using System.Runtime.CompilerServices;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

public static partial class RuleEngine
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Evaluate<TContext>(Func<TContext, Result> rule, TContext context, CancellationToken cancellationToken = default) => Evaluate(rule.ToRule(), context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Evaluate<TContext>(Func<TContext, CancellationToken, Result> rule, TContext context, CancellationToken cancellationToken = default) => Evaluate(rule.ToRule(), context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult> Evaluate<TContext, TResult>(Func<TContext, Result<TResult>> rule, TContext context, CancellationToken cancellationToken = default) => Evaluate(rule.ToRule(), context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TResult> Evaluate<TContext, TResult>(Func<TContext, CancellationToken, Result<TResult>> rule, TContext context, CancellationToken cancellationToken = default) => Evaluate(rule.ToRule(), context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Result> Evaluate<TContext>(Func<TContext, CancellationToken, ValueTask<Result>> rule, TContext context, CancellationToken cancellationToken = default) => Evaluate(rule.ToRule(), context, cancellationToken);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Result<TResult>> Evaluate<TContext, TResult>(Func<TContext, CancellationToken, ValueTask<Result<TResult>>> rule, TContext context, CancellationToken cancellationToken = default) => Evaluate(rule.ToRule(), context, cancellationToken);

}
