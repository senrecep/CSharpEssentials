using CSharpEssentials.Errors;
using FluentAssertions;

namespace CSharpEssentials.Tests.Errors;

public sealed class ErrorFieldKeywordTests
{
    [Fact]
    public void Factory_Code_Null_Should_Throw_ArgumentNullException()
    {
        Action act = () => Error.Failure(null!, "desc");
        act.Should().Throw<ArgumentNullException>().WithParameterName("code");
    }

    [Fact]
    public void Factory_Description_Null_Should_Throw_ArgumentNullException()
    {
        Action act = () => Error.Failure("code", null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("description");
    }

    [Fact]
    public void Factory_Valid_Should_Create_Error()
    {
        var error = Error.Failure("E1", "Something failed");
        error.Code.Should().Be("E1");
        error.Description.Should().Be("Something failed");
    }
}
