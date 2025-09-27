
namespace CSharpEssentials.Rules;

public interface ILinearAsyncRule<TContext> : IAsyncRule<TContext>
{
    IRuleBase<TContext>? Next { get; }
}

public interface ILinearAsyncRule<TContext, TResult> : IAsyncRule<TContext, TResult>
{
    IRuleBase<TContext, TResult>? Next { get; }
}