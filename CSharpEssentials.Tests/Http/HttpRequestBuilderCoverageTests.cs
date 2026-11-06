using System.Net;
using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class HttpRequestBuilderCoverageTests
{
    private static readonly Uri Target = new("https://test.com/api");

    public static TheoryData<Func<HttpRequestBuilder>, string> Factories => new()
    {
        { () => HttpRequestBuilder.Get(Target), "GET" },
        { () => HttpRequestBuilder.Post(Target), "POST" },
        { () => HttpRequestBuilder.Put(Target), "PUT" },
        { () => HttpRequestBuilder.Patch(Target), "PATCH" },
        { () => HttpRequestBuilder.Delete(Target), "DELETE" },
        { () => HttpRequestBuilder.Put("https://test.com/api"), "PUT" },
        { () => HttpRequestBuilder.Delete("https://test.com/api"), "DELETE" },
    };

    [Theory]
    [MemberData(nameof(Factories))]
    public void Factory_Should_Set_Method_And_Uri(Func<HttpRequestBuilder> factory, string method)
    {
        using HttpRequestMessage request = factory().Build().Value;

        request.Method.Method.Should().Be(method);
        request.RequestUri.Should().Be(Target);
    }

    [Fact]
    public void WithMethod_And_WithUri_Should_Override_Factory_Values()
    {
        using HttpRequestMessage request = HttpRequestBuilder.Get("https://old.com")
            .WithMethod(HttpMethod.Options)
            .WithUri(Target)
            .Build().Value;

        request.Method.Should().Be(HttpMethod.Options);
        request.RequestUri.Should().Be(Target);
    }

    [Fact]
    public void WithUri_String_Should_Override_Factory_Uri()
    {
        using HttpRequestMessage request = HttpRequestBuilder.Get(Target)
            .WithUri("https://other.com/x")
            .Build().Value;

        request.RequestUri.Should().Be(new Uri("https://other.com/x"));
    }

    [Fact]
    public void Build_Without_Uri_Should_Return_Validation_Error()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get((Uri)null!).Build();

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("HttpRequestBuilder.UriRequired");
    }

    [Fact]
    public void WithHeaders_Should_Add_All_Headers()
    {
        using HttpRequestMessage request = HttpRequestBuilder.Get(Target)
            .WithHeaders(new Dictionary<string, string> { ["x-a"] = "1", ["x-b"] = "2" })
            .Build().Value;

        request.Headers.GetValues("x-a").Should().Equal("1");
        request.Headers.GetValues("x-b").Should().Equal("2");
    }

    [Fact]
    public void WithQuery_Dictionary_Should_Skip_Null_Values()
    {
        using HttpRequestMessage request = HttpRequestBuilder.Get(Target)
            .WithQuery(new Dictionary<string, string?> { ["page"] = "2", ["filter"] = null })
            .Build().Value;

        request.RequestUri!.Query.Should().Be("?page=2");
    }

    [Fact]
    public void WithContent_Should_Set_Content_And_Replace_Json()
    {
        using var content = new StringContent("raw");

        using HttpRequestMessage request = HttpRequestBuilder.Post(Target)
            .WithJsonContent(new { Name = "x" })
            .WithContent(content)
            .Build().Value;

        request.Content.Should().BeSameAs(content);
    }

    [Fact]
    public async Task WithJsonContent_Should_Replace_Earlier_Content()
    {
        using var content = new StringContent("raw");

        using HttpRequestMessage request = HttpRequestBuilder.Post(Target)
            .WithContent(content)
            .WithJsonContent(new { Name = "x" })
            .Build().Value;

        request.Content.Should().NotBeSameAs(content);
        (await request.Content.ReadAsStringAsync()).Should().Contain("\"x\"");
    }

    [Fact]
    public async Task AsResultAsync_With_Build_Failure_Should_Return_Error_Without_Sending()
    {
        using var handler = new CountingHandler();
        using var client = new HttpClient(handler);

        Result result = await HttpRequestBuilder.Get("https://test.com/{id}")
            .WithRoute("missing", 1)
            .AsResultAsync(client);

        result.FirstError.Code.Should().Be("HttpRequestBuilder.RouteParameterNotFound");
        handler.Count.Should().Be(0);
    }

    [Fact]
    public async Task AsResultAsync_Generic_With_Build_Failure_Should_Return_Error_Without_Sending()
    {
        using var handler = new CountingHandler();
        using var client = new HttpClient(handler);

        Result<int> result = await HttpRequestBuilder.Get("https://test.com/{id}")
            .WithRoute("id", null)
            .AsResultAsync<int>(client);

        result.FirstError.Code.Should().Be("HttpRequestBuilder.RouteValueRequired");
        handler.Count.Should().Be(0);
    }

    [Fact]
    public async Task AsResultAsync_Generic_With_Null_Client_Should_Return_Validation_Error()
    {
        Result<int> result = await HttpRequestBuilder.Get(Target).AsResultAsync<int>(null);

        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("HttpRequestBuilder.ClientRequired");
    }

    private sealed class CountingHandler : HttpMessageHandler
    {
        public int Count { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Count++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
