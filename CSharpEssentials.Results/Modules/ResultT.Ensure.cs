using System.Runtime.CompilerServices;
using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    public Result<TValue> Ensure(Func<TValue, bool> predicate, Error error)
    {
        if (IsFailure)
            return this;
        return predicate(Value) ? this : error;
    }

    public Result<TValue> Ensure(Func<TValue, bool> predicate, Func<TValue, Error> errorFactory)
    {
        if (IsFailure)
            return this;
        return predicate(Value) ? this : errorFactory(Value);
    }

    public Result<TValue> EnsureNotNull(Error error)
    {
        if (IsFailure)
            return this;
        return Value is not null ? this : error;
    }

    public Result<TValue> EnsureNotNull(Func<TValue, Error> errorFactory)
    {
        if (IsFailure)
            return this;
        return Value is not null ? this : errorFactory(Value);
    }

    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> EnsureAsync(Func<TValue, Task<bool>> predicate, Error error, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        return await predicate(Value).WithCancellation(cancellationToken).ConfigureAwait(false) ? this : error;
    }

    [OverloadResolutionPriority(1)]
    public async Task<Result<TValue>> EnsureAsync(Func<TValue, Task<bool>> predicate, Func<TValue, Error> errorFactory, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        return await predicate(Value).WithCancellation(cancellationToken).ConfigureAwait(false) ? this : errorFactory(Value);
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Fails with <paramref name="error"/> when the async predicate rejects the success value. On failure the predicate is never called.
    /// </summary>
    /// <param name="predicate">Checks the success value.</param>
    /// <param name="error">The error used when the predicate returns false.</param>
    /// <param name="cancellationToken">Observed while awaiting the predicate.</param>
    /// <returns>The original result, or a failure with <paramref name="error"/>.</returns>
    public async ValueTask<Result<TValue>> EnsureAsync(Func<TValue, ValueTask<bool>> predicate, Error error, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        return await predicate(Value).WithCancellation(cancellationToken).ConfigureAwait(false) ? this : error;
    }

    /// <summary>
    /// Fails with an error built from the success value when the async predicate rejects it. On failure the predicate is never called.
    /// </summary>
    /// <param name="predicate">Checks the success value.</param>
    /// <param name="errorFactory">Builds the error when the predicate returns false.</param>
    /// <param name="cancellationToken">Observed while awaiting the predicate.</param>
    /// <returns>The original result, or a failure with the built error.</returns>
    public async ValueTask<Result<TValue>> EnsureAsync(Func<TValue, ValueTask<bool>> predicate, Func<TValue, Error> errorFactory, CancellationToken cancellationToken = default)
    {
        if (IsFailure)
            return this;
        return await predicate(Value).WithCancellation(cancellationToken).ConfigureAwait(false) ? this : errorFactory(Value);
    }
#endif
}

public static partial class ResultExtensions
{
    public static async Task<Result<TValue>> EnsureAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, Task<bool>> predicate, Error error, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.EnsureAsync(predicate, error, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<Result<TValue>> EnsureAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, Task<bool>> predicate, Func<TValue, Error> errorFactory, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.EnsureAsync(predicate, errorFactory, cancellationToken).ConfigureAwait(false);
    }

    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> EnsureAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Task<bool>> predicate, Error error, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.EnsureAsync(predicate, error, cancellationToken).ConfigureAwait(false);
    }

    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<TValue>> EnsureAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, Task<bool>> predicate, Func<TValue, Error> errorFactory, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.EnsureAsync(predicate, errorFactory, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the Task result and fails with <paramref name="error"/> when the predicate rejects the success value.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Checks the success value. Called only on success.</param>
    /// <param name="error">The error used when the predicate returns false.</param>
    /// <param name="cancellationToken">Observed while awaiting the source.</param>
    /// <returns>The original result, or a failure with <paramref name="error"/>.</returns>
    public static async Task<Result<TValue>> EnsureAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, bool> predicate, Error error, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Ensure(predicate, error);
    }

    /// <summary>
    /// Awaits the Task result and fails with an error built from the success value when the predicate rejects it.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Checks the success value. Called only on success.</param>
    /// <param name="errorFactory">Builds the error when the predicate returns false.</param>
    /// <param name="cancellationToken">Observed while awaiting the source.</param>
    /// <returns>The original result, or a failure with the built error.</returns>
    public static async Task<Result<TValue>> EnsureAsync<TValue>(this Task<Result<TValue>> task, Func<TValue, bool> predicate, Func<TValue, Error> errorFactory, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Ensure(predicate, errorFactory);
    }

    /// <summary>
    /// Awaits the ValueTask result and fails with <paramref name="error"/> when the predicate rejects the success value.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Checks the success value. Called only on success.</param>
    /// <param name="error">The error used when the predicate returns false.</param>
    /// <param name="cancellationToken">Observed while awaiting the source.</param>
    /// <returns>The original result, or a failure with <paramref name="error"/>.</returns>
    public static async ValueTask<Result<TValue>> EnsureAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, bool> predicate, Error error, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Ensure(predicate, error);
    }

    /// <summary>
    /// Awaits the ValueTask result and fails with an error built from the success value when the predicate rejects it.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Checks the success value. Called only on success.</param>
    /// <param name="errorFactory">Builds the error when the predicate returns false.</param>
    /// <param name="cancellationToken">Observed while awaiting the source.</param>
    /// <returns>The original result, or a failure with the built error.</returns>
    public static async ValueTask<Result<TValue>> EnsureAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, bool> predicate, Func<TValue, Error> errorFactory, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Ensure(predicate, errorFactory);
    }

#if NET9_0_OR_GREATER
    /// <summary>
    /// Awaits the ValueTask result and fails with <paramref name="error"/> when the async predicate rejects the success value.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Checks the success value. Called only on success.</param>
    /// <param name="error">The error used when the predicate returns false.</param>
    /// <param name="cancellationToken">Observed while awaiting the source and the predicate.</param>
    /// <returns>The original result, or a failure with <paramref name="error"/>.</returns>
    public static async ValueTask<Result<TValue>> EnsureAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, ValueTask<bool>> predicate, Error error, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.EnsureAsync(predicate, error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Awaits the ValueTask result and fails with an error built from the success value when the async predicate rejects it.
    /// </summary>
    /// <typeparam name="TValue">The type of the success value.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Checks the success value. Called only on success.</param>
    /// <param name="errorFactory">Builds the error when the predicate returns false.</param>
    /// <param name="cancellationToken">Observed while awaiting the source and the predicate.</param>
    /// <returns>The original result, or a failure with the built error.</returns>
    public static async ValueTask<Result<TValue>> EnsureAsync<TValue>(this ValueTask<Result<TValue>> task, Func<TValue, ValueTask<bool>> predicate, Func<TValue, Error> errorFactory, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.EnsureAsync(predicate, errorFactory, cancellationToken).ConfigureAwait(false);
    }
#endif
}
