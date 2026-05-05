using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Recovers from a specific error type by applying a recovery function.
    /// </summary>
    /// <param name="errorType">The error type to recover from.</param>
    /// <param name="recovery">The function to apply when an error of the specified type is found.</param>
    /// <returns>A new result with the recovered value if the error matched; otherwise, the original failure result.</returns>
    public Result<TValue> Recover(ErrorType errorType, Func<Error, Result<TValue>> recovery)
    {
        if (IsSuccess)
        {
            return this;
        }

        foreach (Error error in ErrorsOrEmptyArray)
        {
            if (error.Type == errorType)
            {
                return recovery(error);
            }
        }

        return this;
    }

    /// <summary>
    /// Recovers from a specific error type by applying a recovery function that produces a value.
    /// </summary>
    /// <param name="errorType">The error type to recover from.</param>
    /// <param name="recovery">The function to apply when an error of the specified type is found.</param>
    /// <returns>A new success result with the recovered value if the error matched; otherwise, the original failure result.</returns>
    public Result<TValue> Recover(ErrorType errorType, Func<Error, TValue> recovery)
    {
        if (IsSuccess)
        {
            return this;
        }

        foreach (Error error in ErrorsOrEmptyArray)
        {
            if (error.Type == errorType)
            {
                return recovery(error);
            }
        }

        return this;
    }

    /// <summary>
    /// Recovers from the first error if it matches the specified type.
    /// </summary>
    /// <param name="errorType">The error type to recover from.</param>
    /// <param name="recovery">The function to apply when the first error is of the specified type.</param>
    /// <returns>A new result with the recovered value if the first error matched; otherwise, the original failure result.</returns>
    public Result<TValue> RecoverFirst(ErrorType errorType, Func<Error, Result<TValue>> recovery)
    {
        if (IsSuccess)
        {
            return this;
        }

        if (FirstError.Type == errorType)
        {
            return recovery(FirstError);
        }

        return this;
    }

    /// <summary>
    /// Recovers from the first error if it matches the specified type by producing a value.
    /// </summary>
    /// <param name="errorType">The error type to recover from.</param>
    /// <param name="recovery">The function to apply when the first error is of the specified type.</param>
    /// <returns>A new success result with the recovered value if the first error matched; otherwise, the original failure result.</returns>
    public Result<TValue> RecoverFirst(ErrorType errorType, Func<Error, TValue> recovery)
    {
        if (IsSuccess)
        {
            return this;
        }

        if (FirstError.Type == errorType)
        {
            return recovery(FirstError);
        }

        return this;
    }

    /// <summary>
    /// Recovers from errors matching the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate to match errors against.</param>
    /// <param name="recovery">The function to apply when a matching error is found.</param>
    /// <returns>A new result with the recovered value if a matching error was found; otherwise, the original failure result.</returns>
    public Result<TValue> Recover(Func<Error, bool> predicate, Func<Error, Result<TValue>> recovery)
    {
        if (IsSuccess)
        {
            return this;
        }

        foreach (Error error in ErrorsOrEmptyArray)
        {
            if (predicate(error))
            {
                return recovery(error);
            }
        }

        return this;
    }

    /// <summary>
    /// Recovers from errors matching the specified predicate by producing a value.
    /// </summary>
    /// <param name="predicate">The predicate to match errors against.</param>
    /// <param name="recovery">The function to apply when a matching error is found.</param>
    /// <returns>A new success result with the recovered value if a matching error was found; otherwise, the original failure result.</returns>
    public Result<TValue> Recover(Func<Error, bool> predicate, Func<Error, TValue> recovery)
    {
        if (IsSuccess)
        {
            return this;
        }

        foreach (Error error in ErrorsOrEmptyArray)
        {
            if (predicate(error))
            {
                return recovery(error);
            }
        }

        return this;
    }
}
