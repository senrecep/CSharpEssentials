using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

public static class TypedDuplicateSlot
{
    public static Result<Dto, NotFoundError, NotFoundError> Get() => UserErrors.NotFound;
}
