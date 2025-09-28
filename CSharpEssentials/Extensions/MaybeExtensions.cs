using CSharpEssentials.Core;
using CSharpEssentials.Errors;
using CSharpEssentials.Results;

namespace CSharpEssentials.Maybe;

public static class MaybeExtensions
{
    private static readonly Error _defaultError = Error.NotFound(code: "Maybe.Result", description: "The 'Maybe' has no value.");
    /// <summary>
    /// Converts the Maybe to a Result.
    /// </summary>
    /// <param name="maybe"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result<T> ToMaybeResult<T>(this Maybe<T> maybe, Error? error = null)
    {
        if (maybe.HasNoValue)
            return error ?? _defaultError;

        return maybe.Value;
    }
    /// <summary>
    /// Converts the Maybe to a Result.
    /// </summary>
    /// <param name="maybe"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result ToMaybeUnitResult<T>(this Maybe<T> maybe, Error? error = null)
    {
        if (maybe.HasNoValue)
            return error ?? _defaultError;

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
    public static async Task<Result<T>> ToMaybeResult<T>(this Task<Maybe<T>> maybeTask, Error? error = null, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.ToMaybeResult(error);
    }

    /// <summary>
    /// Converts the Maybe to a Result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<T>> ToMaybeResult<T>(this ValueTask<Maybe<T>> maybeTask, Error? error = null, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.ToMaybeResult(error);
    }

    /// <summary>
    /// Converts the Maybe to a Result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> ToMaybeUnitResult<T>(this Task<Maybe<T>> maybeTask, Error? error = null, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.ToMaybeUnitResult(error);
    }

    /// <summary>
    /// Converts the Maybe to a Result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> ToMaybeUnitResult<T>(this ValueTask<Maybe<T>> maybeTask, Error? error = null, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken);
        return maybe.ToMaybeUnitResult(error);
    }
}
