// Produced by scripts/generate-typed-results.py; do not edit.
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CSharpEssentials.Core;

namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1, E2, E3>
{
    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result, which has the same error types.
    /// An error passes through in its slot and <paramref name="bind"/> is not called.
    /// </summary>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <param name="bind">Called with the success value.</param>
    /// <returns>The result of <paramref name="bind"/>, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<TOut, E1, E2, E3> Bind<TOut>(Func<T, Result<TOut, E1, E2, E3>> bind)
        where TOut : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => bind(RawValue),
            2 or 3 or 4 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result, which has the same error types.
    /// An error passes through in its slot and <paramref name="bind"/> is not called.
    /// <paramref name="bind"/> returns a task, which is awaited.
    /// </summary>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <param name="bind">Called with the success value.</param>
    /// <returns>The result of <paramref name="bind"/>, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<TOut, E1, E2, E3>> BindAsync<TOut>(Func<T, Task<Result<TOut, E1, E2, E3>>> bind)
        where TOut : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await bind(RawValue).ConfigureAwait(false),
            2 or 3 or 4 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result, which has the same error types.
    /// An error passes through in its slot and <paramref name="bind"/> is not called.
    /// <paramref name="bind"/> returns a value task, which is awaited.
    /// </summary>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <param name="bind">Called with the success value.</param>
    /// <returns>The result of <paramref name="bind"/>, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<TOut, E1, E2, E3>> BindAsync<TOut>(Func<T, ValueTask<Result<TOut, E1, E2, E3>>> bind)
        where TOut : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await bind(RawValue).ConfigureAwait(false),
            2 or 3 or 4 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}

public static partial class ResultExtensions
{
    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result, which has the same error types.
    /// An error passes through in its slot and <paramref name="bind"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>Bind</c>.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="bind">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>The result of <paramref name="bind"/>, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<TOut, E1, E2, E3>> BindAsync<T, E1, E2, E3, TOut>(this Task<Result<T, E1, E2, E3>> task, Func<T, Result<TOut, E1, E2, E3>> bind, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
        where TOut : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Bind<TOut>(bind);
    }

    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result, which has the same error types.
    /// An error passes through in its slot and <paramref name="bind"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="bind"/> returns.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="bind">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="bind"/>.</param>
    /// <returns>The result of <paramref name="bind"/>, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<TOut, E1, E2, E3>> BindAsync<T, E1, E2, E3, TOut>(this Task<Result<T, E1, E2, E3>> task, Func<T, Task<Result<TOut, E1, E2, E3>>> bind, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
        where TOut : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await TypedResultLogic.Step(bind(result.RawValue), cancellationToken).ConfigureAwait(false),
            2 or 3 or 4 => new(result.Tag, default!, result.RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result, which has the same error types.
    /// An error passes through in its slot and <paramref name="bind"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>Bind</c>.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="bind">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>The result of <paramref name="bind"/>, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<TOut, E1, E2, E3>> BindAsync<T, E1, E2, E3, TOut>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, Result<TOut, E1, E2, E3>> bind, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
        where TOut : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Bind<TOut>(bind);
    }

    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result, which has the same error types.
    /// An error passes through in its slot and <paramref name="bind"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="bind"/> returns.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="bind">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="bind"/>.</param>
    /// <returns>The result of <paramref name="bind"/>, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<TOut, E1, E2, E3>> BindAsync<T, E1, E2, E3, TOut>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, ValueTask<Result<TOut, E1, E2, E3>>> bind, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
        where TOut : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await bind(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false),
            2 or 3 or 4 => new(result.Tag, default!, result.RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}
