using CSharpEssentials.Errors;
using FluentAssertions;

namespace CSharpEssentials.Tests.Errors;

public class ErrorOperatorTests
{
    private static readonly Error TestError = Error.Validation("Test.Code", "Test message");

    #region Implicit Error[] Conversion

    [Fact]
    public void ImplicitConversion_ToArray_ShouldReturnSingleElementArray()
    {
        Error[] errors = TestError;

        errors.Should().HaveCount(1);
        errors[0].Should().Be(TestError);
    }

    [Fact]
    public void ImplicitConversion_ShouldWorkInMethodParameter()
    {
        Error[] errors = Error.CreateMany(TestError);

        errors.Should().HaveCount(1);
        errors[0].Should().Be(TestError);
    }

    #endregion

    #region + Operator

    [Fact]
    public void PlusOperator_TwoErrors_ShouldReturnArrayWithBoth()
    {
        var error1 = Error.Validation("ERR1", "First");
        var error2 = Error.Validation("ERR2", "Second");

        Error[] result = error1 + error2;

        result.Should().HaveCount(2);
        result[0].Should().Be(error1);
        result[1].Should().Be(error2);
    }

    [Fact]
    public void PlusOperator_SameError_ShouldReturnArrayWithDuplicates()
    {
        Error[] result = TestError + TestError;

        result.Should().HaveCount(2);
        result[0].Should().Be(TestError);
        result[1].Should().Be(TestError);
    }

    [Fact]
    public void PlusOperator_DifferentTypes_ShouldReturnArrayWithBoth()
    {
        var validation = Error.Validation("VAL", "Validation");
        var notFound = Error.NotFound("NF", "Not found");

        Error[] result = validation + notFound;

        result.Should().HaveCount(2);
        result[0].Type.Should().Be(ErrorType.Validation);
        result[1].Type.Should().Be(ErrorType.NotFound);
    }

    #endregion
}
