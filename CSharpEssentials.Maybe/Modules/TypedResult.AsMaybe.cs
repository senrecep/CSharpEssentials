using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Core;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Maybe;

public static partial class MaybeExtensions
{
    /// <summary>Returns the success value as a Maybe, or <see cref="Maybe{T}.None"/> when the result holds an error.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The error type.</typeparam>
    /// <param name="result">The result.</param>
    /// <returns>A Maybe that holds the success value, or none.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static Maybe<T> AsMaybe<T, E1>(this Result<T, E1> result)
        where T : notnull
        where E1 : notnull =>
        result.Match(static value => (Maybe<T>)value, static _ => Maybe<T>.None);

    /// <summary>Returns the success value as a Maybe, or <see cref="Maybe{T}.None"/> when the result holds an error.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <param name="result">The result.</param>
    /// <returns>A Maybe that holds the success value, or none.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static Maybe<T> AsMaybe<T, E1, E2>(this Result<T, E1, E2> result)
        where T : notnull
        where E1 : notnull
        where E2 : notnull =>
        result.Match(static value => (Maybe<T>)value, static _ => Maybe<T>.None, static _ => Maybe<T>.None);

    /// <summary>Returns the success value as a Maybe, or <see cref="Maybe{T}.None"/> when the result holds an error.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="result">The result.</param>
    /// <returns>A Maybe that holds the success value, or none.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static Maybe<T> AsMaybe<T, E1, E2, E3>(this Result<T, E1, E2, E3> result)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull =>
        result.Match(static value => (Maybe<T>)value, static _ => Maybe<T>.None, static _ => Maybe<T>.None, static _ => Maybe<T>.None);

    /// <summary>Awaits <paramref name="task"/>, then calls <c>AsMaybe</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A Maybe that holds the success value, or none.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Maybe<T>> AsMaybeAsync<T, E1>(this Task<Result<T, E1>> task, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull =>
        (await task.WithCancellation(cancellationToken).ConfigureAwait(false)).AsMaybe();

    /// <summary>Awaits <paramref name="task"/>, then calls <c>AsMaybe</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A Maybe that holds the success value, or none.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Maybe<T>> AsMaybeAsync<T, E1>(this ValueTask<Result<T, E1>> task, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull =>
        (await task.WithCancellation(cancellationToken).ConfigureAwait(false)).AsMaybe();

    /// <summary>Awaits <paramref name="task"/>, then calls <c>AsMaybe</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A Maybe that holds the success value, or none.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Maybe<T>> AsMaybeAsync<T, E1, E2>(this Task<Result<T, E1, E2>> task, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull =>
        (await task.WithCancellation(cancellationToken).ConfigureAwait(false)).AsMaybe();

    /// <summary>Awaits <paramref name="task"/>, then calls <c>AsMaybe</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A Maybe that holds the success value, or none.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Maybe<T>> AsMaybeAsync<T, E1, E2>(this ValueTask<Result<T, E1, E2>> task, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull =>
        (await task.WithCancellation(cancellationToken).ConfigureAwait(false)).AsMaybe();

    /// <summary>Awaits <paramref name="task"/>, then calls <c>AsMaybe</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A Maybe that holds the success value, or none.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async Task<Maybe<T>> AsMaybeAsync<T, E1, E2, E3>(this Task<Result<T, E1, E2, E3>> task, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull =>
        (await task.WithCancellation(cancellationToken).ConfigureAwait(false)).AsMaybe();

    /// <summary>Awaits <paramref name="task"/>, then calls <c>AsMaybe</c>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="E1">The first error type.</typeparam>
    /// <typeparam name="E2">The second error type.</typeparam>
    /// <typeparam name="E3">The third error type.</typeparam>
    /// <param name="task">The pending result.</param>
    /// <param name="cancellationToken">Observed while awaiting <paramref name="task"/>.</param>
    /// <returns>A Maybe that holds the success value, or none.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    [SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the Result<T, E1, E2> shape.")]
    public static async ValueTask<Maybe<T>> AsMaybeAsync<T, E1, E2, E3>(this ValueTask<Result<T, E1, E2, E3>> task, CancellationToken cancellationToken = default)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull =>
        (await task.WithCancellation(cancellationToken).ConfigureAwait(false)).AsMaybe();

    /// <summary>
    /// Converts the Maybe to a typed result: its value, or <paramref name="error"/> when it has none. An <see cref="Errors.Error"/>
    /// argument keeps binding the classic <c>ToResult(Error)</c> overload, which returns <see cref="Result{TValue}"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="maybe">The Maybe.</param>
    /// <param name="error">The error to return when the Maybe has no value.</param>
    /// <returns>A result that holds the value or <paramref name="error"/>.</returns>
    /// <exception cref="ArgumentNullException">The Maybe has no value and <paramref name="error"/> is <see langword="null"/>.</exception>
    public static Result<T, TError> ToResult<T, TError>(this Maybe<T> maybe, TError error)
        where T : notnull
        where TError : notnull =>
        maybe.HasValue ? new Result<T, TError>(maybe.Value) : new Result<T, TError>(error);
}
