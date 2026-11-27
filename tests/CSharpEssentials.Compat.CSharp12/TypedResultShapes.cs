using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Compat.CSharp12;

/// <summary>
/// What a C# 12 caller can do with <c>Result&lt;T, E1..E3&gt;</c>: return a value or an error through the implicit conversions,
/// <c>Match</c>, <c>Switch</c> and <c>TryGetValue</c>. A switch over the cases needs C# 15. The async calls show the shapes that bind
/// without <c>[OverloadResolutionPriority]</c>.
/// </summary>
public static class TypedResultShapes
{
    private static readonly NotFoundError Missing = new("Order.NotFound", "Order not found.");
    private static readonly ConflictError Busy = new("Order.Busy", "Order is busy.");

    public static Result<int, NotFoundError> ReturnValue(int value) => value;

    public static Result<int, NotFoundError> ReturnError() => Missing;

    public static Result<int, NotFoundError, ConflictError> ReturnEither(int id)
    {
        if (id < 0)
            return Missing;
        if (id == 0)
            return Busy;
        return id;
    }

    public static string Match(Result<int, NotFoundError, ConflictError> result) =>
        result.Match(
            value => value.ToString(System.Globalization.CultureInfo.InvariantCulture),
            notFound => notFound.Code,
            conflict => conflict.Code);

    public static int Switch(Result<int, NotFoundError, ConflictError> result)
    {
        int seen = 0;
        result.Switch(_ => seen = 1, _ => seen = 2, _ => seen = 3);
        return seen;
    }

    public static string? TryGetError(Result<int, NotFoundError> result) =>
        result.TryGetValue(out NotFoundError? error) ? error.Code : null;

    public static Task<Result<string, NotFoundError>> BindOnTaskSource(Task<Result<int, NotFoundError>> source) =>
        source.BindAsync(v => new Result<string, NotFoundError>(v.ToString(System.Globalization.CultureInfo.InvariantCulture)));

    public static Task<Result<int, NotFoundError, ConflictError>> BindWidenOnTaskSource(Task<Result<int, NotFoundError>> source) =>
        source.BindWidenAsync(async v =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return v > 0 ? new Result<int, ConflictError>(v) : Busy;
        });

    public static Task<Result<int, NotFoundError>> EnsureOnTaskSource(Task<Result<int, NotFoundError>> source) =>
        source.EnsureAsync<int, NotFoundError>(v => v > 0, Missing);

    public static ValueTask<Result<string, NotFoundError>> MapOnValueTaskSource(ValueTask<Result<int, NotFoundError>> source) =>
        source.MapAsync(async v =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

    public static Task<Result<int, ValidationError>> MapErrorOnTaskSource(Task<Result<int, NotFoundError>> source) =>
        source.MapErrorAsync(async (NotFoundError e) =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return new ValidationError("Order.Invalid", e.Description);
        });

    public static Task<Result<int, NotFoundError, ValidationError>> EnsureAddingASlot(Task<Result<int, NotFoundError>> source) =>
        source.EnsureAsync(v => v > 0, new ValidationError("Order.Invalid", "Order is invalid."));

    public static Task<Result<int, NotFoundError>> BindOnInstance(Result<int, NotFoundError> source) =>
        source.BindAsync((Func<int, Task<Result<int, NotFoundError>>>)(async v =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return v + 1;
        }));
}
