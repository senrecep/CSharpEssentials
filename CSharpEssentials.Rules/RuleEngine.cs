using System.Data;
using CSharpEssentials.Core;
using CSharpEssentials.Results;
using System.Runtime.CompilerServices;

namespace CSharpEssentials.Rules;

public static partial class RuleEngine
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result Evaluate<TContext>(IRuleBase<TContext> rule, TContext context, CancellationToken cancellationToken = default) =>
        InternalEvaluate(rule, context, cancellationToken);

    private static Result Evaluate<TContext>(IRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            Result result = rule.Evaluate(context, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            return RuleErrors.RuleEngineEvaluateError(RuleTypes.SimpleRule, ex);
        }
    }

    private static async ValueTask<Result> Evaluate<TContext>(IAsyncRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            Result result = await rule.EvaluateAsync(context, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            return RuleErrors.RuleEngineEvaluateError(RuleTypes.SimpleAsyncRule, ex);
        }
    }

    private static Result Evaluate<TContext>(ILinearRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            Result result = rule.Evaluate(context, cancellationToken);
            if (result.IsFailure)
                return result;
            if (rule.Next is null)
                return result;

            return InternalEvaluate(rule.Next, context, cancellationToken);
        }
        catch (Exception ex)
        {
            return RuleErrors.RuleEngineEvaluateError(RuleTypes.LinearRule, ex);
        }
    }

    private static async ValueTask<Result> Evaluate<TContext>(ILinearAsyncRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            Result result = await rule.EvaluateAsync(context, cancellationToken);
            if (result.IsFailure)
                return result;
            if (rule.Next is null)
                return result;
#pragma warning disable S6966
            return InternalEvaluate(rule.Next, context, cancellationToken);
#pragma warning restore S6966
        }
        catch (Exception ex)
        {
            return RuleErrors.RuleEngineEvaluateError(RuleTypes.LinearAsyncRule, ex);
        }
    }

    private static Result Evaluate<TContext>(IConditionalRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            Result result = rule.Evaluate(context, cancellationToken);
            if (result.IsFailure)
                if (rule.Failure.IsNull())
                    return result;
                else
                    return InternalEvaluate(rule.Failure, context, cancellationToken);

            if (rule.Success.IsNull())
                return result;
            return InternalEvaluate(rule.Success, context, cancellationToken);
        }
        catch (Exception ex)
        {
            return RuleErrors.RuleEngineEvaluateError(RuleTypes.ConditionalRule, ex);
        }
    }
    private static async ValueTask<Result> Evaluate<TContext>(IConditionalAsyncRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            Result result = await rule.EvaluateAsync(context, cancellationToken);
            if (result.IsFailure)
                if (rule.Failure.IsNull())
                    return result;
                else
#pragma warning disable S6966
                    return InternalEvaluate(rule.Failure, context, cancellationToken);
#pragma warning restore S6966

            if (rule.Success.IsNull())
                return result;
#pragma warning disable S6966
            return InternalEvaluate(rule.Success, context, cancellationToken);
#pragma warning restore S6966
        }
        catch (Exception ex)
        {
            return RuleErrors.RuleEngineEvaluateError(RuleTypes.ConditionalAsyncRule, ex);
        }
    }

    private static Result Evaluate<TContext>(IAndRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            Result result = rule.Evaluate(context, cancellationToken);
            if (result.IsFailure)
                return result;

            return Result.And(rule.Rules.Select(r => InternalEvaluate(r, context, cancellationToken)));
        }
        catch (Exception ex)
        {
            return RuleErrors.RuleEngineEvaluateError(RuleTypes.AndRule, ex);
        }
    }

    private static async ValueTask<Result> Evaluate<TContext>(IAndAsyncRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            Result result = await rule.EvaluateAsync(context, cancellationToken);
            if (result.IsFailure)
                return result;

            return Result.And(rule.Rules.Select(r => InternalEvaluate(r, context, cancellationToken)));
        }
        catch (Exception ex)
        {
            return RuleErrors.RuleEngineEvaluateError(RuleTypes.AndAsyncRule, ex);
        }
    }

    private static Result Evaluate<TContext>(IOrRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            Result result = rule.Evaluate(context, cancellationToken);
            if (result.IsFailure)
                return result;

            return Result.Or(rule.Rules.Select(r => InternalEvaluate(r, context, cancellationToken)));
        }
        catch (Exception ex)
        {
            return RuleErrors.RuleEngineEvaluateError(RuleTypes.OrRule, ex);
        }
    }

    private static async ValueTask<Result> Evaluate<TContext>(IOrAsyncRule<TContext> rule, TContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            Result result = await rule.EvaluateAsync(context, cancellationToken);
            if (result.IsFailure)
                return result;

            return Result.Or(rule.Rules.Select(r => InternalEvaluate(r, context, cancellationToken)));
        }
        catch (Exception ex)
        {
            return RuleErrors.RuleEngineEvaluateError(RuleTypes.OrAsyncRule, ex);
        }
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
