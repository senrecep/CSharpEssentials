using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using FluentAssertions;
using Mediator;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

internal sealed record GetUserCommand(int Id) : ICommand<Result<UserDto, NotFoundError>>;

internal sealed class PositiveIdValidator : Validator<GetUserCommand>
{
    protected override ValueTask Configure(GetUserCommand model, RuleContext<GetUserCommand> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Id).Must(id => id > 0, "User.IdInvalid", "Id must be positive.");
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// A typed result does not implement the internal failure factory (K11), so <see cref="ExceptionHandlingBehavior{TRequest, TResponse}"/>
/// passes a handler exception through unchanged instead of turning it into an error.
/// </summary>
public sealed class TypedResultMediatorTests
{
    private readonly ExceptionHandlingBehavior<GetUserCommand, Result<UserDto, NotFoundError>> _behavior = new();

    [Fact]
    public async Task Handle_Should_ReturnTheTypedResult_When_TheHandlerSucceeds()
    {
        MessageHandlerDelegate<GetUserCommand, Result<UserDto, NotFoundError>> next =
            (command, _) => new ValueTask<Result<UserDto, NotFoundError>>(command.Id == 0 ? UserErrors.NotFound : new UserDto(command.Id));

        Result<UserDto, NotFoundError> found = await _behavior.Handle(new GetUserCommand(4), next, default);
        Result<UserDto, NotFoundError> missing = await _behavior.Handle(new GetUserCommand(0), next, default);

        found.IsSuccess.Should().BeTrue();
        missing.TryGetValue(out NotFoundError error).Should().BeTrue();
        error.Should().BeSameAs(UserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_Should_RethrowTheOriginalException_When_TheHandlerThrowsSynchronously()
    {
        var exception = new InvalidOperationException("boom");
        MessageHandlerDelegate<GetUserCommand, Result<UserDto, NotFoundError>> next = (_, _) => throw exception;

        Func<Task> act = async () => await _behavior.Handle(new GetUserCommand(1), next, default);

        (await act.Should().ThrowExactlyAsync<InvalidOperationException>()).Which.Should().BeSameAs(exception);
    }

    [Fact]
    public async Task Handle_Should_RethrowTheOriginalException_When_TheHandlerFaultsAsynchronously()
    {
        var exception = new InvalidOperationException("boom");
        MessageHandlerDelegate<GetUserCommand, Result<UserDto, NotFoundError>> next = async (_, _) =>
        {
            await Task.Yield();
            throw exception;
        };

        Func<Task> act = async () => await _behavior.Handle(new GetUserCommand(1), next, default);

        (await act.Should().ThrowExactlyAsync<InvalidOperationException>()).Which.Should().BeSameAs(exception);
    }

    [Fact]
    public async Task ValidationBehavior_Should_ThrowEnhancedValidationException_When_ATypedRequestIsInvalid()
    {
        ValidationBehavior<GetUserCommand, Result<UserDto, NotFoundError>> behavior = new([new PositiveIdValidator()]);
        MessageHandlerDelegate<GetUserCommand, Result<UserDto, NotFoundError>> next =
            (command, _) => new ValueTask<Result<UserDto, NotFoundError>>(new UserDto(command.Id));

        Func<Task> act = () => behavior.Handle(new GetUserCommand(-1), next, default).AsTask();

        (await act.Should().ThrowExactlyAsync<EnhancedValidationException>()).Which.Errors.Should().ContainSingle(e => e.Code == "User.IdInvalid");
    }
}
