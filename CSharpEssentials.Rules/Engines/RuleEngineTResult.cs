using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

public static partial class RuleEngine
{
    /// <summary>
    /// Evaluates <paramref name="rule"/> synchronously. Async rules in the tree are blocked on until they complete,
    /// which can deadlock under a <see cref="SynchronizationContext"/> and ties up a thread-pool thread.
    /// Prefer <see cref="EvaluateAsync{TContext, TResult}"/> when the tree contains async rules.
    /// Caller cancellation from an async rule propagates as OperationCanceledException until it reaches an enclosing sync rule
    /// (simple, And, Or, Linear, Conditional), which returns it as an error Result; with no enclosing sync rule Evaluate throws.
    /// On cancellation Evaluate stops waiting for a pending async rule, which may keep running. EvaluateAsync always throws.
    /// </summary>
    public static Result<TResult> Evaluate<TContext, TResult>(IRuleBase<TContext, TResult> rule, TContext context, CancellationToken cancellationToken = default) =>
            InternalEvaluate(rule, context, cancellationToken);

    private static Result<TResult> Evaluate<TContext, TResult>(IRule<TContext, TResult> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return Result.Try(
            () => rule.Evaluate(context, cancellationToken),
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.SimpleRule, ex));
    }
    private static ValueTask<Result<TResult>> Evaluate<TContext, TResult>(IAsyncRule<TContext, TResult> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return new ValueTask<Result<TResult>>(Result.TryAsync(
            () => rule.EvaluateAsync(context, cancellationToken).AsTask(),
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.SimpleAsyncRule, ex),
            cancellationToken));
    }

    private static Result<TResult> Evaluate<TContext, TResult>(ILinearRule<TContext, TResult> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return Result.Try(
            () =>
            {
                Result<TResult> result = rule.Evaluate(context, cancellationToken);
                IRuleBase<TContext, TResult>? next = LinearNext(result.IsFailure, rule.Next);
                if (next is null)
                    return result;
                return InternalEvaluate(next, context, cancellationToken);
            },
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.LinearRule, ex));
    }

    private static ValueTask<Result<TResult>> Evaluate<TContext, TResult>(ILinearAsyncRule<TContext, TResult> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return new ValueTask<Result<TResult>>(Result.TryAsync(
            async () =>
            {
                Result<TResult> result = await rule.EvaluateAsync(context, cancellationToken).ConfigureAwait(false);
                IRuleBase<TContext, TResult>? next = LinearNext(result.IsFailure, rule.Next);
                if (next is null)
                    return result;
                return InternalEvaluate(next, context, cancellationToken);
            },
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.LinearAsyncRule, ex),
            cancellationToken));
    }

    private static Result<TResult> Evaluate<TContext, TResult>(IConditionalRule<TContext, TResult> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return Result.Try(
            () =>
            {
                Result<TResult> result = rule.Evaluate(context, cancellationToken);
                IRuleBase<TContext, TResult>? branch = ConditionalBranch(result.IsFailure, rule.Success, rule.Failure);
                if (branch is null)
                    return result;
                return InternalEvaluate(branch, context, cancellationToken);
            },
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.ConditionalRule, ex));
    }

    private static ValueTask<Result<TResult>> Evaluate<TContext, TResult>(IConditionalAsyncRule<TContext, TResult> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return new ValueTask<Result<TResult>>(Result.TryAsync(
            async () =>
            {
                Result<TResult> result = await rule.EvaluateAsync(context, cancellationToken).ConfigureAwait(false);
                IRuleBase<TContext, TResult>? branch = ConditionalBranch(result.IsFailure, rule.Success, rule.Failure);
                if (branch is null)
                    return result;
                return InternalEvaluate(branch, context, cancellationToken);
            },
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.ConditionalAsyncRule, ex),
            cancellationToken));
    }

    private static Result<TResult> Evaluate<TContext, TResult>(IAndRule<TContext, TResult> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return Result.Try(
            () =>
            {
                Result<TResult> result = rule.Evaluate(context, cancellationToken);
                if (result.IsFailure)
                    return result;

                Result<TResult[]> andResult = Result<TResult>.And(rule.Rules.Select(r => InternalEvaluate(r, context, cancellationToken)));
                return FirstAndValue(andResult);
            },
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.AndRule, ex));
    }

    private static ValueTask<Result<TResult>> Evaluate<TContext, TResult>(IAndAsyncRule<TContext, TResult> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return new ValueTask<Result<TResult>>(Result.TryAsync(
            async () =>
            {
                Result<TResult> result = await rule.EvaluateAsync(context, cancellationToken).ConfigureAwait(false);
                if (result.IsFailure)
                    return result;

                Result<TResult[]> andResult = Result<TResult>.And(rule.Rules.Select(r => InternalEvaluate(r, context, cancellationToken)));
                return FirstAndValue(andResult);
            },
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.AndAsyncRule, ex),
            cancellationToken));
    }

    private static Result<TResult> Evaluate<TContext, TResult>(IOrRule<TContext, TResult> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return Result.Try(
            () =>
            {
                Result<TResult> result = rule.Evaluate(context, cancellationToken);
                if (result.IsFailure)
                    return result;

                return Result<TResult>.Or(rule.Rules.Select(r => InternalEvaluate(r, context, cancellationToken)));
            },
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.OrRule, ex));
    }

    private static ValueTask<Result<TResult>> Evaluate<TContext, TResult>(IOrAsyncRule<TContext, TResult> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return new ValueTask<Result<TResult>>(Result.TryAsync(
            async () =>
            {
                Result<TResult> result = await rule.EvaluateAsync(context, cancellationToken).ConfigureAwait(false);
                if (result.IsFailure)
                    return result;

                return Result<TResult>.Or(rule.Rules.Select(r => InternalEvaluate(r, context, cancellationToken)));
            },
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.OrAsyncRule, ex),
            cancellationToken));
    }

    private static Result<TResult> InternalEvaluate<TContext, TResult>(IRuleBase<TContext, TResult> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return rule switch
        {
            IAsyncRule<TContext, TResult> asyncRule => InternalEvaluateAsync(asyncRule, context, cancellationToken).GetAwaiter().GetResult(),
            IConditionalRule<TContext, TResult> conditionalRule => Evaluate(conditionalRule, context, cancellationToken),
            ILinearRule<TContext, TResult> linearRule => Evaluate(linearRule, context, cancellationToken),
            IAndRule<TContext, TResult> andRule => Evaluate(andRule, context, cancellationToken),
            IOrRule<TContext, TResult> orRule => Evaluate(orRule, context, cancellationToken),
            IRule<TContext, TResult> simpleRule => Evaluate(simpleRule, context, cancellationToken),
            _ => RuleErrors.RuleEngineNotFoundError(rule.GetType().Name)
        };
    }

    private static async Task<Result<TResult>> InternalEvaluateAsync<TContext, TResult>(IRuleBase<TContext, TResult> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return rule switch
        {
            IConditionalAsyncRule<TContext, TResult> asyncConditionalRule => await Evaluate(asyncConditionalRule, context, cancellationToken).ConfigureAwait(false),
            ILinearAsyncRule<TContext, TResult> asyncLinearRule => await Evaluate(asyncLinearRule, context, cancellationToken).ConfigureAwait(false),
            IAndAsyncRule<TContext, TResult> asyncAndRule => await Evaluate(asyncAndRule, context, cancellationToken).ConfigureAwait(false),
            IOrAsyncRule<TContext, TResult> asyncOrRule => await Evaluate(asyncOrRule, context, cancellationToken).ConfigureAwait(false),
            IAsyncRule<TContext, TResult> asyncRule => await Evaluate(asyncRule, context, cancellationToken).ConfigureAwait(false),
            _ => RuleErrors.RuleEngineNotFoundError(rule.GetType().Name)
        };
    }
}
