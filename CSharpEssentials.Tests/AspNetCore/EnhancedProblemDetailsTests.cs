using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using FluentAssertions;

namespace CSharpEssentials.Tests.AspNetCore;

public class EnhancedProblemDetailsTests
{
    [Fact]
    public void EnhancedProblemDetails_ShouldHaveEmptyErrors_ByDefault()
    {
        EnhancedProblemDetails problemDetails = new();

        problemDetails.Errors.Should().BeEmpty();
    }

    [Fact]
    public void EnhancedProblemDetails_ShouldHaveEmptyErrorCodes_ByDefault()
    {
        EnhancedProblemDetails problemDetails = new();

        problemDetails.ErrorCodes.Should().BeEmpty();
    }

    [Fact]
    public void EnhancedProblemDetails_ShouldHaveEmptyErrorMessages_ByDefault()
    {
        EnhancedProblemDetails problemDetails = new();

        problemDetails.ErrorMessages.Should().BeEmpty();
    }

    [Fact]
    public void EnhancedProblemDetails_ShouldStoreErrors()
    {
        Error[] errors =
        [
            Error.Failure("ERR1", "Error 1"),
            Error.Validation("ERR2", "Error 2")
        ];
        EnhancedProblemDetails problemDetails = new() { Errors = errors };

        problemDetails.Errors.Should().HaveCount(2);
        problemDetails.Errors.Should().Contain(e => e.Code == "ERR1");
        problemDetails.Errors.Should().Contain(e => e.Code == "ERR2");
    }

    [Fact]
    public void EnhancedProblemDetails_ShouldStoreErrorCodes()
    {
        HashSet<string> errorCodes = ["CODE1", "CODE2", "CODE3"];
        EnhancedProblemDetails problemDetails = new() { ErrorCodes = errorCodes };

        problemDetails.ErrorCodes.Should().HaveCount(3);
        problemDetails.ErrorCodes.Should().Contain("CODE1");
        problemDetails.ErrorCodes.Should().Contain("CODE2");
        problemDetails.ErrorCodes.Should().Contain("CODE3");
    }

    [Fact]
    public void EnhancedProblemDetails_ShouldStoreErrorMessages()
    {
        HashSet<string> errorMessages = ["Message 1", "Message 2"];
        EnhancedProblemDetails problemDetails = new() { ErrorMessages = errorMessages };

        problemDetails.ErrorMessages.Should().HaveCount(2);
        problemDetails.ErrorMessages.Should().Contain("Message 1");
        problemDetails.ErrorMessages.Should().Contain("Message 2");
    }

    [Fact]
    public void EnhancedProblemDetails_ShouldInheritFromProblemDetails()
    {
        EnhancedProblemDetails problemDetails = new()
        {
            Title = "Test Title",
            Detail = "Test Detail",
            Status = 400,
            Type = "https://example.com/error",
            Instance = "/api/test"
        };

        problemDetails.Title.Should().Be("Test Title");
        problemDetails.Detail.Should().Be("Test Detail");
        problemDetails.Status.Should().Be(400);
        problemDetails.Type.Should().Be("https://example.com/error");
        problemDetails.Instance.Should().Be("/api/test");
    }

    [Fact]
    public void EnhancedProblemDetails_ShouldSupportExtensions()
    {
        EnhancedProblemDetails problemDetails = new();
        problemDetails.Extensions["customKey"] = "customValue";

        problemDetails.Extensions.Should().ContainKey("customKey");
        problemDetails.Extensions["customKey"].Should().Be("customValue");
    }

    [Fact]
    public void ErrorCodes_ShouldBeUniqueSet()
    {
        EnhancedProblemDetails problemDetails = new();
        problemDetails.ErrorCodes.Add("CODE1");
        problemDetails.ErrorCodes.Add("CODE1");
        problemDetails.ErrorCodes.Add("CODE2");

        problemDetails.ErrorCodes.Should().HaveCount(2);
    }

    [Fact]
    public void ErrorMessages_ShouldBeUniqueSet()
    {
        EnhancedProblemDetails problemDetails = new();
        problemDetails.ErrorMessages.Add("Message 1");
        problemDetails.ErrorMessages.Add("Message 1");
        problemDetails.ErrorMessages.Add("Message 2");

        problemDetails.ErrorMessages.Should().HaveCount(2);
    }
}

