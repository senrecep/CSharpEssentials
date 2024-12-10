using CSharpEssentials.Interfaces;

namespace CSharpEssentials;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Creates a new successful result.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<TValue> Success(TValue value) => new(value);

    /// <summary>
    /// Creates a new failure result.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result<TValue> Failure(Error error) => From(error);

    /// <summary>
    /// Creates a new failure result.
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static Result<TValue> Failure(params IEnumerable<Error> errors) => From(errors);

    /// <summary>
    /// Converts a collection of errors to a result.
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static Result<TValue> From(params IEnumerable<Error> errors) => new(errors);


    /// <summary>
    /// Combines a collection of results into a single result.
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    public static Result<TValue[]> And(params IEnumerable<Result<TValue>> results) => And(results.Cast<IResult<TValue>>());

    /// <summary>
    /// Combines a collection of results into a single result.
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    public static Result<TValue[]> And(params IEnumerable<IResult<TValue>> results)
    {
        List<Error> errors = [];
        List<TValue> values = [];
        foreach (IResult<TValue> item in results)
            if (item.IsSuccess)
                values.Add(item.Value);
            else
                errors.AddRange(item.ErrorsOrEmptyArray);

        if (errors.Count > 0)
            return errors;
        return values.ToArray();
    }

    /// <summary>
    /// Combines a collection of results into a single result.
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    public static Result<TValue> Or(params IEnumerable<Result<TValue>> results) => Or(results.Cast<IResult<TValue>>());

    /// <summary>
    /// Combines a collection of results into a single result.
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    public static Result<TValue> Or(params IEnumerable<IResult<TValue>> results)
    {
        List<Error> errors = [];
        foreach (IResult<TValue> result in results)
        {
            if (result.IsSuccess)
                return Success(result.Value);
            errors.AddRange(result.ErrorsOrEmptyArray);
        }

        return errors;
    }


    public static implicit operator Result<TValue>(Error error) => new([error]);
    public static implicit operator Result<TValue>(Error[] errors) => new(errors);
    public static implicit operator Result<TValue>(List<Error> errors) => new(errors);
    public static implicit operator Result<TValue>(HashSet<Error> errors) => new(errors);

    public static implicit operator Result<TValue>(TValue value) => new(value);
}
