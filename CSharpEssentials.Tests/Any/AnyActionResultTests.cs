using CSharpEssentials.Any;
using FluentAssertions;

namespace CSharpEssentials.Tests.Any;

public class AnyActionResultTests
{
    [Fact]
    public void ImplicitConversion_FromResult_ShouldCreateExecuted()
    {
        AnyActionResult<int> result = 42;

        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromStatus_ShouldCreateWithStatus()
    {
        AnyActionResult<int> result = AnyActionStatus.NotExecuted;

        result.Status.Should().Be(AnyActionStatus.NotExecuted);
        result.Result.Should().Be(default);
    }

    [Fact]
    public void Equality_ShouldWork()
    {
        var result1 = new AnyActionResult<int>(AnyActionStatus.Executed, 42);
        var result2 = new AnyActionResult<int>(AnyActionStatus.Executed, 42);
        var result3 = new AnyActionResult<int>(AnyActionStatus.NotExecuted, 0);

        result1.Should().Be(result2);
        result1.Should().NotBe(result3);
    }
}

