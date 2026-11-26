using CSharpEssentials.Errors;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.ResultPattern.Interfaces;

using FluentAssertions;

namespace CSharpEssentials.Tests.Mediator;

public class ResponseFailureTests
{
    private static readonly NotFoundError Missing = new("Order.NotFound", "Order 42 was not found");

    private readonly struct CustomResult(bool isFailure) : IResultBase
    {
        public bool IsFailure { get; } = isFailure;
        public bool IsSuccess => !IsFailure;
        public Error FirstError => Error.NoFirstError;
        public Error LastError => Error.NoFirstError;
        public Error[] Errors => [];
        public Error[] ErrorsOrEmptyArray => [];
    }

    [Fact]
    public void IsFailure_Should_ReadTheResultStructs()
    {
        ResponseFailure.IsFailure(Result.Success()).Should().BeFalse();
        ResponseFailure.IsFailure(Result.Failure(Missing.ToError())).Should().BeTrue();
        ResponseFailure.IsFailure(Result<int>.Success(42)).Should().BeFalse();
        ResponseFailure.IsFailure<Result<int>>(Missing.ToError()).Should().BeTrue();
        ResponseFailure.IsFailure<Result<int, NotFoundError>>(42).Should().BeFalse();
        ResponseFailure.IsFailure<Result<int, NotFoundError>>(Missing).Should().BeTrue();
        ResponseFailure.IsFailure<Result<int, NotFoundError, ConflictError>>(Missing).Should().BeTrue();
        ResponseFailure.IsFailure<Result<int, NotFoundError, ConflictError, ForbiddenError>>(42).Should().BeFalse();
    }

    [Fact]
    public void IsFailure_Should_BeTrue_When_TheResultIsDefault()
    {
        ResponseFailure.IsFailure(default(Result)).Should().BeTrue();
        ResponseFailure.IsFailure(default(Result<int>)).Should().BeTrue();
        ResponseFailure.IsFailure(default(Result<int, NotFoundError>)).Should().BeTrue();
        TransactionOutcome.ShouldCommit(default(Result<int, NotFoundError>)).Should().BeFalse();
    }

    [Fact]
    public void IsFailure_Should_FallBackToIResultBase_When_TheStructIsNotACseResult()
    {
        ResponseFailure.IsFailure(new CustomResult(isFailure: true)).Should().BeTrue();
        ResponseFailure.IsFailure(new CustomResult(isFailure: false)).Should().BeFalse();
        ResponseFailure.IsFailure<Result<int>?>(Result<int>.Failure(Missing.ToError())).Should().BeTrue();
        ResponseFailure.IsFailure<Result<int>?>(null).Should().BeFalse();
    }

    [Fact]
    public void IsFailure_Should_BeFalse_When_TheResponseIsNotAResult()
    {
        ResponseFailure.IsFailure(42).Should().BeFalse();
        ResponseFailure.IsFailure("ok").Should().BeFalse();
        ResponseFailure.IsFailure<object?>(null).Should().BeFalse();
    }
}
