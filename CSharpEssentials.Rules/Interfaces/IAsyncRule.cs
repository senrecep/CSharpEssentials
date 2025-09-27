
using CSharpEssentials.Results;

namespace CSharpEssentials.Rules;

public interface IAsyncRule<TContext> : IAsyncRuleBase<TContext>
{
    ValueTask<Result> EvaluateAsync(TContext context, CancellationToken cancellationToken = default);
}

public interface IAsyncRule<TContext, TResult> : IAsyncRuleBase<TContext, TResult>
{
    ValueTask<Result<TResult>> EvaluateAsync(TContext context, CancellationToken cancellationToken = default);
}