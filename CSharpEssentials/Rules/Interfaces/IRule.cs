
namespace CSharpEssentials;

public interface IRule<TContext> : IRuleBase<TContext>
{
    Result Evaluate(TContext context, CancellationToken cancellationToken = default);
}

public interface IRule<TContext, TResult> : IRuleBase<TContext, TResult>
{
    Result<TResult> Evaluate(TContext context, CancellationToken cancellationToken = default);
}