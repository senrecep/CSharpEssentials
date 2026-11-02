using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

/// <summary>
/// Represents an exception thrown when attempting to unwrap a <see cref="Result{TValue}"/> or <see cref="Result"/> that is in a failure state.
/// </summary>
public sealed class ResultUnwrapException : InvalidOperationException
{
    /// <summary>
    /// Gets the errors associated with the failed result, if any.
    /// </summary>
    public Error[] Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultUnwrapException"/> class.
    /// </summary>
    /// <param name="errors">The errors that caused the unwrap to fail.</param>
    public ResultUnwrapException(Error[] errors)
        : base(BuildMessage(errors)) => Errors = errors;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultUnwrapException"/> class with a specified message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="errors">The errors that caused the unwrap to fail.</param>
    public ResultUnwrapException(string message, Error[] errors)
        : base(message) => Errors = errors;

    private static string BuildMessage(Error[] errors)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(errors);
#else
        if (errors is null)
            throw new ArgumentNullException(nameof(errors));
#endif
        return $"Cannot unwrap a failed result. Errors: {string.Join(", ", errors.Select(e => e.Code))}";
    }
}
