using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    public static Result FailureIf(bool condition, Error error) => condition ? error : Success();

    public static Result FailureIf(Func<bool> predicate, Error error) => predicate() ? error : Success();

    public static Result<TValue> FailureIf<TValue>(bool condition, Error error) => condition ? error : default(TValue)!;
}
