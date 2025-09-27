using CSharpEssentials.Core;
using CSharpEssentials.Errors;
using CSharpEssentials.Results;

namespace CSharpEssentials.Maybe;

public static class MaybeExtensions
{
    /// <summary>
    /// Converts the Maybe to a Result.
    /// </summary>
    /// <param name="maybe"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result<T> ToResult<T>(this Maybe<T> maybe, Error error)
    {
        if (maybe.HasNoValue)
            return error;

        return maybe.Value;
    }
    /// <summary>
    /// Converts the Maybe to a Result.
    /// </summary>
    /// <param name="maybe"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result ToUnitResult<T>(this Maybe<T> maybe, Error error)
    {
        if (maybe.HasNoValue)
            return error;

        return Result.Success();
    }


    /// <summary>
    ///  Converts the Maybe to a Result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<T>> ToResult<T>(this Task<Maybe<T>> maybeTask, Error error, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.ToResult(error);
    }

    /// <summary>
    /// Converts the Maybe to a Result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<T>> ToResult<T>(this ValueTask<Maybe<T>> maybeTask, Error error, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.ToResult(error);
    }

    /// <summary>
    /// Converts the Maybe to a Result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> ToUnitResult<T>(this Task<Maybe<T>> maybeTask, Error error, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.ToUnitResult(error);
    }

    /// <summary>
    /// Converts the Maybe to a Result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> ToUnitResult<T>(this ValueTask<Maybe<T>> maybeTask, Error error, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.ToUnitResult(error);
    }
}
