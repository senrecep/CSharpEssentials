using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

public static partial class RuleEngine
{
    /// <summary>
    /// Evaluates <paramref name="rule"/> asynchronously, awaiting every async rule in the tree with
    /// <c>ConfigureAwait(false)</c>. Sync rules complete without allocating a task.
    /// Composite children are evaluated sequentially, with the same short-circuit and error order as <c>Evaluate</c>.
    /// </summary>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is cancelled; caller cancellation is not converted to an error.
    /// </exception>
    public static ValueTask<Result> EvaluateAsync<TContext>(IRuleBase<TContext> rule, TContext context, CancellationToken cancellationToken = default) =>
        DispatchAsync(rule, context, cancellationToken);

    private static ValueTask<Result> DispatchAsync<TContext>(IRuleBase<TContext> rule, TContext context, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return new ValueTask<Result>(Task.FromCanceled<Result>(cancellationToken));

        return rule switch
        {
            IConditionalAsyncRule<TContext> conditionalAsyncRule => ConditionalAsync(conditionalAsyncRule, conditionalAsyncRule.Success, conditionalAsyncRule.Failure, RuleTypes.ConditionalAsyncRule, context, cancellationToken),
            ILinearAsyncRule<TContext> linearAsyncRule => LinearAsync(linearAsyncRule, linearAsyncRule.Next, RuleTypes.LinearAsyncRule, context, cancellationToken),
            IAndAsyncRule<TContext> andAsyncRule => AndAsync(andAsyncRule, andAsyncRule.Rules, RuleTypes.AndAsyncRule, context, cancellationToken),
            IOrAsyncRule<TContext> orAsyncRule => OrAsync(orAsyncRule, orAsyncRule.Rules, RuleTypes.OrAsyncRule, context, cancellationToken),
            IAsyncRule<TContext> asyncRule => SimpleAsync(asyncRule, context, cancellationToken),
            IConditionalRule<TContext> conditionalRule => ConditionalAsync(conditionalRule, conditionalRule.Success, conditionalRule.Failure, RuleTypes.ConditionalRule, context, cancellationToken),
            ILinearRule<TContext> linearRule => LinearAsync(linearRule, linearRule.Next, RuleTypes.LinearRule, context, cancellationToken),
            IAndRule<TContext> andRule => AndAsync(andRule, andRule.Rules, RuleTypes.AndRule, context, cancellationToken),
            IOrRule<TContext> orRule => OrAsync(orRule, orRule.Rules, RuleTypes.OrRule, context, cancellationToken),
            IRule<TContext> simpleRule => SimpleSync(simpleRule, context, cancellationToken),
            _ => new ValueTask<Result>(RuleErrors.RuleEngineNotFoundError(rule.GetType().Name))
        };
    }

    private static ValueTask<Result> SimpleSync<TContext>(IRule<TContext> rule, TContext context, CancellationToken cancellationToken)
    {
        try
        {
            return new ValueTask<Result>(rule.Evaluate(context, cancellationToken));
        }
        catch (Exception ex) when (IsCallerCancellation(ex, cancellationToken))
        {
            return new ValueTask<Result>(Task.FromCanceled<Result>(cancellationToken));
        }
        catch (Exception ex)
        {
            return new ValueTask<Result>(RuleErrors.RuleEngineEvaluateError(RuleTypes.SimpleRule, ex));
        }
    }

    private static async ValueTask<Result> SimpleAsync<TContext>(IAsyncRule<TContext> rule, TContext context, CancellationToken cancellationToken)
    {
        try
        {
            return await rule.EvaluateAsync(context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (!IsCallerCancellation(ex, cancellationToken))
        {
            return RuleErrors.RuleEngineEvaluateError(RuleTypes.SimpleAsyncRule, ex);
        }
    }

    private static ValueTask<Result> EvaluateHead<TContext>(IRuleBase<TContext> rule, TContext context, CancellationToken cancellationToken) =>
        rule switch
        {
            IAsyncRule<TContext> asyncRule => asyncRule.EvaluateAsync(context, cancellationToken),
            IRule<TContext> syncRule => new ValueTask<Result>(syncRule.Evaluate(context, cancellationToken)),
            _ => new ValueTask<Result>(RuleErrors.RuleEngineNotFoundError(rule.GetType().Name))
        };

    private static async ValueTask<Result> LinearAsync<TContext>(IRuleBase<TContext> rule, IRuleBase<TContext>? next, string ruleType, TContext context, CancellationToken cancellationToken)
    {
        try
        {
            Result result = await EvaluateHead(rule, context, cancellationToken).ConfigureAwait(false);
            IRuleBase<TContext>? nextRule = LinearNext(result.IsFailure, next);
            if (nextRule is null)
                return result;
            return await DispatchAsync(nextRule, context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (!IsCallerCancellation(ex, cancellationToken))
        {
            return RuleErrors.RuleEngineEvaluateError(ruleType, ex);
        }
    }

    private static async ValueTask<Result> ConditionalAsync<TContext>(IRuleBase<TContext> rule, IRuleBase<TContext>? success, IRuleBase<TContext>? failure, string ruleType, TContext context, CancellationToken cancellationToken)
    {
        try
        {
            Result result = await EvaluateHead(rule, context, cancellationToken).ConfigureAwait(false);
            IRuleBase<TContext>? branch = ConditionalBranch(result.IsFailure, success, failure);
            if (branch is null)
                return result;
            return await DispatchAsync(branch, context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (!IsCallerCancellation(ex, cancellationToken))
        {
            return RuleErrors.RuleEngineEvaluateError(ruleType, ex);
        }
    }

    private static async ValueTask<Result> AndAsync<TContext>(IRuleBase<TContext> rule, IRuleBase<TContext>[] rules, string ruleType, TContext context, CancellationToken cancellationToken)
    {
        try
        {
            Result result = await EvaluateHead(rule, context, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
                return result;

            var results = new List<Result>(rules.Length);
            foreach (IRuleBase<TContext> child in rules)
            {
                Result childResult = await DispatchAsync(child, context, cancellationToken).ConfigureAwait(false);
                results.Add(childResult);
                if (childResult.IsFailure)
                    break;
            }
            return Result.And(results);
        }
        catch (Exception ex) when (!IsCallerCancellation(ex, cancellationToken))
        {
            return RuleErrors.RuleEngineEvaluateError(ruleType, ex);
        }
    }

    private static async ValueTask<Result> OrAsync<TContext>(IRuleBase<TContext> rule, IRuleBase<TContext>[] rules, string ruleType, TContext context, CancellationToken cancellationToken)
    {
        try
        {
            Result result = await EvaluateHead(rule, context, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
                return result;

            var results = new List<Result>(rules.Length);
            foreach (IRuleBase<TContext> child in rules)
            {
                Result childResult = await DispatchAsync(child, context, cancellationToken).ConfigureAwait(false);
                results.Add(childResult);
                if (childResult.IsSuccess)
                    break;
            }
            return Result.Or(results);
        }
        catch (Exception ex) when (!IsCallerCancellation(ex, cancellationToken))
        {
            return RuleErrors.RuleEngineEvaluateError(ruleType, ex);
        }
    }
}
