using System.Diagnostics.CodeAnalysis;
using CSharpEssentials.Interfaces;

namespace CSharpEssentials;

[Serializable]
public readonly partial record struct Result : IResult
{
    private readonly Error[]? _errors = null;

    public Result() => throw new InvalidOperationException("Result must have a value or an error");
    private Result(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        Error[] errorArray = [.. errors];

        if (errorArray.Length == 0)
            throw ResultLogic.CreateEmptyErrorArrayException();

        _errors = errorArray;
    }


    [MemberNotNullWhen(true, nameof(_errors))]
    public readonly bool IsFailure => _errors is not null;

    [MemberNotNullWhen(false, nameof(_errors))]
    public readonly bool IsSuccess => _errors is null;

    public readonly Error[] ErrorsOrEmptyArray => IsFailure ? _errors : [];
    public readonly Error[] Errors => IsFailure ? _errors : [Error.NoErrors];
    public readonly Error FirstError => IsFailure ? _errors[0] : Error.NoFirstError;
    public readonly Error LastError => IsFailure ? _errors[^1] : Error.NoLastError;


    public override string ToString()
    {
        if (IsSuccess)
            return "Success";
        if (Errors.Length == 1)
            return $"Failure: {Errors.Length} error, first error: {FirstError}";
        return $"Failure: {Errors.Length} errors, first error: {FirstError}, last error: {LastError}";
    }


    public bool Equals(Result other)
    {
        if (IsSuccess)
            return other.IsSuccess;
        if (other.IsSuccess)
            return false;
        return ResultLogic.CheckIfErrorsAreEqual(ErrorsOrEmptyArray, other.ErrorsOrEmptyArray);
    }

    public override int GetHashCode()
    {
        if (IsSuccess)
            return IsSuccess.GetHashCode();
        return ResultLogic.CreateErrorCodeHash(ErrorsOrEmptyArray);
    }
}