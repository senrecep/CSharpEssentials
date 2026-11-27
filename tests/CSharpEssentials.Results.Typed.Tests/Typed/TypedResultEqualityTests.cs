using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

public sealed class TypedResultEqualityTests
{
    private static readonly NotFoundError Missing = new("Thing.Missing", "Thing is missing.");

    [Fact]
    public void Equals_Should_BeTrue_When_BothHoldAnEqualValue()
    {
        Result<int, NotFoundError> left = 5;
        Result<int, NotFoundError> right = 5;

        (left == right).Should().BeTrue();
        (left != right).Should().BeFalse();
        left.Equals((object)right).Should().BeTrue();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equals_Should_BeFalse_When_TheValuesDiffer()
    {
        Result<int, NotFoundError, ConflictError> left = 5;
        Result<int, NotFoundError, ConflictError> right = 6;

        (left == right).Should().BeFalse();
        (left != right).Should().BeTrue();
    }

    [Fact]
    public void Equals_Should_BeTrue_When_BothHoldAnEqualErrorInTheSameSlot()
    {
        Result<int, NotFoundError, ConflictError, ValidationError> left = new ConflictError("Thing.Taken", "Taken.");
        Result<int, NotFoundError, ConflictError, ValidationError> right = new ConflictError("Thing.Taken", "Taken.");

        (left == right).Should().BeTrue();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void Equals_Should_BeFalse_When_TheSameErrorIsInDifferentSlots()
    {
        var first = new Result<int, NotFoundError, TypedError>(Missing);
        var second = new Result<int, NotFoundError, TypedError>((TypedError)Missing);

        first.Value.Should().BeSameAs(second.Value);
        (first == second).Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_BeFalse_When_AValueIsComparedWithAnErrorOrDefault()
    {
        Result<int, NotFoundError> value = 0;
        Result<int, NotFoundError> error = Missing;

        (value == error).Should().BeFalse();
        (value == default).Should().BeFalse();
        (error == default).Should().BeFalse();
        value.Equals("not a result").Should().BeFalse();
    }

    [Fact]
    public void Equals_Should_BeTrue_When_BothAreDefault()
    {
        (default(Result<int, NotFoundError>) == default).Should().BeTrue();
        (default(Result<int, NotFoundError, ConflictError>) == default).Should().BeTrue();
        (default(Result<int, NotFoundError, ConflictError, ValidationError>) == default).Should().BeTrue();
        default(Result<int, NotFoundError>).GetHashCode().Should().Be(default(Result<int, NotFoundError>).GetHashCode());
    }

    [Fact]
    public void Equals_Should_UseTheStructEquality_When_TheErrorIsAStruct()
    {
        Result<int, RateLimited> left = new RateLimited("Api.RateLimited");
        Result<int, RateLimited> right = new RateLimited("Api.RateLimited");
        Result<int, RateLimited> other = new RateLimited("Api.Other");

        (left == right).Should().BeTrue();
        (left == other).Should().BeFalse();
    }
}
