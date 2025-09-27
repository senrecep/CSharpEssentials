
namespace CSharpEssentials.Rules;

public interface IAndAsyncRule<TContext> : IAsyncRule<TContext>
{
    IRuleBase<TContext>[] Rules { get; }
}

public interface IAndAsyncRule<TContext, TResult> : IAsyncRule<TContext, TResult>
{
    IRuleBase<TContext, TResult>[] Rules { get; }
}