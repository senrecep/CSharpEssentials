using System.Net;
using System.Net.Http.Json;
using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class HttpContentExtensionsTests
{
    [Fact]
    public async Task ReadAsStringAsResultAsync_Should_Return_Content()
    {
        using var content = new StringContent("Hello World");

        Result<string> result = await content.ReadAsStringAsResultAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello World");
    }

    [Fact]
    public async Task ReadFromJsonAsResultAsync_WithValidJson_Should_Return_Value()
    {
        using var content = new StringContent("""{"id":1,"name":"Test"}""");

        Result<TestDto> result = await content.ReadFromJsonAsResultAsync<TestDto>();

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
        result.Value.Name.Should().Be("Test");
    }

    [Fact]
    public async Task ReadFromJsonAsResultAsync_WithNull_Should_Return_NotFoundError()
    {
        using var content = new StringContent("null");

        Result<TestDto> result = await content.ReadFromJsonAsResultAsync<TestDto>();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ReadFromJsonAsResultAsync_WithInvalidJson_Should_Return_ValidationError()
    {
        using var content = new StringContent("invalid json");

        Result<TestDto> result = await content.ReadFromJsonAsResultAsync<TestDto>();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
    }

    private sealed record TestDto(int Id, string Name);
}
