using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

internal sealed class DelegateRuleAdapter<TContext>(
    Func<TContext, bool> predicate,
    Func<TContext, Error> errorFactory
) : IRule<TContext>
{
    public Result Evaluate(TContext context, CancellationToken cancellationToken = default)
        => predicate(context) ? Result.Success() : Result.Failure(errorFactory(context));
}

internal sealed class AsyncDelegateRuleAdapter<TContext>(
    Func<TContext, Task<bool>> predicate,
    Func<TContext, Error> errorFactory
) : IAsyncRule<TContext>
{
    public async ValueTask<Result> EvaluateAsync(TContext context, CancellationToken cancellationToken = default)
        => await predicate(context).ConfigureAwait(false)
            ? Result.Success()
            : Result.Failure(errorFactory(context));
}
