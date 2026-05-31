using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeNewFeaturesTests
{
    // FromTry
    [Fact]
    public void FromTry_Should_ReturnValue_When_FactorySucceeds()
    {
        Maybe<int> result = Maybe<int>.FromTry(() => 42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void FromTry_Should_ReturnNone_When_FactoryThrows()
    {
        Maybe<int> result = Maybe<int>.FromTry(() => throw new InvalidOperationException("boom"));
        result.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void Maybe_FromTry_Should_ReturnValue_When_FactorySucceeds()
    {
        Maybe<string> result = Maybe<string>.FromTry(() => "hello");
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    // TapNone
    [Fact]
    public void TapNone_Should_ExecuteAction_When_HasNoValue()
    {
        bool called = false;
        Maybe<int> maybe = Maybe<int>.None;

        maybe.TapNone(() => called = true);

        called.Should().BeTrue();
    }

    [Fact]
    public void TapNone_Should_NotExecuteAction_When_HasValue()
    {
        bool called = false;
        Maybe<int> maybe = 42;

        maybe.TapNone(() => called = true);

        called.Should().BeFalse();
    }

    [Fact]
    public void TapNone_Should_ReturnSameMaybe()
    {
        Maybe<int> maybe = Maybe<int>.None;
        Maybe<int> result = maybe.TapNone(() => { });
        result.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public async Task TapNoneAsync_Should_ExecuteAction_When_HasNoValue()
    {
        bool called = false;
        Maybe<int> maybe = Maybe<int>.None;

        await maybe.TapNoneAsync(async () => { await Task.Yield(); called = true; });

        called.Should().BeTrue();
    }

    [Fact]
    public async Task TapNoneAsync_Task_SyncAction_Should_ExecuteAction_When_HasNoValue()
    {
        bool called = false;
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        await maybeTask.TapNoneAsync(() => called = true);

        called.Should().BeTrue();
    }

    [Fact]
    public async Task TapNoneAsync_Task_SyncAction_Should_NotExecuteAction_When_HasValue()
    {
        bool called = false;
        Task<Maybe<int>> maybeTask = Task.FromResult<Maybe<int>>(42);

        await maybeTask.TapNoneAsync(() => called = true);

        called.Should().BeFalse();
    }

    [Fact]
    public async Task TapNoneAsync_Task_AsyncAction_Should_ExecuteAction_When_HasNoValue()
    {
        bool called = false;
        Task<Maybe<int>> maybeTask = Task.FromResult(Maybe<int>.None);

        await maybeTask.TapNoneAsync(async () => { await Task.Yield(); called = true; });

        called.Should().BeTrue();
    }

    [Fact]
    public async Task TapNoneAsync_ValueTask_SyncAction_Should_ExecuteAction_When_HasNoValue()
    {
        bool called = false;
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);

        await maybeTask.TapNoneAsync(() => called = true);

        called.Should().BeTrue();
    }

    [Fact]
    public async Task TapNoneAsync_ValueTask_AsyncAction_Should_ExecuteAction_When_HasNoValue()
    {
        bool called = false;
        ValueTask<Maybe<int>> maybeTask = new(Maybe<int>.None);

        await maybeTask.TapNoneAsync(async () => { await Task.Yield(); called = true; });

        called.Should().BeTrue();
    }

    // GetValueOrElse
    [Fact]
    public void GetValueOrElse_Should_ReturnValue_When_HasValue()
    {
        Maybe<int> maybe = 42;
        int result = maybe.GetValueOrElse(() => 99);
        result.Should().Be(42);
    }

    [Fact]
    public void GetValueOrElse_Should_CallFactory_When_HasNoValue()
    {
        Maybe<int> maybe = Maybe<int>.None;
        int result = maybe.GetValueOrElse(() => 99);
        result.Should().Be(99);
    }

    [Fact]
    public void GetValueOrElse_Should_NotCallFactory_When_HasValue()
    {
        bool factoryCalled = false;
        Maybe<int> maybe = 42;
        maybe.GetValueOrElse(() => { factoryCalled = true; return 99; });
        factoryCalled.Should().BeFalse();
    }

    // OrElse
    [Fact]
    public void OrElse_Should_ReturnSelf_When_HasValue()
    {
        Maybe<int> maybe = 42;
        Maybe<int> result = maybe.OrElse(() => 99);
        result.Value.Should().Be(42);
    }

    [Fact]
    public void OrElse_Should_CallFactory_When_HasNoValue()
    {
        Maybe<int> maybe = Maybe<int>.None;
        Maybe<int> result = maybe.OrElse(() => 99);
        result.Value.Should().Be(99);
    }

    [Fact]
    public async Task OrElseAsync_Should_ReturnSelf_When_HasValue()
    {
        Maybe<int> maybe = 42;
        Maybe<int> result = await maybe.OrElseAsync(() => Task.FromResult<Maybe<int>>(99));
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task OrElseAsync_Should_CallFactory_When_HasNoValue()
    {
        Maybe<int> maybe = Maybe<int>.None;
        Maybe<int> result = await maybe.OrElseAsync(() => Task.FromResult<Maybe<int>>(99));
        result.Value.Should().Be(99);
    }
}
