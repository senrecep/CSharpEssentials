using CSharpEssentials.Core;

namespace CSharpEssentials.Maybe;

public readonly partial struct Maybe<T>
{
    /// <summary>
    /// Binds the specified selector to the Maybe monad.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="selector"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<Maybe<TOut>> BindAsync<TOut>(
            Func<T, Task<Maybe<TOut>>> selector,
            CancellationToken cancellationToken = default) =>
                HasNoValue ? Maybe<TOut>.None.AsTask() : selector(Value).WithCancellation(cancellationToken);

    /// <summary>
    /// Binds the specified selector to the Maybe monad.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="selector"></param>
    /// <returns></returns>
    public Maybe<TOut> Bind<TOut>(
        Func<T, Maybe<TOut>> selector) =>
            HasNoValue ? Maybe<TOut>.None : selector(Value);

    /// <summary>
    /// Binds the specified selector to the Maybe monad.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="selector"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public Maybe<TOut> Bind<TOut, TContext>(
        Func<T, TContext, Maybe<TOut>> selector,
        TContext context
    ) => Bind((value) => selector(value, context));
}

public static partial class MaybeExtensions
{
    /// <summary>
    /// Binds the specified selector to the Maybe monad.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="selector"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Maybe<TOut>> BindAsync<T, TOut>(
        this Task<Maybe<T>> maybeTask,
            Func<T, Maybe<TOut>> selector,
            CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.Bind(selector);
    }
    /// <summary>
    /// Binds the specified selector to the Maybe monad.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="selector"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Maybe<TOut>> BindAsync<T, TOut>(
        this ValueTask<Maybe<T>> maybeTask,
            Func<T, ValueTask<Maybe<TOut>>> selector,
            CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        ValueTask<Maybe<TOut>> result = maybe.HasNoValue ? Maybe<TOut>.None.AsValueTask() : selector(maybe.Value).WithCancellation(cancellationToken);
        return await result;
    }
}
