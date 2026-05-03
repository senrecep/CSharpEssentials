using System.Text.Json;
using CSharpEssentials.Errors;
using CSharpEssentials.Json;
using FluentAssertions;

namespace CSharpEssentials.Tests.Meta;

public class JsonExtensionsTests
{
    [Fact]
    public void TryGetProperty_ShouldReturnProperty_WhenFound()
    {
        string json = """{"name":"Alice","age":30}""";
        using JsonDocument doc = JsonDocument.Parse(json);

        var result = doc.RootElement.TryGetProperty("name");

        result.IsSuccess.Should().BeTrue();
        result.Value.GetString().Should().Be("Alice");
    }

    [Fact]
    public void TryGetProperty_ShouldReturnError_WhenNotFound()
    {
        string json = """{"name":"Alice"}""";
        using JsonDocument doc = JsonDocument.Parse(json);

        var result = doc.RootElement.TryGetProperty("missing");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void TryGetProperty_ShouldReturnError_WhenNoNamesProvided()
    {
        string json = """{"name":"Alice"}""";
        using JsonDocument doc = JsonDocument.Parse(json);

        var result = doc.RootElement.TryGetProperty();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void TryGetNestedProperty_ShouldReturnNestedValue_WhenPathExists()
    {
        string json = """{"user":{"profile":{"name":"Alice"}}}""";
        using JsonDocument doc = JsonDocument.Parse(json);

        var result = doc.TryGetNestedProperty("user", "profile", "name");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.GetString().Should().Be("Alice");
    }

    [Fact]
    public void TryGetNestedProperty_ShouldReturnError_WhenPathMissing()
    {
        string json = """{"user":{"profile":{"name":"Alice"}}}""";
        using JsonDocument doc = JsonDocument.Parse(json);

        var result = doc.TryGetNestedProperty("user", "profile", "missing");

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void TryGetNestedProperty_ShouldReturnError_WhenNoNamesProvided()
    {
        string json = """{"name":"Alice"}""";
        using JsonDocument doc = JsonDocument.Parse(json);

        var result = doc.TryGetNestedProperty();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
    }
}
