using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Compat.CSharp12;

public static class TypedDelegateShapes
{
    public static Task<Result<int>> MapError(Result<int> source)
    {
        return source.MapErrorAsync(Mapper);

        static async Task<Error> Mapper(Error error)
        {
            await Task.Delay(1).ConfigureAwait(false);
            return Error.Failure("MAPPED", error.Description);
        }
    }

    public static Task<Result<int>> Bind(Result<int> source) =>
        source.BindAsync((Func<int, Task<Result<int>>>)(async v =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return Result<int>.Success(v * 2);
        }));

    public static Task<Result<int[]>> Traverse(IEnumerable<int> source, CancellationToken cancellationToken)
    {
        return source.TraverseAsync(Selector, cancellationToken);

        async Task<Result<int>> Selector(int value)
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
            return Result<int>.Success(value);
        }
    }
}
