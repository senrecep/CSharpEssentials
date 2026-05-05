namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Returns the success value if the result is successful; otherwise, throws a <see cref="ResultUnwrapException"/>.
    /// </summary>
    /// <returns>The success value.</returns>
    /// <exception cref="ResultUnwrapException">Thrown when the result is in a failure state.</exception>
    public TValue Unwrap()
    {
        if (IsSuccess)
        {
            return Value;
        }

        throw new ResultUnwrapException(ErrorsOrEmptyArray);
    }

    /// <summary>
    /// Returns the success value if the result is successful; otherwise, returns the specified default value.
    /// </summary>
    /// <param name="defaultValue">The value to return if the result is in a failure state.</param>
    /// <returns>The success value, or <paramref name="defaultValue"/> if the result is a failure.</returns>
    public TValue UnwrapOrDefault(TValue defaultValue)
    {
        return IsSuccess ? Value : defaultValue;
    }
}
