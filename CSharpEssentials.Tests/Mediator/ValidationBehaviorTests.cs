using Mediator;

using CSharpEssentials.Errors;
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

        Result<int> result = await behavior.Handle(command, (_, _) => new ValueTask<Result<int>>(Result.Success(42)), default);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
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
        // Non-OCE exceptions are converted to Error.Failure — never rethrown.
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
        // OCE from a validator must still propagate — it is not converted to an Error.
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
        // Validators are registered with Order=1 first, Order=0 second —
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
}
