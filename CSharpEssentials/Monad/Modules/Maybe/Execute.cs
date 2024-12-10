namespace CSharpEssentials;

public readonly partial struct Maybe<T>
{
    /// <summary>
    /// Executes the specified action if the Maybe has a value.
    /// </summary>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Execute(Func<T, Task> action, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return;

        await action(Value).WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value.
    /// </summary>
    /// <param name="valueTask"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Execute(Func<T, ValueTask> valueTask, CancellationToken cancellationToken = default)
    {
        if (HasNoValue)
            return;

        await valueTask(Value).WithCancellation(cancellationToken);
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
    public async Task ExecuteNoValue(Func<Task> action, CancellationToken cancellationToken = default)
    {
        if (HasValue)
            return;

        await action().WithCancellation(cancellationToken);
    }


    /// <summary>
    /// Executes the specified action if the Maybe has no value.
    /// </summary>
    /// <param name="valueTask"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task ExecuteNoValue(Func<ValueTask> valueTask, CancellationToken cancellationToken = default)
    {
        if (HasValue)
            return;

        await valueTask().WithCancellation(cancellationToken);
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
    public static async Task Execute<T>(this Task<Maybe<T>> maybeTask, Action<T> action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

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
    public static async Task Execute<T>(this Task<Maybe<T>> maybeTask, Func<T, Task> asyncAction, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasNoValue)
            return;

        await asyncAction(maybe.Value).WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task Execute<T>(this ValueTask<Maybe<T>> maybeTask, Action<T> action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

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
    public static async Task Execute<T>(this ValueTask<Maybe<T>> maybeTask, Func<T, ValueTask> valueTask, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasNoValue)
            return;

        await valueTask(maybe.Value);
    }


    /// <summary>
    /// Executes the specified action if the Maybe has no value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task ExecuteNoValue<T>(this Task<Maybe<T>> maybeTask, Action action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

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
    public static async Task ExecuteNoValue<T>(this Task<Maybe<T>> maybeTask, Func<Task> asyncAction, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasValue)
            return;

        await asyncAction().WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Executes the specified action if the Maybe has no value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="action"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task ExecuteNoValue<T>(this ValueTask<Maybe<T>> maybeTask, Action action, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

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
    public static async Task ExecuteNoValue<T>(this ValueTask<Maybe<T>> maybeTask, Func<ValueTask> valueTask, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);

        if (maybe.HasValue)
            return;

        await valueTask().WithCancellation(cancellationToken);
    }

}
