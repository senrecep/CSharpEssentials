using System.Net;
using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class HttpClientResultExtensionsTests
{
    [Fact]
    public async Task GetFromJsonAsResultAsync_WithSuccess_Should_Return_Value()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"id":1,"name":"Test"}""")
        };
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        Result<TestDto> result = await client.GetFromJsonAsResultAsync<TestDto>(new Uri("https://test.com"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
        result.Value.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetFromJsonAsResultAsync_WithNotFound_Should_Return_Failure()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        Result<TestDto> result = await client.GetFromJsonAsResultAsync<TestDto>(new Uri("https://test.com"));

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetFromJsonAsResultAsync_WithEmptyBody_Should_Return_NotFoundError()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null")
        };
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        Result<TestDto> result = await client.GetFromJsonAsResultAsync<TestDto>(new Uri("https://test.com"));

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task PostAsJsonAsResultAsync_WithSuccess_Should_Return_Value()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.Created)
        {
            Content = new StringContent("""{"id":2,"name":"Created"}""")
        };
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        Result<TestDto> result = await client.PostAsJsonAsResultAsync<TestDto>(new Uri("https://test.com"), new { Name = "Created" });

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Created");
    }

    [Fact]
    public async Task SendAsResultAsync_WithSuccess_Should_Return_Success()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Delete, new Uri("https://test.com"));

        Result result = await client.SendAsResultAsync(request);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsResultAsync_WithFailure_Should_Return_Failure()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://test.com"));

        Result result = await client.SendAsResultAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task PutAsJsonAsResultAsync_WithSuccess_Should_Return_Value()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"id":3,"name":"Updated"}""")
        };
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        Result<TestDto> result = await client.PutAsJsonAsResultAsync<TestDto>(new Uri("https://test.com"), new { Name = "Updated" });

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task PatchAsJsonAsResultAsync_WithSuccess_Should_Return_Value()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"id":4,"name":"Patched"}""")
        };
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        Result<TestDto> result = await client.PatchAsJsonAsResultAsync<TestDto>(new Uri("https://test.com"), new { Name = "Patched" });

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Patched");
    }

    [Fact]
    public async Task DeleteAsResultAsync_WithSuccess_Should_Return_Success()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        Result result = await client.DeleteAsResultAsync(new Uri("https://test.com"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsResultAsync_WithFailure_Should_Return_Failure()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        Result result = await client.DeleteAsResultAsync(new Uri("https://test.com"));

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task PostAsResultAsync_WithSuccess_Should_Return_Success()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        using var content = new StringContent("test");
        Result result = await client.PostAsResultAsync(new Uri("https://test.com"), content);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task PutAsResultAsync_WithSuccess_Should_Return_Success()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        using var content = new StringContent("test");
        Result result = await client.PutAsResultAsync(new Uri("https://test.com"), content);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsResultAsync_Generic_WithSuccess_Should_Return_Value()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"id":5,"name":"Sent"}""")
        };
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://test.com"));

        Result<TestDto> result = await client.SendAsResultAsync<TestDto>(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Sent");
    }

    public static TheoryData<string> DisposingMethods =>
    [
        "GetFromJson",
        "PostAsJson",
        "Post",
        "PutAsJson",
        "Put",
        "PatchAsJson",
        "Delete",
        "Send",
        "SendT",
        "GetFromJsonTypeInfo",
        "PostAsJsonTypeInfo",
        "PutAsJsonTypeInfo",
        "PatchAsJsonTypeInfo",
        "SendTypeInfo",
    ];

    [Theory]
    [MemberData(nameof(DisposingMethods))]
    public async Task ResultMethod_Should_Dispose_Response(string method)
    {
        var content = new TrackingContent("""{"id":1,"name":"Test"}""");
        using var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);
        using var body = new StringContent("body");
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://test.com"));
        var uri = new Uri("https://test.com");
        var dto = new TestDto(1, "Test");
        var typeDto = new TypeInfoDto("Test", 1);
        var info = TypeInfoJsonContext.Default.TypeInfoDto;

        bool success = method switch
        {
            "GetFromJson" => (await client.GetFromJsonAsResultAsync<TestDto>(uri)).IsSuccess,
            "PostAsJson" => (await client.PostAsJsonAsResultAsync<TestDto>(uri, dto)).IsSuccess,
            "Post" => (await client.PostAsResultAsync(uri, body)).IsSuccess,
            "PutAsJson" => (await client.PutAsJsonAsResultAsync<TestDto>(uri, dto)).IsSuccess,
            "Put" => (await client.PutAsResultAsync(uri, body)).IsSuccess,
            "PatchAsJson" => (await client.PatchAsJsonAsResultAsync<TestDto>(uri, dto)).IsSuccess,
            "Delete" => (await client.DeleteAsResultAsync(uri)).IsSuccess,
            "Send" => (await client.SendAsResultAsync(request)).IsSuccess,
            "SendT" => (await client.SendAsResultAsync<TestDto>(request)).IsSuccess,
            "GetFromJsonTypeInfo" => (await client.GetFromJsonAsResultAsync(uri, info)).IsSuccess,
            "PostAsJsonTypeInfo" => (await client.PostAsJsonAsResultAsync(uri, typeDto, info, info)).IsSuccess,
            "PutAsJsonTypeInfo" => (await client.PutAsJsonAsResultAsync(uri, typeDto, info, info)).IsSuccess,
            "PatchAsJsonTypeInfo" => (await client.PatchAsJsonAsResultAsync(uri, typeDto, info, info)).IsSuccess,
            "SendTypeInfo" => (await client.SendAsResultAsync(request, info)).IsSuccess,
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, null),
        };

        success.Should().BeTrue();
        content.Disposed.Should().BeTrue();
    }

    [Fact]
    public async Task GetFromJsonAsResultAsync_WithFailureStatus_Should_Dispose_Response()
    {
        var content = new TrackingContent("""{"title":"missing"}""");
        using var response = new HttpResponseMessage(HttpStatusCode.NotFound) { Content = content };
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        Result<TestDto> result = await client.GetFromJsonAsResultAsync<TestDto>(new Uri("https://test.com"));

        result.IsFailure.Should().BeTrue();
        content.Disposed.Should().BeTrue();
    }

    private sealed record TestDto(int Id, string Name);

    private sealed class MockHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(response);
    }

    private sealed class TrackingContent(string body) : StringContent(body)
    {
        public bool Disposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }
}
