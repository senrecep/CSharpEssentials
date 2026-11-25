// Produced by scripts/generate-typed-results.py; do not edit.
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed.Emitted;

public sealed class ResultTE1RecoverTests
{
    [Fact]
    public void RecoverNotFoundError_Should_KeepTheValue_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> source = 5;
        int calls = 0;

        int result = source.Recover((NotFoundError e) => { calls++; return 9; });

        calls.Should().Be(0);
        result.Should().Be(5);
    }

    [Fact]
    public void RecoverNotFoundError_Should_ReturnTheRecoveredValue_When_TheResultHoldsNotFoundError()
    {
        Result<int, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;

        int result = source.Recover((NotFoundError e) => { calls++; return 9; });

        calls.Should().Be(1);
        result.Should().Be(9);
    }

    [Fact]
    public void RecoverNotFoundError_Should_Throw_When_TheResultIsDefault()
    {
        Result<int, NotFoundError> source = default;
        int calls = 0;

        Action act = () => _ = source.Recover((NotFoundError e) => { calls++; return 9; });

        act.Should().Throw<InvalidOperationException>();
        calls.Should().Be(0);
    }

    [Fact]
    public void Recover_Should_Throw_When_TheRecoveryReturnsNull()
    {
        Result<string, NotFoundError> source = TypedFixture.NotFound;
        int calls = 0;

        Action act = () => _ = source.Recover((NotFoundError e) => { calls++; return (string)null!; });

        act.Should().Throw<ArgumentNullException>();
        calls.Should().Be(1);
    }
}
