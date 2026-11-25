using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

public static class TypedDefault
{
    public static Result<Dto, NotFoundError> Get() => default;
}
