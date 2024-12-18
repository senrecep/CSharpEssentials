using CSharpEssentials.Comparers;

namespace CSharpEssentials.Interfaces;
/// <summary>
/// Represents a result that can either be a success or a failure, with various methods to handle both cases.
/// </summary>
/// <typeparam name="TValue">The type of the value contained in the result.</typeparam>
public interface IResult<TValue> : IResultBase
{
    /// <summary>
    /// A static readonly instance of <see cref="IEqualityComparer{T}"/> for comparing instances of <see cref="IResult{TValue}"/>.
    /// </summary>
    public static new readonly IEqualityComparer<IResult<TValue>> Comparer = new ResultComparer<TValue>();
    /// <summary>
    /// Gets the value of the result.
    /// </summary>
    TValue Value { get; }

    /// <summary>
    /// Converts the result to a non-generic result.
    /// </summary>
    /// <returns>A non-generic result.</returns>
    Result ToResult();

    /// <summary>
    /// Handles failure by providing a single error.
    /// </summary>
    /// <param name="onFailure">A function that takes a list of errors and returns a single error.</param>
    /// <returns>A result containing the value or the provided error.</returns>
    Result<TValue> Else(Func<Error[], Error> onFailure);

    /// <summary>
    /// Handles failure by providing a list of errors.
    /// </summary>
    /// <param name="onFailure">A function that takes a list of errors and returns a list of errors.</param>
    /// <returns>A result containing the value or the provided list of errors.</returns>
    Result<TValue> Else(Func<Error[], Error[]> onFailure);

    /// <summary>
    /// Handles failure by providing a specific error.
    /// </summary>
    /// <param name="error">The error to be provided in case of failure.</param>
    /// <returns>A result containing the value or the provided error.</returns>
    Result<TValue> Else(Error error);

    /// <summary>
    /// Handles failure by providing a value.
    /// </summary>
    /// <param name="onFailure">A function that takes a list of errors and returns a value.</param>
    /// <returns>A result containing the value or the provided value.</returns>
    Result<TValue> Else(Func<Error[], TValue> onFailure);

    /// <summary>
    /// Handles failure by providing a specific value.
    /// </summary>
    /// <param name="onFailure">The value to be provided in case of failure.</param>
    /// <returns>A result containing the value or the provided value.</returns>
    Result<TValue> Else(TValue onFailure);

    /// <summary>
    /// Asynchronously handles failure by providing a value.
    /// </summary>
    /// <param name="onFailure">A function that takes a list of errors and returns a task of value.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result with the value or the provided value.</returns>
    Task<Result<TValue>> ElseAsync(Func<Error[], Task<TValue>> onFailure, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously handles failure by providing a single error.
    /// </summary>
    /// <param name="onFailure">A function that takes a list of errors and returns a task of error.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result with the value or the provided error.</returns>
    Task<Result<TValue>> ElseAsync(Func<Error[], Task<Error>> onFailure, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously handles failure by providing a list of errors.
    /// </summary>
    /// <param name="onFailure">A function that takes a list of errors and returns a task of list of errors.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result with the value or the provided list of errors.</returns>
    Task<Result<TValue>> ElseAsync(Func<Error[], Task<Error[]>> onFailure, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously handles failure by providing a specific error.
    /// </summary>
    /// <param name="error">A task of error to be provided in case of failure.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result with the value or the provided error.</returns>
    Task<Result<TValue>> ElseAsync(Task<Error> error, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously handles failure by providing a specific value.
    /// </summary>
    /// <param name="onFailure">A task of value to be provided in case of failure.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result with the value or the provided value.</returns>
    Task<Result<TValue>> ElseAsync(Task<TValue> onFailure, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fails if the specified condition is met.
    /// </summary>
    /// <param name="onSuccess">A function that takes the value and returns a boolean indicating if the condition is met.</param>
    /// <param name="error">The error to be provided if the condition is met.</param>
    /// <returns>A result containing the value or the provided error if the condition is met.</returns>
    Result<TValue> FailIf(Func<TValue, bool> onSuccess, Error error);

    /// <summary>
    /// Fails if the specified condition is met, using a function to generate the error.
    /// </summary>
    /// <param name="onSuccess">A function that takes the value and returns a boolean indicating if the condition is met.</param>
    /// <param name="func">A function that takes the value and returns the error to be provided if the condition is met.</param>
    /// <returns>A result containing the value or the provided error if the condition is met.</returns>
    Result<TValue> FailIf(Func<TValue, bool> onSuccess, Func<TValue, Error> func);

    /// <summary>
    /// Asynchronously fails if the specified condition is met.
    /// </summary>
    /// <param name="onSuccess">A function that takes the value and returns a task of boolean indicating if the condition is met.</param>
    /// <param name="error">The error to be provided if the condition is met.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result with the value or the provided error if the condition is met.</returns>
    Task<Result<TValue>> FailIfAsync(Func<TValue, Task<bool>> onSuccess, Error error, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously fails if the specified condition is met, using a function to generate the error.
    /// </summary>
    /// <param name="onSuccess">A function that takes the value and returns a task of boolean indicating if the condition is met.</param>
    /// <param name="func">A function that takes the value and returns a task of error to be provided if the condition is met.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result with the value or the provided error if the condition is met.</returns>
    Task<Result<TValue>> FailIfAsync(Func<TValue, Task<bool>> onSuccess, Func<TValue, Task<Error>> func, CancellationToken cancellationToken = default);

    /// <summary>
    /// Matches the result to a value based on success or failure.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="onSuccess">A function that takes the value and returns a result value.</param>
    /// <param name="onError">A function that takes a list of errors and returns a result value.</param>
    /// <returns>The result value based on success or failure.</returns>
    T Match<T>(Func<TValue, T> onSuccess, Func<Error[], T> onError);

    /// <summary>
    /// Asynchronously matches the result to a value based on success or failure.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="onSuccess">A function that takes the value and returns a task of result value.</param>
    /// <param name="onError">A function that takes a list of errors and returns a task of result value.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result value based on success or failure.</returns>
    Task<T> MatchAsync<T>(Func<TValue, Task<T>> onSuccess, Func<Error[], Task<T>> onError, CancellationToken cancellationToken = default);

    /// <summary>
    /// Matches the result to a value based on success or the first error.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="onSuccess">A function that takes the value and returns a result value.</param>
    /// <param name="onFirstError">A function that takes the first error and returns a result value.</param>
    T MatchFirst<T>(Func<TValue, T> onSuccess, Func<Error, T> onFirstError);

    /// <summary>
    /// Asynchronously matches the result to a value based on success or the first error.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="onSuccess">A function that takes the value and returns a task of result value.</param>
    /// <param name="onFirstError">A function that takes the first error and returns a task of result value.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result value based on success or the first error.</returns>
    Task<T> MatchFirstAsync<T>(Func<TValue, Task<T>> onSuccess, Func<Error, Task<T>> onFirstError, CancellationToken cancellationToken = default);

    /// <summary>
    /// Matches the result to a value based on success or the last error.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="onSuccess">A function that takes the value and returns a result value.</param>
    /// <param name="onLastError">A function that takes the last error and returns a result value.</param>
    /// <returns>The result value based on success or the last error.</returns>
    T MatchLast<T>(Func<TValue, T> onSuccess, Func<Error, T> onLastError);

    /// <summary>
    /// Asynchronously matches the result to a value based on success or the last error.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="onSuccess">A function that takes the value and returns a task of result value.</param>
    /// <param name="onLastError">A function that takes the last error and returns a task of result value.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the result value based on success or the last error.</returns>
    Task<T> MatchLastAsync<T>(Func<TValue, Task<T>> onSuccess, Func<Error, Task<T>> onLastError, CancellationToken cancellationToken = default);

    /// <summary>
    /// Switches between success and failure actions.
    /// </summary>
    /// <param name="onSuccess">An action to be executed on success.</param>
    /// <param name="onError">An action to be executed on failure.</param>
    void Switch(Action<TValue> onSuccess, Action<Error[]> onError);

    /// <summary>
    /// Asynchronously switches between success and failure actions.
    /// </summary>
    /// <param name="onSuccess">A function that returns a task to be executed on success.</param>
    /// <param name="onError">A function that returns a task to be executed on failure.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SwitchAsync(Func<TValue, Task> onSuccess, Func<Error[], Task> onError, CancellationToken cancellationToken = default);

    /// <summary>
    /// Switches between success and the first error actions.
    /// </summary>
    /// <param name="onSuccess">An action to be executed on success.</param>
    /// <param name="onFirstError">An action to be executed on the first error.</param>
    void SwitchFirst(Action<TValue> onSuccess, Action<Error> onFirstError);

    /// <summary>
    /// Asynchronously switches between success and the first error actions.
    /// </summary>
    /// <param name="onSuccess">A function that returns a task to be executed on success.</param>
    /// <param name="onFirstError">A function that returns a task to be executed on the first error.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SwitchFirstAsync(Func<TValue, Task> onSuccess, Func<Error, Task> onFirstError, CancellationToken cancellationToken = default);

    /// <summary>
    /// Switches between success and the last error actions.
    /// </summary>
    /// <param name="onSuccess">An action to be executed on success.</param>
    /// <param name="onLastError">An action to be executed on the last error.</param>
    void SwitchLast(Action<TValue> onSuccess, Action<Error> onLastError);

    /// <summary>
    /// Asynchronously switches between success and the last error actions.
    /// </summary>
    /// <param name="onSuccess">A function that returns a task to be executed on success.</param>
    /// <param name="onLastError">A function that returns a task to be executed on the last error.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SwitchLastAsync(Func<TValue, Task> onSuccess, Func<Error, Task> onLastError, CancellationToken cancellationToken = default);

    /// <summary>
    /// Chains another result based on the success value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="onSuccess">A function that takes the value and returns a result.</param>
    /// <returns>A result containing the chained result.</returns>
    Result<T> Then<T>(Func<TValue, Result<T>> onSuccess);

    /// <summary>
    /// Executes an action based on the success value.
    /// </summary>
    /// <param name="action">An action to be executed on success.</param>
    /// <returns>A result containing the original value.</returns>
    Result<TValue> ThenDo(Action<TValue> action);

    /// <summary>
    /// Chains another result based on the success value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="onSuccess">A function that takes the value and returns a result value.</param>
    /// <returns>A result containing the chained result.</returns>
    Result<T> Then<T>(Func<TValue, T> onSuccess);

    /// <summary>
    /// Asynchronously chains another result based on the success value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="onSuccess">A function that takes the value and returns a task of result.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the chained result.</returns>
    Task<Result<T>> ThenAsync<T>(Func<TValue, Task<Result<T>>> onSuccess, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously executes an action based on the success value.
    /// </summary>
    /// <param name="action">A function that returns a task to be executed on success.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the original value.</returns>
    Task<Result<TValue>> ThenDoAsync(Func<TValue, Task> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously chains another result based on the success value.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="onSuccess">A function that takes the value and returns a task of result value.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous operation, containing the chained result.</returns>
    Task<Result<T>> ThenAsync<T>(Func<TValue, Task<T>> onSuccess, CancellationToken cancellationToken = default);

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    Result<TOut> Bind<TOut>(Func<TValue, Result<TOut>> func);

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    Result Bind(Func<TValue, Result> func);

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    Task<Result<TOut>> Bind<TOut>(Func<TValue, Task<Result<TOut>>> func);

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    Task<Result> Bind(Func<TValue, Task<Result>> func);

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="valueTask"></param>
    /// <returns></returns>
    ValueTask<Result<TOut>> Bind<TOut>(Func<TValue, ValueTask<Result<TOut>>> valueTask);

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="valueTask"></param>
    /// <returns></returns>
    ValueTask<Result> Bind(Func<TValue, ValueTask<Result>> valueTask);
}