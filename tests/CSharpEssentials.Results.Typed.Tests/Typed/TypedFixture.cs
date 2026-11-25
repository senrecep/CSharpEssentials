using System.Globalization;
using CSharpEssentials.Errors;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

/// <summary>
/// The cases the emitted combinator tests put into Result&lt;int, NotFoundError, ConflictError, RateLimited&gt; and its smaller
/// arities: a predefined static error, a derived error in a base-kind slot, a struct error, and the error types that combinators add.
/// </summary>
internal static class TypedFixture
{
    public static readonly NotFoundError NotFound = UserErrors.NotFound;
    public static readonly ConflictError Conflict = new UserErrors.Locked(7);
    public static readonly RateLimited Limited = new("Rate.Limited");
    public static readonly ValidationError Invalid = new(UserErrors.Codes.Invalid, "User is invalid.");
    public static readonly PlainError Plain = new("plain");

    public static string Text(int value) => "v" + value.ToString(CultureInfo.InvariantCulture);
}
