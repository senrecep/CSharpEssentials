
namespace CSharpEssentials;

public interface IConditionalAsyncRule<TContext> : IAsyncRule<TContext>
{
    IRuleBase<TContext>? Success { get; }
    IRuleBase<TContext>? Failure { get; }
}

public interface IConditionalAsyncRule<TContext, TResult> : IAsyncRule<TContext, TResult>
{
    IRuleBase<TContext, TResult>? Success { get; }
    IRuleBase<TContext, TResult>? Failure { get; }
}