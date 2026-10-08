using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

public class ResultMapErrorAsyncTests
{
    private static readonly Error[] ThreeErrors =
    [
        Error.Validation("First.Code", "First"),
        Error.NotFound("Second.Code", "Second"),
        Error.Conflict("Third.Code", "Third")
    ];

    private static readonly string[] OriginalCodes = ["First.Code", "Second.Code", "Third.Code"];
    private static readonly string[] MappedCodes = ["Mapped.First.Code", "Mapped.Second.Code", "Mapped.Third.Code"];

    private readonly List<string> _seen = [];
    private int _calls;
    private int _inFlight;
    private int _maxInFlight;

    private static Error Prefix(Error error) => Error.Failure($"Mapped.{error.Code}", error.Description);

    private Error MapOne(Error error)
    {
        _calls++;
        _seen.Add(error.Code);
        return Prefix(error);
    }

    private Error[] MapAll(Error[] errors)
    {
        _calls++;
        _seen.AddRange(errors.Select(e => e.Code));
        return errors.Select(Prefix).ToArray();
    }

    private async Task<Error> MapOneTask(Error error)
    {
        _maxInFlight = Math.Max(_maxInFlight, ++_inFlight);
        await Task.Yield();
        _inFlight--;
        return MapOne(error);
    }

    private async Task<Error[]> MapAllTask(Error[] errors)
    {
        await Task.Yield();
        return MapAll(errors);
    }

    private async ValueTask<Error> MapOneValueTask(Error error)
    {
        _maxInFlight = Math.Max(_maxInFlight, ++_inFlight);
        await Task.Yield();
        _inFlight--;
        return MapOne(error);
    }

    private async ValueTask<Error[]> MapAllValueTask(Error[] errors)
    {
        await Task.Yield();
        return MapAll(errors);
    }

    #region Result.MapErrorAsync

    [Fact]
    public async Task Result_MapErrorAsync_Instance_TaskError_WithSuccess_ShouldNotInvokeMapper()
    {
        Result mapped = await Result.Success().MapErrorAsync(MapOneTask);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_MapErrorAsync_Instance_TaskError_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result mapped = await Result.Failure(ThreeErrors).MapErrorAsync(MapOneTask);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(3);
        _seen.Should().Equal(OriginalCodes);
        _maxInFlight.Should().Be(1);
    }

    [Fact]
    public async Task Result_MapErrorAsync_Instance_TaskArray_WithSuccess_ShouldNotInvokeMapper()
    {
        Result mapped = await Result.Success().MapErrorAsync(MapAllTask);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_MapErrorAsync_Instance_TaskArray_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result mapped = await Result.Failure(ThreeErrors).MapErrorAsync(MapAllTask);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(1);
        _seen.Should().Equal(OriginalCodes);
    }

    [Fact]
    public async Task Result_MapErrorAsync_Instance_ValueTaskError_WithSuccess_ShouldNotInvokeMapper()
    {
        Result mapped = await Result.Success().MapErrorAsync(MapOneValueTask);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_MapErrorAsync_Instance_ValueTaskError_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result mapped = await Result.Failure(ThreeErrors).MapErrorAsync(MapOneValueTask);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(3);
        _seen.Should().Equal(OriginalCodes);
        _maxInFlight.Should().Be(1);
    }

    [Fact]
    public async Task Result_MapErrorAsync_Instance_ValueTaskArray_WithSuccess_ShouldNotInvokeMapper()
    {
        Result mapped = await Result.Success().MapErrorAsync(MapAllValueTask);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_MapErrorAsync_Instance_ValueTaskArray_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result mapped = await Result.Failure(ThreeErrors).MapErrorAsync(MapAllValueTask);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(1);
        _seen.Should().Equal(OriginalCodes);
    }

    [Fact]
    public async Task Result_MapErrorAsync_TaskSource_SyncError_WithSuccess_ShouldNotInvokeMapper()
    {
        Result mapped = await Task.FromResult(Result.Success()).MapErrorAsync(MapOne);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_MapErrorAsync_TaskSource_SyncError_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result mapped = await Task.FromResult(Result.Failure(ThreeErrors)).MapErrorAsync(MapOne);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(3);
        _seen.Should().Equal(OriginalCodes);
    }

    [Fact]
    public async Task Result_MapErrorAsync_TaskSource_SyncArray_WithSuccess_ShouldNotInvokeMapper()
    {
        Result mapped = await Task.FromResult(Result.Success()).MapErrorAsync(MapAll);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_MapErrorAsync_TaskSource_SyncArray_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result mapped = await Task.FromResult(Result.Failure(ThreeErrors)).MapErrorAsync(MapAll);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(1);
        _seen.Should().Equal(OriginalCodes);
    }

    [Fact]
    public async Task Result_MapErrorAsync_TaskSource_TaskError_WithSuccess_ShouldNotInvokeMapper()
    {
        Result mapped = await Task.FromResult(Result.Success()).MapErrorAsync(MapOneTask);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_MapErrorAsync_TaskSource_TaskError_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result mapped = await Task.FromResult(Result.Failure(ThreeErrors)).MapErrorAsync(MapOneTask);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(3);
        _seen.Should().Equal(OriginalCodes);
        _maxInFlight.Should().Be(1);
    }

    [Fact]
    public async Task Result_MapErrorAsync_TaskSource_TaskArray_WithSuccess_ShouldNotInvokeMapper()
    {
        Result mapped = await Task.FromResult(Result.Success()).MapErrorAsync(MapAllTask);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_MapErrorAsync_TaskSource_TaskArray_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result mapped = await Task.FromResult(Result.Failure(ThreeErrors)).MapErrorAsync(MapAllTask);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(1);
        _seen.Should().Equal(OriginalCodes);
    }

    [Fact]
    public async Task Result_MapErrorAsync_ValueTaskSource_SyncError_WithSuccess_ShouldNotInvokeMapper()
    {
        Result mapped = await new ValueTask<Result>(Result.Success()).MapErrorAsync(MapOne);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_MapErrorAsync_ValueTaskSource_SyncError_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result mapped = await new ValueTask<Result>(Result.Failure(ThreeErrors)).MapErrorAsync(MapOne);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(3);
        _seen.Should().Equal(OriginalCodes);
    }

    [Fact]
    public async Task Result_MapErrorAsync_ValueTaskSource_SyncArray_WithSuccess_ShouldNotInvokeMapper()
    {
        Result mapped = await new ValueTask<Result>(Result.Success()).MapErrorAsync(MapAll);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_MapErrorAsync_ValueTaskSource_SyncArray_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result mapped = await new ValueTask<Result>(Result.Failure(ThreeErrors)).MapErrorAsync(MapAll);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(1);
        _seen.Should().Equal(OriginalCodes);
    }

    [Fact]
    public async Task Result_MapErrorAsync_ValueTaskSource_ValueTaskError_WithSuccess_ShouldNotInvokeMapper()
    {
        Result mapped = await new ValueTask<Result>(Result.Success()).MapErrorAsync(MapOneValueTask);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_MapErrorAsync_ValueTaskSource_ValueTaskError_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result mapped = await new ValueTask<Result>(Result.Failure(ThreeErrors)).MapErrorAsync(MapOneValueTask);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(3);
        _seen.Should().Equal(OriginalCodes);
        _maxInFlight.Should().Be(1);
    }

    [Fact]
    public async Task Result_MapErrorAsync_ValueTaskSource_ValueTaskArray_WithSuccess_ShouldNotInvokeMapper()
    {
        Result mapped = await new ValueTask<Result>(Result.Success()).MapErrorAsync(MapAllValueTask);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Result_MapErrorAsync_ValueTaskSource_ValueTaskArray_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result mapped = await new ValueTask<Result>(Result.Failure(ThreeErrors)).MapErrorAsync(MapAllValueTask);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(1);
        _seen.Should().Equal(OriginalCodes);
    }

    #endregion

    #region Result<int>.MapErrorAsync

    [Fact]
    public async Task ResultT_MapErrorAsync_Instance_TaskError_WithSuccess_ShouldNotInvokeMapper()
    {
        Result<int> mapped = await 42.ToResult().MapErrorAsync(MapOneTask);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_Instance_TaskError_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result<int> mapped = await Result<int>.Failure(ThreeErrors).MapErrorAsync(MapOneTask);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(3);
        _seen.Should().Equal(OriginalCodes);
        _maxInFlight.Should().Be(1);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_Instance_TaskArray_WithSuccess_ShouldNotInvokeMapper()
    {
        Result<int> mapped = await 42.ToResult().MapErrorAsync(MapAllTask);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_Instance_TaskArray_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result<int> mapped = await Result<int>.Failure(ThreeErrors).MapErrorAsync(MapAllTask);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(1);
        _seen.Should().Equal(OriginalCodes);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_Instance_ValueTaskError_WithSuccess_ShouldNotInvokeMapper()
    {
        Result<int> mapped = await 42.ToResult().MapErrorAsync(MapOneValueTask);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_Instance_ValueTaskError_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result<int> mapped = await Result<int>.Failure(ThreeErrors).MapErrorAsync(MapOneValueTask);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(3);
        _seen.Should().Equal(OriginalCodes);
        _maxInFlight.Should().Be(1);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_Instance_ValueTaskArray_WithSuccess_ShouldNotInvokeMapper()
    {
        Result<int> mapped = await 42.ToResult().MapErrorAsync(MapAllValueTask);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_Instance_ValueTaskArray_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result<int> mapped = await Result<int>.Failure(ThreeErrors).MapErrorAsync(MapAllValueTask);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(1);
        _seen.Should().Equal(OriginalCodes);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_TaskSource_SyncError_WithSuccess_ShouldNotInvokeMapper()
    {
        Result<int> mapped = await Task.FromResult(42.ToResult()).MapErrorAsync(MapOne);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_TaskSource_SyncError_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result<int> mapped = await Task.FromResult(Result<int>.Failure(ThreeErrors)).MapErrorAsync(MapOne);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(3);
        _seen.Should().Equal(OriginalCodes);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_TaskSource_SyncArray_WithSuccess_ShouldNotInvokeMapper()
    {
        Result<int> mapped = await Task.FromResult(42.ToResult()).MapErrorAsync(MapAll);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_TaskSource_SyncArray_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result<int> mapped = await Task.FromResult(Result<int>.Failure(ThreeErrors)).MapErrorAsync(MapAll);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(1);
        _seen.Should().Equal(OriginalCodes);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_TaskSource_TaskError_WithSuccess_ShouldNotInvokeMapper()
    {
        Result<int> mapped = await Task.FromResult(42.ToResult()).MapErrorAsync(MapOneTask);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_TaskSource_TaskError_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result<int> mapped = await Task.FromResult(Result<int>.Failure(ThreeErrors)).MapErrorAsync(MapOneTask);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(3);
        _seen.Should().Equal(OriginalCodes);
        _maxInFlight.Should().Be(1);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_TaskSource_TaskArray_WithSuccess_ShouldNotInvokeMapper()
    {
        Result<int> mapped = await Task.FromResult(42.ToResult()).MapErrorAsync(MapAllTask);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_TaskSource_TaskArray_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result<int> mapped = await Task.FromResult(Result<int>.Failure(ThreeErrors)).MapErrorAsync(MapAllTask);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(1);
        _seen.Should().Equal(OriginalCodes);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_ValueTaskSource_SyncError_WithSuccess_ShouldNotInvokeMapper()
    {
        Result<int> mapped = await new ValueTask<Result<int>>(42.ToResult()).MapErrorAsync(MapOne);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_ValueTaskSource_SyncError_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result<int> mapped = await new ValueTask<Result<int>>(Result<int>.Failure(ThreeErrors)).MapErrorAsync(MapOne);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(3);
        _seen.Should().Equal(OriginalCodes);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_ValueTaskSource_SyncArray_WithSuccess_ShouldNotInvokeMapper()
    {
        Result<int> mapped = await new ValueTask<Result<int>>(42.ToResult()).MapErrorAsync(MapAll);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_ValueTaskSource_SyncArray_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result<int> mapped = await new ValueTask<Result<int>>(Result<int>.Failure(ThreeErrors)).MapErrorAsync(MapAll);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(1);
        _seen.Should().Equal(OriginalCodes);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_ValueTaskSource_ValueTaskError_WithSuccess_ShouldNotInvokeMapper()
    {
        Result<int> mapped = await new ValueTask<Result<int>>(42.ToResult()).MapErrorAsync(MapOneValueTask);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_ValueTaskSource_ValueTaskError_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result<int> mapped = await new ValueTask<Result<int>>(Result<int>.Failure(ThreeErrors)).MapErrorAsync(MapOneValueTask);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(3);
        _seen.Should().Equal(OriginalCodes);
        _maxInFlight.Should().Be(1);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_ValueTaskSource_ValueTaskArray_WithSuccess_ShouldNotInvokeMapper()
    {
        Result<int> mapped = await new ValueTask<Result<int>>(42.ToResult()).MapErrorAsync(MapAllValueTask);

        _calls.Should().Be(0);
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public async Task ResultT_MapErrorAsync_ValueTaskSource_ValueTaskArray_WithThreeErrors_ShouldMapEveryErrorInOrder()
    {
        Result<int> mapped = await new ValueTask<Result<int>>(Result<int>.Failure(ThreeErrors)).MapErrorAsync(MapAllValueTask);

        mapped.IsFailure.Should().BeTrue();
        mapped.Errors.Select(e => e.Code).Should().Equal(MappedCodes);
        _calls.Should().Be(1);
        _seen.Should().Equal(OriginalCodes);
    }

    #endregion
}
