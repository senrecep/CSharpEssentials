using System.Threading.Tasks;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

public static class TypedAsyncChain
{
    private static async Task<Result<Dto, NotFoundError, UserErrors.Locked>> GetAsync(int id)
    {
        await Task.Yield();
        return Users.Get(id);
    }

    private static async Task<Result<string, ValidationError>> NameAsync(Dto dto)
    {
        await Task.Yield();
        return dto.ToString();
    }

    public static Task<Result<string, NotFoundError, UserErrors.Locked, ValidationError>> Widen(int id) =>
        GetAsync(id).BindWidenAsync(NameAsync);

    public static async Task<Result<Dto, NotFoundError, UserErrors.Locked>> Checked(int id)
    {
        Result<Dto, NotFoundError, UserErrors.Locked> found = Users.Get(id);
        Result<Dto, NotFoundError, UserErrors.Locked> bound = await found.BindAsync(async dto =>
        {
            await Task.Yield();
            return (Result<Dto, NotFoundError, UserErrors.Locked>)dto;
        });
        return await new ValueTask<Result<Dto, NotFoundError, UserErrors.Locked>>(bound)
            .EnsureAsync(async dto =>
            {
                await Task.Yield();
                return dto.Id > 0;
            }, UserErrors.NotFound);
    }
}
