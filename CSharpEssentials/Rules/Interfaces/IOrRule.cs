
namespace CSharpEssentials;

public interface IOrRule<TContext> : IRule<TContext>
{
    IRuleBase<TContext>[] Rules { get; }
}

public interface IOrRule<TContext, TResult> : IRule<TContext, TResult>
{
    IRuleBase<TContext, TResult>[] Rules { get; }
}