// Produced by scripts/generate-typed-results.py; do not edit.
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CSharpEssentials.Core;

namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1, E2, E3>
{
    /// <summary>Replaces the error in the <typeparamref name="E1"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.</summary>
    /// <param name="compensate">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E2, E3> Compensate(Func<E1, Result<T, E2, E3>> compensate)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => compensate((E1)RawError!),
            3 => new(2, default!, RawError),
            4 => new(3, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E2"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.</summary>
    /// <param name="compensate">Called with the error of type <typeparamref name="E2"/>. Give its parameter type to choose the slot.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E3> Compensate(Func<E2, Result<T, E1, E3>> compensate)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => new(2, default!, RawError),
            3 => compensate((E2)RawError!),
            4 => new(3, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E3"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.</summary>
    /// <param name="compensate">Called with the error of type <typeparamref name="E3"/>. Give its parameter type to choose the slot.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E2> Compensate(Func<E3, Result<T, E1, E2>> compensate)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => new(2, default!, RawError),
            3 => new(3, default!, RawError),
            4 => compensate((E3)RawError!),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E1"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.
    /// <paramref name="compensate"/> returns a task, which is awaited.</summary>
    /// <param name="compensate">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, E2, E3>> CompensateAsync(Func<E1, Task<Result<T, E2, E3>>> compensate, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => await compensate((E1)RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            3 => new(2, default!, RawError),
            4 => new(3, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E2"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.
    /// <paramref name="compensate"/> returns a task, which is awaited.</summary>
    /// <param name="compensate">Called with the error of type <typeparamref name="E2"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, E1, E3>> CompensateAsync(Func<E2, Task<Result<T, E1, E3>>> compensate, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => new(2, default!, RawError),
            3 => await compensate((E2)RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            4 => new(3, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E3"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.
    /// <paramref name="compensate"/> returns a task, which is awaited.</summary>
    /// <param name="compensate">Called with the error of type <typeparamref name="E3"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, E1, E2>> CompensateAsync(Func<E3, Task<Result<T, E1, E2>>> compensate, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => new(2, default!, RawError),
            3 => new(3, default!, RawError),
            4 => await compensate((E3)RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

#if NET9_0_OR_GREATER
    /// <summary>Replaces the error in the <typeparamref name="E1"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.
    /// <paramref name="compensate"/> returns a value task, which is awaited.</summary>
    /// <param name="compensate">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, E2, E3>> CompensateAsync(Func<E1, ValueTask<Result<T, E2, E3>>> compensate, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => await compensate((E1)RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            3 => new(2, default!, RawError),
            4 => new(3, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E2"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.
    /// <paramref name="compensate"/> returns a value task, which is awaited.</summary>
    /// <param name="compensate">Called with the error of type <typeparamref name="E2"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, E1, E3>> CompensateAsync(Func<E2, ValueTask<Result<T, E1, E3>>> compensate, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => new(2, default!, RawError),
            3 => await compensate((E2)RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            4 => new(3, default!, RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E3"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.
    /// <paramref name="compensate"/> returns a value task, which is awaited.</summary>
    /// <param name="compensate">Called with the error of type <typeparamref name="E3"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, E1, E2>> CompensateAsync(Func<E3, ValueTask<Result<T, E1, E2>>> compensate, CancellationToken cancellationToken = default)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => new(Tag, RawValue, null),
            2 => new(2, default!, RawError),
            3 => new(3, default!, RawError),
            4 => await compensate((E3)RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
#endif
}

public static partial class ResultExtensions
{
    /// <summary>Replaces the error in the <typeparamref name="E1"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="compensate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="compensate">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E2, E3>> CompensateAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, Func<E1, Task<Result<T, E2, E3>>> compensate, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => new(result.Tag, result.RawValue, null),
            2 => await compensate((E1)result.RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            3 => new(2, default!, result.RawError),
            4 => new(3, default!, result.RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E1"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="compensate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="compensate">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T, E2, E3>> CompensateAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<E1, Task<Result<T, E2, E3>>> compensate, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => new(result.Tag, result.RawValue, null),
            2 => await compensate((E1)result.RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            3 => new(2, default!, result.RawError),
            4 => new(3, default!, result.RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E2"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="compensate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="compensate">Called with the error of type <typeparamref name="E2"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, E3>> CompensateAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, Func<E2, Task<Result<T, E1, E3>>> compensate, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => new(result.Tag, result.RawValue, null),
            2 => new(2, default!, result.RawError),
            3 => await compensate((E2)result.RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            4 => new(3, default!, result.RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E2"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="compensate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="compensate">Called with the error of type <typeparamref name="E2"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T, E1, E3>> CompensateAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<E2, Task<Result<T, E1, E3>>> compensate, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => new(result.Tag, result.RawValue, null),
            2 => new(2, default!, result.RawError),
            3 => await compensate((E2)result.RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            4 => new(3, default!, result.RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E3"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="compensate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="compensate">Called with the error of type <typeparamref name="E3"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, E2>> CompensateAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, Func<E3, Task<Result<T, E1, E2>>> compensate, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => new(result.Tag, result.RawValue, null),
            2 => new(2, default!, result.RawError),
            3 => new(3, default!, result.RawError),
            4 => await compensate((E3)result.RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E3"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="compensate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="compensate">Called with the error of type <typeparamref name="E3"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T, E1, E2>> CompensateAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<E3, Task<Result<T, E1, E2>>> compensate, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => new(result.Tag, result.RawValue, null),
            2 => new(2, default!, result.RawError),
            3 => new(3, default!, result.RawError),
            4 => await compensate((E3)result.RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

#if NET9_0_OR_GREATER
    /// <summary>Replaces the error in the <typeparamref name="E1"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="compensate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="compensate">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E2, E3>> CompensateAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<E1, ValueTask<Result<T, E2, E3>>> compensate, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => new(result.Tag, result.RawValue, null),
            2 => await compensate((E1)result.RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            3 => new(2, default!, result.RawError),
            4 => new(3, default!, result.RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E2"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="compensate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="compensate">Called with the error of type <typeparamref name="E2"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, E3>> CompensateAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<E2, ValueTask<Result<T, E1, E3>>> compensate, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => new(result.Tag, result.RawValue, null),
            2 => new(2, default!, result.RawError),
            3 => await compensate((E2)result.RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            4 => new(3, default!, result.RawError),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }

    /// <summary>Replaces the error in the <typeparamref name="E3"/> slot with the result of <paramref name="compensate"/> and removes the slot.
    /// Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="compensate"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="compensate">Called with the error of type <typeparamref name="E3"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="compensate"/>.</param>
    /// <returns>The result of <paramref name="compensate"/>, or the same case.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, E2>> CompensateAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<E3, ValueTask<Result<T, E1, E2>>> compensate, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tag switch
        {
            TypedResultLogic.SuccessTag => new(result.Tag, result.RawValue, null),
            2 => new(2, default!, result.RawError),
            3 => new(3, default!, result.RawError),
            4 => await compensate((E3)result.RawError!).WithCancellation(cancellationToken).ConfigureAwait(false),
            _ => throw TypedResultLogic.DefaultInitialized(),
        };
    }
#endif
}
