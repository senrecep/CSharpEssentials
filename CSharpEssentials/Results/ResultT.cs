using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Interfaces;

namespace CSharpEssentials;

[Serializable]
public readonly partial record struct Result<TValue> : IResult<TValue>
{
    private readonly EqualityComparer<TValue> _comparer = EqualityComparer<TValue>.Default;
    private readonly Error[]? _errors = null;
    private readonly TValue? _value = default;

    public Result() => throw new InvalidOperationException("Result must have a value or an error");
    private Result(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);
        _value = value;
    }

    private Result(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        Error[] errorArray = [.. errors];

        if (errorArray.Length == 0)
            throw ResultLogic.CreateEmptyErrorArrayException();

        _errors = errorArray;
    }

    [MemberNotNullWhen(true, nameof(Errors))]
    [MemberNotNullWhen(true, nameof(_errors))]
    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(false, nameof(_value))]
    public readonly bool IsFailure => _errors is not null;

    [MemberNotNullWhen(false, nameof(Errors))]
    [MemberNotNullWhen(false, nameof(_errors))]
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(true, nameof(_value))]
    public readonly bool IsSuccess => _errors is null;

    public readonly Error[] ErrorsOrEmptyArray => IsFailure ? _errors : [];
    public readonly TValue Value => IsSuccess ? _value : default!;
    public readonly Error[] Errors => IsFailure ? _errors : [Error.NoErrors];
    public readonly Error FirstError => IsFailure ? _errors[0] : Error.NoFirstError;
    public readonly Error LastError => IsFailure ? _errors[^1] : Error.NoLastError;

    /// <summary>
    /// Converts a result to a result with no value.
    /// </summary>
    /// <returns></returns>
    public Result ToResult() => IsSuccess ?
        Result.Success() :
        Result.Failure(Errors);



    public override string ToString()
    {
        if (IsSuccess)
            return $"Success: {Value}";
        if (Errors.Length == 1)
            return $"Failure: {Errors.Length} error, first error: {FirstError}";
        return $"Failure: {Errors.Length} errors, first error: {FirstError}, last error: {LastError}";
    }

    public bool Equals(Result<TValue> other)
    {
        if (IsSuccess)
            return other.IsSuccess && _comparer.Equals(_value, other._value);
        return other.IsFailure && ResultLogic.CheckIfErrorsAreEqual(ErrorsOrEmptyArray, other.ErrorsOrEmptyArray);
    }

    public override int GetHashCode()
    {
        if (IsSuccess)
            return _value.GetHashCode();
        return ResultLogic.CreateErrorCodeHash(ErrorsOrEmptyArray);
    }

    public static Result<TValue[]> operator +(Result<TValue> left, Result<TValue> right) => And(left, right);

}