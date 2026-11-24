using CSharpEssentials.Errors;

public static class DuplicateSlot
{
    public static R<Dto, NotFoundError, NotFoundError> Get() => Orders.NotFound;
}
