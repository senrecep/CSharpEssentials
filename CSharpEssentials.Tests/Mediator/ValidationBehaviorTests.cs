using Mediator;
using System.Linq.Expressions;

using CSharpEssentials.Errors;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;

using Moq;

namespace CSharpEssentials.Tests.Mediator;

public sealed record TestValidationCommand(string Name) : ICommand<Result>;

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
        var validator = new Mock<IValidator<TestValidationCommand>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<TestValidationCommand, Result>([validator.Object]);
        var command = new TestValidationCommand("test");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Validation_Fails()
    {
        var validator = new Mock<IValidator<TestValidationCommand>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([
                new ValidationFailure("Name", "Name is required") { ErrorCode = "NameRequired" }
            ]));

        var behavior = new ValidationBehavior<TestValidationCommand, Result>([validator.Object]);
        var command = new TestValidationCommand("");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("NameRequired");
        result.FirstError.Description.Should().Be("Name is required");
    }

    [Fact]
    public async Task Handle_Should_Return_Generic_Failure_For_Generic_Result()
    {
        var validator = new Mock<IValidator<TestValidationCommand>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([
                new ValidationFailure("Name", "Name is required") { ErrorCode = "NameRequired" }
            ]));

        var behavior = new ValidationBehavior<TestValidationCommand, Result<int>>([validator.Object]);
        var command = new TestValidationCommand("");

        Result<int> result = await behavior.Handle(command, (_, _) => new ValueTask<Result<int>>(Result.Success(42)), default);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_Should_Aggregate_Errors_From_Multiple_Validators()
    {
        var validator1 = new Mock<IValidator<TestValidationCommand>>();
        validator1.Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([
                new ValidationFailure("Name", "Error 1") { ErrorCode = "E1" }
            ]));

        var validator2 = new Mock<IValidator<TestValidationCommand>>();
        validator2.Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([
                new ValidationFailure("Name", "Error 2") { ErrorCode = "E2" }
            ]));

        var behavior = new ValidationBehavior<TestValidationCommand, Result>([validator1.Object, validator2.Object]);
        var command = new TestValidationCommand("");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_Should_Include_PropertyName_In_Metadata()
    {
        var validator = new Mock<IValidator<TestValidationCommand>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<IValidationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([
                new ValidationFailure("Name", "Invalid") { ErrorCode = "Invalid" }
            ]));

        var behavior = new ValidationBehavior<TestValidationCommand, Result>([validator.Object]);
        var command = new TestValidationCommand("");

        Result result = await behavior.Handle(command, SuccessNext, default);

        result.FirstError.Metadata.Should().ContainKey("PropertyName");
        result.FirstError.Metadata["PropertyName"].Should().Be("Name");
    }
}
