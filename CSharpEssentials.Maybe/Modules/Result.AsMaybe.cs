using CSharpEssentials.Core;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Maybe;

public static partial class MaybeExtensions
{
    public static Maybe<TValue> AsMaybe<TValue>(this Result<TValue> result)
    {
        if (result.IsSuccess)
            return result.Value;
        return Maybe<TValue>.None;
    }

    public static async Task<Maybe<TValue>> AsMaybeAsync<TValue>(this Task<Result<TValue>> task, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.AsMaybe();
    }

    public static async ValueTask<Maybe<TValue>> AsMaybeAsync<TValue>(this ValueTask<Result<TValue>> task, CancellationToken cancellationToken = default)
    {
        Result<TValue> result = await task.WithCancellation(cancellationToken);
        return result.AsMaybe();
    }
}
