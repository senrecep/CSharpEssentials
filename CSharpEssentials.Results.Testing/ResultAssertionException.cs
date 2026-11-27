namespace CSharpEssentials.ResultPattern.Testing;

/// <summary>Thrown by the assertion helpers of this package when a result, maybe or HTTP response is not what the test expected.</summary>
public sealed class ResultAssertionException : Exception
{
    /// <summary>Creates the exception.</summary>
    public ResultAssertionException()
    {
    }

    /// <summary>Creates the exception with a message.</summary>
    /// <param name="message">What was expected and what the value held.</param>
    public ResultAssertionException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a message and the exception that caused it.</summary>
    /// <param name="message">What was expected and what the value held.</param>
    /// <param name="innerException">The exception that caused the assertion to fail.</param>
    public ResultAssertionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
