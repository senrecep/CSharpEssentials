// Produced by scripts/generate-typed-results.py; do not edit.
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CSharpEssentials.Core;

namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1, E2, E3>
{
    /// <summary>Replaces the success value with the return value of <paramref name="selector"/>, as <c>Map</c> does. It makes
    /// <c>select</c> work in query syntax. An error passes through in its slot.</summary>
    /// <typeparam name="TOut">The new success value type.</typeparam>
    /// <param name="selector">Called with the success value.</param>
    /// <returns>A result that holds the mapped value or the same error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<TOut, E1, E2, E3> Select<TOut>(Func<T, TOut> selector)
        where TOut : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(TypedResultLogic.SuccessTag, TypedResultLogic.NotNull(selector(RawValue), nameof(selector)), null),
            2 or 3 or 4 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>
    /// Calls <paramref name="selector"/> with the success value, then <paramref name="resultSelector"/> with both values.
    /// It makes a second <c>from</c> work in query syntax when the inner result has the same error types. An error passes
    /// through in its slot.
    /// </summary>
    /// <typeparam name="TMiddle">The success value type of the inner result.</typeparam>
    /// <typeparam name="TOut">The new success value type.</typeparam>
    /// <param name="selector">Called with the success value.</param>
    /// <param name="resultSelector">Called with the success value and the inner success value.</param>
    /// <returns>A result that holds the projected value or the first error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="resultSelector"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public Result<TOut, E1, E2, E3> SelectMany<TMiddle, TOut>(Func<T, Result<TMiddle, E1, E2, E3>> selector, Func<T, TMiddle, TOut> resultSelector)
        where TMiddle : notnull
        where TOut : notnull
    {
        if (Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(Tag), default!, RawError);

        Result<TMiddle, E1, E2, E3> inner = selector(RawValue);
        if (inner.Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(inner.Tag), default!, inner.RawError);

        return new(TypedResultLogic.SuccessTag, TypedResultLogic.NotNull(resultSelector(RawValue, inner.RawValue), nameof(resultSelector)), null);
    }
}

public static partial class ResultExtensions
{
    /// <summary>Replaces the success value with the return value of <paramref name="selector"/>, as <c>Map</c> does. It makes
    /// <c>select</c> work in query syntax. An error passes through in its slot.
    /// Awaits <paramref name="task"/>, then calls <c>Select</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <typeparam name="TOut">The new success value type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="selector">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A result that holds the mapped value or the same error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<TOut, E1, E2, E3>> SelectAsync<T, E1, E2, E3, TOut>(this Task<Result<T, E1, E2, E3>> task, Func<T, TOut> selector, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
        where TOut : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Select<TOut>(selector);
    }

    /// <summary>Replaces the success value with the return value of <paramref name="selector"/>, as <c>Map</c> does. It makes
    /// <c>select</c> work in query syntax. An error passes through in its slot.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="selector"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <typeparam name="TOut">The new success value type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="selector">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="selector"/>.</param>
    /// <returns>A result that holds the mapped value or the same error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<TOut, E1, E2, E3>> SelectAsync<T, E1, E2, E3, TOut>(this Task<Result<T, E1, E2, E3>> task, Func<T, Task<TOut>> selector, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
        where TOut : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => new(TypedResultLogic.SuccessTag, TypedResultLogic.NotNull(await selector(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false), nameof(selector)), null),
            2 or 3 or 4 => new(result.Tag, default!, result.RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the success value with the return value of <paramref name="selector"/>, as <c>Map</c> does. It makes
    /// <c>select</c> work in query syntax. An error passes through in its slot.
    /// Awaits <paramref name="task"/>, then calls <c>Select</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <typeparam name="TOut">The new success value type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="selector">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A result that holds the mapped value or the same error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="selector"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<TOut, E1, E2, E3>> SelectAsync<T, E1, E2, E3, TOut>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, TOut> selector, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
        where TOut : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Select<TOut>(selector);
    }
}
