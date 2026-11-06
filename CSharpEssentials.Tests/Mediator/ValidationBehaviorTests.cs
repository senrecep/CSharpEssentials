using Mediator;

using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;

using FluentAssertions;

namespace CSharpEssentials.Tests.Mediator;

internal sealed record TestValidationCommand(string Name) : ICommand<Result>;

internal sealed class StubValidator : Validator<TestValidationCommand>
{
    private readonly string? _errorCode;
    private readonly string? _message;

    public StubValidator() { }

    public StubValidator(string errorCode, string message)
    {
        _errorCode = errorCode;
        _message = message;
    }

    protected override ValueTask Configure(TestValidationCommand model, RuleContext<TestValidationCommand> rules, CancellationToken ct = default)
    {
        if (_errorCode is not null)
            rules.For(() => model.Name).Must(_ => false, _errorCode, _message!);
        return ValueTask.CompletedTask;
    }
}

internal sealed class ThrowingValidator : Validator<TestValidationCommand>
{
    protected override ValueTask Configure(TestValidationCommand model, RuleContext<TestValidationCommand> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Name).Must(_ => throw new InvalidOperationException("Validator exploded"), "code", "msg");
        return ValueTask.CompletedTask;
    }
}

internal sealed class OceCancellingValidator : Validator<TestValidationCommand>
{
    protected override ValueTask Configure(TestValidationCommand model, RuleContext<TestValidationCommand> rules, CancellationToken ct = default)
        => throw new OperationCanceledException();
}

internal sealed class OrderedStubValidator(int order, string errorCode, string message) : Validator<TestValidationCommand>
{
    public override int Order => order;

    protected override ValueTask Configure(TestValidationCommand model, RuleContext<TestValidationCommand> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Name).Must(_ => false, errorCode, message);
        return ValueTask.CompletedTask;
    }
}

public class ValidationBehaviorTests
{
    private static readonly MessageHandlerDelegate<TestValidationCommand, Result> SuccessNext =
        (message, ct) => new ValueTask<Result>(Result.Success());

    [Fact]
    public async Task Handle_Should_Call_Next_When_No_Validators_Provided()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([]);
        TestValidationCommand command = new("test");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Call_Next_When_Validation_Succeeds()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([new StubValidator()]);
        TestValidationCommand command = new("test");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Validation_Fails()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([new StubValidator("NameRequired", "Name is required")]);
        TestValidationCommand command = new("");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("NameRequired");
        result.FirstError.Description.Should().Be("Name is required");
    }

    [Fact]
    public async Task Handle_Should_Return_Generic_Failure_For_Generic_Result()
    {
        ValidationBehavior<TestValidationCommand, Result<int>> behavior = new([new StubValidator("NameRequired", "Name is required")]);
        TestValidationCommand command = new("");

        Result<int> result = await behavior.Handle(command, (_, _) => new ValueTask<Result<int>>(42), default);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_Should_Throw_EnhancedValidationException_When_TResponse_Is_Not_Result()
    {
        // When TResponse cannot carry error info (not Result/Result<T>), EnhancedValidationException is thrown.
        ValidationBehavior<TestValidationCommand, string> behavior = new([new StubValidator("NameRequired", "Name is required")]);
        TestValidationCommand command = new("");

        Func<Task> act = () => behavior.Handle(command, (_, _) => new ValueTask<string>("ok"), default).AsTask();

        var assertion = await act.Should().ThrowAsync<EnhancedValidationException>();
        assertion.Which.Errors.Should().ContainSingle(e => e.Code == "NameRequired");
    }

    [Fact]
    public async Task Handle_Should_Aggregate_Errors_From_Multiple_Validators()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([
            new StubValidator("E1", "Error 1"),
            new StubValidator("E2", "Error 2")
        ]);
        TestValidationCommand command = new("");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_Should_Return_ValidationErrorType_For_All_Errors()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([new StubValidator("Invalid", "Invalid value")]);
        TestValidationCommand command = new("");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.Errors.Should().AllSatisfy(e => e.Type.Should().Be(ErrorType.Validation));
    }

    [Fact]
    public async Task Handle_Should_Throw_OperationCanceledException_When_TokenAlreadyCancelled()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([new StubValidator()]);
        TestValidationCommand command = new("test");
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        Func<Task> act = () => behavior.Handle(command, SuccessNext, cts.Token).AsTask();

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_Should_Return_ExceptionError_When_Validator_Throws()
    {
        // Non-OCE exceptions are converted to Error.Failure, never rethrown.
        ValidationBehavior<TestValidationCommand, Result> behavior = new([new ThrowingValidator()]);
        TestValidationCommand command = new("test");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Validator.Exception");
        result.FirstError.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public async Task Handle_Should_Propagate_OperationCanceledException_From_Validator()
    {
        // OCE from a validator must still propagate, it is not converted to an Error.
        ValidationBehavior<TestValidationCommand, Result> behavior = new([new OceCancellingValidator()]);
        TestValidationCommand command = new("test");

        Func<Task> act = () => behavior.Handle(command, SuccessNext, default).AsTask();

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_Should_Deduplicate_Identical_Errors_From_Multiple_Validators()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([
            new StubValidator("DupCode", "Duplicate error"),
            new StubValidator("DupCode", "Duplicate error")
        ]);
        TestValidationCommand command = new("");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
    }

    // =========================================================================
    // Order
    // =========================================================================

    [Fact]
    public async Task Handle_Should_AccumulateErrors_InGroupOrder_WhenValidatorsHaveDifferentOrders()
    {
        // Validators are registered with Order=1 first, Order=0 second,
        // but errors must appear in ascending Order (0 before 1).
        ValidationBehavior<TestValidationCommand, Result> behavior = new([
            new OrderedStubValidator(1, "E_ORDER_1", "Error from order 1"),
            new OrderedStubValidator(0, "E_ORDER_0", "Error from order 0")
        ]);
        TestValidationCommand command = new("");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors[0].Code.Should().Be("E_ORDER_0");
        result.Errors[1].Code.Should().Be("E_ORDER_1");
    }

    [Fact]
    public async Task Handle_Should_RunSameOrderValidators_AndAccumulateBothErrors()
    {
        // Two validators sharing Order=0 run concurrently; both errors are collected.
        ValidationBehavior<TestValidationCommand, Result> behavior = new([
            new OrderedStubValidator(0, "E_A", "Error A"),
            new OrderedStubValidator(0, "E_B", "Error B")
        ]);
        TestValidationCommand command = new("");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.Code == "E_A");
        result.Errors.Should().Contain(e => e.Code == "E_B");
    }

    [Fact]
    public async Task Handle_Should_Keep_Group_Order_For_Each_Behavior_Built_For_The_Same_Request_Type()
    {
        ValidationBehavior<TestValidationCommand, Result> sameOrder = new([
            new OrderedStubValidator(3, "E_A", "Error A"),
            new OrderedStubValidator(3, "E_B", "Error B")
        ]);
        ValidationBehavior<TestValidationCommand, Result> mixedOrder = new([
            new OrderedStubValidator(2, "E_LATE", "Error late"),
            new OrderedStubValidator(1, "E_EARLY", "Error early"),
            new OrderedStubValidator(2, "E_LATE_2", "Error late 2")
        ]);
        TestValidationCommand command = new("");

        Result same = await sameOrder.Handle(command, SuccessNext, default);
        Result mixed = await mixedOrder.Handle(command, SuccessNext, default);

        same.Errors.Select(e => e.Code).Should().Equal("E_A", "E_B");
        mixed.Errors.Select(e => e.Code).Should().Equal("E_EARLY", "E_LATE", "E_LATE_2");
    }

    [Fact]
    public async Task Handle_Should_CallNext_WhenAllOrderedGroupsPass()
    {
        // Multiple validators (all Order=0) all pass → next must be called.
        ValidationBehavior<TestValidationCommand, Result> behavior = new([
            new StubValidator(),
            new StubValidator()
        ]);
        TestValidationCommand command = new("valid");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Return_The_Validator_Errors_Without_Copying()
    {
        Error[] errors = [Error.Validation("NameRequired", "Name is required")];
        ValidationBehavior<TestValidationCommand, Result<int>> behavior = new([new FixedResultValidator(errors)]);

        Result<int> result = await behavior.Handle(new TestValidationCommand(""), (_, _) => new ValueTask<Result<int>>(42), default);

        result.Errors.Should().BeSameAs(errors);
    }

    [Fact]
    public async Task Handle_Should_Throw_EnhancedValidationException_When_TResponse_Is_Nullable_Result()
    {
        ValidationBehavior<TestValidationCommand, Result<int>?> behavior = new([new StubValidator("NameRequired", "Name is required")]);

        Func<Task> act = () => behavior.Handle(new TestValidationCommand(""), (_, _) => new ValueTask<Result<int>?>(42), default).AsTask();

        await act.Should().ThrowAsync<EnhancedValidationException>();
    }

    [Fact]
    public async Task Handle_Should_Complete_Synchronously_When_Enforce_Fails_For_Result_Without_Observers()
    {
        ValidationBehavior<TestValidationCommand, Result<int>> behavior = new([new StubValidator("NameRequired", "Name is required")]);

        ValueTask<Result<int>> pending = behavior.Handle(new TestValidationCommand(""), (_, _) => new ValueTask<Result<int>>(42), default);

        pending.IsCompletedSuccessfully.Should().BeTrue();
        Result<int> result = await pending;
        result.FirstError.Code.Should().Be("NameRequired");
    }

    [Fact]
    public async Task Handle_Should_Fault_Instead_Of_Throwing_When_TResponse_Is_Not_Result()
    {
        ValidationBehavior<TestValidationCommand, string> behavior = new([new StubValidator("NameRequired", "Name is required")]);

        ValueTask<string> pending = behavior.Handle(new TestValidationCommand(""), (_, _) => new ValueTask<string>("ok"), default);

        pending.IsFaulted.Should().BeTrue();
        Func<Task> act = () => pending.AsTask();
        await act.Should().ThrowAsync<EnhancedValidationException>();
    }

    [Fact]
    public void Handle_Should_Return_Canceled_Task_Instead_Of_Throwing_When_Token_Already_Cancelled()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([new StubValidator()]);

        ValueTask<Result> pending = behavior.Handle(new TestValidationCommand("test"), SuccessNext, new CancellationToken(canceled: true));

        pending.IsCanceled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Return_Canceled_Task_Instead_Of_Throwing_When_Validator_Throws_Cancellation_Synchronously()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([new SyncCancellingValidator()]);

        ValueTask<Result> pending = behavior.Handle(new TestValidationCommand("test"), SuccessNext, default);

        pending.IsCanceled.Should().BeTrue();
        Func<Task> act = () => pending.AsTask();
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_Should_Fault_Instead_Of_Throwing_When_Next_Throws_Synchronously()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([new StubValidator()]);

        ValueTask<Result> pending = behavior.Handle(new TestValidationCommand("test"), (_, _) => throw new InvalidOperationException("next exploded"), default);

        pending.IsFaulted.Should().BeTrue();
        Func<Task> act = () => pending.AsTask();
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("next exploded");
    }

    [Fact]
    public void Handle_Should_Fault_Instead_Of_Throwing_When_Next_Throws_Synchronously_Without_Validators()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([]);

        ValueTask<Result> pending = behavior.Handle(new TestValidationCommand("test"), (_, _) => throw new InvalidOperationException("next exploded"), default);

        pending.IsFaulted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Call_Next_When_Async_Validator_Passes()
    {
        ValidationBehavior<TestValidationCommand, Result<int>> behavior = new([new YieldingValidator(null)]);

        Result<int> result = await behavior.Handle(new TestValidationCommand("ok"), (_, _) => new ValueTask<Result<int>>(42), default);

        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Async_Validator_Fails()
    {
        Error[] errors = [Error.Validation("NameRequired", "Name is required")];
        ValidationBehavior<TestValidationCommand, Result<int>> behavior = new([new YieldingValidator(errors)]);

        Result<int> result = await behavior.Handle(new TestValidationCommand(""), (_, _) => new ValueTask<Result<int>>(42), default);

        result.Errors.Should().BeSameAs(errors);
    }

    [Fact]
    public async Task Handle_Should_Throw_EnhancedValidationException_When_Async_Validator_Fails_For_Non_Result()
    {
        ValidationBehavior<TestValidationCommand, string> behavior = new([new YieldingValidator([Error.Validation("NameRequired", "Name is required")])]);

        Func<Task> act = () => behavior.Handle(new TestValidationCommand(""), (_, _) => new ValueTask<string>("ok"), default).AsTask();

        await act.Should().ThrowAsync<EnhancedValidationException>();
    }

    [Fact]
    public async Task Handle_Should_Call_Next_When_There_Are_No_Validators()
    {
        ValidationBehavior<TestValidationCommand, Result<int>> behavior = new([]);

        Result<int> result = await behavior.Handle(new TestValidationCommand(""), (_, _) => new ValueTask<Result<int>>(42), default);

        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task Handle_Should_Complete_Synchronously_With_Errors_In_Validator_Order_When_Several_Sync_Validators_Fail()
    {
        ValidationBehavior<TestValidationCommand, Result<int>> behavior = new([
            new OrderedStubValidator(1, "E_LATE", "Error late"),
            new StubValidator("E1", "Error 1"),
            new StubValidator("E2", "Error 2")
        ]);

        ValueTask<Result<int>> pending = behavior.Handle(new TestValidationCommand(""), (_, _) => new ValueTask<Result<int>>(42), default);

        pending.IsCompletedSuccessfully.Should().BeTrue();
        Result<int> result = await pending;
        result.Errors.Select(e => e.Code).Should().Equal("E1", "E2", "E_LATE");
    }

    [Fact]
    public async Task Handle_Should_Keep_Validator_Order_When_An_Async_Validator_Sits_Between_Sync_Ones()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([
            new OrderedStubValidator(0, "E_A", "Error A"),
            new OrderedYieldingValidator(0, [Error.Validation("E_B", "Error B")]),
            new OrderedStubValidator(0, "E_C", "Error C"),
            new OrderedYieldingValidator(1, [Error.Validation("E_D", "Error D")]),
            new OrderedStubValidator(1, "E_E", "Error E"),
            new OrderedStubValidator(2, "E_F", "Error F")
        ]);

        Result result = await behavior.Handle(new TestValidationCommand(""), SuccessNext, default);

        result.Errors.Select(e => e.Code).Should().Equal("E_A", "E_B", "E_C", "E_D", "E_E", "E_F");
    }

    [Fact]
    public async Task Handle_Should_Call_Next_When_Async_And_Sync_Validators_In_Several_Groups_Pass()
    {
        ValidationBehavior<TestValidationCommand, Result<int>> behavior = new([
            new StubValidator(),
            new OrderedYieldingValidator(0, null),
            new OrderedYieldingValidator(1, null),
            new OrderedYieldingValidator(2, null)
        ]);

        Result<int> result = await behavior.Handle(new TestValidationCommand("ok"), (_, _) => new ValueTask<Result<int>>(42), default);

        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task Handle_Should_Keep_First_Seen_Order_When_Removing_Duplicate_Errors()
    {
        ValidationBehavior<TestValidationCommand, Result> behavior = new([
            new FixedResultValidator([Error.Validation("A", "a"), Error.Validation("B", "b")]),
            new FixedResultValidator([Error.Validation("C", "c"), Error.Validation("B", "b"), Error.Validation("A", "a")])
        ]);

        Result result = await behavior.Handle(new TestValidationCommand(""), SuccessNext, default);

        result.Errors.Select(e => e.Code).Should().Equal("A", "B", "C");
    }

    [Fact]
    public async Task Handle_Should_Remove_Duplicate_Errors_When_There_Are_Many_Errors()
    {
        Error[] many = [.. Enumerable.Range(0, 20).Select(i => Error.Validation($"E{i}", $"Error {i}"))];
        ValidationBehavior<TestValidationCommand, Result> behavior = new([
            new FixedResultValidator(many),
            new FixedResultValidator([many[19], Error.Validation("Extra", "extra"), many[0]])
        ]);

        Result result = await behavior.Handle(new TestValidationCommand(""), SuccessNext, default);

        result.Errors.Select(e => e.Code).Should().Equal([.. many.Select(e => e.Code), "Extra"]);
    }
}

internal sealed class FixedResultValidator(Error[] errors) : IValidator<TestValidationCommand>
{
    public ValueTask<Result<TestValidationCommand>> ValidateAsync(TestValidationCommand instance, CancellationToken ct = default) =>
        new(Result<TestValidationCommand>.Failure(errors));
}

internal sealed class YieldingValidator(Error[]? errors) : IValidator<TestValidationCommand>
{
    public async ValueTask<Result<TestValidationCommand>> ValidateAsync(TestValidationCommand instance, CancellationToken ct = default)
    {
        await Task.Yield();
        return errors is null ? instance : Result<TestValidationCommand>.Failure(errors);
    }
}

internal sealed class SyncCancellingValidator : IValidator<TestValidationCommand>
{
    public ValueTask<Result<TestValidationCommand>> ValidateAsync(TestValidationCommand instance, CancellationToken ct = default) =>
        throw new OperationCanceledException();
}

internal sealed class OrderedYieldingValidator(int order, Error[]? errors) : IValidator<TestValidationCommand>
{
    public int Order => order;

    public async ValueTask<Result<TestValidationCommand>> ValidateAsync(TestValidationCommand instance, CancellationToken ct = default)
    {
        await Task.Yield();
        return errors is null ? instance : Result<TestValidationCommand>.Failure(errors);
    }
}
