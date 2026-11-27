// Produced by scripts/generate-typed-results.py; do not edit.
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CSharpEssentials.Core;

namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1, E2, E3>
{
    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E2, E3> Ensure(Func<T, bool> predicate, E1 error)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? this : new(2, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E2, E3> Ensure(Func<T, bool> predicate, E2 error)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? this : new(3, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E2, E3> Ensure(Func<T, bool> predicate, E3 error)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? this : new(4, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => this,
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
    public Result<T, E1, E2, E3> FailIf(Func<T, bool> predicate, E1 error)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? new(2, default!, TypedResultLogic.NotNull(error, nameof(error))) : this,
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E2, E3> FailIf(Func<T, bool> predicate, E2 error)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? new(3, default!, TypedResultLogic.NotNull(error, nameof(error))) : this,
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E2, E3> FailIf(Func<T, bool> predicate, E3 error)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => predicate(RawValue) ? new(4, default!, TypedResultLogic.NotNull(error, nameof(error))) : this,
            2 or 3 or 4 => this,
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
    public async Task<Result<T, E1, E2, E3>> EnsureAsync(Func<T, Task<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? this : new(2, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// <paramref name="predicate"/> returns a task, which is awaited.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, E1, E2, E3>> EnsureAsync(Func<T, Task<bool>> predicate, E2 error, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? this : new(3, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// <paramref name="predicate"/> returns a task, which is awaited.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, E1, E2, E3>> EnsureAsync(Func<T, Task<bool>> predicate, E3 error, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? this : new(4, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => this,
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
    public async Task<Result<T, E1, E2, E3>> FailIfAsync(Func<T, Task<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(2, default!, TypedResultLogic.NotNull(error, nameof(error))) : this,
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// <paramref name="predicate"/> returns a task, which is awaited.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, E1, E2, E3>> FailIfAsync(Func<T, Task<bool>> predicate, E2 error, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(3, default!, TypedResultLogic.NotNull(error, nameof(error))) : this,
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// <paramref name="predicate"/> returns a task, which is awaited.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, E1, E2, E3>> FailIfAsync(Func<T, Task<bool>> predicate, E3 error, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(4, default!, TypedResultLogic.NotNull(error, nameof(error))) : this,
            2 or 3 or 4 => this,
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
    public async ValueTask<Result<T, E1, E2, E3>> EnsureAsync(Func<T, ValueTask<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? this : new(2, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// <paramref name="predicate"/> returns a value task, which is awaited.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, E1, E2, E3>> EnsureAsync(Func<T, ValueTask<bool>> predicate, E2 error, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? this : new(3, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// <paramref name="predicate"/> returns a value task, which is awaited.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, E1, E2, E3>> EnsureAsync(Func<T, ValueTask<bool>> predicate, E3 error, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? this : new(4, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => this,
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
    public async ValueTask<Result<T, E1, E2, E3>> FailIfAsync(Func<T, ValueTask<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(2, default!, TypedResultLogic.NotNull(error, nameof(error))) : this,
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// <paramref name="predicate"/> returns a value task, which is awaited.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, E1, E2, E3>> FailIfAsync(Func<T, ValueTask<bool>> predicate, E2 error, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(3, default!, TypedResultLogic.NotNull(error, nameof(error))) : this,
            2 or 3 or 4 => this,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// <paramref name="predicate"/> returns a value task, which is awaited.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, E1, E2, E3>> FailIfAsync(Func<T, ValueTask<bool>> predicate, E3 error, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(4, default!, TypedResultLogic.NotNull(error, nameof(error))) : this,
            2 or 3 or 4 => this,
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
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, E2, E3>> EnsureAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, Func<T, bool> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Ensure(predicate, error);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, E2, E3>> EnsureAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, Func<T, Task<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? result : new(2, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>Ensure</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, E2, E3>> EnsureAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, bool> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Ensure(predicate, error);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T, E1, E2, E3>> EnsureAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, Task<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? result : new(2, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>Ensure</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, E2, E3>> EnsureAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, Func<T, bool> predicate, E2 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Ensure(predicate, error);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, E2, E3>> EnsureAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, Func<T, Task<bool>> predicate, E2 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? result : new(3, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>Ensure</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, E2, E3>> EnsureAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, bool> predicate, E2 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Ensure(predicate, error);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T, E1, E2, E3>> EnsureAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, Task<bool>> predicate, E2 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? result : new(3, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>Ensure</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, E2, E3>> EnsureAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, Func<T, bool> predicate, E3 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Ensure(predicate, error);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, E2, E3>> EnsureAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, Func<T, Task<bool>> predicate, E3 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? result : new(4, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>Ensure</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, E2, E3>> EnsureAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, bool> predicate, E3 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Ensure(predicate, error);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T, E1, E2, E3>> EnsureAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, Task<bool>> predicate, E3 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? result : new(4, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>FailIf</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, E2, E3>> FailIfAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, Func<T, bool> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.FailIf(predicate, error);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, E2, E3>> FailIfAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, Func<T, Task<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(2, default!, TypedResultLogic.NotNull(error, nameof(error))) : result,
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>FailIf</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, E2, E3>> FailIfAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, bool> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.FailIf(predicate, error);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T, E1, E2, E3>> FailIfAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, Task<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(2, default!, TypedResultLogic.NotNull(error, nameof(error))) : result,
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>FailIf</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, E2, E3>> FailIfAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, Func<T, bool> predicate, E2 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.FailIf(predicate, error);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, E2, E3>> FailIfAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, Func<T, Task<bool>> predicate, E2 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(3, default!, TypedResultLogic.NotNull(error, nameof(error))) : result,
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>FailIf</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, E2, E3>> FailIfAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, bool> predicate, E2 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.FailIf(predicate, error);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T, E1, E2, E3>> FailIfAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, Task<bool>> predicate, E2 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(3, default!, TypedResultLogic.NotNull(error, nameof(error))) : result,
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>FailIf</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, E2, E3>> FailIfAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, Func<T, bool> predicate, E3 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.FailIf(predicate, error);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, E2, E3>> FailIfAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, Func<T, Task<bool>> predicate, E3 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(4, default!, TypedResultLogic.NotNull(error, nameof(error))) : result,
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>FailIf</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, E2, E3>> FailIfAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, bool> predicate, E3 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.FailIf(predicate, error);
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T, E1, E2, E3>> FailIfAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, Task<bool>> predicate, E3 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(4, default!, TypedResultLogic.NotNull(error, nameof(error))) : result,
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

#if NET9_0_OR_GREATER
    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, E2, E3>> EnsureAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, ValueTask<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? result : new(2, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, E2, E3>> EnsureAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, ValueTask<bool>> predicate, E2 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? result : new(3, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> does not hold.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="false"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="false"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, E2, E3>> EnsureAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, ValueTask<bool>> predicate, E3 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? result : new(4, default!, TypedResultLogic.NotNull(error, nameof(error))),
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E1"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, E2, E3>> FailIfAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, ValueTask<bool>> predicate, E1 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(2, default!, TypedResultLogic.NotNull(error, nameof(error))) : result,
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E2"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, E2, E3>> FailIfAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, ValueTask<bool>> predicate, E2 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(3, default!, TypedResultLogic.NotNull(error, nameof(error))) : result,
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Turns a success into the error <paramref name="error"/> in the <typeparamref name="E3"/> slot when <paramref name="predicate"/> holds.
    /// An error passes through and <paramref name="predicate"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="predicate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="error">The error to return when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="predicate"/>.</param>
    /// <returns>This result, or a result that holds <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <see langword="null"/> and <paramref name="predicate"/> returns <see langword="true"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, E2, E3>> FailIfAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, ValueTask<bool>> predicate, E3 error, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await predicate(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false) ? new(4, default!, TypedResultLogic.NotNull(error, nameof(error))) : result,
            2 or 3 or 4 => result,
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
#endif
}
