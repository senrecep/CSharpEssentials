using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using MaybeFactory = CSharpEssentials.Maybe.Maybe;

namespace CSharpEssentials.Tests.Maybe;

/// <summary>The obsolete unsuffixed Maybe Task/ValueTask names forward to their <c>*Async</c> replacements.</summary>
[Obsolete("Tests the obsolete unsuffixed async names kept until 7.0.")]
public sealed class MaybeAsyncNamingForwarderTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");

    [Fact]
    public async Task Execute_Should_ForwardToExecuteAsync_When_InstanceCalledWithTaskFunc()
    {
        Maybe<int> maybe = Maybe<int>.From(5);
        int captured = 0;

        await maybe.Execute(v =>
        {
            captured = v;
            return Task.CompletedTask;
        });

        captured.Should().Be(5);
    }

    [Fact]
    public async Task Execute_Should_ForwardToExecuteAsync_When_SourceIsValueTask()
    {
        ValueTask<Maybe<int>> source = ValueTask.FromResult(Maybe<int>.From(7));
        int captured = 0;

        await source.Execute(v => captured = v);

        captured.Should().Be(7);
    }

    [Fact]
    public async Task ExecuteNoValue_Should_ForwardToExecuteNoValueAsync_When_SourceIsTask()
    {
        Task<Maybe<int>> source = Task.FromResult(Maybe<int>.None);
        bool called = false;

        await source.ExecuteNoValue(() => called = true);

        called.Should().BeTrue();
    }

    [Fact]
    public async Task Or_Should_ForwardToOrAsync_When_InstanceCalledWithTaskFunc()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Maybe<int> result = await maybe.Or(() => Task.FromResult(9));

        result.Value.Should().Be(9);
    }

    [Fact]
    public async Task Or_Should_ForwardToOrAsync_When_SourceIsValueTask()
    {
        ValueTask<Maybe<int>> source = ValueTask.FromResult(Maybe<int>.None);

        Maybe<int> result = await source.Or(ValueTask.FromResult(4));

        result.Value.Should().Be(4);
    }

    [Fact]
    public async Task Match_Should_ForwardToMatchAsync_When_InstanceCalledWithValueTaskFuncs()
    {
        Maybe<int> maybe = Maybe<int>.From(3);

        string result = await maybe.Match(
            (v, _) => ValueTask.FromResult($"some{v}"),
            _ => ValueTask.FromResult("none"));

        result.Should().Be("some3");
    }

    [Fact]
    public async Task Match_Should_ForwardToMatchAsync_When_KeyValuePairMaybeCalledWithTaskFuncs()
    {
        Maybe<KeyValuePair<string, int>> maybe = Maybe<KeyValuePair<string, int>>.From(new KeyValuePair<string, int>("k", 2));

        string result = await maybe.Match(
            (k, v, _) => Task.FromResult($"{k}{v}"),
            _ => Task.FromResult("none"));

        result.Should().Be("k2");
    }

    [Fact]
    public async Task ToMaybeResult_Should_ForwardToToMaybeResultAsync_When_SourceIsTask()
    {
        Task<Maybe<int>> source = Task.FromResult(Maybe<int>.None);

        Result<int> result = await source.ToMaybeResult(TestError);

        result.FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task ToMaybeUnitResult_Should_ForwardToToMaybeUnitResultAsync_When_SourceIsValueTask()
    {
        ValueTask<Maybe<int>> source = ValueTask.FromResult(Maybe<int>.From(1));

        Result result = await source.ToMaybeUnitResult();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task From_Should_ForwardToFromAsync_When_StaticFactoryGetsTask()
    {
        Task<string?> task = Task.FromResult<string?>("hello");

        Maybe<string> result = await MaybeFactory.From<string>(task);

        result.Value.Should().Be("hello");
    }
}
