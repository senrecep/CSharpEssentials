using CSharpEssentials.Errors;

public static class ReturningStatement
{
    // Covers every case but has no `case null`, so the end of the method stays reachable: CS0161.
    public static int Status(int id)
    {
        switch (Orders.Get(id))
        {
            case Dto:
                return 200;
            case NotFoundError:
                return 404;
            case ConflictError:
                return 409;
        }
    }
}
