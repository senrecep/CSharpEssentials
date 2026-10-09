using System.Net;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class HttpRelativeUriQueryTests
{
    [Fact]
    public void Build_RelativeUri_WithQuery_Should_Append_Query()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("/users/1")
            .WithQuery("include", "profile")
            .Build();

        result.IsSuccess.Should().BeTrue();
        using HttpRequestMessage request = result.Value;
        request.RequestUri!.IsAbsoluteUri.Should().BeFalse();
        request.RequestUri.OriginalString.Should().Be("/users/1?include=profile");
    }

    [Fact]
    public void Build_RelativeUri_WithMultipleQueryValues_Should_Join_And_Escape()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("users")
            .WithQuery("q", "a b&c")
            .WithQuery("page", 2)
            .Build();

        result.IsSuccess.Should().BeTrue();
        using HttpRequestMessage request = result.Value;
        request.RequestUri!.OriginalString.Should().Be("users?q=a%20b%26c&page=2");
    }

    [Fact]
    public void Build_RelativeUri_WithExistingQuery_Should_Merge_With_Ampersand()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("/users?sort=name")
            .WithQuery("page", "1")
            .Build();

        result.IsSuccess.Should().BeTrue();
        using HttpRequestMessage request = result.Value;
        request.RequestUri!.OriginalString.Should().Be("/users?sort=name&page=1");
    }

    [Fact]
    public void Build_RelativeUri_WithFragment_Should_Insert_Query_Before_Fragment()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("/users#top")
            .WithQuery("page", "1")
            .Build();

        result.IsSuccess.Should().BeTrue();
        using HttpRequestMessage request = result.Value;
        request.RequestUri!.OriginalString.Should().Be("/users?page=1#top");
    }

    [Fact]
    public void Build_RelativeUri_WithExistingQueryAndFragment_Should_Merge_Before_Fragment()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("/users?sort=name#top")
            .WithQuery("page", "1")
            .Build();

        result.IsSuccess.Should().BeTrue();
        using HttpRequestMessage request = result.Value;
        request.RequestUri!.OriginalString.Should().Be("/users?sort=name&page=1#top");
    }

    [Fact]
    public async Task AsResultAsync_RelativeUri_WithQuery_Should_Send_Request_To_BaseAddress()
    {
        using var handler = new CapturingHandler();
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://test.com") };

        Result result = await HttpRequestBuilder.Get("/users/1")
            .WithQuery("include", "profile")
            .AsResultAsync(client);

        result.IsSuccess.Should().BeTrue();
        handler.RequestUri.Should().Be(new Uri("https://test.com/users/1?include=profile"));
    }

    [Fact]
    public void WithQueryString_RelativeUri_Should_Append_Query()
    {
        var uri = new Uri("/x", UriKind.Relative);

        Result<Uri> result = uri.WithQueryString("a", "1");

        result.IsSuccess.Should().BeTrue();
        result.Value.IsAbsoluteUri.Should().BeFalse();
        result.Value.OriginalString.Should().Be("/x?a=1");
    }

    [Fact]
    public void WithQueryString_RelativeUri_Dictionary_WithExistingQueryAndFragment_Should_Merge()
    {
        var uri = new Uri("/x?b=2#f", UriKind.Relative);
        var parameters = new Dictionary<string, string?> { { "a", "1" } };

        Result<Uri> result = uri.WithQueryString(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.OriginalString.Should().Be("/x?b=2&a=1#f");
    }

    [Fact]
    public void WithQueryString_RelativeUri_Object_Should_Append_Query()
    {
        var uri = new Uri("/x", UriKind.Relative);

        Result<Uri> result = uri.WithQueryString(new { Q = "csharp", Page = 2 });

        result.IsSuccess.Should().BeTrue();
        result.Value.OriginalString.Should().Be("/x?Q=csharp&Page=2");
    }

    [Fact]
    public void Build_AbsoluteUri_WithQuery_Should_Be_Unchanged()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("https://test.com/api")
            .WithQuery("page", "1")
            .Build();

        result.IsSuccess.Should().BeTrue();
        using HttpRequestMessage request = result.Value;
        request.RequestUri!.AbsoluteUri.Should().Be("https://test.com/api?page=1");
    }

    [Fact]
    public void Build_AbsoluteUri_WithExistingQuery_Should_Merge_With_Ampersand()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("https://test.com/api?sort=name")
            .WithQuery("page", "1")
            .Build();

        result.IsSuccess.Should().BeTrue();
        using HttpRequestMessage request = result.Value;
        request.RequestUri!.AbsoluteUri.Should().Be("https://test.com/api?sort=name&page=1");
    }

    [Fact]
    public void Build_AbsoluteUri_WithExistingQueryAndFragment_Should_Keep_Fragment()
    {
        Result<HttpRequestMessage> result = HttpRequestBuilder.Get("https://test.com/api?sort=name#top")
            .WithQuery("page", "1")
            .Build();

        result.IsSuccess.Should().BeTrue();
        using HttpRequestMessage request = result.Value;
        request.RequestUri!.AbsoluteUri.Should().Be("https://test.com/api?sort=name&page=1#top");
    }

    [Fact]
    public void WithQueryString_AbsoluteUri_WithFragment_Should_Keep_Fragment()
    {
        var uri = new Uri("https://test.com/api#top");

        Result<Uri> result = uri.WithQueryString("a", "1");

        result.IsSuccess.Should().BeTrue();
        result.Value.AbsoluteUri.Should().Be("https://test.com/api?a=1#top");
    }

    [Fact]
    public void WithQueryString_AbsoluteUri_EmptyQuery_Should_Return_Same_Uri()
    {
        var uri = new Uri("https://test.com/api");

        Result<Uri> result = uri.WithQueryString(new Dictionary<string, string?>());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(uri);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        }
    }
}
