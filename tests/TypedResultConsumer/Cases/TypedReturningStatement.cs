using CSharpEssentials.Errors;

public static class TypedReturningStatement
{
    // Covers every case but has no `case null`, so the end of the method stays reachable: CS0161.
    public static int Status(int id)
    {
        switch (Users.Get(id))
        {
            case Dto:
                return 200;
            case NotFoundError:
                return 404;
            case UserErrors.Locked:
                return 423;
        }
    }
}
