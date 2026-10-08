using CSharpEssentials.Errors;
using CSharpEssentials.Maybe;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Results;

/// <summary>
/// Compile-time guards for the <c>*Async</c> names. The typed locals stop compiling if a future overload changes the binding.
/// </summary>
public sealed class AsyncNamingBindingTests
{
    private static readonly Error TestError = Error.Failure("TEST", "Test error");

    [Fact]
    public async Task BindAsync_Should_BindTaskOverload_When_ResultTGetsAsyncLambdaReturningResultT()
    {
        Result<int> source = 5;

        Task<Result<string>> pending = source.BindAsync(async v =>
        {
            await Task.Yield();
            return (Result<string>)$"v{v}";
        });

        (await pending).Value.Should().Be("v5");
    }

    [Fact]
    public async Task BindAsync_Should_BindTaskOverload_When_ResultTGetsAsyncLambdaReturningResult()
    {
        Result<int> source = 5;

        Task<Result> pending = source.BindAsync(async v =>
        {
            await Task.Yield();
            return v > 3 ? Result.Failure(TestError) : Result.Success();
        });

        (await pending).FirstError.Should().Be(TestError);
    }

    [Fact]
    public async Task BindAsync_Should_BindTaskOverload_When_ResultGetsAsyncLambdaReturningResultT()
    {
        Result source = Result.Success();

        Task<Result<int>> pending = source.BindAsync(async () =>
        {
            await Task.Yield();
            return (Result<int>)42;
        });

        (await pending).Value.Should().Be(42);
    }

    [Fact]
    public async Task BindAsync_Should_BindTaskOverload_When_ResultGetsAsyncLambdaReturningResult()
    {
        Result source = Result.Failure(TestError);
        bool called = false;

        Task<Result> pending = source.BindAsync(async () =>
        {
            await Task.Yield();
            called = true;
            return Result.Success();
        });

        ((await pending).IsFailure, called).Should().Be((true, false));
    }

    [Fact]
    public async Task WhereAsync_Should_BindTaskOverload_When_MaybeGetsAsyncPredicate()
    {
        Maybe<int> maybe = Maybe<int>.From(10);

        Task<Maybe<int>> pending = maybe.WhereAsync(async v =>
        {
            await Task.Yield();
            return v > 5;
        });

        (await pending).Value.Should().Be(10);
    }

    [Fact]
    public async Task SwitchAsync_Should_AwaitTaskHandlers_When_TaskSourceGetsAsyncLambdasWithoutToken()
    {
        Task<Result> task = Task.FromResult(Result.Success());
        bool called = false;

        await task.SwitchAsync(
            async () =>
            {
                await Task.Delay(50);
                called = true;
            },
            async _ => await Task.Yield());

        called.Should().BeTrue();
    }

    [Fact]
    public async Task ThenDoAsync_Should_AwaitTaskAction_When_TaskSourceGetsAsyncLambdaWithoutToken()
    {
        Task<Result> task = Task.FromResult(Result.Success());
        bool called = false;

        Result result = await task.ThenDoAsync(async () =>
        {
            await Task.Delay(50);
            called = true;
        });

        (result.IsSuccess, called).Should().Be((true, true));
    }

    [Fact]
    public async Task MatchAsync_Should_BindTaskHandlers_When_TaskSourceGetsAsyncLambdas()
    {
        Task<Result<int>> task = Task.FromResult<Result<int>>(5);

        Task<string> pending = task.MatchAsync(
            async v =>
            {
                await Task.Yield();
                return $"ok {v}";
            },
            async _ =>
            {
                await Task.Yield();
                return "failed";
            });

        (await pending).Should().Be("ok 5");
    }

    [Fact]
    public async Task ThenAsync_Should_BindTaskOverload_When_TaskSourceGetsAsyncLambda()
    {
        Task<Result<int>> task = Task.FromResult<Result<int>>(5);

        Task<Result<int>> pending = task.ThenAsync(async v =>
        {
            await Task.Yield();
            return v + 1;
        });

        (await pending).Value.Should().Be(6);
    }

    [Fact]
    public async Task MatchLastAsync_Should_Compile_When_TaskSourceOmitsToken()
    {
        Task<Result> task = Task.FromResult<Result>(TestError);

        string matched = await task.MatchLastAsync(
            () => Task.FromResult("ok"),
            e => Task.FromResult(e.Code));

        matched.Should().Be("TEST");
    }
}
