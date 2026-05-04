using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public static partial class ResultExtensions
{
    /// <summary>
    /// Converts an error to a result.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result ToResult(this Error error) => error;
    /// <summary>
    /// Converts a collection of errors to a result.
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static Result ToResult(this IEnumerable<Error> errors) => errors.ToArray();

    /// <summary>
    /// Converts a value to a result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<TValue> ToResult<TValue>(this TValue value) => value;

    /// <summary>
    /// Converts an error to a result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result<TValue> ToResult<TValue>(this Error error) => error;

    /// <summary>
    /// Converts a collection of errors to a result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static Result<TValue> ToResult<TValue>(this IEnumerable<Error> errors) => errors.ToArray();
}
