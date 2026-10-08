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
        var mappedErrors = new Error[_errors.Length];
        for (int i = 0; i < _errors.Length; i++)
            mappedErrors[i] = errorMapper(_errors[i]);
        return mappedErrors;
    }
}
