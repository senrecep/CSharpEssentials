
namespace CSharpEssentials;

public interface IConditionalRule<TContext> : IRule<TContext>
{
    IRuleBase<TContext>? Success { get; }
    IRuleBase<TContext>? Failure { get; }
}

public interface IConditionalRule<TContext, TResult> : IRule<TContext, TResult>
{
    IRuleBase<TContext, TResult>? Success { get; }
    IRuleBase<TContext, TResult>? Failure { get; }
}