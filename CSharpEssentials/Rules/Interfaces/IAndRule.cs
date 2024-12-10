
namespace CSharpEssentials;

public interface IAndRule<TContext> : IRule<TContext>
{
    IRuleBase<TContext>[] Rules { get; }
}

public interface IAndRule<TContext, TResult> : IRule<TContext, TResult>
{
    IRuleBase<TContext, TResult>[] Rules { get; }
}