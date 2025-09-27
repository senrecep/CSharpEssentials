namespace CSharpEssentials.Results;

public readonly partial record struct Result<TValue>
{
    /// <summary>
    /// Gets the value of the result.
    /// </summary>
    /// <returns></returns>
    public TValue GetValueOrDefault() => IsSuccess ? Value : default!;

    /// <summary>
    /// Get the value or a default value if the result is a failure. 
    /// </summary>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public TValue GetValueOrDefault(TValue defaultValue) => IsSuccess ? Value : defaultValue;

    /// <summary>
    /// Get the value or throw an exception if the result is a failure.
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public TValue GetValueOrThrow(string? errorMessage = null) => IsSuccess ? Value : throw new InvalidOperationException(errorMessage ?? "Result has no value.");
    /// <summary>
    /// Get the value or throw an exception if the result is a failure.
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public TValue GetValueOrThrow(Exception exception) => IsSuccess ? Value : throw exception;

}
public static partial class ResultExtensions
{

}