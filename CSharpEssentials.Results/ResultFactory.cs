using CSharpEssentials.Errors;
using CSharpEssentials.Results.Interfaces;

namespace CSharpEssentials.Results;

public readonly partial record struct Result
{
    /// <summary>
    /// Creates a new successful result.
    /// </summary>
    /// <returns></returns>
    public static Result Success() => default;

    /// <summary>
    /// Creates a new failure result.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result Failure(Error error) => From(error);

    /// <summary>
    /// Creates a new failure result.
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static Result Failure(params IEnumerable<Error> errors) => From(errors);

    /// <summary>
    /// Converts a collection of errors to a result.
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static Result From(params IEnumerable<Error> errors) => new(errors);

    /// <summary>
    /// Combines multiple results into a single result.
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    public static Result And(params IEnumerable<Result> results) => And(results.Cast<IResultBase>());

    /// <summary>
    /// Combines multiple results into a single result.
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    public static Result And(params IEnumerable<IResultBase> results)
    {
        foreach (IResultBase result in results)
        {
            if (result.IsFailure)
                return result.Errors;
        }
        return Success();
    }

    /// <summary>
    /// Combines multiple results into a single result.
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    public static Result Or(params IEnumerable<Result> results) => Or(results.Cast<IResultBase>());

    /// <summary>
    /// Combines multiple results into a single result.
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    public static Result Or(params IEnumerable<IResultBase> results)
    {
        List<Error> errors = [];
        foreach (IResultBase result in results)
        {
            if (result.IsSuccess)
                return Success();
            errors.AddRange(result.ErrorsOrEmptyArray);
        }

        return errors;
    }


    /// <summary>
    /// Creates a new successful result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);
    /// <summary>
    /// Creates a new failure result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.From(error);
    /// <summary>
    /// Creates a new failure result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static Result<TValue> Failure<TValue>(params IEnumerable<Error> errors) => Result<TValue>.From(errors);
    /// <summary>
    /// Converts an error to a result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result<TValue> From<TValue>(Error error) => Result<TValue>.From(error);
    /// <summary>
    /// Converts a collection of errors to a result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static Result<TValue> From<TValue>(params IEnumerable<Error> errors) => Result<TValue>.From(errors);


    public static implicit operator Result(Error error) => new([error]);
    public static implicit operator Result(Error[] errors) => new(errors);
    public static implicit operator Result(List<Error> errors) => new(errors);
    public static implicit operator Result(HashSet<Error> errors) => new(errors);
    public static implicit operator Result(bool isSuccess) => isSuccess ? Success() : Error.False;
}
