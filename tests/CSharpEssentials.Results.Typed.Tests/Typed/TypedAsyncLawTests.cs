using System.Globalization;
using CsCheck;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

/// <summary>Every async cell returns what its synchronous twin returns, for any case of <c>Result&lt;int, NotFoundError, ConflictError&gt;</c>.</summary>
public sealed class TypedAsyncLawTests
{
    private const long Iterations = 300;

    private static readonly NotFoundError Missing = new("Law.Missing", "missing");
    private static readonly ConflictError Busy = new("Law.Busy", "busy");
    private static readonly ValidationError Invalid = new("Law.Invalid", "invalid");

    private static readonly Gen<Result<int, NotFoundError, ConflictError>> GenResult =
        Gen.Select(Gen.Int[0, 2], Gen.Int, Gen.String)
            .Select(sample => sample.Item1 switch
            {
                0 => (Result<int, NotFoundError, ConflictError>)sample.Item2,
                1 => new NotFoundError(sample.Item3, "not found"),
                _ => new ConflictError(sample.Item3, "conflict"),
            });

    private static Result<string, NotFoundError, ConflictError> Next(int value) =>
        value % 3 == 0 ? Missing : value.ToString(CultureInfo.InvariantCulture);

    private static Result<string, ValidationError> Check(int value) =>
        value % 3 == 0 ? Invalid : value.ToString(CultureInfo.InvariantCulture);

    private static Result<int, ConflictError> Retry(NotFoundError error) =>
        error.Code.Length % 2 == 0 ? error.Code.Length : Busy;

    private static ValueTask<TCase> Done<TCase>(TCase value) => new(value);

    [Fact]
    public Task BindAsync_Should_EqualBind_When_AwaitedInEveryCell() =>
        GenResult.SampleAsync(async result =>
        {
            Result<string, NotFoundError, ConflictError> expected = result.Bind(Next);

            (await result.BindAsync(v => Task.FromResult(Next(v)))).Should().Be(expected);
            (await result.BindAsync(v => Done(Next(v)))).Should().Be(expected);
            (await Task.FromResult(result).BindAsync(Next)).Should().Be(expected);
            (await Task.FromResult(result).BindAsync(v => Task.FromResult(Next(v)))).Should().Be(expected);
            (await Done(result).BindAsync(Next)).Should().Be(expected);
            (await Done(result).BindAsync(v => Done(Next(v)))).Should().Be(expected);
        }, iter: Iterations);

    [Fact]
    public Task BindWidenAsync_Should_EqualBindWiden_When_AwaitedInEveryCell() =>
        GenResult.SampleAsync(async result =>
        {
            Result<string, NotFoundError, ConflictError, ValidationError> expected = result.BindWiden(Check);

            (await result.BindWidenAsync(v => Task.FromResult(Check(v)))).Should().Be(expected);
            (await result.BindWidenAsync(v => Done(Check(v)))).Should().Be(expected);
            (await Task.FromResult(result).BindWidenAsync(Check)).Should().Be(expected);
            (await Task.FromResult(result).BindWidenAsync(v => Task.FromResult(Check(v)))).Should().Be(expected);
            (await Done(result).BindWidenAsync(Check)).Should().Be(expected);
            (await Done(result).BindWidenAsync(v => Done(Check(v)))).Should().Be(expected);
        }, iter: Iterations);

    [Fact]
    public Task MapAsync_Should_EqualMap_When_AwaitedInEveryCell() =>
        GenResult.SampleAsync(async result =>
        {
            Result<long, NotFoundError, ConflictError> expected = result.Map(v => (long)v * 2);

            (await result.MapAsync(v => Task.FromResult((long)v * 2))).Should().Be(expected);
#if !TYPED_NETSTANDARD_ASSET
            (await result.MapAsync(v => Done((long)v * 2))).Should().Be(expected);
#endif
            (await Task.FromResult(result).MapAsync(v => (long)v * 2)).Should().Be(expected);
            (await Task.FromResult(result).MapAsync(v => Task.FromResult((long)v * 2))).Should().Be(expected);
            (await Done(result).MapAsync(v => (long)v * 2)).Should().Be(expected);
            (await Done(result).MapAsync(v => Done((long)v * 2))).Should().Be(expected);
        }, iter: Iterations);

    [Fact]
    public Task EnsureAsync_Should_EqualEnsure_When_TheErrorHasASlotType() =>
        GenResult.SampleAsync(async result =>
        {
            Result<int, NotFoundError, ConflictError> expected = result.Ensure(v => v > 0, Missing);

            (await result.EnsureAsync(v => Task.FromResult(v > 0), Missing)).Should().Be(expected);
#if !TYPED_NETSTANDARD_ASSET
            (await result.EnsureAsync(v => Done(v > 0), Missing)).Should().Be(expected);
            (await Done(result).EnsureAsync(v => Done(v > 0), Missing)).Should().Be(expected);
#endif
            (await Task.FromResult(result).EnsureAsync(v => v > 0, Missing)).Should().Be(expected);
            (await Task.FromResult(result).EnsureAsync(v => Task.FromResult(v > 0), Missing)).Should().Be(expected);
            (await Done(result).EnsureAsync(v => v > 0, Missing)).Should().Be(expected);
            (await Done(result).EnsureAsync(v => Task.FromResult(v > 0), Missing)).Should().Be(expected);
        }, iter: Iterations);

    [Fact]
    public Task FailIfAsync_Should_EqualFailIf_When_TheErrorAddsASlot() =>
        GenResult.SampleAsync(async result =>
        {
            Result<int, NotFoundError, ConflictError, ValidationError> expected = result.FailIf(v => v < 0, Invalid);

            (await result.FailIfAsync(v => Task.FromResult(v < 0), Invalid)).Should().Be(expected);
#if !TYPED_NETSTANDARD_ASSET
            (await result.FailIfAsync(v => Done(v < 0), Invalid)).Should().Be(expected);
            (await Done(result).FailIfAsync(v => Done(v < 0), Invalid)).Should().Be(expected);
#endif
            (await Task.FromResult(result).FailIfAsync(v => v < 0, Invalid)).Should().Be(expected);
            (await Task.FromResult(result).FailIfAsync(v => Task.FromResult(v < 0), Invalid)).Should().Be(expected);
            (await Done(result).FailIfAsync(v => v < 0, Invalid)).Should().Be(expected);
            (await Done(result).FailIfAsync(v => Task.FromResult(v < 0), Invalid)).Should().Be(expected);
        }, iter: Iterations);

    [Fact]
    public Task MapErrorAsync_Should_EqualMapError_When_AwaitedInEveryCell() =>
        GenResult.SampleAsync(async result =>
        {
            Result<int, ValidationError, ConflictError> expected = result.MapError((NotFoundError e) => new ValidationError(e.Code, "mapped"));

            (await result.MapErrorAsync((NotFoundError e) => Task.FromResult(new ValidationError(e.Code, "mapped")))).Should().Be(expected);
            (await result.MapErrorAsync((NotFoundError e) => Done(new ValidationError(e.Code, "mapped")))).Should().Be(expected);
            (await Task.FromResult(result).MapErrorAsync((NotFoundError e) => new ValidationError(e.Code, "mapped"))).Should().Be(expected);
            (await Task.FromResult(result).MapErrorAsync((NotFoundError e) => Task.FromResult(new ValidationError(e.Code, "mapped")))).Should().Be(expected);
            (await Done(result).MapErrorAsync((NotFoundError e) => new ValidationError(e.Code, "mapped"))).Should().Be(expected);
            (await Done(result).MapErrorAsync((NotFoundError e) => Done(new ValidationError(e.Code, "mapped")))).Should().Be(expected);
        }, iter: Iterations);

    [Fact]
    public Task CompensateAsync_Should_EqualCompensate_When_AwaitedInEveryCell() =>
        GenResult.SampleAsync(async result =>
        {
            Result<int, ConflictError> expected = result.Compensate((NotFoundError e) => Retry(e));

            (await result.CompensateAsync((NotFoundError e) => Task.FromResult(Retry(e)))).Should().Be(expected);
#if !TYPED_NETSTANDARD_ASSET
            (await result.CompensateAsync((NotFoundError e) => Done(Retry(e)))).Should().Be(expected);
            (await Done(result).CompensateAsync((NotFoundError e) => Done(Retry(e)))).Should().Be(expected);
#endif
            (await Task.FromResult(result).CompensateAsync((NotFoundError e) => Task.FromResult(Retry(e)))).Should().Be(expected);
            (await Done(result).CompensateAsync((NotFoundError e) => Task.FromResult(Retry(e)))).Should().Be(expected);
        }, iter: Iterations);
}
