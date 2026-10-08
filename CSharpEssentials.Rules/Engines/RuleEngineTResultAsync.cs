using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

public static partial class RuleEngine
{
    /// <summary>
    /// Evaluates <paramref name="rule"/> asynchronously, awaiting every async rule in the tree with
    /// <c>ConfigureAwait(false)</c>. Sync rules complete without allocating a task.
    /// Composite children are evaluated sequentially, with the same short-circuit and error order as <c>Evaluate</c>.
    /// Rules must observe <paramref name="cancellationToken"/>: an in-flight rule is awaited until it returns, never abandoned.
    /// </summary>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <paramref name="cancellationToken"/> is cancelled; caller cancellation is not converted to an error.
    /// </exception>
    public static ValueTask<Result<TResult>> EvaluateAsync<TContext, TResult>(IRuleBase<TContext, TResult> rule, TContext context, CancellationToken cancellationToken = default) =>
        DispatchAsync(rule, context, cancellationToken);

    private static ValueTask<Result<TResult>> DispatchAsync<TContext, TResult>(IRuleBase<TContext, TResult> rule, TContext context, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return new ValueTask<Result<TResult>>(Task.FromCanceled<Result<TResult>>(cancellationToken));

        return rule switch
        {
            IConditionalAsyncRule<TContext, TResult> conditionalAsyncRule => ConditionalAsync(conditionalAsyncRule, conditionalAsyncRule.Success, conditionalAsyncRule.Failure, RuleTypes.ConditionalAsyncRule, context, cancellationToken),
            ILinearAsyncRule<TContext, TResult> linearAsyncRule => LinearAsync(linearAsyncRule, linearAsyncRule.Next, RuleTypes.LinearAsyncRule, context, cancellationToken),
            IAndAsyncRule<TContext, TResult> andAsyncRule => AndAsync(andAsyncRule, andAsyncRule.Rules, RuleTypes.AndAsyncRule, context, cancellationToken),
            IOrAsyncRule<TContext, TResult> orAsyncRule => OrAsync(orAsyncRule, orAsyncRule.Rules, RuleTypes.OrAsyncRule, context, cancellationToken),
            IAsyncRule<TContext, TResult> asyncRule => SimpleAsync(asyncRule, context, cancellationToken),
            IConditionalRule<TContext, TResult> conditionalRule => ConditionalAsync(conditionalRule, conditionalRule.Success, conditionalRule.Failure, RuleTypes.ConditionalRule, context, cancellationToken),
            ILinearRule<TContext, TResult> linearRule => LinearAsync(linearRule, linearRule.Next, RuleTypes.LinearRule, context, cancellationToken),
            IAndRule<TContext, TResult> andRule => AndAsync(andRule, andRule.Rules, RuleTypes.AndRule, context, cancellationToken),
            IOrRule<TContext, TResult> orRule => OrAsync(orRule, orRule.Rules, RuleTypes.OrRule, context, cancellationToken),
            IRule<TContext, TResult> simpleRule => SimpleSync(simpleRule, context, cancellationToken),
            _ => new ValueTask<Result<TResult>>(RuleErrors.RuleEngineNotFoundError(rule.GetType().Name))
        };
    }

    private static ValueTask<Result<TResult>> SimpleSync<TContext, TResult>(IRule<TContext, TResult> rule, TContext context, CancellationToken cancellationToken)
    {
        try
        {
            return new ValueTask<Result<TResult>>(rule.Evaluate(context, cancellationToken));
        }
        catch (Exception ex) when (IsCallerCancellation(ex, cancellationToken))
        {
            return new ValueTask<Result<TResult>>(Task.FromCanceled<Result<TResult>>(cancellationToken));
        }
        catch (Exception ex)
        {
            return new ValueTask<Result<TResult>>(RuleErrors.RuleEngineEvaluateError(RuleTypes.SimpleRule, ex));
        }
    }

    private static async ValueTask<Result<TResult>> SimpleAsync<TContext, TResult>(IAsyncRule<TContext, TResult> rule, TContext context, CancellationToken cancellationToken)
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

    private static ValueTask<Result<TResult>> EvaluateHead<TContext, TResult>(IRuleBase<TContext, TResult> rule, TContext context, CancellationToken cancellationToken) =>
        rule switch
        {
            IAsyncRule<TContext, TResult> asyncRule => asyncRule.EvaluateAsync(context, cancellationToken),
            IRule<TContext, TResult> syncRule => new ValueTask<Result<TResult>>(syncRule.Evaluate(context, cancellationToken)),
            _ => new ValueTask<Result<TResult>>(RuleErrors.RuleEngineNotFoundError(rule.GetType().Name))
        };

    private static async ValueTask<Result<TResult>> LinearAsync<TContext, TResult>(IRuleBase<TContext, TResult> rule, IRuleBase<TContext, TResult>? next, string ruleType, TContext context, CancellationToken cancellationToken)
    {
        try
        {
            Result<TResult> result = await EvaluateHead(rule, context, cancellationToken).ConfigureAwait(false);
            IRuleBase<TContext, TResult>? nextRule = LinearNext(result.IsFailure, next);
            if (nextRule is null)
                return result;
            return await DispatchAsync(nextRule, context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (!IsCallerCancellation(ex, cancellationToken))
        {
            return RuleErrors.RuleEngineEvaluateError(ruleType, ex);
        }
    }

    private static async ValueTask<Result<TResult>> ConditionalAsync<TContext, TResult>(IRuleBase<TContext, TResult> rule, IRuleBase<TContext, TResult>? success, IRuleBase<TContext, TResult>? failure, string ruleType, TContext context, CancellationToken cancellationToken)
    {
        try
        {
            Result<TResult> result = await EvaluateHead(rule, context, cancellationToken).ConfigureAwait(false);
            IRuleBase<TContext, TResult>? branch = ConditionalBranch(result.IsFailure, success, failure);
            if (branch is null)
                return result;
            return await DispatchAsync(branch, context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (!IsCallerCancellation(ex, cancellationToken))
        {
            return RuleErrors.RuleEngineEvaluateError(ruleType, ex);
        }
    }

    private static async ValueTask<Result<TResult>> AndAsync<TContext, TResult>(IRuleBase<TContext, TResult> rule, IRuleBase<TContext, TResult>[] rules, string ruleType, TContext context, CancellationToken cancellationToken)
    {
        try
        {
            Result<TResult> result = await EvaluateHead(rule, context, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
                return result;

            var results = new List<Result<TResult>>(rules.Length);
            foreach (IRuleBase<TContext, TResult> child in rules)
            {
                Result<TResult> childResult = await DispatchAsync(child, context, cancellationToken).ConfigureAwait(false);
                results.Add(childResult);
            }
            return FirstAndValue(Result<TResult>.And(results));
        }
        catch (Exception ex) when (!IsCallerCancellation(ex, cancellationToken))
        {
            return RuleErrors.RuleEngineEvaluateError(ruleType, ex);
        }
    }

    private static async ValueTask<Result<TResult>> OrAsync<TContext, TResult>(IRuleBase<TContext, TResult> rule, IRuleBase<TContext, TResult>[] rules, string ruleType, TContext context, CancellationToken cancellationToken)
    {
        try
        {
            Result<TResult> result = await EvaluateHead(rule, context, cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
                return result;

            var results = new List<Result<TResult>>(rules.Length);
            foreach (IRuleBase<TContext, TResult> child in rules)
            {
                Result<TResult> childResult = await DispatchAsync(child, context, cancellationToken).ConfigureAwait(false);
                results.Add(childResult);
                if (childResult.IsSuccess)
                    break;
            }
            return Result<TResult>.Or(results);
        }
        catch (Exception ex) when (!IsCallerCancellation(ex, cancellationToken))
        {
            return RuleErrors.RuleEngineEvaluateError(ruleType, ex);
        }
    }
}
