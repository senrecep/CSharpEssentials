using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    public Result<TValue> MapError(Func<Error[], Error[]> errorMapper)
    {
        if (IsSuccess)
            return this;
        return Result<TValue>.Failure(errorMapper(Errors));
    }

    public Result<TValue> MapError(Func<Error, Error> errorMapper)
    {
        if (IsSuccess)
            return this;
        return Result<TValue>.Failure(errorMapper(FirstError));
    }
}
