// Produced by scripts/generate-typed-results.py; do not edit.
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CSharpEssentials.Core;

namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1, E2>
{
    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result, which has the same error types.
    /// An error passes through in its slot and <paramref name="bind"/> is not called.
    /// </summary>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <param name="bind">Called with the success value.</param>
    /// <returns>The result of <paramref name="bind"/>, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<TOut, E1, E2> Bind<TOut>(Func<T, Result<TOut, E1, E2>> bind)
        where TOut : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => bind(RawValue),
            2 or 3 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result with the error types of this result
    /// added in front: an error of <paramref name="bind"/> moves to the matching added slot. An error of this result passes
    /// through in its slot and <paramref name="bind"/> is not called. Use <c>Bind</c> when <paramref name="bind"/> returns
    /// the same error types.
    /// </summary>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <typeparam name="TError1">The first error type that <paramref name="bind"/> adds.</typeparam>
    /// <param name="bind">Called with the success value.</param>
    /// <returns>The result of <paramref name="bind"/> in the widened error set, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<TOut, E1, E2, TError1> BindWiden<TOut, TError1>(Func<T, Result<TOut, TError1>> bind)
        where TOut : notnull
        where TError1 : notnull
    {
        if (Tag == TypedResultLogic.SuccessTag)
        {
            Result<TOut, TError1> inner = bind(RawValue);
            return new(TypedResultLogic.Remap(inner.Tag, 4), inner.RawValue, inner.RawError);
        }

        return Tag switch
        {
            2 or 3 => new(Tag, default!, RawError),
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
    public async Task<Result<TOut, E1, E2>> BindAsync<TOut>(Func<T, Task<Result<TOut, E1, E2>>> bind)
        where TOut : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await bind(RawValue).ConfigureAwait(false),
            2 or 3 => new(Tag, default!, RawError),
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
    public async ValueTask<Result<TOut, E1, E2>> BindAsync<TOut>(Func<T, ValueTask<Result<TOut, E1, E2>>> bind)
        where TOut : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => await bind(RawValue).ConfigureAwait(false),
            2 or 3 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result with the error types of this result
    /// added in front: an error of <paramref name="bind"/> moves to the matching added slot. An error of this result passes
    /// through in its slot and <paramref name="bind"/> is not called. Use <c>Bind</c> when <paramref name="bind"/> returns
    /// the same error types.
    /// <paramref name="bind"/> returns a task, which is awaited.
    /// </summary>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <typeparam name="TError1">The first error type that <paramref name="bind"/> adds.</typeparam>
    /// <param name="bind">Called with the success value.</param>
    /// <returns>The result of <paramref name="bind"/> in the widened error set, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<TOut, E1, E2, TError1>> BindWidenAsync<TOut, TError1>(Func<T, Task<Result<TOut, TError1>>> bind)
        where TOut : notnull
        where TError1 : notnull
    {
        if (Tag == TypedResultLogic.SuccessTag)
        {
            Result<TOut, TError1> inner = await bind(RawValue).ConfigureAwait(false);
            return new(TypedResultLogic.Remap(inner.Tag, 4), inner.RawValue, inner.RawError);
        }

        return Tag switch
        {
            2 or 3 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result with the error types of this result
    /// added in front: an error of <paramref name="bind"/> moves to the matching added slot. An error of this result passes
    /// through in its slot and <paramref name="bind"/> is not called. Use <c>Bind</c> when <paramref name="bind"/> returns
    /// the same error types.
    /// <paramref name="bind"/> returns a value task, which is awaited.
    /// </summary>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <typeparam name="TError1">The first error type that <paramref name="bind"/> adds.</typeparam>
    /// <param name="bind">Called with the success value.</param>
    /// <returns>The result of <paramref name="bind"/> in the widened error set, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<TOut, E1, E2, TError1>> BindWidenAsync<TOut, TError1>(Func<T, ValueTask<Result<TOut, TError1>>> bind)
        where TOut : notnull
        where TError1 : notnull
    {
        if (Tag == TypedResultLogic.SuccessTag)
        {
            Result<TOut, TError1> inner = await bind(RawValue).ConfigureAwait(false);
            return new(TypedResultLogic.Remap(inner.Tag, 4), inner.RawValue, inner.RawError);
        }

        return Tag switch
        {
            2 or 3 => new(Tag, default!, RawError),
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
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="bind">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>The result of <paramref name="bind"/>, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<TOut, E1, E2>> BindAsync<T, E1, E2, TOut>(this Task<Result<T, E1, E2>> task, Func<T, Result<TOut, E1, E2>> bind, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where TOut : notnull
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
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
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="bind">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="bind"/>.</param>
    /// <returns>The result of <paramref name="bind"/>, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<TOut, E1, E2>> BindAsync<T, E1, E2, TOut>(this Task<Result<T, E1, E2>> task, Func<T, Task<Result<TOut, E1, E2>>> bind, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where TOut : notnull
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await TypedResultLogic.Step(bind(result.RawValue), cancellationToken).ConfigureAwait(false),
            2 or 3 => new(result.Tag, default!, result.RawError),
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
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="bind">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>The result of <paramref name="bind"/>, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<TOut, E1, E2>> BindAsync<T, E1, E2, TOut>(this ValueTask<Result<T, E1, E2>> task, Func<T, Result<TOut, E1, E2>> bind, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where TOut : notnull
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
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
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="bind">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="bind"/>.</param>
    /// <returns>The result of <paramref name="bind"/>, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<TOut, E1, E2>> BindAsync<T, E1, E2, TOut>(this ValueTask<Result<T, E1, E2>> task, Func<T, ValueTask<Result<TOut, E1, E2>>> bind, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where TOut : notnull
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => await bind(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false),
            2 or 3 => new(result.Tag, default!, result.RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result with the error types of this result
    /// added in front: an error of <paramref name="bind"/> moves to the matching added slot. An error of this result passes
    /// through in its slot and <paramref name="bind"/> is not called. Use <c>Bind</c> when <paramref name="bind"/> returns
    /// the same error types.
    /// Awaits <paramref name="task"/>, then calls <c>BindWiden</c>.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <typeparam name="TError1">The first error type that <paramref name="bind"/> adds.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="bind">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>The result of <paramref name="bind"/> in the widened error set, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<TOut, E1, E2, TError1>> BindWidenAsync<T, E1, E2, TOut, TError1>(this Task<Result<T, E1, E2>> task, Func<T, Result<TOut, TError1>> bind, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where TOut : notnull
        where TError1 : notnull
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.BindWiden<TOut, TError1>(bind);
    }

    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result with the error types of this result
    /// added in front: an error of <paramref name="bind"/> moves to the matching added slot. An error of this result passes
    /// through in its slot and <paramref name="bind"/> is not called. Use <c>Bind</c> when <paramref name="bind"/> returns
    /// the same error types.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="bind"/> returns.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <typeparam name="TError1">The first error type that <paramref name="bind"/> adds.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="bind">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="bind"/>.</param>
    /// <returns>The result of <paramref name="bind"/> in the widened error set, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<TOut, E1, E2, TError1>> BindWidenAsync<T, E1, E2, TOut, TError1>(this Task<Result<T, E1, E2>> task, Func<T, Task<Result<TOut, TError1>>> bind, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where TOut : notnull
        where TError1 : notnull
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.Tag == TypedResultLogic.SuccessTag)
        {
            Result<TOut, TError1> inner = await TypedResultLogic.Step(bind(result.RawValue), cancellationToken).ConfigureAwait(false);
            return new(TypedResultLogic.Remap(inner.Tag, 4), inner.RawValue, inner.RawError);
        }

        return result.Tag switch
        {
            2 or 3 => new(result.Tag, default!, result.RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result with the error types of this result
    /// added in front: an error of <paramref name="bind"/> moves to the matching added slot. An error of this result passes
    /// through in its slot and <paramref name="bind"/> is not called. Use <c>Bind</c> when <paramref name="bind"/> returns
    /// the same error types.
    /// Awaits <paramref name="task"/>, then calls <c>BindWiden</c>.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <typeparam name="TError1">The first error type that <paramref name="bind"/> adds.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="bind">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>The result of <paramref name="bind"/> in the widened error set, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<TOut, E1, E2, TError1>> BindWidenAsync<T, E1, E2, TOut, TError1>(this ValueTask<Result<T, E1, E2>> task, Func<T, Result<TOut, TError1>> bind, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where TOut : notnull
        where TError1 : notnull
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.BindWiden<TOut, TError1>(bind);
    }

    /// <summary>
    /// Calls <paramref name="bind"/> with the success value and returns its result with the error types of this result
    /// added in front: an error of <paramref name="bind"/> moves to the matching added slot. An error of this result passes
    /// through in its slot and <paramref name="bind"/> is not called. Use <c>Bind</c> when <paramref name="bind"/> returns
    /// the same error types.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="bind"/> returns.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="TOut">The success value type of the returned result.</typeparam>
    /// <typeparam name="TError1">The first error type that <paramref name="bind"/> adds.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="bind">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="bind"/>.</param>
    /// <returns>The result of <paramref name="bind"/> in the widened error set, or a result that holds the same error.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<TOut, E1, E2, TError1>> BindWidenAsync<T, E1, E2, TOut, TError1>(this ValueTask<Result<T, E1, E2>> task, Func<T, ValueTask<Result<TOut, TError1>>> bind, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where TOut : notnull
        where TError1 : notnull
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.Tag == TypedResultLogic.SuccessTag)
        {
            Result<TOut, TError1> inner = await bind(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false);
            return new(TypedResultLogic.Remap(inner.Tag, 4), inner.RawValue, inner.RawError);
        }

        return result.Tag switch
        {
            2 or 3 => new(result.Tag, default!, result.RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}
