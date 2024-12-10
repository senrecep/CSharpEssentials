
namespace CSharpEssentials;

public interface IAsyncRuleBase<TContext> : IRuleBase<TContext>;
public interface IAsyncRuleBase<TContext, TResult> : IRuleBase<TContext, TResult>;