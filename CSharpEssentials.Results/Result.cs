using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern.Interfaces;

namespace CSharpEssentials.ResultPattern;

/// <summary>
/// A result that is either a success or a failure carrying one or more errors.
/// <see langword="default"/>(<see cref="Result"/>) is uninitialized and fails closed: it reports a failure with the
/// <c>Result.Default</c> error, and combinators that would read its errors throw <see cref="InvalidOperationException"/>.
/// </summary>
public readonly partial record struct Result : IResult, IFailureFactory<Result>, IFailureProbe<Result>
{
    private readonly Error[]? _errors = null;

    [JsonConstructor]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by System.Text.Json")]
    private Result(bool isFailure, Error[]? errorsOrEmptyArray = null) => _errors = isFailure ? ResultLogic.ValidateDeserializedFailureErrors(errorsOrEmptyArray) : [];

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


    /// <summary>
    /// <c>true</c> for a failure and for <see langword="default"/>(<see cref="Result"/>).
    /// </summary>
    public readonly bool IsFailure => !IsSuccess;

    bool IFailureProbe<Result>.IsFailureOf(in Result value) => value.IsFailure;

    /// <summary>
    /// <c>true</c> only for a result created as a success.
    /// </summary>
    [MemberNotNullWhen(true, nameof(_errors))]
    public readonly bool IsSuccess => _errors is { Length: 0 };
    [JsonPropertyName("errors")]
    public readonly Error[] ErrorsOrEmptyArray => _errors ?? ResultLogic.DefaultErrors;
    [JsonIgnore]
    public readonly Error[] Errors
    {
        get
        {
            if (IsSuccess)
                return Error.NoErrors;
            return _errors ?? ResultLogic.DefaultErrors;
        }
    }
    [JsonIgnore]
    public readonly Error FirstError => IsSuccess ? Error.NoFirstError : ErrorsOrEmptyArray[0];
    [JsonIgnore]
    public readonly Error LastError => IsSuccess ? Error.NoLastError : ErrorsOrEmptyArray[^1];

    internal Error[] InitializedErrors => ResultLogic.InitializedErrors(_errors);


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
