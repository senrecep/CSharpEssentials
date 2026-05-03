using CSharpEssentials.Core;
namespace CSharpEssentials.ResultPattern;

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
    /// Get the value or a default value if the result is a failure.
    /// </summary>
    /// <param name="defaultValueFactory"></param>
    /// <returns></returns>
    public TValue GetValueOrDefault(Func<TValue> defaultValueFactory) => IsSuccess ? Value : defaultValueFactory();

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
    /// <summary>
    /// Gets the value of the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<TValue> GetValueOrDefaultAsync<TValue>(this Task<Result<TValue>> task, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.GetValueOrDefault();
    }

    /// <summary>
    /// Get the value or a default value if the result is a failure.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="defaultValue"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<TValue> GetValueOrDefaultAsync<TValue>(this Task<Result<TValue>> task, TValue defaultValue, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.GetValueOrDefault(defaultValue);
    }

    /// <summary>
    /// Get the value or a default value if the result is a failure.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="defaultValueFactory"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<TValue> GetValueOrDefaultAsync<TValue>(this Task<Result<TValue>> task, Func<TValue> defaultValueFactory, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.GetValueOrDefault(defaultValueFactory);
    }

    /// <summary>
    /// Gets the value of the result.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<TValue> GetValueOrDefaultAsync<TValue>(this ValueTask<Result<TValue>> task, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.GetValueOrDefault();
    }

    /// <summary>
    /// Get the value or a default value if the result is a failure.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="defaultValue"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<TValue> GetValueOrDefaultAsync<TValue>(this ValueTask<Result<TValue>> task, TValue defaultValue, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.GetValueOrDefault(defaultValue);
    }

    /// <summary>
    /// Get the value or a default value if the result is a failure.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="task"></param>
    /// <param name="defaultValueFactory"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<TValue> GetValueOrDefaultAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue> defaultValueFactory, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.GetValueOrDefault(defaultValueFactory);
    }
}
