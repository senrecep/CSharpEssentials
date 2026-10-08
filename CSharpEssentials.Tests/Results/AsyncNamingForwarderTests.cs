using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

/// <summary>The obsolete unsuffixed Task/ValueTask names forward to their <c>*Async</c> replacements.</summary>
[Obsolete("Tests the obsolete unsuffixed async names kept until 7.0.")]
public sealed class AsyncNamingForwarderTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");

    [Fact]
    public async Task Bind_Should_ForwardToBindAsync_When_InstanceCalledWithTaskFunc()
    {
        Result<int> source = 5;

        Result<int> bound = await source.Bind(v => Task.FromResult<Result<int>>(v * 2));

        bound.Value.Should().Be(10);
    }

    [Fact]
    public async Task Bind_Should_ForwardToBindAsync_When_InstanceCalledWithValueTaskFunc()
    {
        Result source = Result.Success();

        Result bound = await source.Bind(() => ValueTask.FromResult<Result>(TestError));

        bound.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task Else_Should_ForwardToElseAsync_When_SourceIsTask()
    {
        Task<Result<int>> task = Task.FromResult<Result<int>>(TestError);

        Result<int> result = await task.Else(99);

        result.Value.Should().Be(99);
    }

    [Fact]
    public async Task FailIf_Should_ForwardToFailIfAsync_When_SourceIsValueTask()
    {
        ValueTask<Result<int>> task = ValueTask.FromResult<Result<int>>(5);

        Result<int> result = await task.FailIf(v => v > 3, TestError);

        result.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task Match_Should_ForwardToMatchAsync_When_SourceIsTask()
    {
        Task<Result<int>> task = Task.FromResult<Result<int>>(5);

        string matched = await task.Match(v => $"ok {v}", _ => "failed");

        matched.Should().Be("ok 5");
    }

    [Fact]
    public async Task MatchLast_Should_ForwardToMatchLastAsync_When_SourceIsValueTask()
    {
        ValueTask<Result> task = ValueTask.FromResult<Result>(TestError);

        string matched = await task.MatchLast(() => "ok", e => e.Code);

        matched.Should().Be("TEST");
    }

    [Fact]
    public async Task Switch_Should_ForwardToSwitchAsync_When_SourceIsTask()
    {
        Task<Result> task = Task.FromResult(Result.Success());
        bool called = false;

        await task.Switch(() => called = true, _ => { }, CancellationToken.None);

        called.Should().BeTrue();
    }

    [Fact]
    public async Task Then_Should_ForwardToThenAsync_When_SourceIsTask()
    {
        Task<Result<int>> task = Task.FromResult<Result<int>>(5);

        Result<int> result = await task.Then(v => v + 1);

        result.Value.Should().Be(6);
    }

    [Fact]
    public async Task ThenDo_Should_ForwardToThenDoAsync_When_SourceIsTask()
    {
        Task<Result> task = Task.FromResult(Result.Success());
        bool called = false;

        Result result = await task.ThenDo(() => called = true, CancellationToken.None);

        (result.IsSuccess, called).Should().Be((true, true));
    }

    [Fact]
    public async Task Where_Should_ForwardToWhereAsync_When_InstanceCalledWithTaskPredicate()
    {
        Maybe<int> maybe = 10;

        Maybe<int> result = await maybe.Where(v => Task.FromResult(v > 5));

        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task Where_Should_ForwardToWhereAsync_When_SourceIsValueTask()
    {
        ValueTask<Maybe<int>> task = ValueTask.FromResult<Maybe<int>>(3);

        Maybe<int> result = await task.Where(v => v > 5);

        result.HasNoValue.Should().BeTrue();
    }
}
