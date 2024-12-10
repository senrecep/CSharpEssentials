
namespace CSharpEssentials;

public readonly partial struct Maybe<T>
{
    /// <summary>
    /// Converts the Maybe to a Result.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public Result<T> ToResult(Error error)
    {
        if (HasNoValue)
            return error;

        return Value;
    }

    /// <summary>
    /// Converts the Maybe to a Result.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public Result ToUnitResult(Error error)
    {
        if (HasNoValue)
            return error;

        return Result.Success();
    }

}

public static partial class MaybeExtensions
{
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
