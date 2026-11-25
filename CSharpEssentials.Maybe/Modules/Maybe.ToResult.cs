using CSharpEssentials.Core;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Maybe;

public static partial class MaybeExtensions
{
    /// <summary>
    /// Converts the Maybe to a Result. Alias for ToMaybeResult.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result<T> ToResult<T>(this Maybe<T> maybe, Error? error = null) =>
        maybe.ToMaybeResult(error);

    /// <summary>
    /// Converts the Maybe to a Result with <paramref name="error"/> as the error when it has no value. It keeps an
    /// <see cref="Error"/> argument on this classic overload: without it the typed <c>ToResult&lt;T, TError&gt;</c> would bind,
    /// because <see cref="Error"/> is a struct and reaches <c>Error?</c> only through a nullable conversion. A <see langword="default"/>
    /// error counts as no error, as it did when <c>ToResult(default)</c> bound the <c>Error?</c> overload.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="maybe">The Maybe.</param>
    /// <param name="error">The error to return when the Maybe has no value.</param>
    /// <returns>A result that holds the value or <paramref name="error"/>.</returns>
    public static Result<T> ToResult<T>(this Maybe<T> maybe, Error error) =>
        maybe.ToMaybeResult(error == default ? null : error);

    /// <summary>
    /// Converts the Maybe to a unit Result. Alias for ToMaybeUnitResult.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybe"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result ToUnitResult<T>(this Maybe<T> maybe, Error? error = null) =>
        maybe.ToMaybeUnitResult(error);

    /// <summary>
    /// Converts the Maybe to a Result asynchronously.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result<T>> ToResultAsync<T>(this Task<Maybe<T>> maybeTask, Error? error = null, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken).ConfigureAwait(false);
        return maybe.ToResult(error);
    }

    /// <summary>
    /// Converts the Maybe to a Result asynchronously.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result<T>> ToResultAsync<T>(this ValueTask<Maybe<T>> maybeTask, Error? error = null, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken).ConfigureAwait(false);
        return maybe.ToResult(error);
    }

    /// <summary>
    /// Converts the Maybe to a unit Result asynchronously.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<Result> ToUnitResultAsync<T>(this Task<Maybe<T>> maybeTask, Error? error = null, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken).ConfigureAwait(false);
        return maybe.ToUnitResult(error);
    }

    /// <summary>
    /// Converts the Maybe to a unit Result asynchronously.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="maybeTask"></param>
    /// <param name="error"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<Result> ToUnitResultAsync<T>(this ValueTask<Maybe<T>> maybeTask, Error? error = null, CancellationToken cancellationToken = default)
    {
        Maybe<T> maybe = await maybeTask.WithCancellation(cancellationToken).ConfigureAwait(false);
        return maybe.ToUnitResult(error);
    }
}
