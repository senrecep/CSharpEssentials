// Produced by scripts/generate-typed-results.py; do not edit.
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CSharpEssentials.Core;

namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1, E2, E3>
{
    /// <summary>Replaces the success value with the return value of <paramref name="map"/>. An error passes through in its slot.</summary>
    /// <typeparam name="TOut">The new success value type.</typeparam>
    /// <param name="map">Called with the success value.</param>
    /// <returns>A result that holds the mapped value or the same error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<TOut, E1, E2, E3> Map<TOut>(Func<T, TOut> map)
        where TOut : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(TypedResultLogic.SuccessTag, TypedResultLogic.NotNull(map(RawValue), nameof(map)), null),
            2 or 3 or 4 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the success value with the return value of <paramref name="map"/>. An error passes through in its slot.
    /// <paramref name="map"/> returns a task, which is awaited.</summary>
    /// <typeparam name="TOut">The new success value type.</typeparam>
    /// <param name="map">Called with the success value.</param>
    /// <param name="cancellationToken">Checked before <paramref name="map"/> is called. Observed while awaiting <paramref name="map"/>.</param>
    /// <returns>A result that holds the mapped value or the same error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<TOut, E1, E2, E3>> MapAsync<TOut>(Func<T, Task<TOut>> map, CancellationToken cancellationToken = default)
        where TOut : notnull
    {
        if (Tag == TypedResultLogic.SuccessTag)
            cancellationToken.ThrowIfCancellationRequested();

        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(TypedResultLogic.SuccessTag, TypedResultLogic.NotNull(await map(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false), nameof(map)), null),
            2 or 3 or 4 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

#if NET9_0_OR_GREATER
    /// <summary>Replaces the success value with the return value of <paramref name="map"/>. An error passes through in its slot.
    /// <paramref name="map"/> returns a value task, which is awaited.</summary>
    /// <typeparam name="TOut">The new success value type.</typeparam>
    /// <param name="map">Called with the success value.</param>
    /// <param name="cancellationToken">Checked before <paramref name="map"/> is called. Observed while awaiting <paramref name="map"/>.</param>
    /// <returns>A result that holds the mapped value or the same error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<TOut, E1, E2, E3>> MapAsync<TOut>(Func<T, ValueTask<TOut>> map, CancellationToken cancellationToken = default)
        where TOut : notnull
    {
        if (Tag == TypedResultLogic.SuccessTag)
            cancellationToken.ThrowIfCancellationRequested();

        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(TypedResultLogic.SuccessTag, TypedResultLogic.NotNull(await map(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false), nameof(map)), null),
            2 or 3 or 4 => new(Tag, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
#endif
}

public static partial class ResultExtensions
{
    /// <summary>Replaces the success value with the return value of <paramref name="map"/>. An error passes through in its slot.
    /// Awaits <paramref name="task"/>, then calls <c>Map</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <typeparam name="TOut">The new success value type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="map">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A result that holds the mapped value or the same error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<TOut, E1, E2, E3>> MapAsync<T, E1, E2, E3, TOut>(this Task<Result<T, E1, E2, E3>> task, Func<T, TOut> map, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
        where TOut : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Map<TOut>(map);
    }

    /// <summary>Replaces the success value with the return value of <paramref name="map"/>. An error passes through in its slot.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="map"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <typeparam name="TOut">The new success value type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="map">Called with the success value.</param>
    /// <param name="cancellationToken">Checked before <paramref name="map"/> is called. Observed while awaiting <paramref name="task"/> and <paramref name="map"/>.</param>
    /// <returns>A result that holds the mapped value or the same error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<TOut, E1, E2, E3>> MapAsync<T, E1, E2, E3, TOut>(this Task<Result<T, E1, E2, E3>> task, Func<T, Task<TOut>> map, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
        where TOut : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.MapAsync<TOut>(map, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Replaces the success value with the return value of <paramref name="map"/>. An error passes through in its slot.
    /// Awaits <paramref name="task"/>, then calls <c>Map</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <typeparam name="TOut">The new success value type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="map">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A result that holds the mapped value or the same error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<TOut, E1, E2, E3>> MapAsync<T, E1, E2, E3, TOut>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, TOut> map, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
        where TOut : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Map<TOut>(map);
    }

    /// <summary>Replaces the success value with the return value of <paramref name="map"/>. An error passes through in its slot.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="map"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <typeparam name="TOut">The new success value type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="map">Called with the success value.</param>
    /// <param name="cancellationToken">Checked before <paramref name="map"/> is called. Observed while awaiting <paramref name="task"/> and <paramref name="map"/>.</param>
    /// <returns>A result that holds the mapped value or the same error.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<TOut, E1, E2, E3>> MapAsync<T, E1, E2, E3, TOut>(this ValueTask<Result<T, E1, E2, E3>> task, Func<T, ValueTask<TOut>> map, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
        where TOut : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.Tag == TypedResultLogic.SuccessTag)
            cancellationToken.ThrowIfCancellationRequested();

        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => new(TypedResultLogic.SuccessTag, TypedResultLogic.NotNull(await map(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false), nameof(map)), null),
            2 or 3 or 4 => new(result.Tag, default!, result.RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}
