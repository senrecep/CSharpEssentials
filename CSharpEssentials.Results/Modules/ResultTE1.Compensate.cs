// Produced by scripts/generate-typed-results.py; do not edit.
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CSharpEssentials.Core;

namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1>
{
    /// <summary>Replaces the error with the result of <paramref name="compensate"/>, which can fail again with the same error type.
    /// A success passes through.</summary>
    /// <param name="compensate">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1> Compensate(Func<E1, Result<T, E1>> compensate)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => this,
            2 => compensate((E1)RawError!),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error with the result of <paramref name="compensate"/>, which can fail again with the same error type.
    /// A success passes through.
    /// <paramref name="compensate"/> returns a task, which is awaited.</summary>
    /// <param name="compensate">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, E1>> CompensateAsync(Func<E1, Task<Result<T, E1>>> compensate, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => this,
            2 => await compensate((E1)RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

#if NET9_0_OR_GREATER
    /// <summary>Replaces the error with the result of <paramref name="compensate"/>, which can fail again with the same error type.
    /// A success passes through.
    /// <paramref name="compensate"/> returns a value task, which is awaited.</summary>
    /// <param name="compensate">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, E1>> CompensateAsync(Func<E1, ValueTask<Result<T, E1>>> compensate, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => this,
            2 => await compensate((E1)RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
#endif
}

public static partial class ResultExtensions
{
    /// <summary>Replaces the error with the result of <paramref name="compensate"/>, which can fail again with the same error type.
    /// A success passes through.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="compensate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="compensate">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1>> CompensateAsync<T, E1>(this Task<Result<T, E1>> task, Func<E1, Task<Result<T, E1>>> compensate, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => result,
            2 => await compensate((E1)result.RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error with the result of <paramref name="compensate"/>, which can fail again with the same error type.
    /// A success passes through.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="compensate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="compensate">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T, E1>> CompensateAsync<T, E1>(this ValueTask<Result<T, E1>> task, Func<E1, Task<Result<T, E1>>> compensate, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => result,
            2 => await compensate((E1)result.RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

#if NET9_0_OR_GREATER
    /// <summary>Replaces the error with the result of <paramref name="compensate"/>, which can fail again with the same error type.
    /// A success passes through.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="compensate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="compensate">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1>> CompensateAsync<T, E1>(this ValueTask<Result<T, E1>> task, Func<E1, ValueTask<Result<T, E1>>> compensate, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => result,
            2 => await compensate((E1)result.RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
#endif
}
