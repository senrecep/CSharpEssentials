using Mediator;

using CSharpEssentials.Errors;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;

using FluentAssertions;
using FluentValidation;

namespace CSharpEssentials.Tests.Mediator;

internal sealed record TestValidationCommand(string Name) : ICommand<Result>;

internal sealed class StubValidator : AbstractValidator<TestValidationCommand>
{
    public StubValidator() { }

    public StubValidator(string errorCode, string message)
    {
        RuleFor(x => x.Name).Must(_ => false).WithErrorCode(errorCode).WithMessage(message);
    }
}

public class ValidationBehaviorTests
{
    private static readonly MessageHandlerDelegate<TestValidationCommand, Result> SuccessNext =
        (message, ct) => new ValueTask<Result>(Result.Success());

    [Fact]
    public async Task Handle_Should_Call_Next_When_No_Validators_Provided()
    {
        var behavior = new ValidationBehavior<TestValidationCommand, Result>([]);
        var command = new TestValidationCommand("test");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Call_Next_When_Validation_Succeeds()
    {
        var behavior = new ValidationBehavior<TestValidationCommand, Result>([new StubValidator()]);
        var command = new TestValidationCommand("test");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Validation_Fails()
    {
        var behavior = new ValidationBehavior<TestValidationCommand, Result>([new StubValidator("NameRequired", "Name is required")]);
        var command = new TestValidationCommand("");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("NameRequired");
        result.FirstError.Description.Should().Be("Name is required");
    }

    [Fact]
    public async Task Handle_Should_Return_Generic_Failure_For_Generic_Result()
    {
        var behavior = new ValidationBehavior<TestValidationCommand, Result<int>>([new StubValidator("NameRequired", "Name is required")]);
        var command = new TestValidationCommand("");

        Result<int> result = await behavior.Handle(command, (_, _) => new ValueTask<Result<int>>(Result.Success(42)), default);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_Should_Aggregate_Errors_From_Multiple_Validators()
    {
        var behavior = new ValidationBehavior<TestValidationCommand, Result>([
            new StubValidator("E1", "Error 1"),
            new StubValidator("E2", "Error 2")
        ]);
        var command = new TestValidationCommand("");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_Should_Include_PropertyName_In_Metadata()
    {
        var behavior = new ValidationBehavior<TestValidationCommand, Result>([new StubValidator("Invalid", "Invalid")]);
        var command = new TestValidationCommand("");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.FirstError.Metadata.Should().ContainKey("PropertyName");
        result.FirstError.Metadata["PropertyName"].Should().Be("Name");
    }
}
