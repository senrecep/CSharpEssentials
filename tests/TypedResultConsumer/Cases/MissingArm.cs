using CSharpEssentials.Errors;

public static class MissingArm
{
    public static int Status(int id) => Orders.Get(id) switch { Dto => 200, NotFoundError => 404 };
}
