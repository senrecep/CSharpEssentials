
namespace CSharpEssentials;

public interface IOrAsyncRule<TContext> : IAsyncRule<TContext>
{
    IRuleBase<TContext>[] Rules { get; }
}

public interface IOrAsyncRule<TContext, TResult> : IAsyncRule<TContext, TResult>
{
    IRuleBase<TContext, TResult>[] Rules { get; }
}