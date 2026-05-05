using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    public Result MapError(Func<Error[], Error[]> errorMapper)
    {
        if (IsSuccess)
            return this;
        return errorMapper(Errors);
    }

    public Result MapError(Func<Error, Error> errorMapper)
    {
        if (IsSuccess)
            return this;
        return errorMapper(FirstError);
    }
}
