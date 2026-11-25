using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern.Interfaces;

namespace CSharpEssentials.ResultPattern.Testing;

/// <summary>
/// Assertions for the typed <c>Result&lt;T, E1&gt;</c>, <c>Result&lt;T, E1, E2&gt;</c> and <c>Result&lt;T, E1, E2, E3&gt;</c>.
/// A failed assertion throws <see cref="ResultAssertionException"/> whose message shows the case the result holds.
/// The <c>ShouldHaveError</c> helpers of <see cref="ResultAssertionExtensions"/> work on typed results too.
/// </summary>
[SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the documented Result<T, E1, E2> shape.")]
public static class TypedResultAssertionExtensions
{
    extension<T, E1>(Result<T, E1> result)
        where T : notnull
        where E1 : notnull
    {
        /// <summary>Asserts that the result holds the success value and returns it.</summary>
        /// <returns>The success value.</returns>
        /// <exception cref="ResultAssertionException">The result holds an error or is <see langword="default"/>.</exception>
        public T ShouldBeSuccess() =>
            result.TryGetValue(out T value) ? value : throw NotSuccess(result.HasValue, result.Value);
    }

    extension<T, E1, E2>(Result<T, E1, E2> result)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
    {
        /// <summary>Asserts that the result holds the success value and returns it.</summary>
        /// <returns>The success value.</returns>
        /// <exception cref="ResultAssertionException">The result holds an error or is <see langword="default"/>.</exception>
        public T ShouldBeSuccess() =>
            result.TryGetValue(out T value) ? value : throw NotSuccess(result.HasValue, result.Value);
    }

    extension<T, E1, E2, E3>(Result<T, E1, E2, E3> result)
        where T : notnull
        where E1 : notnull
        where E2 : notnull
        where E3 : notnull
    {
        /// <summary>Asserts that the result holds the success value and returns it.</summary>
        /// <returns>The success value.</returns>
        /// <exception cref="ResultAssertionException">The result holds an error or is <see langword="default"/>.</exception>
        public T ShouldBeSuccess() =>
            result.TryGetValue(out T value) ? value : throw NotSuccess(result.HasValue, result.Value);
    }

    /// <summary>
    /// Asserts that the typed result holds an error of type <typeparamref name="TError"/> and returns it.
    /// <see cref="Result"/> and <see cref="Result{TValue}"/> hold <see cref="Error"/> values only, so use
    /// <c>ShouldHaveError</c> on them instead.
    /// </summary>
    /// <typeparam name="TError">The expected error type: one of the error slots or a base type of one.</typeparam>
    /// <param name="result">The typed result under test. It is boxed for the call, so only <typeparamref name="TError"/> is written.</param>
    /// <returns>The error.</returns>
    /// <exception cref="ResultAssertionException">The result succeeded, holds another error, or is <see langword="default"/>.</exception>
    public static TError ShouldBeFailureOf<TError>(this ITypedResult result)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(result);
#else
        if (result is null)
            throw new ArgumentNullException(nameof(result));
#endif
        object? current = result.Value;
        string expected = "Expected an error of type " + typeof(TError).Name;
        if (!result.HasValue)
            throw new ResultAssertionException(expected + ", but the result is default and holds no case.");
        if (result.IsSuccess)
            throw new ResultAssertionException(expected + ", but the result succeeded with " + AssertionMessages.Case(current) + ".");
        if (current is TError error)
            return error;
        throw new ResultAssertionException(expected + ", but the result holds the error " + AssertionMessages.Case(current) + ".");
    }

    private static ResultAssertionException NotSuccess(bool hasValue, object? current) =>
        new(hasValue
            ? "Expected a success, but the result holds the error " + AssertionMessages.Case(current) + "."
            : "Expected a success, but the result is default and holds no case.");
}
