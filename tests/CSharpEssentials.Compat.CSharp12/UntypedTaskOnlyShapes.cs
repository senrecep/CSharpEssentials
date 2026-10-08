#if !NET9_0_OR_GREATER
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace CSharpEssentials.Compat.CSharp12;

/// <summary>
/// Untyped async lambda shapes that bind under C# 12 only when the consumer gets the netstandard2.1 asset (no ValueTask twins).
/// On net9.0+ the Task and ValueTask handlers are both applicable and C# 12 ignores
/// <c>[OverloadResolutionPriority]</c>, so the same calls report CS0121; <c>TypedTwinShapes</c> holds the net9.0 form.
/// </summary>
public static class UntypedTaskOnlyShapes
{
    public static Task<Result<string>> Map(Result<int> source) =>
        source.MapAsync(async v =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
        });

    public static Task<Result<int>> MapResult(Result source) =>
        source.MapAsync(async () =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return 1;
        });

    public static Task<Result<int>> Tap(Result<int> source) =>
        source.TapAsync(async _ => await Task.Delay(1).ConfigureAwait(false));

    public static Task<Result> TapResult(Result source) =>
        source.TapAsync(async () => await Task.Delay(1).ConfigureAwait(false));

    public static Task<string> Match(Result<int> source) =>
        source.MatchAsync(
            async v =>
            {
                await Task.Delay(1).ConfigureAwait(false);
                return v.ToString(System.Globalization.CultureInfo.InvariantCulture);
            },
            async errors =>
            {
                await Task.Delay(1).ConfigureAwait(false);
                return errors[0].Code;
            });

    public static Task<Result<int>> Ensure(Result<int> source, Error error) =>
        source.EnsureAsync(async v =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return v > 0;
        }, error);

    public static Task<Result<int>> Then(Result<int> source) =>
        source.ThenAsync(async v =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return Result<int>.Success(v + 1);
        });

    public static ValueTask<Result> ThenFromValueTask(ValueTask<Result> source) =>
        source.ThenAsync(async () =>
        {
            await Task.Delay(1).ConfigureAwait(false);
            return Result.Success();
        });
}
#endif
