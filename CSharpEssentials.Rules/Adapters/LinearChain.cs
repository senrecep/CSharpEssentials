namespace CSharpEssentials.Rules;

internal static class LinearChain
{
    internal static IRuleBase<TContext> Append<TContext>(IRuleBase<TContext> tail, IRuleBase<TContext> next) =>
        tail switch
        {
            ILinearRule<TContext> { Next: { } rest } linear => new LinearRuleAdapter<TContext>(linear, Append(rest, next)),
            ILinearAsyncRule<TContext> { Next: { } rest } linear => new LinearAsyncRuleAdapter<TContext>(linear, Append(rest, next)),
            IRule<TContext> rule => new LinearRuleAdapter<TContext>(rule, next),
            IAsyncRule<TContext> rule => new LinearAsyncRuleAdapter<TContext>(rule, next),
            _ => tail
        };

    internal static IRuleBase<TContext, TResult> Append<TContext, TResult>(IRuleBase<TContext, TResult> tail, IRuleBase<TContext, TResult> next) =>
        tail switch
        {
            ILinearRule<TContext, TResult> { Next: { } rest } linear => new LinearRuleAdapter<TContext, TResult>(linear, Append(rest, next)),
            ILinearAsyncRule<TContext, TResult> { Next: { } rest } linear => new LinearAsyncRuleAdapter<TContext, TResult>(linear, Append(rest, next)),
            IRule<TContext, TResult> rule => new LinearRuleAdapter<TContext, TResult>(rule, next),
            IAsyncRule<TContext, TResult> rule => new LinearAsyncRuleAdapter<TContext, TResult>(rule, next),
            _ => tail
        };
}
