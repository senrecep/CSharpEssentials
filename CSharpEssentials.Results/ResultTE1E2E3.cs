using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern.Interfaces;

namespace CSharpEssentials.ResultPattern;

/// <summary>
/// A result that holds either a success value of type <typeparamref name="T"/> or exactly one error of one of the types <typeparamref name="E1"/>, <typeparamref name="E2"/> and <typeparamref name="E3"/>.
/// With C# 15 a switch expression over it is exhaustive once every case has an arm, with no discard or null arm.
/// </summary>
/// <remarks>
/// <see langword="default"/> holds no case: <see cref="HasValue"/> is <see langword="false"/>, every <c>TryGetValue</c> returns
/// <see langword="false"/>, a switch expression without a <c>null</c> arm throws <see cref="SwitchExpressionException"/>, and
/// <see cref="Match{TOut}"/>, <see cref="Switch"/> and <see cref="ToResult"/> throw <see cref="InvalidOperationException"/>.
/// A struct error is boxed when it is stored.
/// </remarks>
/// <typeparam name="T">The success value type.</typeparam>
/// <typeparam name="E1">The first error type.</typeparam>
/// <typeparam name="E2">The second error type.</typeparam>
/// <typeparam name="E3">The third error type.</typeparam>
[SuppressMessage("Naming", "CA1715:Identifiers should have correct prefix", Justification = "E1..E3 name the error slots, matching the documented Result<T, E1, E2> shape.")]
[Union]
[JsonConverter(typeof(TypedResultJsonConverterFactory))]
public readonly partial struct Result<T, E1, E2, E3> : IUnion, IValueResult, ITypedResult, IEquatable<Result<T, E1, E2, E3>>
    where T : notnull
    where E1 : notnull
    where E2 : notnull
    where E3 : notnull
{
    internal Result(byte tag, T value, object? error)
    {
        Tag = tag;
        RawValue = value;
        RawError = error;
    }

    /// <summary>Creates a successful result.</summary>
    /// <param name="value">The success value.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public Result(T value)
    {
        RawValue = TypedResultLogic.NotNull(value, nameof(value));
        RawError = null;
        Tag = TypedResultLogic.SuccessTag;
    }

    /// <summary>Creates a failed result that holds an error of type <typeparamref name="E1"/>.</summary>
    /// <param name="error">The error.</param>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
    public Result(E1 error)
    {
        RawValue = default!;
        RawError = TypedResultLogic.NotNull(error, nameof(error));
        Tag = 2;
    }

    /// <summary>Creates a failed result that holds an error of type <typeparamref name="E2"/>.</summary>
    /// <param name="error">The error.</param>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
    public Result(E2 error)
    {
        RawValue = default!;
        RawError = TypedResultLogic.NotNull(error, nameof(error));
        Tag = 3;
    }

    /// <summary>Creates a failed result that holds an error of type <typeparamref name="E3"/>.</summary>
    /// <param name="error">The error.</param>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is null.</exception>
    public Result(E3 error)
    {
        RawValue = default!;
        RawError = TypedResultLogic.NotNull(error, nameof(error));
        Tag = 4;
    }

    internal T RawValue { get; }

    internal object? RawError { get; }

    internal byte Tag { get; }

    /// <summary>Gets a value indicating whether the result holds a case. It is <see langword="false"/> only for <see langword="default"/>.</summary>
    public bool HasValue => Tag != TypedResultLogic.DefaultTag;

    /// <summary>Gets the case the result holds as <see cref="object"/> (boxing a value type), or <see langword="null"/> for <see langword="default"/>.</summary>
    public object? Value => Tag switch
    {
        TypedResultLogic.DefaultTag => null,
        TypedResultLogic.SuccessTag => RawValue,
        _ => RawError,
    };

    /// <summary>Gets a value indicating whether the result holds the success value.</summary>
    public bool IsSuccess => Tag == TypedResultLogic.SuccessTag;

    /// <summary>Gets a value indicating whether the result holds no success value: an error, or nothing for <see langword="default"/>.</summary>
    public bool IsFailure => Tag != TypedResultLogic.SuccessTag;

    object? IValueResult.BoxedValue => IsSuccess ? RawValue : null;

    Error IResultBase.FirstError => IsSuccess ? Error.NoFirstError : CurrentError();

    Error IResultBase.LastError => IsSuccess ? Error.NoLastError : CurrentError();

    Error[] IResultBase.Errors => IsSuccess ? Error.NoErrors : CurrentError();

    Error[] IResultBase.ErrorsOrEmptyArray => IsSuccess ? [] : CurrentError();

    /// <summary>Gets the success value.</summary>
    /// <param name="value">The success value, or <see langword="default"/> when the result holds none.</param>
    /// <returns><see langword="true"/> when the result holds the success value.</returns>
    public bool TryGetValue(out T value)
    {
        value = RawValue;
        return Tag == TypedResultLogic.SuccessTag;
    }

    /// <summary>Gets the error of type <typeparamref name="E1"/>.</summary>
    /// <param name="value">The error, or <see langword="default"/> when the result holds another case.</param>
    /// <returns><see langword="true"/> when the result holds an error of type <typeparamref name="E1"/>.</returns>
    public bool TryGetValue(out E1 value)
    {
        if (Tag == 2)
        {
            value = (E1)RawError!;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>Gets the error of type <typeparamref name="E2"/>.</summary>
    /// <param name="value">The error, or <see langword="default"/> when the result holds another case.</param>
    /// <returns><see langword="true"/> when the result holds an error of type <typeparamref name="E2"/>.</returns>
    public bool TryGetValue(out E2 value)
    {
        if (Tag == 3)
        {
            value = (E2)RawError!;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>Gets the error of type <typeparamref name="E3"/>.</summary>
    /// <param name="value">The error, or <see langword="default"/> when the result holds another case.</param>
    /// <returns><see langword="true"/> when the result holds an error of type <typeparamref name="E3"/>.</returns>
    public bool TryGetValue(out E3 value)
    {
        if (Tag == 4)
        {
            value = (E3)RawError!;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>Calls the function of the case the result holds and returns its result.</summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="onSuccess">Called with the success value.</param>
    /// <param name="onError1">Called with the error of type <typeparamref name="E1"/>.</param>
    /// <param name="onError2">Called with the error of type <typeparamref name="E2"/>.</param>
    /// <param name="onError3">Called with the error of type <typeparamref name="E3"/>.</param>
    /// <returns>The return value of the called function.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<E1, TOut> onError1, Func<E2, TOut> onError2, Func<E3, TOut> onError3) => Tag switch
    {
        TypedResultLogic.SuccessTag => onSuccess(RawValue),
        2 => onError1((E1)RawError!),
        3 => onError2((E2)RawError!),
        4 => onError3((E3)RawError!),
        _ => throw TypedResultLogic.DefaultInitialized(),
    };

    /// <summary>Calls the action of the case the result holds.</summary>
    /// <param name="onSuccess">Called with the success value.</param>
    /// <param name="onError1">Called with the error of type <typeparamref name="E1"/>.</param>
    /// <param name="onError2">Called with the error of type <typeparamref name="E2"/>.</param>
    /// <param name="onError3">Called with the error of type <typeparamref name="E3"/>.</param>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public void Switch(Action<T> onSuccess, Action<E1> onError1, Action<E2> onError2, Action<E3> onError3)
    {
        switch (Tag)
        {
            case TypedResultLogic.SuccessTag:
                onSuccess(RawValue);
                break;
            case 2:
                onError1((E1)RawError!);
                break;
            case 3:
                onError2((E2)RawError!);
                break;
            case 4:
                onError3((E3)RawError!);
                break;
            default:
                throw TypedResultLogic.DefaultInitialized();
        }
    }

    /// <summary>
    /// Converts to the classic <see cref="Result{TValue}"/>. An <see cref="IError"/> keeps its code, description, type and
    /// metadata instance, an exception becomes <see cref="Error.Exception(Exception, ErrorType, ErrorMetadata?)"/>, and any other
    /// error becomes an <see cref="ErrorType.Unexpected"/> error whose code is the name of its slot type.
    /// </summary>
    /// <returns>The classic result.</returns>
    /// <exception cref="InvalidOperationException">The result is <see langword="default"/>.</exception>
    public Result<T> ToResult() => Tag switch
    {
        TypedResultLogic.SuccessTag => Result<T>.Success(RawValue),
        TypedResultLogic.DefaultTag => throw TypedResultLogic.DefaultInitialized(),
        _ => Result<T>.Failure(CurrentError()),
    };

    /// <summary>Creates a successful result.</summary>
    /// <param name="value">The success value.</param>
    public static implicit operator Result<T, E1, E2, E3>(T value) => new(value);

    /// <summary>Creates a failed result that holds an error of type <typeparamref name="E1"/>.</summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result<T, E1, E2, E3>(E1 error) => new(error);

    /// <summary>Creates a failed result that holds an error of type <typeparamref name="E2"/>.</summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result<T, E1, E2, E3>(E2 error) => new(error);

    /// <summary>Creates a failed result that holds an error of type <typeparamref name="E3"/>.</summary>
    /// <param name="error">The error.</param>
    public static implicit operator Result<T, E1, E2, E3>(E3 error) => new(error);

    /// <summary>Widens a <see cref="Result{T, E1}"/> into this result. The held case stays the same.</summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator Result<T, E1, E2, E3>(Result<T, E1> result) =>
        new(TypedResultLogic.Remap(result.Tag, 2), result.RawValue, result.RawError);

    /// <summary>Widens a <see cref="Result{T, E2}"/> into this result. The held case stays the same.</summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator Result<T, E1, E2, E3>(Result<T, E2> result) =>
        new(TypedResultLogic.Remap(result.Tag, 3), result.RawValue, result.RawError);

    /// <summary>Widens a <see cref="Result{T, E3}"/> into this result. The held case stays the same.</summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator Result<T, E1, E2, E3>(Result<T, E3> result) =>
        new(TypedResultLogic.Remap(result.Tag, 4), result.RawValue, result.RawError);

    /// <summary>Widens a <see cref="Result{T, E1, E2}"/> into this result. The held case stays the same.</summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator Result<T, E1, E2, E3>(Result<T, E1, E2> result) =>
        new(TypedResultLogic.Remap(result.Tag, 2, 3), result.RawValue, result.RawError);

    /// <summary>Widens a <see cref="Result{T, E1, E3}"/> into this result. The held case stays the same.</summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator Result<T, E1, E2, E3>(Result<T, E1, E3> result) =>
        new(TypedResultLogic.Remap(result.Tag, 2, 4), result.RawValue, result.RawError);

    /// <summary>Widens a <see cref="Result{T, E2, E1}"/> into this result. The held case stays the same.</summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator Result<T, E1, E2, E3>(Result<T, E2, E1> result) =>
        new(TypedResultLogic.Remap(result.Tag, 3, 2), result.RawValue, result.RawError);

    /// <summary>Widens a <see cref="Result{T, E2, E3}"/> into this result. The held case stays the same.</summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator Result<T, E1, E2, E3>(Result<T, E2, E3> result) =>
        new(TypedResultLogic.Remap(result.Tag, 3, 4), result.RawValue, result.RawError);

    /// <summary>Widens a <see cref="Result{T, E3, E1}"/> into this result. The held case stays the same.</summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator Result<T, E1, E2, E3>(Result<T, E3, E1> result) =>
        new(TypedResultLogic.Remap(result.Tag, 4, 2), result.RawValue, result.RawError);

    /// <summary>Widens a <see cref="Result{T, E3, E2}"/> into this result. The held case stays the same.</summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator Result<T, E1, E2, E3>(Result<T, E3, E2> result) =>
        new(TypedResultLogic.Remap(result.Tag, 4, 3), result.RawValue, result.RawError);

    /// <summary>Reorders a <see cref="Result{T, E1, E3, E2}"/> into this result. The held case stays the same.</summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator Result<T, E1, E2, E3>(Result<T, E1, E3, E2> result) =>
        new(TypedResultLogic.Remap(result.Tag, 2, 4, 3), result.RawValue, result.RawError);

    /// <summary>Reorders a <see cref="Result{T, E2, E1, E3}"/> into this result. The held case stays the same.</summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator Result<T, E1, E2, E3>(Result<T, E2, E1, E3> result) =>
        new(TypedResultLogic.Remap(result.Tag, 3, 2, 4), result.RawValue, result.RawError);

    /// <summary>Reorders a <see cref="Result{T, E2, E3, E1}"/> into this result. The held case stays the same.</summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator Result<T, E1, E2, E3>(Result<T, E2, E3, E1> result) =>
        new(TypedResultLogic.Remap(result.Tag, 3, 4, 2), result.RawValue, result.RawError);

    /// <summary>Reorders a <see cref="Result{T, E3, E1, E2}"/> into this result. The held case stays the same.</summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator Result<T, E1, E2, E3>(Result<T, E3, E1, E2> result) =>
        new(TypedResultLogic.Remap(result.Tag, 4, 2, 3), result.RawValue, result.RawError);

    /// <summary>Reorders a <see cref="Result{T, E3, E2, E1}"/> into this result. The held case stays the same.</summary>
    /// <param name="result">The result to convert.</param>
    public static implicit operator Result<T, E1, E2, E3>(Result<T, E3, E2, E1> result) =>
        new(TypedResultLogic.Remap(result.Tag, 4, 3, 2), result.RawValue, result.RawError);

    /// <summary>
    /// Returns whether both results hold the same case: the same slot, and an equal value or error. Two <see langword="default"/>
    /// results are equal. The same error in different slots is not.
    /// </summary>
    /// <param name="other">The result to compare with.</param>
    /// <returns><see langword="true"/> when both results hold the same case.</returns>
    public bool Equals(Result<T, E1, E2, E3> other) =>
        TypedResultLogic.CaseEquals(Tag, RawValue, RawError, other.Tag, other.RawValue, other.RawError);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Result<T, E1, E2, E3> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => TypedResultLogic.CaseHashCode(Tag, RawValue, RawError);

    /// <summary>Returns whether both results hold the same case.</summary>
    /// <param name="left">The first result.</param>
    /// <param name="right">The second result.</param>
    public static bool operator ==(Result<T, E1, E2, E3> left, Result<T, E1, E2, E3> right) => left.Equals(right);

    /// <summary>Returns whether the results hold different cases.</summary>
    /// <param name="left">The first result.</param>
    /// <param name="right">The second result.</param>
    public static bool operator !=(Result<T, E1, E2, E3> left, Result<T, E1, E2, E3> right) => !left.Equals(right);

    internal Error CurrentError() => Tag switch
    {
        2 => TypedResultLogic.ToError(RawError!, typeof(E1)),
        3 => TypedResultLogic.ToError(RawError!, typeof(E2)),
        4 => TypedResultLogic.ToError(RawError!, typeof(E3)),
        _ => TypedResultLogic.DefaultError,
    };
}
