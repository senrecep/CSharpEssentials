using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    public static Result FailureIf(bool condition, Error error) => condition ? Failure(error) : Success();

    public static Result FailureIf(Func<bool> predicate, Error error) => predicate() ? Failure(error) : Success();

    public static Result<TValue> FailureIf<TValue>(bool condition, Error error) => condition ? Failure<TValue>(error) : Result<TValue>.Success(default!);
}
