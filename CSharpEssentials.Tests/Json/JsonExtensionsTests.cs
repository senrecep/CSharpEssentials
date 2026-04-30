using CSharpEssentials.Json;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using System.Text.Json;

namespace CSharpEssentials.Tests.Json;

public class JsonExtensionsTests
{
    [Fact]
    public void TryGetProperty_ShouldReturnProperty_WhenExists()
    {
        string json = """{"name":"John","age":30}""";
        var document = JsonDocument.Parse(json);
        JsonElement element = document.RootElement;

        Result<JsonElement> result = element.TryGetProperty("name");

        result.IsSuccess.Should().BeTrue();
        result.Value.GetString().Should().Be("John");
    }

    [Fact]
    public void TryGetProperty_ShouldReturnFirstMatch_WhenMultipleNamesProvided()
    {
        string json = """{"name":"John","age":30}""";
        var document = JsonDocument.Parse(json);
        JsonElement element = document.RootElement;

        Result<JsonElement> result = element.TryGetProperty("missing", "name", "age");

        result.IsSuccess.Should().BeTrue();
        result.Value.GetString().Should().Be("John");
    }

    [Fact]
    public void TryGetProperty_ShouldReturnNotFound_WhenPropertyDoesNotExist()
    {
        string json = """{"name":"John"}""";
        var document = JsonDocument.Parse(json);
        JsonElement element = document.RootElement;

        Result<JsonElement> result = element.TryGetProperty("missing");

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("PropertyNotFound");
    }

    [Fact]
    public void TryGetProperty_ShouldReturnValidationError_WhenNoPropertyNamesProvided()
    {
        string json = """{"name":"John"}""";
        var document = JsonDocument.Parse(json);
        JsonElement element = document.RootElement;

        Result<JsonElement> result = element.TryGetProperty(Array.Empty<string>());

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("NoPropertyNames");
    }

    [Fact]
    public void TryGetProperty_ShouldReturnValidationError_WhenNullPropertyNames()
    {
        string json = """{"name":"John"}""";
        var document = JsonDocument.Parse(json);
        JsonElement element = document.RootElement;

        Result<JsonElement> result = element.TryGetProperty(null!);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("NoPropertyNames");
    }

    [Fact]
    public void TryGetNestedProperty_ShouldReturnNestedProperty_WhenPathExists()
    {
        string json = """{"user":{"profile":{"name":"John"}}}""";
        var document = JsonDocument.Parse(json);

        Result<JsonElement?> result = document.TryGetNestedProperty("user", "profile", "name");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.GetString().Should().Be("John");
    }

    [Fact]
    public void TryGetNestedProperty_ShouldReturnNotFound_WhenPathDoesNotExist()
    {
        string json = """{"user":{"profile":{"name":"John"}}}""";
        var document = JsonDocument.Parse(json);

        Result<JsonElement?> result = document.TryGetNestedProperty("user", "missing", "name");

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("PropertyNotFound");
    }

    [Fact]
    public void TryGetNestedProperty_ShouldReturnValidationError_WhenDocumentIsNull()
    {
        JsonDocument? document = null;

        Result<JsonElement?> result = document!.TryGetNestedProperty("user");

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("DocumentIsNull");
    }

    [Fact]
    public void TryGetNestedProperty_ShouldReturnValidationError_WhenNoPropertyNames()
    {
        string json = """{"user":{"name":"John"}}""";
        var document = JsonDocument.Parse(json);

        Result<JsonElement?> result = document.TryGetNestedProperty([]);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("NoPropertyNames");
    }

    [Fact]
    public void TryGetNestedProperty_ShouldReturnRootElement_WhenSinglePropertyName()
    {
        string json = """{"name":"John"}""";
        var document = JsonDocument.Parse(json);

        Result<JsonElement?> result = document.TryGetNestedProperty("name");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.GetString().Should().Be("John");
    }
}

