namespace CSharpEssentials;

public static partial class ResultExtensions
{
    /// <summary>
    /// Converts an error to a result.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result ToResult(this Error error) => Result.Failure(error);
    /// <summary>
    /// Converts a collection of errors to a result.
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static Result ToResult(this IEnumerable<Error> errors) => Result.Failure(errors);

    /// <summary>
    /// Converts a value to a result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<TValue> ToResult<TValue>(this TValue value) => Result<TValue>.Success(value);

    /// <summary>
    /// Converts an error to a result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result<TValue> ToResult<TValue>(this Error error) => Result<TValue>.Failure(error);

    /// <summary>
    /// Converts a collection of errors to a result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static Result<TValue> ToResult<TValue>(this IEnumerable<Error> errors) => Result<TValue>.Failure(errors);
}
