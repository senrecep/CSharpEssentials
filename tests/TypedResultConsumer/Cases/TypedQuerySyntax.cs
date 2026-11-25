using System;
using System.Threading.Tasks;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

public static class TypedQuerySyntax
{
    private static readonly ValidationError Invalid = new("User.Invalid", "User is invalid.");

    private static Result<string, NotFoundError, UserErrors.Locked> Name(Dto dto) => dto.ToString()!;

    private static Result<int, ValidationError> Check(Dto dto) => dto.Id > 0 ? dto.Id : Invalid;

    public static Result<string, NotFoundError, UserErrors.Locked> SameErrors(int id) =>
        from dto in Users.Get(id)
        from name in Name(dto)
        select name + dto.Id;

    public static Result<int, NotFoundError, UserErrors.Locked, ValidationError> AddedErrors(int id) =>
        from dto in Users.Get(id)
        from checkedId in Check(dto)
        select checkedId * 2;

    public static async Task<int> Rest(int id)
    {
        Result<Dto, NotFoundError, UserErrors.Locked> found = await Task.FromResult(Users.Get(id))
            .TapAsync(async dto => await Task.Yield())
            .TapErrorAsync(async (NotFoundError _) => await Task.Yield());
        Result<int, NotFoundError, UserErrors.Locked, FormatException> parsed = found.TryCatch<int, FormatException>(dto => int.Parse(dto.ToString()!));
        Result<Dto[], NotFoundError, UserErrors.Locked> all = new[] { 2, 3 }.Traverse(Users.Get);
        return found.TapError((Error e) => Console.WriteLine(e.Code)).GetValueOr((Error e) => new Dto(e.Code.Length)).Id
            + parsed.GetValueOrDefault() + all.GetValueOr([]).Length;
    }
}
