using System.Net;
using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class HttpClientRedirectExtensionsTests
{
    [Fact]
    public async Task SendWithRedirectsAsResultAsync_WithSingleRedirect_Should_Follow_And_Return_Success()
    {
        using var handler = new RedirectMockHandler(new Uri("https://test.com/final"), HttpStatusCode.OK);
        using var client = new HttpClient(handler);

        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://test.com/redirect"));
        Result result = await client.SendWithRedirectsAsResultAsync(request);

        result.IsSuccess.Should().BeTrue();
        handler.RequestCount.Should().Be(2);
    }

    [Fact]
    public async Task SendWithRedirectsAsResultAsync_Generic_WithSingleRedirect_Should_Follow_And_Return_Value()
    {
        using var handler = new RedirectMockHandler(
            new Uri("https://test.com/final"),
            HttpStatusCode.OK,
            """{"id":1,"name":"Test"}""");
        using var client = new HttpClient(handler);

        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://test.com/redirect"));
        Result<TestDto> result = await client.SendWithRedirectsAsResultAsync<TestDto>(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test");
        handler.RequestCount.Should().Be(2);
    }

    [Fact]
    public async Task SendWithRedirectsAsResultAsync_WithRedirectLimitExceeded_Should_Return_Error()
    {
        using var handler = new InfiniteRedirectMockHandler();
        using var client = new HttpClient(handler);

        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://test.com/redirect"));
        Result result = await client.SendWithRedirectsAsResultAsync(request, maxRedirects: 2);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Unexpected);
        result.FirstError.Code.Should().Be("Http.RedirectLimitExceeded");
        handler.RequestCount.Should().Be(3);
    }

    [Fact]
    public async Task SendWithRedirectsAsResultAsync_Generic_WithRedirectLimitExceeded_Should_Return_Error()
    {
        using var handler = new InfiniteRedirectMockHandler();
        using var client = new HttpClient(handler);

        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://test.com/redirect"));
        Result<TestDto> result = await client.SendWithRedirectsAsResultAsync<TestDto>(request, maxRedirects: 2);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Http.RedirectLimitExceeded");
    }

    [Fact]
    public async Task SendWithRedirectsAsResultAsync_WithMissingLocation_Should_Return_Error()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.MovedPermanently);
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://test.com/redirect"));
        Result result = await client.SendWithRedirectsAsResultAsync(request);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Http.MissingRedirectLocation");
    }

    [Fact]
    public async Task SendWithRedirectsAsResultAsync_WithNonRedirect_Should_Return_Normal_Result()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://test.com/api"));
        Result result = await client.SendWithRedirectsAsResultAsync(request);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task SendWithRedirectsAsResultAsync_WithNullClient_Should_Return_ValidationError()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://test.com"));
        Result result = await ((HttpClient)null!).SendWithRedirectsAsResultAsync(request);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Http.ClientRequired");
    }

    [Fact]
    public async Task SendWithRedirectsAsResultAsync_WithNullRequest_Should_Return_ValidationError()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        using var handler = new MockHandler(response);
        using var client = new HttpClient(handler);

        Result result = await client.SendWithRedirectsAsResultAsync(null!);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Code.Should().Be("Http.RequestRequired");
    }

    [Fact]
    public async Task HttpRequestBuilder_WithFollowRedirects_Should_Follow_Redirects()
    {
        using var handler = new RedirectMockHandler(
            new Uri("https://test.com/final"),
            HttpStatusCode.OK,
            """{"id":1,"name":"Test"}""");
        using var client = new HttpClient(handler);

        Result<TestDto> result = await HttpRequestBuilder
            .Get("https://test.com/redirect")
            .FollowRedirects()
            .AsResultAsync<TestDto>(client);

        result.IsSuccess.Should().BeTrue();
        handler.RequestCount.Should().Be(2);
    }

    [Fact]
    public async Task HttpRequestBuilder_WithFollowRedirects_And_Failure_Should_Return_Error()
    {
        using var handler = new RedirectMockHandler(
            new Uri("https://test.com/final"),
            HttpStatusCode.NotFound);
        using var client = new HttpClient(handler);

        Result result = await HttpRequestBuilder
            .Get("https://test.com/redirect")
            .FollowRedirects()
            .AsResultAsync(client);

        result.IsFailure.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        handler.RequestCount.Should().Be(2);
    }

    [Fact]
    public async Task SendWithRedirectsAsResultAsync_WithRelativeLocation_Should_Resolve_Absolute_Uri()
    {
        using var handler = new RelativeRedirectMockHandler(HttpStatusCode.OK, """{"id":1,"name":"Relative"}""");
        using var client = new HttpClient(handler);

        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://test.com/api/redirect"));
        Result<TestDto> result = await client.SendWithRedirectsAsResultAsync<TestDto>(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Relative");
        handler.RequestCount.Should().Be(2);
    }

    private sealed record TestDto(int Id, string Name);

    private sealed class MockHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(response);
    }

    private sealed class RedirectMockHandler : HttpMessageHandler
    {
        private readonly Uri _redirectTarget;
        private readonly HttpStatusCode _finalStatus;
        private readonly string? _finalContent;

        public RedirectMockHandler(Uri redirectTarget, HttpStatusCode finalStatus, string? finalContent = null)
        {
            _redirectTarget = redirectTarget;
            _finalStatus = finalStatus;
            _finalContent = finalContent;
        }

        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            if (request.RequestUri!.ToString().Contains("/redirect"))
            {
                var response = new HttpResponseMessage(HttpStatusCode.MovedPermanently);
                response.Headers.Location = _redirectTarget;
                return Task.FromResult(response);
            }

            var finalResponse = new HttpResponseMessage(_finalStatus);
            if (_finalContent is not null)
                finalResponse.Content = new StringContent(_finalContent);

            return Task.FromResult(finalResponse);
        }
    }

    private sealed class InfiniteRedirectMockHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            var response = new HttpResponseMessage(HttpStatusCode.MovedPermanently);
            response.Headers.Location = new Uri($"https://test.com/redirect{RequestCount}");
            return Task.FromResult(response);
        }
    }

    private sealed class RelativeRedirectMockHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _finalStatus;
        private readonly string? _finalContent;

        public RelativeRedirectMockHandler(HttpStatusCode finalStatus, string? finalContent = null)
        {
            _finalStatus = finalStatus;
            _finalContent = finalContent;
        }

        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            if (request.RequestUri!.ToString().EndsWith("/redirect", StringComparison.OrdinalIgnoreCase))
            {
                var response = new HttpResponseMessage(HttpStatusCode.MovedPermanently);
                response.Headers.Location = new Uri("/api/final", UriKind.Relative);
                return Task.FromResult(response);
            }

            var finalResponse = new HttpResponseMessage(_finalStatus);
            if (_finalContent is not null)
                finalResponse.Content = new StringContent(_finalContent);

            return Task.FromResult(finalResponse);
        }
    }
}
