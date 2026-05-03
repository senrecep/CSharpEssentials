using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result
{
    public void Deconstruct(out bool isSuccess, out Error[] errors)
    {
        isSuccess = IsSuccess;
        errors = ErrorsOrEmptyArray;
    }
}
