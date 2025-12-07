using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Rules;

public static partial class RuleEngine
{
     public static Result And<TContext>(IRule<TContext>[] rules, TContext context, CancellationToken cancellationToken = default) =>
          Evaluate(rules.And(), context, cancellationToken);

     public static Result<TResult> And<TContext, TResult>(IRule<TContext, TResult>[] rules, TContext context, CancellationToken cancellationToken = default) =>
          Evaluate(rules.And(), context, cancellationToken);

     public static Result And<TContext>(Func<TContext, Result>[] rules, TContext context, CancellationToken cancellationToken = default) =>
          Evaluate(rules.And(), context, cancellationToken);

     public static Result<TResult> And<TContext, TResult>(Func<TContext, Result<TResult>>[] rules, TContext context, CancellationToken cancellationToken = default) =>
          Evaluate(rules.And(), context, cancellationToken);

     public static Result And<TContext>(Func<TContext, CancellationToken, Result>[] rules, TContext context, CancellationToken cancellationToken = default) =>
         Evaluate(rules.And(), context, cancellationToken);

     public static Result<TResult> And<TContext, TResult>(Func<TContext, CancellationToken, Result<TResult>>[] rules, TContext context, CancellationToken cancellationToken = default) =>
          Evaluate(rules.And(), context, cancellationToken);

     public static Result And<TContext>(Func<TContext, CancellationToken, ValueTask<Result>>[] rules, TContext context, CancellationToken cancellationToken = default) =>
          Evaluate(rules.And(), context, cancellationToken);

     public static Result<TResult> And<TContext, TResult>(Func<TContext, CancellationToken, ValueTask<Result<TResult>>>[] rules, TContext context, CancellationToken cancellationToken = default) =>
          Evaluate(rules.And(), context, cancellationToken);
}