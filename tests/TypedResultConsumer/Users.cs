using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

public static class UserErrors
{
    public static class Codes
    {
        public const string NotFound = "User.NotFound";
        public const string Deleted = "User.Deleted";
        public const string Locked = "User.Locked";
    }

    public static readonly NotFoundError NotFound = new(Codes.NotFound, "User not found.");
    public static readonly NotFoundError Deleted = new(Codes.Deleted, "User was deleted.");

    public sealed record Locked(int Id) : ConflictError(Codes.Locked, "User is locked.");
}

public static class Users
{
    public static Result<Dto, NotFoundError, UserErrors.Locked> Get(int id) => id switch
    {
        0 => UserErrors.NotFound,
        1 => UserErrors.Deleted,
        7 => new UserErrors.Locked(id),
        _ => new Dto(id),
    };
}
