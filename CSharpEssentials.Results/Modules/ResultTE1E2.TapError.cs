// Produced by scripts/generate-typed-results.py; do not edit.
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1, E2>
{
    /// <summary>Calls <paramref name="action"/> with the error in the <typeparamref name="E1"/> slot. Every other case passes through.</summary>
    /// <param name="action">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E2> TapError(Action<E1> action)
    {
        TypedResultLogic.EnsureInitialized(Tag);
        if (Tag == 2)
            action((E1)RawError!);

        return this;
    }

    /// <summary>Calls <paramref name="action"/> with the error in the <typeparamref name="E2"/> slot. Every other case passes through.</summary>
    /// <param name="action">Called with the error of type <typeparamref name="E2"/>. Give its parameter type to choose the slot.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T, E1, E2> TapError(Action<E2> action)
    {
        TypedResultLogic.EnsureInitialized(Tag);
        if (Tag == 3)
            action((E2)RawError!);

        return this;
    }

    /// <summary>Calls <paramref name="action"/> with the error in the <typeparamref name="E1"/> slot. Every other case passes through.
    /// <paramref name="action"/> returns a task, which is awaited.</summary>
    /// <param name="action">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, E1, E2>> TapErrorAsync(Func<E1, Task> action, CancellationToken cancellationToken = default)
    {
        TypedResultLogic.EnsureInitialized(Tag);
        if (Tag == 2)
            await action((E1)RawError!).WithCancellation(cancellationToken).ConfigureAwait(false);

        return this;
    }

    /// <summary>Calls <paramref name="action"/> with the error in the <typeparamref name="E2"/> slot. Every other case passes through.
    /// <paramref name="action"/> returns a task, which is awaited.</summary>
    /// <param name="action">Called with the error of type <typeparamref name="E2"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [OverloadResolutionPriority(1)]
    public async Task<Result<T, E1, E2>> TapErrorAsync(Func<E2, Task> action, CancellationToken cancellationToken = default)
    {
        TypedResultLogic.EnsureInitialized(Tag);
        if (Tag == 3)
            await action((E2)RawError!).WithCancellation(cancellationToken).ConfigureAwait(false);

        return this;
    }

#if NET9_0_OR_GREATER
    /// <summary>Calls <paramref name="action"/> with the error in the <typeparamref name="E1"/> slot. Every other case passes through.
    /// <paramref name="action"/> returns a value task, which is awaited.</summary>
    /// <param name="action">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, E1, E2>> TapErrorAsync(Func<E1, ValueTask> action, CancellationToken cancellationToken = default)
    {
        TypedResultLogic.EnsureInitialized(Tag);
        if (Tag == 2)
            await action((E1)RawError!).WithCancellation(cancellationToken).ConfigureAwait(false);

        return this;
    }

    /// <summary>Calls <paramref name="action"/> with the error in the <typeparamref name="E2"/> slot. Every other case passes through.
    /// <paramref name="action"/> returns a value task, which is awaited.</summary>
    /// <param name="action">Called with the error of type <typeparamref name="E2"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public async ValueTask<Result<T, E1, E2>> TapErrorAsync(Func<E2, ValueTask> action, CancellationToken cancellationToken = default)
    {
        TypedResultLogic.EnsureInitialized(Tag);
        if (Tag == 3)
            await action((E2)RawError!).WithCancellation(cancellationToken).ConfigureAwait(false);

        return this;
    }
#endif
}

public static partial class ResultExtensions
{
    /// <summary>Calls <paramref name="action"/> with the error in the <typeparamref name="E1"/> slot. Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(2)]
    public static async Task<Result<T, E1, E2>> TapErrorAsync<T, E1, E2>(this Task<Result<T, E1, E2>> task, Func<E1, Task> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapErrorAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Calls <paramref name="action"/> with the error in the <typeparamref name="E1"/> slot. Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(3)]
    public static async ValueTask<Result<T, E1, E2>> TapErrorAsync<T, E1, E2>(this ValueTask<Result<T, E1, E2>> task, Func<E1, Task> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapErrorAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Calls <paramref name="action"/> with the error in the <typeparamref name="E2"/> slot. Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with the error of type <typeparamref name="E2"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(2)]
    public static async Task<Result<T, E1, E2>> TapErrorAsync<T, E1, E2>(this Task<Result<T, E1, E2>> task, Func<E2, Task> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapErrorAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Calls <paramref name="action"/> with the error in the <typeparamref name="E2"/> slot. Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with the error of type <typeparamref name="E2"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(3)]
    public static async ValueTask<Result<T, E1, E2>> TapErrorAsync<T, E1, E2>(this ValueTask<Result<T, E1, E2>> task, Func<E2, Task> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapErrorAsync(action, cancellationToken).ConfigureAwait(false);
    }

#if NET9_0_OR_GREATER
    /// <summary>Calls <paramref name="action"/> with the error in the <typeparamref name="E1"/> slot. Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with the error of type <typeparamref name="E1"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(2)]
    public static async ValueTask<Result<T, E1, E2>> TapErrorAsync<T, E1, E2>(this ValueTask<Result<T, E1, E2>> task, Func<E1, ValueTask> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapErrorAsync(action, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Calls <paramref name="action"/> with the error in the <typeparamref name="E2"/> slot. Every other case passes through.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with the error of type <typeparamref name="E2"/>. Give its parameter type to choose the slot.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(2)]
    public static async ValueTask<Result<T, E1, E2>> TapErrorAsync<T, E1, E2>(this ValueTask<Result<T, E1, E2>> task, Func<E2, ValueTask> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await result.TapErrorAsync(action, cancellationToken).ConfigureAwait(false);
    }
#endif

    /// <summary>Calls <paramref name="action"/> with the error converted to <see cref="Error"/>, whatever slot holds it. A success
    /// passes through.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="action">Called with the error.</param>
    /// <remarks>It exists only when every error type implements <see cref="IError"/>; the error is converted as by <c>ToResult</c>. Give the lambda parameter type <see cref="Error"/> on a result whose slots are typed errors.</remarks>
    /// <returns>The result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static Result<T, E1, E2> TapError<T, E1, E2>(this Result<T, E1, E2> result, Action<Error> action)
        where T : notnull
        where E1 : notnull, IError
        where E2 : notnull, IError
    {
        TypedResultLogic.EnsureInitialized(result.Tag);
        if (result.Tag != TypedResultLogic.SuccessTag)
            action(result.CurrentError());

        return result;
    }

    /// <summary>Calls <paramref name="action"/> with the error converted to <see cref="Error"/>, whatever slot holds it. A success
    /// passes through. <paramref name="action"/> returns a task, which is awaited.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="action">Called with the error.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="action"/>.</param>
    /// <remarks>It exists only when every error type implements <see cref="IError"/>; the error is converted as by <c>ToResult</c>. Give the lambda parameter type <see cref="Error"/> on a result whose slots are typed errors.</remarks>
    /// <returns>The result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async Task<Result<T, E1, E2>> TapErrorAsync<T, E1, E2>(this Result<T, E1, E2> result, Func<Error, Task> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull, IError
        where E2 : notnull, IError
    {
        TypedResultLogic.EnsureInitialized(result.Tag);
        if (result.Tag != TypedResultLogic.SuccessTag)
            await action(result.CurrentError()).WithCancellation(cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>Calls <paramref name="action"/> with the error converted to <see cref="Error"/>, whatever slot holds it. A success
    /// passes through. Awaits <paramref name="task"/>, then awaits the task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with the error.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <remarks>It exists only when every error type implements <see cref="IError"/>; the error is converted as by <c>ToResult</c>. Give the lambda parameter type <see cref="Error"/> on a result whose slots are typed errors.</remarks>
    /// <returns>The result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1, E2>> TapErrorAsync<T, E1, E2>(this Task<Result<T, E1, E2>> task, Func<Error, Task> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull, IError
        where E2 : notnull, IError
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        TypedResultLogic.EnsureInitialized(result.Tag);
        if (result.Tag != TypedResultLogic.SuccessTag)
            await action(result.CurrentError()).WithCancellation(cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>Calls <paramref name="action"/> with the error converted to <see cref="Error"/>, whatever slot holds it. A success
    /// passes through. Awaits <paramref name="task"/>, then awaits the task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with the error.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <remarks>It exists only when every error type implements <see cref="IError"/>; the error is converted as by <c>ToResult</c>. Give the lambda parameter type <see cref="Error"/> on a result whose slots are typed errors.</remarks>
    /// <returns>The result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T, E1, E2>> TapErrorAsync<T, E1, E2>(this ValueTask<Result<T, E1, E2>> task, Func<Error, Task> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull, IError
        where E2 : notnull, IError
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        TypedResultLogic.EnsureInitialized(result.Tag);
        if (result.Tag != TypedResultLogic.SuccessTag)
            await action(result.CurrentError()).WithCancellation(cancellationToken).ConfigureAwait(false);

        return result;
    }

#if NET9_0_OR_GREATER
    /// <summary>Calls <paramref name="action"/> with the error converted to <see cref="Error"/>, whatever slot holds it. A success
    /// passes through. <paramref name="action"/> returns a value task, which is awaited.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="action">Called with the error.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="action"/>.</param>
    /// <remarks>It exists only when every error type implements <see cref="IError"/>; the error is converted as by <c>ToResult</c>. Give the lambda parameter type <see cref="Error"/> on a result whose slots are typed errors.</remarks>
    /// <returns>The result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, E2>> TapErrorAsync<T, E1, E2>(this Result<T, E1, E2> result, Func<Error, ValueTask> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull, IError
        where E2 : notnull, IError
    {
        TypedResultLogic.EnsureInitialized(result.Tag);
        if (result.Tag != TypedResultLogic.SuccessTag)
            await action(result.CurrentError()).WithCancellation(cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>Calls <paramref name="action"/> with the error converted to <see cref="Error"/>, whatever slot holds it. A success
    /// passes through. Awaits <paramref name="task"/>, then awaits the task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with the error.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <remarks>It exists only when every error type implements <see cref="IError"/>; the error is converted as by <c>ToResult</c>. Give the lambda parameter type <see cref="Error"/> on a result whose slots are typed errors.</remarks>
    /// <returns>The result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1, E2>> TapErrorAsync<T, E1, E2>(this ValueTask<Result<T, E1, E2>> task, Func<Error, ValueTask> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull, IError
        where E2 : notnull, IError
    {
        Result<T, E1, E2> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        TypedResultLogic.EnsureInitialized(result.Tag);
        if (result.Tag != TypedResultLogic.SuccessTag)
            await action(result.CurrentError()).WithCancellation(cancellationToken).ConfigureAwait(false);

        return result;
    }
#endif
}
