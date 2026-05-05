using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern.Interfaces;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result : IResult
{
    private readonly Error[]? _errors = null;

    [JsonConstructor]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by System.Text.Json")]
    private Result(bool isFailure, Error[]? errorsOrEmptyArray = null) => _errors = isFailure ? errorsOrEmptyArray : null;

    private Result(IEnumerable<Error> errors)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(errors);
#else
        if (errors is null)
            throw new ArgumentNullException(nameof(errors));
#endif
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
    public readonly Error[] Errors
    {
        get
        {
            if (IsFailure)
                return _errors;
            return Error.NoErrors;
        }
    }
    [JsonIgnore]
    public readonly Error FirstError => IsFailure ? _errors[0] : Error.NoFirstError;
    [JsonIgnore]
    public readonly Error LastError => IsFailure ? _errors[_errors.Length - 1] : Error.NoLastError;


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
