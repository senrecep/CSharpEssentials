namespace CSharpEssentials;

public static partial class MaybeExtensions
{
    /// <summary>
    /// Gets the value of the Maybe or throws an InvalidOperationException if the Maybe is None.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<T> GetValueOrThrowAsync<T>(this Task<Maybe<T>> maybeTask, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.GetValueOrThrow();
    }

    /// <summary>
    /// Gets the value of the Maybe or throws an InvalidOperationException if the Maybe is None.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="errorMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<T> GetValueOrThrowAsync<T>(this Task<Maybe<T>> maybeTask, string errorMessage, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.GetValueOrThrow(errorMessage);
    }

    /// <summary>
    /// Gets the value of the Maybe or throws an InvalidOperationException if the Maybe is None.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<T> GetValueOrThrowAsync<T>(this ValueTask<Maybe<T>> maybeTask, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.GetValueOrThrow();
    }

    /// <summary>
    /// Gets the value of the Maybe or throws an InvalidOperationException if the Maybe is None.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="errorMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<T> GetValueOrThrowAsync<T>(this ValueTask<Maybe<T>> maybeTask, string errorMessage, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.GetValueOrThrow(errorMessage);
    }
}
