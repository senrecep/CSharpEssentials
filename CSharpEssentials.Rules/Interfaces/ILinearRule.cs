
namespace CSharpEssentials.Rules;

public interface ILinearRule<TContext> : IRule<TContext>
{
    IRuleBase<TContext>? Next { get; }
}

public interface ILinearRule<TContext, TResult> : IRule<TContext, TResult>
{
    IRuleBase<TContext, TResult>? Next { get; }
}