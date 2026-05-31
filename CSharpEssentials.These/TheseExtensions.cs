using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.These;

public static class TheseExtensions
{
    public static These<Error, TValue> FromResult<TValue>(Result<TValue> result)
        => result.IsSuccess
            ? These<Error, TValue>.Right(result.Value)
            : These<Error, TValue>.Left(result.FirstError);

    public static Result<TValue> ToResult<TValue>(this These<Error, TValue> these)
    {
        if (these.IsLeft)
            return Result<TValue>.Failure(these.GetLeft().Value);
        return these.GetRight().Value;
    }

    public static Result<TValue> ToResultLenient<TValue>(this These<Error, TValue> these)
    {
        if (these.IsLeft && !these.IsBoth)
            return Result<TValue>.Failure(these.GetLeft().Value);
        return these.GetRight().Value;
    }
}
