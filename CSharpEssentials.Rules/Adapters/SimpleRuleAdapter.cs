
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

internal readonly record struct SimpleRuleAdapter<TContext>(
    Func<TContext, Result> Rule
) : IRule<TContext>
{
    public Result Evaluate(TContext context, CancellationToken cancellationToken = default) => Rule(context);

    internal static SimpleRuleAdapter<TContext> From(Func<TContext, Result> rule) => new(rule);
}

internal readonly record struct SimpleRuleAdapter<TContext, TResult>(
    Func<TContext, Result<TResult>> Rule
) : IRule<TContext, TResult>
{
    public Result<TResult> Evaluate(TContext context, CancellationToken cancellationToken = default) => Rule(context);

    internal static SimpleRuleAdapter<TContext, TResult> From(Func<TContext, Result<TResult>> rule) => new(rule);
}
