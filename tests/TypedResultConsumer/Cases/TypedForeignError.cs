using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

public static class TypedForeignError
{
    public static Result<Dto, NotFoundError, UserErrors.Locked> Get() => new ValidationError("User.Invalid", "User is invalid");
}
