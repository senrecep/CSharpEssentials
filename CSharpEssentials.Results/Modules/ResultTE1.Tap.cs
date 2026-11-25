// Produced by scripts/generate-typed-results.py; do not edit.
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CSharpEssentials.Core;

namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1>
{
    /// <summary>Calls <paramref name="action"/> with the success value. An error passes through and <paramref name="action"/> is not called.</summary>
    /// <param name="action">Called with the success value.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1> Tap(Action<T> action)
    {
        TypedResultLogic.EnsureInitialized(Tag);
        if (Tag == TypedResultLogic.SuccessTag)
            action(RawValue);

        return this;
    }

    /// <summary>Calls <paramref name="action"/> with the success value when <paramref name="predicate"/> holds. An error passes
    /// through and neither delegate is called.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="action">Called with the success value when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1> TapIf(Func<T, bool> predicate, Action<T> action)
    {
        TypedResultLogic.EnsureInitialized(Tag);
        if (Tag == TypedResultLogic.SuccessTag && predicate(RawValue))
            action(RawValue);

        return this;
    }

    /// <summary>Calls <paramref name="action"/> with the success value. An error passes through and <paramref name="action"/> is not called.
    /// <paramref name="action"/> returns a task, which is awaited.</summary>
    /// <param name="action">Called with the success value.</param>
    /// <param name="cancellationToken">Checked before <paramref name="action"/> is called. Observed while awaiting <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, E1>> TapAsync(Func<T, Task> action, CancellationToken cancellationToken = default)
    {
        if (Tag == TypedResultLogic.SuccessTag)
            cancellationToken.ThrowIfCancellationRequested();

        TypedResultLogic.EnsureInitialized(Tag);
        if (Tag == TypedResultLogic.SuccessTag)
            await action(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false);

        return this;
    }

    /// <summary>Calls <paramref name="action"/> with the success value when <paramref name="predicate"/> holds. An error passes
    /// through and neither delegate is called.
    /// <paramref name="action"/> returns a task, which is awaited.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="action">Called with the success value when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, E1>> TapIfAsync(Func<T, bool> predicate, Func<T, Task> action, CancellationToken cancellationToken = default)
    {
        TypedResultLogic.EnsureInitialized(Tag);
        if (Tag == TypedResultLogic.SuccessTag && predicate(RawValue))
            await action(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false);

        return this;
    }

#if NET9_0_OR_GREATER
    /// <summary>Calls <paramref name="action"/> with the success value. An error passes through and <paramref name="action"/> is not called.
    /// <paramref name="action"/> returns a value task, which is awaited.</summary>
    /// <param name="action">Called with the success value.</param>
    /// <param name="cancellationToken">Checked before <paramref name="action"/> is called. Observed while awaiting <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, E1>> TapAsync(Func<T, ValueTask> action, CancellationToken cancellationToken = default)
    {
        if (Tag == TypedResultLogic.SuccessTag)
            cancellationToken.ThrowIfCancellationRequested();

        TypedResultLogic.EnsureInitialized(Tag);
        if (Tag == TypedResultLogic.SuccessTag)
            await action(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false);

        return this;
    }

    /// <summary>Calls <paramref name="action"/> with the success value when <paramref name="predicate"/> holds. An error passes
    /// through and neither delegate is called.
    /// <paramref name="action"/> returns a value task, which is awaited.</summary>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="action">Called with the success value when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, E1>> TapIfAsync(Func<T, bool> predicate, Func<T, ValueTask> action, CancellationToken cancellationToken = default)
    {
        TypedResultLogic.EnsureInitialized(Tag);
        if (Tag == TypedResultLogic.SuccessTag && predicate(RawValue))
            await action(RawValue).WithCancellation(cancellationToken).ConfigureAwait(false);

        return this;
    }
#endif
}

public static partial class ResultExtensions
{
    /// <summary>Calls <paramref name="action"/> with the success value. An error passes through and <paramref name="action"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>Tap</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1>> TapAsync<T, E1>(this Task<Result<T, E1>> task, Action<T> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tap(action);
    }

    /// <summary>Calls <paramref name="action"/> with the success value. An error passes through and <paramref name="action"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with the success value.</param>
    /// <param name="cancellationToken">Checked before <paramref name="action"/> is called. Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1>> TapAsync<T, E1>(this Task<Result<T, E1>> task, Func<T, Task> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Calls <paramref name="action"/> with the success value. An error passes through and <paramref name="action"/> is not called.
    /// Awaits <paramref name="task"/>, then calls <c>Tap</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1>> TapAsync<T, E1>(this ValueTask<Result<T, E1>> task, Action<T> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Tap(action);
    }

    /// <summary>Calls <paramref name="action"/> with the success value. An error passes through and <paramref name="action"/> is not called.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with the success value.</param>
    /// <param name="cancellationToken">Checked before <paramref name="action"/> is called. Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1>> TapAsync<T, E1>(this ValueTask<Result<T, E1>> task, Func<T, ValueTask> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        if (result.Tag == TypedResultLogic.SuccessTag)
            cancellationToken.ThrowIfCancellationRequested();

        TypedResultLogic.EnsureInitialized(result.Tag);
        if (result.Tag == TypedResultLogic.SuccessTag)
            await action(result.RawValue).WithCancellation(cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>Calls <paramref name="action"/> with the success value when <paramref name="predicate"/> holds. An error passes
    /// through and neither delegate is called.
    /// Awaits <paramref name="task"/>, then calls <c>TapIf</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="action">Called with the success value when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1>> TapIfAsync<T, E1>(this Task<Result<T, E1>> task, Func<T, bool> predicate, Action<T> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.TapIf(predicate, action);
    }

    /// <summary>Calls <paramref name="action"/> with the success value when <paramref name="predicate"/> holds. An error passes
    /// through and neither delegate is called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="action">Called with the success value when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1>> TapIfAsync<T, E1>(this Task<Result<T, E1>> task, Func<T, bool> predicate, Func<T, Task> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapIfAsync(predicate, action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Calls <paramref name="action"/> with the success value when <paramref name="predicate"/> holds. An error passes
    /// through and neither delegate is called.
    /// Awaits <paramref name="task"/>, then calls <c>TapIf</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="action">Called with the success value when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1>> TapIfAsync<T, E1>(this ValueTask<Result<T, E1>> task, Func<T, bool> predicate, Action<T> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.TapIf(predicate, action);
    }

    /// <summary>Calls <paramref name="action"/> with the success value when <paramref name="predicate"/> holds. An error passes
    /// through and neither delegate is called.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="action">Called with the success value when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T, E1>> TapIfAsync<T, E1>(this ValueTask<Result<T, E1>> task, Func<T, bool> predicate, Func<T, Task> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapIfAsync(predicate, action, cancellationToken).ConfigureAwait(false);
    }

#if NET9_0_OR_GREATER
    /// <summary>Calls <paramref name="action"/> with the success value when <paramref name="predicate"/> holds. An error passes
    /// through and neither delegate is called.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="predicate">Called with the success value.</param>
    /// <param name="action">Called with the success value when <paramref name="predicate"/> returns <see langword="true"/>.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1>> TapIfAsync<T, E1>(this ValueTask<Result<T, E1>> task, Func<T, bool> predicate, Func<T, ValueTask> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapIfAsync(predicate, action, cancellationToken).ConfigureAwait(false);
    }
#endif
}
