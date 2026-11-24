using CSharpEssentials.Errors;

public static class AllArms
{
    public static int Status(int id) => Orders.Get(id) switch { Dto => 200, NotFoundError => 404, ConflictError => 409 };
}
