using CSharpEssentials.Errors;

namespace CSharpEssentials.ResultPattern;

public readonly partial record struct Result<TValue>
{
    public void Deconstruct(out bool isSuccess, out TValue value, out Error[] errors)
    {
        isSuccess = IsSuccess;
        value = IsSuccess ? Value : default!;
        errors = ErrorsOrEmptyArray;
    }
}
