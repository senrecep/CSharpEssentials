using CSharpEssentials.Errors;

public static class TypedMissingArm
{
    public static int Status(int id) => Users.Get(id) switch { Dto => 200, NotFoundError => 404 };
}
