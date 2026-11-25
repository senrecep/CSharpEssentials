using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern.Interfaces;

namespace CSharpEssentials.ResultPattern.Testing;

/// <summary>
/// Assertions for <see cref="Result"/>, <see cref="Result{TValue}"/> and, through <see cref="IResultBase"/>, the typed
/// <c>Result&lt;T, E1..E3&gt;</c>. A failed assertion throws <see cref="ResultAssertionException"/> whose message lists every
/// error with its type, code, description and metadata.
/// </summary>
public static class ResultAssertionExtensions
{
    /// <summary>Asserts that the result is a success.</summary>
    /// <param name="result">The result under test.</param>
    /// <exception cref="ResultAssertionException">The result is a failure.</exception>
    public static void ShouldBeSuccess(this Result result)
    {
        if (result.IsFailure)
            throw new ResultAssertionException("Expected a success, but the result failed with " + AssertionMessages.Errors(result.Errors));
    }

    /// <summary>Asserts that the result is a success and returns its value.</summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="result">The result under test.</param>
    /// <returns>The success value.</returns>
    /// <exception cref="ResultAssertionException">The result is a failure.</exception>
    public static TValue ShouldBeSuccess<TValue>(this Result<TValue> result)
    {
        if (result.IsFailure)
            throw new ResultAssertionException("Expected a success, but the result failed with " + AssertionMessages.Errors(result.Errors));
        return result.Value;
    }

    /// <summary>Asserts that the result is a failure and returns its errors.</summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="result">The result under test.</param>
    /// <returns>The errors of the result. A typed result gives its one error converted to an <see cref="Error"/>.</returns>
    /// <exception cref="ResultAssertionException">The result is a success.</exception>
    public static Error[] ShouldBeFailure<TResult>(this TResult result)
        where TResult : IResultBase
    {
        if (result is null)
            throw new ArgumentNullException(nameof(result));
        if (result.IsSuccess)
            throw new ResultAssertionException(result is IValueResult { BoxedValue: { } value }
                ? "Expected a failure, but the result succeeded with " + AssertionMessages.Case(value) + "."
                : "Expected a failure, but the result succeeded.");
        return result.Errors;
    }

    /// <summary>Asserts that the result is a failure with an error of the given code and returns that error.</summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="result">The result under test.</param>
    /// <param name="code">The expected error code, compared ordinally.</param>
    /// <returns>The first error with the code.</returns>
    /// <exception cref="ResultAssertionException">The result is a success or has no error with the code.</exception>
    public static Error ShouldHaveError<TResult>(this TResult result, string code)
        where TResult : IResultBase =>
        Find(result, e => string.Equals(e.Code, code, StringComparison.Ordinal), "code " + code);

    /// <summary>Asserts that the result is a failure with an error of the given type and returns that error.</summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="result">The result under test.</param>
    /// <param name="type">The expected error type.</param>
    /// <returns>The first error of the type.</returns>
    /// <exception cref="ResultAssertionException">The result is a success or has no error of the type.</exception>
    public static Error ShouldHaveError<TResult>(this TResult result, ErrorType type)
        where TResult : IResultBase =>
        Find(result, e => e.Type == type, "type " + type);

    /// <summary>Asserts that the result is a failure with an error of the given type and code and returns that error.</summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="result">The result under test.</param>
    /// <param name="type">The expected error type.</param>
    /// <param name="code">The expected error code, compared ordinally.</param>
    /// <returns>The first error of the type with the code.</returns>
    /// <exception cref="ResultAssertionException">The result is a success or has no such error.</exception>
    public static Error ShouldHaveError<TResult>(this TResult result, ErrorType type, string code)
        where TResult : IResultBase =>
        Find(result, e => e.Type == type && string.Equals(e.Code, code, StringComparison.Ordinal), "type " + type + " and code " + code);

    private static Error Find<TResult>(TResult result, Func<Error, bool> match, string expected)
        where TResult : IResultBase
    {
        Error[] errors = result.ShouldBeFailure();
        foreach (Error error in errors)
            if (match(error))
                return error;
        throw new ResultAssertionException("Expected an error with " + expected + ", but the result failed with " + AssertionMessages.Errors(errors));
    }
}
