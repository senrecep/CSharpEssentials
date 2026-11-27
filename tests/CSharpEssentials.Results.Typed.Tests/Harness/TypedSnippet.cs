namespace CSharpEssentials.Results.Typed.Tests.Harness;

/// <summary>
/// Wraps snippet members into a probe class that uses the shipped <c>Result&lt;T, E1..E3&gt;</c>. No union polyfill is added:
/// before net11.0 the snippet relies on the internal UnionAttribute and IUnion that ship inside CSharpEssentials.Results.
/// </summary>
internal static class TypedSnippet
{
    private const string Header = """
        using CSharpEssentials.Errors;
        using CSharpEssentials.ResultPattern;

        public sealed class Dto
        {
            public Dto(int id) => Id = id;
            public int Id { get; }
        }

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

            public sealed record Locked : ConflictError
            {
                public Locked(int id) : base(Codes.Locked, "User is locked.") => Id = id;
                public int Id { get; }
            }
        }

        public static class Probe
        {
            public static Result<Dto, NotFoundError, UserErrors.Locked> Get(int id) =>
                id switch { 0 => UserErrors.NotFound, 1 => UserErrors.Deleted, 7 => new UserErrors.Locked(id), _ => new Dto(id) };

        """;

    public static string With(string members) => Header + "    " + members + "\n}\n";
}
