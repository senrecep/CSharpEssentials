// Produced by scripts/generate-typed-results.py; do not edit.
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CSharpEssentials.Core;

namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1>
{
    /// <summary>Replaces the error in the <typeparamref name="E1"/> slot with the return value of <paramref name="map"/>. Every other case passes through.</summary>
    /// <typeparam name="TError">The error type that replaces <typeparamref name="E1"/>.</typeparam>
    /// <param name="map">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <returns>A result whose slot holds the mapped error, or the same case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, TError> MapError<TError>(Func<E1, TError> map)
        where TError : notnull
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => new(Tag, default!, TypedResultLogic.NotNull(map((E1)RawError!), nameof(map))),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E1"/> slot with the return value of <paramref name="map"/>. Every other case passes through.
    /// <paramref name="map"/> returns a task, which is awaited.</summary>
    /// <typeparam name="TError">The error type that replaces <typeparamref name="E1"/>.</typeparam>
    /// <param name="map">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Checked before <paramref name="map"/> is called. A running <paramref name="map"/> is not abandoned.</param>
    /// <returns>A result whose slot holds the mapped error, or the same case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, TError>> MapErrorAsync<TError>(Func<E1, Task<TError>> map, CancellationToken cancellationToken = default)
        where TError : notnull
    {
        if (Tag == 2)
            cancellationToken.ThrowIfCancellationRequested();

        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => new(Tag, default!, TypedResultLogic.NotNull(await map((E1)RawError!).ConfigureAwait(false), nameof(map))),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E1"/> slot with the return value of <paramref name="map"/>. Every other case passes through.
    /// <paramref name="map"/> returns a value task, which is awaited.</summary>
    /// <typeparam name="TError">The error type that replaces <typeparamref name="E1"/>.</typeparam>
    /// <param name="map">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Checked before <paramref name="map"/> is called. A running <paramref name="map"/> is not abandoned.</param>
    /// <returns>A result whose slot holds the mapped error, or the same case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, TError>> MapErrorAsync<TError>(Func<E1, ValueTask<TError>> map, CancellationToken cancellationToken = default)
        where TError : notnull
    {
        if (Tag == 2)
            cancellationToken.ThrowIfCancellationRequested();

        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => new(Tag, default!, TypedResultLogic.NotNull(await map((E1)RawError!).ConfigureAwait(false), nameof(map))),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}

public static partial class ResultExtensions
{
    /// <summary>Replaces the error in the <typeparamref name="E1"/> slot with the return value of <paramref name="map"/>. Every other case passes through.
    /// Awaits <paramref name="task"/>, then calls <c>MapError</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TError">The error type that replaces <typeparamref name="E1"/>.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="map">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A result whose slot holds the mapped error, or the same case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, TError>> MapErrorAsync<T, E1, TError>(this Task<Result<T, E1>> task, Func<E1, TError> map, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TError : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.MapError<TError>(map);
    }

    /// <summary>Replaces the error in the <typeparamref name="E1"/> slot with the return value of <paramref name="map"/>. Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="map"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TError">The error type that replaces <typeparamref name="E1"/>.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="map">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Checked before <paramref name="map"/> is called. Observed while awaiting <paramref name="task"/>; a running <paramref name="map"/> is not abandoned.</param>
    /// <returns>A result whose slot holds the mapped error, or the same case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, TError>> MapErrorAsync<T, E1, TError>(this Task<Result<T, E1>> task, Func<E1, Task<TError>> map, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TError : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.Tag == 2)
            cancellationToken.ThrowIfCancellationRequested();

        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => new(result.Tag, result.RawValue, null),
            2 => new(result.Tag, default!, TypedResultLogic.NotNull(await map((E1)result.RawError!).ConfigureAwait(false), nameof(map))),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E1"/> slot with the return value of <paramref name="map"/>. Every other case passes through.
    /// Awaits <paramref name="task"/>, then calls <c>MapError</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TError">The error type that replaces <typeparamref name="E1"/>.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="map">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A result whose slot holds the mapped error, or the same case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, TError>> MapErrorAsync<T, E1, TError>(this ValueTask<Result<T, E1>> task, Func<E1, TError> map, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TError : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.MapError<TError>(map);
    }

    /// <summary>Replaces the error in the <typeparamref name="E1"/> slot with the return value of <paramref name="map"/>. Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="map"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TError">The error type that replaces <typeparamref name="E1"/>.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="map">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Checked before <paramref name="map"/> is called. Observed while awaiting <paramref name="task"/>; a running <paramref name="map"/> is not abandoned.</param>
    /// <returns>A result whose slot holds the mapped error, or the same case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, TError>> MapErrorAsync<T, E1, TError>(this ValueTask<Result<T, E1>> task, Func<E1, ValueTask<TError>> map, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TError : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.Tag == 2)
            cancellationToken.ThrowIfCancellationRequested();

        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => new(result.Tag, result.RawValue, null),
            2 => new(result.Tag, default!, TypedResultLogic.NotNull(await map((E1)result.RawError!).ConfigureAwait(false), nameof(map))),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
}
