using System.Runtime.CompilerServices;
using CSharpEssentials.Core;

namespace CSharpEssentials.Maybe;

public readonly partial struct Maybe<T>
{
    /// <summary>
    /// Executes the specified action if the Maybe has a value.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public async Task ExecuteAsync(Func<T, Task> action, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return;

        await action(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value.
    /// </summary>
    /// <param name="valueTask"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ExecuteAsync(Func<T, ValueTask> valueTask, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return;

        await valueTask(Value).WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value.
    /// </summary>
    /// <param name="action"></param>
    public void Execute(Action<T> action)
    {
        if (HasNoValue)
            return;

        action(Value);
    }


    /// <summary>
    /// Executes the specified action if the Maybe has no value.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [OverloadResolutionPriority(1)]
    public async Task ExecuteNoValueAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        if (HasValue)
            return;

        await action().WithCancellation(cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Executes the specified action if the Maybe has no value.
    /// </summary>
    /// <param name="valueTask"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ExecuteNoValueAsync(Func<ValueTask> valueTask, CancellationToken cancellationToken = default)
    {
        if (HasValue)
            return;

        await valueTask().WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has no value.
    /// </summary>
    /// <param name="action"></param>
    public void ExecuteNoValue(Action action)
    {
        if (HasValue)
            return;

        action();
    }


}

public static partial class MaybeExtensions
{
    /// <summary>
    /// Executes the specified action if the Maybe has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task ExecuteAsync<T>(this Task<Maybe<T>> maybeTask, Action<T> action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken).ConfigureAwait(false);

        if (maybe.HasNoValue)
            return;

        action(maybe.Value);
    }


    /// <summary>
    /// Executes the specified action if the Maybe has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="asyncAction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task ExecuteAsync<T>(this Task<Maybe<T>> maybeTask, Func<T, Task> asyncAction, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken).ConfigureAwait(false);

        if (maybe.HasNoValue)
            return;

        await asyncAction(maybe.Value).WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task ExecuteAsync<T>(this ValueTask<Maybe<T>> maybeTask, Action<T> action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken).ConfigureAwait(false);

        if (maybe.HasNoValue)
            return;

        action(maybe.Value);
    }



    /// <summary>
    /// Executes the specified action if the Maybe has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="valueTask"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task ExecuteAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<T, ValueTask> valueTask, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken).ConfigureAwait(false);

        if (maybe.HasNoValue)
            return;

        await valueTask(maybe.Value).ConfigureAwait(false);
    }


    /// <summary>
    /// Executes the specified action if the Maybe has no value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task ExecuteNoValueAsync<T>(this Task<Maybe<T>> maybeTask, Action action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken).ConfigureAwait(false);

        if (maybe.HasValue)
            return;

        action();
    }

    /// <summary>
    /// Executes the specified action if the Maybe has no value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="asyncAction"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task ExecuteNoValueAsync<T>(this Task<Maybe<T>> maybeTask, Func<Task> asyncAction, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken).ConfigureAwait(false);

        if (maybe.HasValue)
            return;

        await asyncAction().WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has no value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task ExecuteNoValueAsync<T>(this ValueTask<Maybe<T>> maybeTask, Action action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken).ConfigureAwait(false);

        if (maybe.HasValue)
            return;

        action();
    }


    /// <summary>
    /// Executes the specified action if the Maybe has no value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="valueTask"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task ExecuteNoValueAsync<T>(this ValueTask<Maybe<T>> maybeTask, Func<ValueTask> valueTask, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken).ConfigureAwait(false);

        if (maybe.HasValue)
            return;

        await valueTask().WithCancellation(cancellationToken).ConfigureAwait(false);
    }

}
