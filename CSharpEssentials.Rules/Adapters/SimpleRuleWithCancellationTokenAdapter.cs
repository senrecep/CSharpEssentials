
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

internal readonly record struct SimpleRuleWithCancellationTokenAdapter<TContext>(
    Func<TContext, CancellationToken, Result> Rule
) : IRule<TContext>
{
    public Result Evaluate(TContext context, CancellationToken cancellationToken = default) => Rule(context, cancellationToken);

    internal static SimpleRuleWithCancellationTokenAdapter<TContext> From(Func<TContext, CancellationToken, Result> rule) => new(rule);
}


internal readonly record struct SimpleRuleWithCancellationTokenAdapter<TContext, TResult>(
    Func<TContext, CancellationToken, Result<TResult>> Rule
) : IRule<TContext, TResult>
{
    public Result<TResult> Evaluate(TContext context, CancellationToken cancellationToken = default) => Rule(context, cancellationToken);

    internal static SimpleRuleWithCancellationTokenAdapter<TContext, TResult> From(Func<TContext, CancellationToken, Result<TResult>> rule) => new(rule);
}

