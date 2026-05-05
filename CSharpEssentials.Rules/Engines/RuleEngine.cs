using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

public static partial class RuleEngine
{
    public static Result Evaluate<TContext>(IRuleBase<TContext> rule, TContext context, CancellationToken cancellationToken = default) =>
        InternalEvaluate(rule, context, cancellationToken);

    private static Result Evaluate<TContext>(IRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return Result.Try(
            () => rule.Evaluate(context, cancellationToken),
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.SimpleRule, ex));
    }

    private static ValueTask<Result> Evaluate<TContext>(IAsyncRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return new ValueTask<Result>(Result.TryAsync(
            () => rule.EvaluateAsync(context, cancellationToken).AsTask(),
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.SimpleAsyncRule, ex),
            cancellationToken));
    }

    private static Result Evaluate<TContext>(ILinearRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return Result.Try(
            () =>
            {
                Result result = rule.Evaluate(context, cancellationToken);
                if (result.IsFailure)
                    return result;
                if (rule.Next is null)
                    return result;

                return InternalEvaluate(rule.Next, context, cancellationToken);
            },
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.LinearRule, ex));
    }

    private static ValueTask<Result> Evaluate<TContext>(ILinearAsyncRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return new ValueTask<Result>(Result.TryAsync(
            async () =>
            {
                Result result = await rule.EvaluateAsync(context, cancellationToken);
                if (result.IsFailure)
                    return result;
                if (rule.Next is null)
                    return result;
                return InternalEvaluate(rule.Next, context, cancellationToken);
            },
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.LinearAsyncRule, ex),
            cancellationToken));
    }

    private static Result Evaluate<TContext>(IConditionalRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return Result.Try(
            () =>
            {
                Result result = rule.Evaluate(context, cancellationToken);
                if (result.IsFailure)
                    if (rule.Failure is null)
                        return result;
                    else
                        return InternalEvaluate(rule.Failure, context, cancellationToken);

                if (rule.Success is null)
                    return result;
                return InternalEvaluate(rule.Success, context, cancellationToken);
            },
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.ConditionalRule, ex));
    }
    private static ValueTask<Result> Evaluate<TContext>(IConditionalAsyncRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return new ValueTask<Result>(Result.TryAsync(
            async () =>
            {
                Result result = await rule.EvaluateAsync(context, cancellationToken);
                if (result.IsFailure)
                    if (rule.Failure is null)
                        return result;
                    else
                        return InternalEvaluate(rule.Failure, context, cancellationToken);

                if (rule.Success is null)
                    return result;
                return InternalEvaluate(rule.Success, context, cancellationToken);
            },
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.ConditionalAsyncRule, ex),
            cancellationToken));
    }

    private static Result Evaluate<TContext>(IAndRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return Result.Try(
            () =>
            {
                Result result = rule.Evaluate(context, cancellationToken);
                if (result.IsFailure)
                    return result;

                return Result.And(rule.Rules.Select(r => InternalEvaluate(r, context, cancellationToken)));
            },
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.AndRule, ex));
    }

    private static ValueTask<Result> Evaluate<TContext>(IAndAsyncRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return new ValueTask<Result>(Result.TryAsync(
            async () =>
            {
                Result result = await rule.EvaluateAsync(context, cancellationToken);
                if (result.IsFailure)
                    return result;

                return Result.And(rule.Rules.Select(r => InternalEvaluate(r, context, cancellationToken)));
            },
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.AndAsyncRule, ex),
            cancellationToken));
    }

    private static Result Evaluate<TContext>(IOrRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return Result.Try(
            () =>
            {
                Result result = rule.Evaluate(context, cancellationToken);
                if (result.IsFailure)
                    return result;

                return Result.Or(rule.Rules.Select(r => InternalEvaluate(r, context, cancellationToken)));
            },
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.OrRule, ex));
    }

    private static ValueTask<Result> Evaluate<TContext>(IOrAsyncRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return new ValueTask<Result>(Result.TryAsync(
            async () =>
            {
                Result result = await rule.EvaluateAsync(context, cancellationToken);
                if (result.IsFailure)
                    return result;

                return Result.Or(rule.Rules.Select(r => InternalEvaluate(r, context, cancellationToken)));
            },
            ex => RuleErrors.RuleEngineEvaluateError(RuleTypes.OrAsyncRule, ex),
            cancellationToken));
    }

    private static Result InternalEvaluate<TContext>(IRuleBase<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return rule switch
        {
            IAsyncRule<TContext> asyncRule => InternalEvaluateAsync(asyncRule, context, cancellationToken).GetAwaiter().GetResult(),
            IConditionalRule<TContext> conditionalRule => Evaluate(conditionalRule, context, cancellationToken),
            ILinearRule<TContext> linearRule => Evaluate(linearRule, context, cancellationToken),
            IAndRule<TContext> andRule => Evaluate(andRule, context, cancellationToken),
            IOrRule<TContext> orRule => Evaluate(orRule, context, cancellationToken),
            IRule<TContext> simpleRule => Evaluate(simpleRule, context, cancellationToken),
            _ => RuleErrors.RuleEngineNotFoundError(rule.GetType().Name)
        };
    }

    private static async Task<Result> InternalEvaluateAsync<TContext>(IRuleBase<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        return rule switch
        {
            IConditionalAsyncRule<TContext> asyncConditionalRule => await Evaluate(asyncConditionalRule, context, cancellationToken),
            ILinearAsyncRule<TContext> asyncLinearRule => await Evaluate(asyncLinearRule, context, cancellationToken),
            IAndAsyncRule<TContext> asyncAndRule => await Evaluate(asyncAndRule, context, cancellationToken),
            IOrAsyncRule<TContext> asyncOrRule => await Evaluate(asyncOrRule, context, cancellationToken),
            IAsyncRule<TContext> asyncRule => await Evaluate(asyncRule, context, cancellationToken),
            _ => RuleErrors.RuleEngineNotFoundError(rule.GetType().Name)
        };
    }
}
