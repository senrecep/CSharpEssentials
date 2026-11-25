using CSharpEssentials.Errors;
using CSharpEssentials.Exceptions;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using FluentAssertions;
using Mediator;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

internal sealed record GetUserCommand(int Id) : ICommand<Result<UserDto, NotFoundError>>;

internal sealed record LockUserByIdCommand(int Id) : ICommand<Result<UserDto, NotFoundError, UserErrors.Locked>>;

internal sealed class PositiveIdValidator : Validator<GetUserCommand>
{
    protected override ValueTask Configure(GetUserCommand model, RuleContext<GetUserCommand> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Id).Must(id => id > 0, "User.IdInvalid", "Id must be positive.");
        return ValueTask.CompletedTask;
    }
}

internal sealed class PositiveLockIdValidator : Validator<LockUserByIdCommand>
{
    protected override ValueTask Configure(LockUserByIdCommand model, RuleContext<LockUserByIdCommand> rules, CancellationToken ct = default)
    {
        rules.For(() => model.Id).Must(id => id > 0, "User.IdInvalid", "Id must be positive.");
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// A typed result does not implement the internal failure factory (K11), so <see cref="ExceptionHandlingBehavior{TRequest, TResponse}"/>
/// passes a handler exception through unchanged instead of turning it into an error, and <see cref="ValidationBehavior{TRequest, TResponse}"/>
/// throws <see cref="EnhancedValidationException"/> instead of building a failed typed result.
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

    [Fact]
    public async Task Handle_Should_ReturnTheTypedResult_When_TheHandlerOfATwoSlotResultSucceedsOrFails()
    {
        ExceptionHandlingBehavior<LockUserByIdCommand, Result<UserDto, NotFoundError, UserErrors.Locked>> behavior = new();
        UserErrors.Locked locked = new(5);
        MessageHandlerDelegate<LockUserByIdCommand, Result<UserDto, NotFoundError, UserErrors.Locked>> next =
            (command, _) => new ValueTask<Result<UserDto, NotFoundError, UserErrors.Locked>>(command.Id == 5 ? locked : new UserDto(command.Id));

        Result<UserDto, NotFoundError, UserErrors.Locked> found = await behavior.Handle(new LockUserByIdCommand(4), next, default);
        Result<UserDto, NotFoundError, UserErrors.Locked> failed = await behavior.Handle(new LockUserByIdCommand(5), next, default);

        found.IsSuccess.Should().BeTrue();
        failed.TryGetValue(out UserErrors.Locked error).Should().BeTrue();
        error.Should().BeSameAs(locked);
    }

    [Fact]
    public async Task Handle_Should_RethrowTheOriginalException_When_TheHandlerOfATwoSlotResultThrows()
    {
        ExceptionHandlingBehavior<LockUserByIdCommand, Result<UserDto, NotFoundError, UserErrors.Locked>> behavior = new();
        var exception = new InvalidOperationException("boom");
        MessageHandlerDelegate<LockUserByIdCommand, Result<UserDto, NotFoundError, UserErrors.Locked>> sync = (_, _) => throw exception;
        MessageHandlerDelegate<LockUserByIdCommand, Result<UserDto, NotFoundError, UserErrors.Locked>> faulted = async (_, _) =>
        {
            await Task.Yield();
            throw exception;
        };

        Func<Task> syncAct = async () => await behavior.Handle(new LockUserByIdCommand(1), sync, default);
        Func<Task> asyncAct = async () => await behavior.Handle(new LockUserByIdCommand(1), faulted, default);

        (await syncAct.Should().ThrowExactlyAsync<InvalidOperationException>()).Which.Should().BeSameAs(exception);
        (await asyncAct.Should().ThrowExactlyAsync<InvalidOperationException>()).Which.Should().BeSameAs(exception);
    }

    [Fact]
    public async Task ValidationBehavior_Should_NotRunTheHandler_When_ATypedRequestIsInvalid()
    {
        ValidationBehavior<GetUserCommand, Result<UserDto, NotFoundError>> behavior = new([new PositiveIdValidator()]);
        int runs = 0;
        MessageHandlerDelegate<GetUserCommand, Result<UserDto, NotFoundError>> next = (command, _) =>
        {
            runs++;
            return new ValueTask<Result<UserDto, NotFoundError>>(new UserDto(command.Id));
        };

        Func<Task> act = () => behavior.Handle(new GetUserCommand(-1), next, default).AsTask();

        await act.Should().ThrowExactlyAsync<EnhancedValidationException>();
        runs.Should().Be(0);
    }

    [Fact]
    public async Task ValidationBehavior_Should_ReturnTheHandlerResult_When_ATypedRequestIsValid()
    {
        ValidationBehavior<GetUserCommand, Result<UserDto, NotFoundError>> one = new([new PositiveIdValidator()]);
        ValidationBehavior<LockUserByIdCommand, Result<UserDto, NotFoundError, UserErrors.Locked>> two = new([new PositiveLockIdValidator()]);

        Result<UserDto, NotFoundError> missing = await one.Handle(
            new GetUserCommand(3), (_, _) => new ValueTask<Result<UserDto, NotFoundError>>(UserErrors.NotFound), default);
        Result<UserDto, NotFoundError, UserErrors.Locked> locked = await two.Handle(
            new LockUserByIdCommand(3), (command, _) => new ValueTask<Result<UserDto, NotFoundError, UserErrors.Locked>>(new UserErrors.Locked(command.Id)), default);

        missing.TryGetValue(out NotFoundError notFound).Should().BeTrue();
        notFound.Should().BeSameAs(UserErrors.NotFound);
        locked.TryGetValue(out UserErrors.Locked error).Should().BeTrue();
        error.Id.Should().Be(3);
    }

    [Fact]
    public async Task ValidationBehavior_Should_ThrowEnhancedValidationException_When_ATwoSlotTypedRequestIsInvalid()
    {
        ValidationBehavior<LockUserByIdCommand, Result<UserDto, NotFoundError, UserErrors.Locked>> behavior = new([new PositiveLockIdValidator()]);
        MessageHandlerDelegate<LockUserByIdCommand, Result<UserDto, NotFoundError, UserErrors.Locked>> next =
            (command, _) => new ValueTask<Result<UserDto, NotFoundError, UserErrors.Locked>>(new UserDto(command.Id));

        Func<Task> act = () => behavior.Handle(new LockUserByIdCommand(0), next, default).AsTask();

        (await act.Should().ThrowExactlyAsync<EnhancedValidationException>()).Which.Errors.Should().ContainSingle(e => e.Code == "User.IdInvalid");
    }
}
