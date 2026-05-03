using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    public static Result SuccessIf(bool condition, Error error) => condition ? Success() : Failure(error);

    public static Result SuccessIf(Func<bool> predicate, Error error) => predicate() ? Success() : Failure(error);

    public static Result<TValue> SuccessIf<TValue>(bool condition, TValue value, Error error) => condition ? Success(value) : Failure<TValue>(error);
}
