using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using CSharpEssentials.Interfaces;

namespace CSharpEssentials;

public readonly partial record struct Result : IResult
{
    private readonly Error[]? _errors = null;

    [JsonConstructor]
#pragma warning disable IDE0051
    private Result(bool isFailure, Error[]? errorsOrEmptyArray = null) => _errors = isFailure ? errorsOrEmptyArray : null;
#pragma warning restore IDE0051

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
    [JsonPropertyName("errors")]
    public readonly Error[] ErrorsOrEmptyArray => IsFailure ? _errors : [];
    [JsonIgnore]
    public readonly Error[] Errors => IsFailure ? _errors : [Error.NoErrors];
    [JsonIgnore]
    public readonly Error FirstError => IsFailure ? _errors[0] : Error.NoFirstError;
    [JsonIgnore]
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
