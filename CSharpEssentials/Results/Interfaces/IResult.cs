
using CSharpEssentials.Comparers;

namespace CSharpEssentials.Interfaces;


/// <summary>
/// Represents a result that can be either successful or a failure with associated errors.
/// Provides methods for handling success, failure, and chaining operations.
/// </summary>
public interface IResult : IResultBase
{
    /// <summary>
    /// A static comparer for comparing two <see cref="IResult"/> instances.
    /// </summary>
    public static new readonly IEqualityComparer<IResult> Comparer = new ResultComparer();

    /// <summary>
    /// Returns a new result if the current result is a failure, using the provided function to handle errors.
    /// </summary>
    /// <param name="onFailure">A function to handle failure, providing an array of errors.</param>
    /// <returns>A new result.</returns>
    Result Else(Func<Error[], Error> onFailure);

    /// <summary>
    /// Returns a new result if the current result is a failure, using the provided function to handle errors.
    /// </summary>
    /// <param name="onFailure">A function to handle failure, providing an array of errors.</param>
    /// <returns>A new result.</returns>
    Result Else(Func<Error[], IEnumerable<Error>> onFailure);

    /// <summary>
    /// Returns a new result if the current result is a failure, using the provided error value.
    /// </summary>
    /// <param name="error">An error to return as a result.</param>
    /// <returns>A new result.</returns>
    Result Else(Error error);

    /// <summary>
    /// Asynchronously returns a new result if the current result is a failure, using the provided async function to handle errors.
    /// </summary>
    /// <param name="onFailure">An async function to handle failure, providing an array of errors.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation that returns a new result.</returns>
    Task<Result> ElseAsync(Func<Error[], Task<Error>> onFailure, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously returns a new result if the current result is a failure, using the provided async function to handle errors.
    /// </summary>
    /// <param name="onFailure">An async function to handle failure, providing an array of errors.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation that returns a new result.</returns>
    Task<Result> ElseAsync(Func<Error[], Task<IEnumerable<Error>>> onFailure, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously returns a new result if the current result is a failure, using the provided async error task.
    /// </summary>
    /// <param name="error">An async error task to return as a result.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation that returns a new result.</returns>
    Task<Result> ElseAsync(Task<Error> error, CancellationToken cancellationToken = default);

    /// <summary>
    /// Matches the result by executing the corresponding function for success or failure.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="onSuccess">A function to execute on success.</param>
    /// <param name="onFailure">A function to execute on failure, providing an array of errors.</param>
    /// <returns>The result of the matching function.</returns>
    T Match<T>(Func<T> onSuccess, Func<Error[], T> onFailure);

    /// <summary>
    /// Asynchronously matches the result by executing the corresponding async function for success or failure.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="onSuccess">An async function to execute on success.</param>
    /// <param name="onFailure">An async function to execute on failure, providing an array of errors.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation that returns the result of the matching function.</returns>
    Task<T> MatchAsync<T>(Func<Task<T>> onSuccess, Func<Error[], Task<T>> onFailure, CancellationToken cancellationToken = default);

    /// <summary>
    /// Matches the result by executing the corresponding function for success or the first encountered error.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="onSuccess">A function to execute on success.</param>
    /// <param name="onFirstError">A function to execute on the first error encountered.</param>
    /// <returns>The result of the matching function.</returns>
    T MatchFirst<T>(Func<T> onSuccess, Func<Error, T> onFirstError);

    /// <summary>
    /// Matches the result by executing the corresponding function for success or the last encountered error.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="onSuccess">A function to execute on success.</param>
    /// <param name="onLastError">A function to execute on the last error encountered.</param>
    /// <returns>The result of the matching function.</returns>
    T MatchLast<T>(Func<T> onSuccess, Func<Error, T> onLastError);

    /// <summary>
    /// Asynchronously matches the result by executing the corresponding async function for success or the first encountered error.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="onSuccess">An async function to execute on success.</param>
    /// <param name="onFirstError">An async function to execute on the first error encountered.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation that returns the result of the matching function.</returns>
    Task<T> MatchFirstAsync<T>(Func<Task<T>> onSuccess, Func<Error, Task<T>> onFirstError, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously matches the result by executing the corresponding async function for success or the last encountered error.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="onSuccess">An async function to execute on success.</param>
    /// <param name="onLastError">An async function to execute on the last error encountered.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation that returns the result of the matching function.</returns>
    Task<T> MatchLastAsync<T>(Func<Task<T>> onSuccess, Func<Error, Task<T>> onLastError, CancellationToken cancellationToken = default);

    /// <summary>
    /// Switches on the result, executing a corresponding action for success or failure.
    /// </summary>
    /// <param name="onSuccess">An action to execute on success.</param>
    /// <param name="onFailure">An action to execute on failure, providing an array of errors.</param>
    void Switch(Action onSuccess, Action<Error[]> onFailure);

    /// <summary>
    /// Asynchronously switches on the result, executing a corresponding async action for success or failure.
    /// </summary>
    /// <param name="onSuccess">An async action to execute on success.</param>
    /// <param name="onFailure">An async action to execute on failure, providing an array of errors.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SwitchAsync(Func<Task> onSuccess, Func<Error[], Task> onFailure, CancellationToken cancellationToken = default);

    /// <summary>
    /// Switches on the result, executing a corresponding action for success or the first encountered error.
    /// </summary>
    /// <param name="onSuccess">An action to execute on success.</param>
    /// <param name="onFirstError">An action to execute on the first error encountered.</param>
    void SwitchFirst(Action onSuccess, Action<Error> onFirstError);

    /// <summary>
    /// Asynchronously switches on the result, executing a corresponding async action for success or the first encountered error.
    /// </summary>
    /// <param name="onSuccess">An async action to execute on success.</param>
    /// <param name="onFirstError">An async action to execute on the first error encountered.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SwitchFirstAsync(Func<Task> onSuccess, Func<Error, Task> onFirstError, CancellationToken cancellationToken = default);

    /// <summary>
    /// Switches on the result, executing a corresponding action for success or the last encountered error.
    /// </summary>
    /// <param name="onSuccess">An action to execute on success.</param>
    /// <param name="onLastError">An action to execute on the last error encountered.</param>
    void SwitchLast(Action onSuccess, Action<Error> onLastError);

    /// <summary>
    /// Asynchronously switches on the result, executing a corresponding async action for success or the last encountered error.
    /// </summary>
    /// <param name="onSuccess">An async action to execute on success.</param>
    /// <param name="onLastError">An async action to execute on the last error encountered.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SwitchLastAsync(Func<Task> onSuccess, Func<Error, Task> onLastError, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the provided function if the result is successful.
    /// </summary>
    /// <param name="onSuccess">A function to execute on success.</param>
    /// <returns>A new result.</returns>
    Result Then(Func<Result> onSuccess);

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    /// <param name="action">An action to execute on success.</param>
    Result ThenDo(Action action);

    /// <summary>
    /// Asynchronously executes the provided async function if the result is successful.
    /// </summary>
    /// <param name="onSuccess">An async function to execute on success.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation that returns a new result.</returns>
    Task<Result> ThenAsync(Func<Task<Result>> onSuccess, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously executes the provided async action if the result is successful.
    /// </summary>
    /// <param name="action">An async action to execute on success.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<Result> ThenDoAsync(Func<Task> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    Result<TOut> Bind<TOut>(Func<Result<TOut>> func);

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    Result Bind(Func<Result> func);

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    Task<Result<TOut>> Bind<TOut>(Func<Task<Result<TOut>>> func);

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="func"></param>
    /// <returns></returns>
    Task<Result> Bind(Func<Task<Result>> func);

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <typeparam name="TOut"></typeparam>
    /// <param name="valueTask"></param>
    /// <returns></returns>
    ValueTask<Result<TOut>> Bind<TOut>(Func<ValueTask<Result<TOut>>> valueTask);

    /// <summary>
    /// Binds a function to the result.
    /// </summary>
    /// <param name="valueTask"></param>
    /// <returns></returns>
    ValueTask<Result> Bind(Func<ValueTask<Result>> valueTask);
}