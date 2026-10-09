using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using MaybeFactory = CSharpEssentials.Maybe.Maybe;

namespace CSharpEssentials.Tests.Maybe;

/// <summary>
/// Compile-time guards for the Maybe <c>*Async</c> names. The typed locals stop compiling if a future overload changes the binding.
/// </summary>
public sealed class MaybeAsyncNamingBindingTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");

    [Fact]
    public async Task ExecuteAsync_Should_BindTaskOverload_When_InstanceGetsUntypedAsyncLambda()
    {
        Maybe<int> maybe = Maybe<int>.From(5);
        int captured = 0;

        Task pending = maybe.ExecuteAsync(async v =>
        {
            await Task.Yield();
            captured = v;
        });
        await pending;

        captured.Should().Be(5);
    }

    [Fact]
    public async Task ExecuteAsync_Should_BindValueTaskOverload_When_InstanceGetsValueTaskLambda()
    {
        Maybe<int> maybe = Maybe<int>.From(5);
        int captured = 0;

        Task pending = maybe.ExecuteAsync(v =>
        {
            captured = v;
            return ValueTask.CompletedTask;
        });
        await pending;

        captured.Should().Be(5);
    }

    [Fact]
    public async Task ExecuteAsync_Should_BindTaskOverload_When_TaskSourceGetsAsyncLambda()
    {
        Task<Maybe<int>> source = Task.FromResult(Maybe<int>.From(7));
        int captured = 0;

        Task pending = source.ExecuteAsync(async v =>
        {
            await Task.Yield();
            captured = v;
        });
        await pending;

        captured.Should().Be(7);
    }

    [Fact]
    public async Task ExecuteAsync_Should_BindValueTaskOverload_When_ValueTaskSourceGetsValueTaskLambda()
    {
        ValueTask<Maybe<int>> source = ValueTask.FromResult(Maybe<int>.From(7));
        int captured = 0;

        Task pending = source.ExecuteAsync(v =>
        {
            captured = v;
            return ValueTask.CompletedTask;
        });
        await pending;

        captured.Should().Be(7);
    }

    [Fact]
    public async Task ExecuteNoValueAsync_Should_BindTaskOverload_When_InstanceGetsUntypedAsyncLambda()
    {
        Maybe<int> maybe = Maybe<int>.None;
        bool called = false;

        Task pending = maybe.ExecuteNoValueAsync(async () =>
        {
            await Task.Yield();
            called = true;
        });
        await pending;

        called.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteNoValueAsync_Should_BindValueTaskOverload_When_ValueTaskSourceGetsValueTaskLambda()
    {
        ValueTask<Maybe<int>> source = ValueTask.FromResult(Maybe<int>.None);
        bool called = false;

        Task pending = source.ExecuteNoValueAsync(() =>
        {
            called = true;
            return ValueTask.CompletedTask;
        });
        await pending;

        called.Should().BeTrue();
    }

    [Fact]
    public async Task OrAsync_Should_BindTaskOverload_When_InstanceGetsUntypedAsyncFallback()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Task<Maybe<int>> pending = maybe.OrAsync(async () =>
        {
            await Task.Yield();
            return 9;
        });

        (await pending).Value.Should().Be(9);
    }

    [Fact]
    public async Task OrAsync_Should_BindValueTaskOverload_When_InstanceGetsValueTaskFallback()
    {
        Maybe<int> maybe = Maybe<int>.None;

        ValueTask<Maybe<int>> pending = maybe.OrAsync(() => ValueTask.FromResult(9));

        (await pending).Value.Should().Be(9);
    }

    [Fact]
    public async Task OrAsync_Should_BindTaskOverload_When_TaskSourceGetsTaskFallback()
    {
        Task<Maybe<int>> source = Task.FromResult(Maybe<int>.None);

        Task<Maybe<int>> pending = source.OrAsync(Task.FromResult(4));

        (await pending).Value.Should().Be(4);
    }

    [Fact]
    public async Task OrAsync_Should_BindValueTaskOverload_When_ValueTaskSourceGetsValueTaskFallback()
    {
        ValueTask<Maybe<int>> source = ValueTask.FromResult(Maybe<int>.None);

        ValueTask<Maybe<int>> pending = source.OrAsync(ValueTask.FromResult(4));

        (await pending).Value.Should().Be(4);
    }

    [Fact]
    public async Task MatchAsync_Should_BindTaskOverload_When_InstanceGetsUntypedAsyncLambdas()
    {
        Maybe<int> maybe = Maybe<int>.From(3);

        Task<string> pending = maybe.MatchAsync(
            async (v, _) =>
            {
                await Task.Yield();
                return $"some{v}";
            },
            async _ =>
            {
                await Task.Yield();
                return "none";
            });

        (await pending).Should().Be("some3");
    }

    [Fact]
    public async Task MatchAsync_Should_BindValueTaskOverload_When_InstanceGetsValueTaskLambdas()
    {
        Maybe<int> maybe = Maybe<int>.None;

        ValueTask<string> pending = maybe.MatchAsync(
            (v, _) => ValueTask.FromResult($"some{v}"),
            _ => ValueTask.FromResult("none"));

        (await pending).Should().Be("none");
    }

    [Fact]
    public async Task MatchAsync_Should_BindTaskOverload_When_KeyValuePairMaybeGetsAsyncLambdas()
    {
        Maybe<KeyValuePair<string, int>> maybe = Maybe<KeyValuePair<string, int>>.From(new KeyValuePair<string, int>("k", 2));

        Task<string> pending = maybe.MatchAsync(
            async (k, v, _) =>
            {
                await Task.Yield();
                return $"{k}{v}";
            },
            async _ =>
            {
                await Task.Yield();
                return "none";
            });

        (await pending).Should().Be("k2");
    }

    [Fact]
    public async Task ToMaybeResultAsync_Should_BindTaskOverload_When_TaskSourceHasValue()
    {
        Task<Maybe<int>> source = Task.FromResult(Maybe<int>.From(1));

        Task<Result<int>> pending = source.ToMaybeResultAsync();

        (await pending).Value.Should().Be(1);
    }

    [Fact]
    public async Task ToMaybeResultAsync_Should_BindValueTaskOverload_When_ValueTaskSourceHasNoValue()
    {
        ValueTask<Maybe<int>> source = ValueTask.FromResult(Maybe<int>.None);

        ValueTask<Result<int>> pending = source.ToMaybeResultAsync(TestError);

        (await pending).FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task ToMaybeUnitResultAsync_Should_BindTaskOverload_When_TaskSourceHasNoValue()
    {
        Task<Maybe<int>> source = Task.FromResult(Maybe<int>.None);

        Task<Result> pending = source.ToMaybeUnitResultAsync(TestError);

        (await pending).FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task ToMaybeUnitResultAsync_Should_BindValueTaskOverload_When_ValueTaskSourceHasValue()
    {
        ValueTask<Maybe<int>> source = ValueTask.FromResult(Maybe<int>.From(1));

        ValueTask<Result> pending = source.ToMaybeUnitResultAsync();

        (await pending).IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task FromAsync_Should_BindTaskOverload_When_StaticFactoryGetsTask()
    {
        Task<string?> task = Task.FromResult<string?>("hello");

        Task<Maybe<string>> pending = MaybeFactory.FromAsync(task);

        (await pending).Value.Should().Be("hello");
    }

    [Fact]
    public async Task FromAsync_Should_BindTaskFuncOverload_When_StaticFactoryGetsAsyncLambda()
    {
        Task<Maybe<string>> pending = MaybeFactory.FromAsync(async () =>
        {
            await Task.Yield();
            return (string?)null;
        });

        (await pending).HasNoValue.Should().BeTrue();
    }
}
