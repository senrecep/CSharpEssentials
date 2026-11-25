// Produced by scripts/generate-typed-results.py; do not edit.
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CSharpEssentials.Core;

namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1>
{
    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1> Ensure(Func<T, bool> predicate, E1 error)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? this : new(2, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> does not hold.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// </summary>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, TError> Ensure<TError>(Func<T, bool> predicate, TError error)
        where TError : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? new(TypedResultLogic.SuccessTag, RawValue, null) : new(3, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1> FailIf(Func<T, bool> predicate, E1 error)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? new(2, default!, TypedResultLogic.NotNull(error, nameof(error))) : this,
            2 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> holds.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// </summary>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, TError> FailIf<TError>(Func<T, bool> predicate, TError error)
        where TError : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? new(3, default!, TypedResultLogic.NotNull(error, nameof(error))) : new(TypedResultLogic.SuccessTag, RawValue, null),
            2 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// <paramref name="predicate"/> returns a task, which is awaited.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, E1>> EnsureAsync(Func<T, Task<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? this : new(2, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> does not hold.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// <paramref name="predicate"/> returns a task, which is awaited.
    /// </summary>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="predicate"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, E1, TError>> EnsureAsync<TError>(Func<T, Task<bool>> predicate, TError error, CancellationToken cancellationToken = default)
        where TError : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(TypedResultLogic.SuccessTag, RawValue, null) : new(3, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// <paramref name="predicate"/> returns a task, which is awaited.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, E1>> FailIfAsync(Func<T, Task<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(2, default!, TypedResultLogic.NotNull(error, nameof(error))) : this,
            2 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> holds.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// <paramref name="predicate"/> returns a task, which is awaited.
    /// </summary>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="predicate"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, E1, TError>> FailIfAsync<TError>(Func<T, Task<bool>> predicate, TError error, CancellationToken cancellationToken = default)
        where TError : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(3, default!, TypedResultLogic.NotNull(error, nameof(error))) : new(TypedResultLogic.SuccessTag, RawValue, null),
            2 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

#if NET9_0_OR_GREATER
    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// <paramref name="predicate"/> returns a value task, which is awaited.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, E1>> EnsureAsync(Func<T, ValueTask<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? this : new(2, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> does not hold.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// <paramref name="predicate"/> returns a value task, which is awaited.
    /// </summary>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="predicate"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, E1, TError>> EnsureAsync<TError>(Func<T, ValueTask<bool>> predicate, TError error, CancellationToken cancellationToken = default)
        where TError : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(TypedResultLogic.SuccessTag, RawValue, null) : new(3, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// <paramref name="predicate"/> returns a value task, which is awaited.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, E1>> FailIfAsync(Func<T, ValueTask<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(2, default!, TypedResultLogic.NotNull(error, nameof(error))) : this,
            2 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> holds.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// <paramref name="predicate"/> returns a value task, which is awaited.
    /// </summary>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="predicate"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, E1, TError>> FailIfAsync<TError>(Func<T, ValueTask<bool>> predicate, TError error, CancellationToken cancellationToken = default)
        where TError : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(3, default!, TypedResultLogic.NotNull(error, nameof(error))) : new(TypedResultLogic.SuccessTag, RawValue, null),
            2 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
#endif
}

public static partial class ResultExtensions
{
    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>Ensure</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(2)]
    public static async Task<Result<T, E1>> EnsureAsync<T, E1>(this Task<Result<T, E1>> task, Func<T, bool> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Ensure(predicate, error);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(2)]
    public static async Task<Result<T, E1>> EnsureAsync<T, E1>(this Task<Result<T, E1>> task, Func<T, Task<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.EnsureAsync(predicate, error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>Ensure</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(2)]
    public static async ValueTask<Result<T, E1>> EnsureAsync<T, E1>(this ValueTask<Result<T, E1>> task, Func<T, bool> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Ensure(predicate, error);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(3)]
    public static async ValueTask<Result<T, E1>> EnsureAsync<T, E1>(this ValueTask<Result<T, E1>> task, Func<T, Task<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.EnsureAsync(predicate, error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> does not hold.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// Awaits <paramref name="task"/>, then calls <c>Ensure</c>.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, TError>> EnsureAsync<T, E1, TError>(this Task<Result<T, E1>> task, Func<T, bool> predicate, TError error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TError : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Ensure<TError>(predicate, error);
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> does not hold.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, TError>> EnsureAsync<T, E1, TError>(this Task<Result<T, E1>> task, Func<T, Task<bool>> predicate, TError error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TError : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.EnsureAsync<TError>(predicate, error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> does not hold.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// Awaits <paramref name="task"/>, then calls <c>Ensure</c>.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, TError>> EnsureAsync<T, E1, TError>(this ValueTask<Result<T, E1>> task, Func<T, bool> predicate, TError error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TError : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Ensure<TError>(predicate, error);
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> does not hold.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T, E1, TError>> EnsureAsync<T, E1, TError>(this ValueTask<Result<T, E1>> task, Func<T, Task<bool>> predicate, TError error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TError : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.EnsureAsync<TError>(predicate, error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>FailIf</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(2)]
    public static async Task<Result<T, E1>> FailIfAsync<T, E1>(this Task<Result<T, E1>> task, Func<T, bool> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.FailIf(predicate, error);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(2)]
    public static async Task<Result<T, E1>> FailIfAsync<T, E1>(this Task<Result<T, E1>> task, Func<T, Task<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.FailIfAsync(predicate, error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>FailIf</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(2)]
    public static async ValueTask<Result<T, E1>> FailIfAsync<T, E1>(this ValueTask<Result<T, E1>> task, Func<T, bool> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.FailIf(predicate, error);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(3)]
    public static async ValueTask<Result<T, E1>> FailIfAsync<T, E1>(this ValueTask<Result<T, E1>> task, Func<T, Task<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.FailIfAsync(predicate, error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> holds.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// Awaits <paramref name="task"/>, then calls <c>FailIf</c>.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, TError>> FailIfAsync<T, E1, TError>(this Task<Result<T, E1>> task, Func<T, bool> predicate, TError error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TError : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.FailIf<TError>(predicate, error);
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> holds.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, TError>> FailIfAsync<T, E1, TError>(this Task<Result<T, E1>> task, Func<T, Task<bool>> predicate, TError error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TError : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.FailIfAsync<TError>(predicate, error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> holds.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// Awaits <paramref name="task"/>, then calls <c>FailIf</c>.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, TError>> FailIfAsync<T, E1, TError>(this ValueTask<Result<T, E1>> task, Func<T, bool> predicate, TError error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TError : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.FailIf<TError>(predicate, error);
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> holds.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T, E1, TError>> FailIfAsync<T, E1, TError>(this ValueTask<Result<T, E1>> task, Func<T, Task<bool>> predicate, TError error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TError : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.FailIfAsync<TError>(predicate, error, cancellationToken).ConfigureAwait(false);
    }

#if NET9_0_OR_GREATER
    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(2)]
    public static async ValueTask<Result<T, E1>> EnsureAsync<T, E1>(this ValueTask<Result<T, E1>> task, Func<T, ValueTask<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.EnsureAsync(predicate, error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> does not hold.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="predicate"/> returns.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, TError>> EnsureAsync<T, E1, TError>(this ValueTask<Result<T, E1>> task, Func<T, ValueTask<bool>> predicate, TError error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TError : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.EnsureAsync<TError>(predicate, error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(2)]
    public static async ValueTask<Result<T, E1>> FailIfAsync<T, E1>(this ValueTask<Result<T, E1>> task, Func<T, ValueTask<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.FailIfAsync(predicate, error, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Turns a success into the error <paramref name="error"/> in a new last slot when <paramref name="predicate"/> holds.
    /// An error passes through in its slot and <paramref name="predicate"/> is not called. When <paramref name="error"/> has
    /// the exact type of an existing slot, the overload that keeps the error types is chosen instead.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="predicate"/> returns.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TError">The added error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>A result with <typeparamref name="TError"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, TError>> FailIfAsync<T, E1, TError>(this ValueTask<Result<T, E1>> task, Func<T, ValueTask<bool>> predicate, TError error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TError : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.FailIfAsync<TError>(predicate, error, cancellationToken).ConfigureAwait(false);
    }
#endif
}
