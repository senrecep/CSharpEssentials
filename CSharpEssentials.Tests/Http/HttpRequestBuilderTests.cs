using System.Net;
using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class HttpRequestBuilderTests
{
    [Fact]
    public void Build_Should_Create_Request_With_Method_And_Uri()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("https://test.com/api").Build();

        result.IsSuccess.Should().BeTrue();
        using HttpRequestMessage request = result.Value;
        request.Method.Should().Be(HttpMethod.Get);
        request.RequestUri!.ToString().Should().Be("https://test.com/api");
    }

    [Fact]
    public void Build_WithQuery_Should_Append_QueryString()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("https://test.com/api")
            .WithQuery("page", "1")
            .Build();

        result.IsSuccess.Should().BeTrue();
        using HttpRequestMessage request = result.Value;
        request.RequestUri!.Query.Should().Contain("page=1");
    }

    [Fact]
    public void Build_WithHeader_Should_Add_Header()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("https://test.com/api")
            .WithHeader("x-api-key", "secret")
            .Build();

        result.IsSuccess.Should().BeTrue();
        using HttpRequestMessage request = result.Value;
        request.Headers.Contains("x-api-key").Should().BeTrue();
    }

    [Fact]
    public void Build_WithJsonContent_Should_Set_Content()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Post("https://test.com/api")
            .WithJsonContent(new { Name = "Alice" })
            .Build();

        result.IsSuccess.Should().BeTrue();
        using HttpRequestMessage request = result.Value;
        request.Content.Should().NotBeNull();
    }

    [Fact]
    public async Task AsResultAsync_Should_Execute_And_Return_Success()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        Result result = await HttpRequestBuilder.Delete("https://test.com/api/1")
            .AsResultAsync(client);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AsResultAsync_Generic_Should_Execute_And_Return_Value()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"id":1,"name":"Test"}""")
        };
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        Result<TestDto> result = await HttpRequestBuilder
            .Get("https://test.com/api/1")
            .AsResultAsync<TestDto>(client);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test");
    }

    [Fact]
    public async Task AsResultAsync_WithFailure_Should_Return_Error()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        Result result = await HttpRequestBuilder.Post("https://test.com/api")
            .WithJsonContent(new { })
            .AsResultAsync(client);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task AsResultAsync_WithNullClient_Should_Return_ValidationError()
    {
        Result result = await HttpRequestBuilder.Get("https://test.com/api")
            .AsResultAsync(null!);

        result.IsFailure.Should().BeTrue();
    }

    private sealed record TestDto(int Id, string Name);

    private sealed class MockHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(response);
    }
}
