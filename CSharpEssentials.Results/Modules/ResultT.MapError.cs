using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    public Result<TValue> MapError(Func<Error[], Error[]> errorMapper)
    {
        if (IsSuccess)
            return this;
        return errorMapper(Errors).ToResult<TValue>();
    }

    public Result<TValue> MapError(Func<Error, Error> errorMapper)
    {
        if (IsSuccess)
            return this;
        return errorMapper(FirstError).ToResult<TValue>();
    }
}
