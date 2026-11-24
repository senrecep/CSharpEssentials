using CSharpEssentials.Errors;

public static class ForeignError
{
    public static R<Dto, NotFoundError, ConflictError> Get() => new ValidationError("Order.Invalid", "Order is invalid");
}
