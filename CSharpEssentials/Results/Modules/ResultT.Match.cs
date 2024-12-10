namespace CSharpEssentials;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Match the result with the provided functions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <returns></returns>
    public T Match<T>(Func<TValue, T> onSuccess, Func<IReadOnlyList<Error>, T> onError)
    {
        if (IsFailure)
            return onError(Errors);
        return onSuccess(Value);
    }

    /// <summary>
    /// Match the result with the provided functions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<T> MatchAsync<T>(Func<TValue, Task<T>> onSuccess, Func<IReadOnlyList<Error>, Task<T>> onError, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return await onError(Errors).WithCancellation(cancellationToken);
        return await onSuccess(Value).WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Match the result with the provided functions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="onSuccess"></param>
    /// <param name="onFirstError"></param>
    /// <returns></returns>
    public T MatchFirst<T>(Func<TValue, T> onSuccess, Func<Error, T> onFirstError)
    {
        if (IsFailure)
            return onFirstError(FirstError);
        return onSuccess(Value);
    }


    /// <summary>
    /// Match the result with the provided functions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="onSuccess"></param>
    /// <param name="onFirstError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<T> MatchFirstAsync<T>(Func<TValue, Task<T>> onSuccess, Func<Error, Task<T>> onFirstError, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return await onFirstError(FirstError).WithCancellation(cancellationToken);
        return await onSuccess(Value).WithCancellation(cancellationToken);
    }

    /// <summary>
    /// Match the result with the provided functions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="onSuccess"></param>
    /// <param name="onLastError"></param>
    /// <returns></returns>
    public T MatchLast<T>(Func<TValue, T> onSuccess, Func<Error, T> onLastError)
    {
        if (IsFailure)
            return onLastError(LastError);
        return onSuccess(Value);
    }

    /// <summary>
    /// Match the result with the provided functions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="onSuccess"></param>
    /// <param name="onLastError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<T> MatchLastAsync<T>(Func<TValue, Task<T>> onSuccess, Func<Error, Task<T>> onLastError, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return await onLastError(LastError).WithCancellation(cancellationToken);
        return await onSuccess(Value).WithCancellation(cancellationToken);
    }
}

public static partial class ResultExtensions
{
    /// <summary>
    /// Match the result with the provided functions.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<T> Match<TValue, T>(this Task<Result<TValue>> task, Func<TValue, T> onSuccess, Func<IReadOnlyList<Error>, T> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.Match(onSuccess, onError);
    }

    /// <summary>
    /// Match the result with the provided functions.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<T> MatchAsync<TValue, T>(this Task<Result<TValue>> task, Func<TValue, Task<T>> onSuccess, Func<IReadOnlyList<Error>, Task<T>> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.MatchAsync(onSuccess, onError, cancellationToken);
    }

    /// <summary>
    /// Match the result with the provided functions.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<T> MatchFirst<TValue, T>(this Task<Result<TValue>> task, Func<TValue, T> onSuccess, Func<Error, T> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.MatchFirst(onSuccess, onError);
    }

    /// <summary>
    /// Match the result with the provided functions.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="task"></param>
    /// <param name="onSuccess"></param>
    /// <param name="onError"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<T> MatchFirstAsync<TValue, T>(this Task<Result<TValue>> task, Func<TValue, Task<T>> onSuccess, Func<Error, Task<T>> onError, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return await result.MatchFirstAsync(onSuccess, onError, cancellationToken);
    }
}