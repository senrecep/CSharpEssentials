using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

public static class TypedAllArms
{
    public static int Status(int id) => Users.Get(id) switch
    {
        Dto => 200,
        NotFoundError { Code: UserErrors.Codes.Deleted } => 410,
        NotFoundError => 404,
        UserErrors.Locked => 423,
    };

    public static Result<Dto, NotFoundError, UserErrors.Locked, ValidationError> Widened(int id) => Users.Get(id);
}
