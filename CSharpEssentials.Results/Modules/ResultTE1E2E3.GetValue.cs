// Produced by scripts/generate-typed-results.py; do not edit.
using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Core;
using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial struct Result<T, E1, E2, E3>
{
    /// <summary>Returns the success value, or <see langword="default"/> of <typeparamref name="T"/> when the result holds an
    /// error or is <see langword="default"/>. It never throws.</summary>
    /// <returns>The success value or <see langword="default"/>.</returns>
    public T? GetValueOrDefault()
    {
        return Tag == TypedResultLogic.SuccessTag ? RawValue : default;
    }

    /// <summary>Returns the success value, or <paramref name="fallback"/> when the result holds an error or is
    /// <see langword="default"/>. It never throws.</summary>
    /// <param name="fallback">The value to return when the result holds no success value.</param>
    /// <returns>The success value or <paramref name="fallback"/>.</returns>
    public T GetValueOr(T fallback)
    {
        return Tag == TypedResultLogic.SuccessTag ? RawValue : fallback;
    }

    /// <summary>Returns the success value, or throws when the result holds an error.</summary>
    /// <param name="errorMessage">The message of the exception; a default message when <see langword="null"/>.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="InvalidOperationException">The result holds an error or is <see langword="default"/>.</exception>
    public T GetValueOrThrow(string? errorMessage = null)
    {
        return Tag switch
        {
            TypedResultLogic.SuccessTag => RawValue,
            TypedResultLogic.DefaultTag => throw TypedResultLogic.DefaultInitialized(),
            _ => throw new InvalidOperationException(errorMessage ?? "Result has no value."),
        };
    }
}

public static partial class ResultExtensions
{
    /// <summary>Awaits <paramref name="task"/>, then calls <c>GetValueOrDefault</c>. It throws only when the token is canceled.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>The success value or the fallback.</returns>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<T?> GetValueOrDefaultAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.GetValueOrDefault();
    }

    /// <summary>Awaits <paramref name="task"/>, then calls <c>GetValueOr</c>. It throws only when the token is canceled.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="fallback">The value to return when the result holds no success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>The success value or the fallback.</returns>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<T> GetValueOrAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, T fallback, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.GetValueOr(fallback);
    }

    /// <summary>Awaits <paramref name="task"/>, then calls <c>GetValueOrDefault</c>. It throws only when the token is canceled.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>The success value or the fallback.</returns>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<T?> GetValueOrDefaultAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.GetValueOrDefault();
    }

    /// <summary>Awaits <paramref name="task"/>, then calls <c>GetValueOr</c>. It throws only when the token is canceled.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="fallback">The value to return when the result holds no success value.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>The success value or the fallback.</returns>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<T> GetValueOrAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, T fallback, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        return result.GetValueOr(fallback);
    }

    /// <summary>Returns the success value, or the return value of <paramref name="fallback"/> called with the error converted to
    /// <see cref="Error"/>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="result">The result.</param>
    /// <param name="fallback">Called with the error.</param>
    /// <remarks>It exists only when every error type implements <see cref="IError"/>; the error is converted as by <c>ToResult</c>.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="fallback"/> returns <see langword="null"/>.</exception>
    /// <returns>The success value or the return value of <paramref name="fallback"/>.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static T GetValueOr<T, E1, E2, E3>(this Result<T, E1, E2, E3> result, Func<Error, T> fallback)
        where T : notnull
        where E1 : notnull, IError
        where E2 : notnull, IError
        where E3 : notnull, IError
    {
        TypedResultLogic.EnsureInitialized(result.Tag);
        return result.Tag == TypedResultLogic.SuccessTag ? result.RawValue : TypedResultLogic.NotNull(fallback(result.CurrentError()), nameof(fallback));
    }

    /// <summary>Returns the success value, or the return value of <paramref name="fallback"/> called with the error converted to
    /// <see cref="Error"/>. Awaits <paramref name="task"/>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="fallback">Called with the error.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <remarks>It exists only when every error type implements <see cref="IError"/>; the error is converted as by <c>ToResult</c>.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="fallback"/> returns <see langword="null"/>.</exception>
    /// <returns>The success value or the return value of <paramref name="fallback"/>.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<T> GetValueOrAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, Func<Error, T> fallback, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull, IError
        where E2 : notnull, IError
        where E3 : notnull, IError
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        TypedResultLogic.EnsureInitialized(result.Tag);
        return result.Tag == TypedResultLogic.SuccessTag ? result.RawValue : TypedResultLogic.NotNull(fallback(result.CurrentError()), nameof(fallback));
    }

    /// <summary>Returns the success value, or the return value of <paramref name="fallback"/> called with the error converted to
    /// <see cref="Error"/>. Awaits <paramref name="task"/>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="fallback">Called with the error.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <remarks>It exists only when every error type implements <see cref="IError"/>; the error is converted as by <c>ToResult</c>.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="fallback"/> returns <see langword="null"/>.</exception>
    /// <returns>The success value or the return value of <paramref name="fallback"/>.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<T> GetValueOrAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, Func<Error, T> fallback, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull, IError
        where E2 : notnull, IError
        where E3 : notnull, IError
    {
        Result<T, E1, E2, E3> result = await task.WithCancellation(cancellationToken).ConfigureAwait(false);
        TypedResultLogic.EnsureInitialized(result.Tag);
        return result.Tag == TypedResultLogic.SuccessTag ? result.RawValue : TypedResultLogic.NotNull(fallback(result.CurrentError()), nameof(fallback));
    }
}
