// Produced by scripts/generate-typed-results.py; do not edit.
using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Core;

namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1>
{
    /// <summary>
    /// Calls <paramref name="func"/> with the success value and returns its return value. An exception of type
    /// <typeparamref name="TException"/> that <paramref name="func"/> throws becomes an error in a new last slot; any other
    /// exception propagates. An error passes through in its slot and <paramref name="func"/> is not called. Give both type
    /// arguments: <typeparamref name="TException"/> cannot be inferred.
    /// </summary>
    /// <typeparam name="TOut">The new success value type.</typeparam>
    /// <typeparam name="TException">The exception type that is caught and added as the last slot.</typeparam>
    /// <param name="func">Called with the success value.</param>
    /// <returns>A result with <typeparamref name="TException"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="func"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<TOut, E1, TException> TryCatch<TOut, TException>(Func<T, TOut> func)
        where TOut : notnull
        where TException : Exception
    {
        if (Tag != TypedResultLogic.SuccessTag)
            return new(TypedResultLogic.ErrorTag(Tag), default!, RawError);

        TOut value;
        try
        {
            value = func(RawValue);
        }
        catch (TException exception)
        {
            return new(3, default!, exception);
        }

        return new(TypedResultLogic.SuccessTag, TypedResultLogic.NotNull(value, nameof(func)), null);
    }
}

public static partial class ResultExtensions
{
    /// <summary>
    /// Calls <paramref name="func"/> with the success value and returns its return value. An exception of type
    /// <typeparamref name="TException"/> that <paramref name="func"/> throws becomes an error in a new last slot; any other
    /// exception propagates. An error passes through in its slot and <paramref name="func"/> is not called. Give both type
    /// arguments: <typeparamref name="TException"/> cannot be inferred.
    /// Awaits <paramref name="task"/>, then calls <c>TryCatch</c>.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TOut">The new success value type.</typeparam>
    /// <typeparam name="TException">The exception type that is caught and added as the last slot.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="func">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A result with <typeparamref name="TException"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="func"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Result<TOut, E1, TException>> TryCatchAsync<T, E1, TOut, TException>(this Task<Result<T, E1>> task, Func<T, TOut> func, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TOut : notnull
        where TException : Exception
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.TryCatch<TOut, TException>(func);
    }

    /// <summary>
    /// Calls <paramref name="func"/> with the success value and returns its return value. An exception of type
    /// <typeparamref name="TException"/> that <paramref name="func"/> throws becomes an error in a new last slot; any other
    /// exception propagates. An error passes through in its slot and <paramref name="func"/> is not called. Give both type
    /// arguments: <typeparamref name="TException"/> cannot be inferred.
    /// Awaits <paramref name="task"/>, then calls <c>TryCatch</c>.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="TOut">The new success value type.</typeparam>
    /// <typeparam name="TException">The exception type that is caught and added as the last slot.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="func">Called with the success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A result with <typeparamref name="TException"/> added as the last slot.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="func"/> returns <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Result<TOut, E1, TException>> TryCatchAsync<T, E1, TOut, TException>(this ValueTask<Result<T, E1>> task, Func<T, TOut> func, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where TOut : notnull
        where TException : Exception
    {
        Result<T, E1> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.TryCatch<TOut, TException>(func);
    }
}
