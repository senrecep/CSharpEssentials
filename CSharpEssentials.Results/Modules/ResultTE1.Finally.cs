// Produced by scripts/generate-typed-results.py; do not edit.
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CSharpEssentials.Core;

namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1>
{
    /// <summary>Calls <paramref name="func"/> with this result, whatever case it holds, and returns its return value. Unlike the other combinators it does not throw on <see langword="default"/>.</summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="func">Called with this result.</param>
    /// <returns>The return value of <paramref name="func"/>.</returns>
    public TOut Finally<TOut>(Func<Result<T, E1>, TOut> func)
        where TOut : notnull
    {
        return func(this);
    }

    /// <summary>Calls <paramref name="action"/> with this result, whatever case it holds. Unlike the other combinators it does not throw on <see langword="default"/>.</summary>
    /// <param name="action">Called with this result.</param>
    /// <returns>This result.</returns>
    public Result<T, E1> Finally(Action<Result<T, E1>> action)
    {
        action(this);

        return this;
    }
}

public static partial class ResultExtensions
{
    /// <summary>Calls <paramref name="func"/> with this result, whatever case it holds, and returns its return value. Unlike the other combinators it does not throw on <see langword="default"/>.
    /// Awaits <paramref name="task"/>, then calls <c>Finally</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="func">Called with this result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>The return value of <paramref name="func"/>.</returns>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<TOut> FinallyAsync<T, E1, TOut>(this Task<Result<T, E1>> task, Func<Result<T, E1>, TOut> func, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TOut : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Finally<TOut>(func);
    }

    /// <summary>Calls <paramref name="func"/> with this result, whatever case it holds, and returns its return value. Unlike the other combinators it does not throw on <see langword="default"/>.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="func"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="func">Called with this result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="func"/>.</param>
    /// <returns>The return value of <paramref name="func"/>.</returns>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<TOut> FinallyAsync<T, E1, TOut>(this Task<Result<T, E1>> task, Func<Result<T, E1>, Task<TOut>> func, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TOut : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await func(result).WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Calls <paramref name="func"/> with this result, whatever case it holds, and returns its return value. Unlike the other combinators it does not throw on <see langword="default"/>.
    /// Awaits <paramref name="task"/>, then calls <c>Finally</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="func">Called with this result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>The return value of <paramref name="func"/>.</returns>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<TOut> FinallyAsync<T, E1, TOut>(this ValueTask<Result<T, E1>> task, Func<Result<T, E1>, TOut> func, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TOut : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Finally<TOut>(func);
    }

    /// <summary>Calls <paramref name="action"/> with this result, whatever case it holds. Unlike the other combinators it does not throw on <see langword="default"/>.
    /// Awaits <paramref name="task"/>, then calls <c>Finally</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with this result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result.</returns>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1>> FinallyAsync<T, E1>(this Task<Result<T, E1>> task, Action<Result<T, E1>> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Finally(action);
    }

    /// <summary>Calls <paramref name="action"/> with this result, whatever case it holds. Unlike the other combinators it does not throw on <see langword="default"/>.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with this result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<T, E1>> FinallyAsync<T, E1>(this Task<Result<T, E1>> task, Func<Result<T, E1>, Task> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        await action(result).WithCancellation(cancellationToken).ConfigureAwait(false);

        return result;
    }

    /// <summary>Calls <paramref name="action"/> with this result, whatever case it holds. Unlike the other combinators it does not throw on <see langword="default"/>.
    /// Awaits <paramref name="task"/>, then calls <c>Finally</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with this result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>This result.</returns>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1>> FinallyAsync<T, E1>(this ValueTask<Result<T, E1>> task, Action<Result<T, E1>> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.Finally(action);
    }

    /// <summary>Calls <paramref name="action"/> with this result, whatever case it holds. Unlike the other combinators it does not throw on <see langword="default"/>.
    /// Awaits <paramref name="task"/>, then awaits the task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with this result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    [OverloadResolutionPriority(1)]
    public static async ValueTask<Result<T, E1>> FinallyAsync<T, E1>(this ValueTask<Result<T, E1>> task, Func<Result<T, E1>, Task> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        await action(result).WithCancellation(cancellationToken).ConfigureAwait(false);

        return result;
    }

#if NET9_0_OR_GREATER
    /// <summary>Calls <paramref name="func"/> with this result, whatever case it holds, and returns its return value. Unlike the other combinators it does not throw on <see langword="default"/>.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="func"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="func">Called with this result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="func"/>.</param>
    /// <returns>The return value of <paramref name="func"/>.</returns>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<TOut> FinallyAsync<T, E1, TOut>(this ValueTask<Result<T, E1>> task, Func<Result<T, E1>, ValueTask<TOut>> func, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TOut : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return await func(result).WithCancellation(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Calls <paramref name="action"/> with this result, whatever case it holds. Unlike the other combinators it does not throw on <see langword="default"/>.
    /// Awaits <paramref name="task"/>, then awaits the value task that <paramref name="action"/> returns.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="action">Called with this result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/> and <paramref name="action"/>.</param>
    /// <returns>This result.</returns>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<T, E1>> FinallyAsync<T, E1>(this ValueTask<Result<T, E1>> task, Func<Result<T, E1>, ValueTask> action, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        await action(result).WithCancellation(cancellationToken).ConfigureAwait(false);

        return result;
    }
#endif
}
