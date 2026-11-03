using CSharpEssentials.Errors;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;

using FluentAssertions;

using Mediator;

namespace CSharpEssentials.Tests.Mediator;

public sealed class ValidationBehaviorAsyncValidatorTests
{
    private static readonly MessageHandlerDelegate<TestValidationCommand, Result> SuccessNext =
        (_, _) => new ValueTask<Result>(Result.Success());

    [Fact]
    public async Task Single_Async_Validator_Failure_Should_Return_Its_Errors()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior =
            new([new YieldingValidator(_ => Error.Validation("AsyncFailed", "async failed"))]);

        Result result = await behavior.Handle(new TestValidationCommand(""), SuccessNext, default);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Code.Should().Be("AsyncFailed");
    }

    [Fact]
    public async Task Single_Async_Validator_Success_Should_Call_Next()
    {
        bool nextCalled = false;
        ValidationBehavior<TestValidationCommand, Result> behavior =
            new([new YieldingValidator(command => command)]);

        Result result = await behavior.Handle(
            new TestValidationCommand("ok"),
            (_, _) =>
            {
                nextCalled = true;
                return new ValueTask<Result>(Result.Success());
            },
            default);

        result.IsSuccess.Should().BeTrue();
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Single_Async_Validator_Exception_Should_Become_Validator_Exception_Error()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior =
            new([new YieldingValidator(_ => throw new InvalidOperationException("async boom"))]);

        Result result = await behavior.Handle(new TestValidationCommand("x"), SuccessNext, default);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Validator.Exception");
        result.FirstError.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public async Task Single_Async_Validator_Cancellation_Should_Propagate()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior =
            new([new YieldingValidator(_ => throw new OperationCanceledException())]);

        Func<Task> act = () => behavior.Handle(new TestValidationCommand("x"), SuccessNext, default).AsTask();

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Mixed_Group_Should_Keep_Registration_Order_Of_Errors()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([
            new YieldingValidator(_ => Error.Validation("AsyncFirst", "async first")),
            new StubValidator("SyncSecond", "sync second"),
            new YieldingValidator(_ => Error.Validation("AsyncThird", "async third"))
        ]);

        Result result = await behavior.Handle(new TestValidationCommand(""), SuccessNext, default);

        result.Errors.Select(error => error.Code).Should().Equal("AsyncFirst", "SyncSecond", "AsyncThird");
    }

    [Fact]
    public async Task Group_With_Throwing_Async_Validator_Should_Collect_Exception_Error_And_Other_Errors()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([
            new YieldingValidator(_ => throw new InvalidOperationException("async boom")),
            new StubValidator("SyncFailed", "sync failed")
        ]);

        Result result = await behavior.Handle(new TestValidationCommand(""), SuccessNext, default);

        result.Errors.Select(error => error.Code).Should().Equal("Validator.Exception", "SyncFailed");
    }

    [Fact]
    public async Task Group_With_Cancelled_Async_Validator_Should_Propagate()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([
            new StubValidator(),
            new YieldingValidator(_ => throw new OperationCanceledException())
        ]);

        Func<Task> act = () => behavior.Handle(new TestValidationCommand("x"), SuccessNext, default).AsTask();

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Group_Of_Passing_Async_Validators_Should_Call_Next()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([
            new YieldingValidator(command => command),
            new YieldingValidator(command => command)
        ]);

        Result result = await behavior.Handle(new TestValidationCommand("ok"), (_, _) => new ValueTask<Result>(Result.Failure(Error.Failure("FromNext", "next"))), default);

        result.FirstError.Code.Should().Be("FromNext");
    }

    [Fact]
    public async Task Synchronously_Throwing_Validator_Cancellation_Should_Propagate()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([new ThrowingIValidator(new OperationCanceledException())]);

        Func<Task> act = () => behavior.Handle(new TestValidationCommand("x"), SuccessNext, default).AsTask();

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Synchronously_Throwing_Validator_Exception_Should_Become_Validator_Exception_Error()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([new ThrowingIValidator(new InvalidOperationException("sync boom"))]);

        Result result = await behavior.Handle(new TestValidationCommand("x"), SuccessNext, default);

        result.FirstError.Code.Should().Be("Validator.Exception");
    }

    private sealed class YieldingValidator(Func<TestValidationCommand, Result<TestValidationCommand>> validate)
        : IValidator<TestValidationCommand>
    {
        public async ValueTask<Result<TestValidationCommand>> ValidateAsync(TestValidationCommand instance, CancellationToken ct = default)
        {
            await Task.Yield();
            return validate(instance);
        }
    }

    private sealed class ThrowingIValidator(Exception exception) : IValidator<TestValidationCommand>
    {
        public ValueTask<Result<TestValidationCommand>> ValidateAsync(TestValidationCommand instance, CancellationToken ct = default) =>
            throw exception;
    }
}
