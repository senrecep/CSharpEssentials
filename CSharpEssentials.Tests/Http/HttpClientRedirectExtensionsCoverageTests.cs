using System.Net;
using System.Net.Http.Headers;
using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Tests.Http;

public class HttpClientRedirectExtensionsCoverageTests
{
    [Fact]
    public async Task Generic_WithNullClient_Should_Return_ValidationError()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://test.com/"));

        Result<TestDto> result = await ((HttpClient?)null).SendWithRedirectsAsResultAsync<TestDto>(request);

        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("Http.ClientRequired");
    }

    [Fact]
    public async Task Generic_WithNullRequest_Should_Return_ValidationError()
    {
        using var handler = new ScriptedHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using var client = new HttpClient(handler);

        Result<TestDto> result = await client.SendWithRedirectsAsResultAsync<TestDto>(null);

        result.FirstError.Type.Should().Be(ErrorType.Validation);
        result.FirstError.Code.Should().Be("Http.RequestRequired");
    }

    [Fact]
    public async Task Generic_WithMissingLocation_Should_Return_Error()
    {
        using var handler = new ScriptedHandler(_ => new HttpResponseMessage(HttpStatusCode.Found));
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://test.com/redirect"));

        Result<TestDto> result = await client.SendWithRedirectsAsResultAsync<TestDto>(request);

        result.FirstError.Type.Should().Be(ErrorType.Unexpected);
        result.FirstError.Code.Should().Be("Http.MissingRedirectLocation");
    }

    [Fact]
    public async Task Generic_WithFailureAfterRedirect_Should_Map_Status()
    {
        using var handler = new ScriptedHandler(request => request.RequestUri!.AbsolutePath == "/start"
            ? Redirect(HttpStatusCode.Redirect, "https://test.com/final")
            : new HttpResponseMessage(HttpStatusCode.NotFound));
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://test.com/start"));

        Result<TestDto> result = await client.SendWithRedirectsAsResultAsync<TestDto>(request);

        result.FirstError.Type.Should().Be(ErrorType.NotFound);
        result.FirstError.Code.Should().Be("Http.404");
    }

    [Fact]
    public async Task Generic_WithNullJsonBody_Should_Return_NotFound()
    {
        using var handler = new ScriptedHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null")
        });
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://test.com/final"));

        Result<TestDto> result = await client.SendWithRedirectsAsResultAsync<TestDto>(request);

        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handler_Exception_Should_Become_Unexpected_Error()
    {
        using var handler = new ScriptedHandler(_ => throw new HttpRequestException("boom"));
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://test.com/"));

        Result result = await client.SendWithRedirectsAsResultAsync(request);

        result.FirstError.Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact]
    public async Task Redirect_Should_Resend_Headers_Body_And_Content_Headers()
    {
        var seen = new List<(string Path, string? Header, string? Body, string? ContentType)>();
        using var handler = new ScriptedHandler(request =>
        {
            seen.Add((
                request.RequestUri!.AbsolutePath,
                request.Headers.TryGetValues("x-trace", out IEnumerable<string>? values) ? values.Single() : null,
                request.Content?.ReadAsStringAsync().GetAwaiter().GetResult(),
                request.Content?.Headers.ContentType?.MediaType));
            return request.RequestUri.AbsolutePath == "/a"
                ? Redirect(HttpStatusCode.TemporaryRedirect, "https://test.com/b")
                : new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://test.com/a"))
        {
            Content = new StringContent("payload", new MediaTypeHeaderValue("text/plain"))
        };
        request.Headers.Add("x-trace", "t1");

        Result result = await client.SendWithRedirectsAsResultAsync(request);

        result.IsSuccess.Should().BeTrue();
        seen.Should().Equal(
            ("/a", "t1", "payload", "text/plain"),
            ("/b", "t1", "payload", "text/plain"));
    }

    [Fact]
    public async Task Generic_Multiple_Redirects_Should_Keep_Headers_And_Return_Value()
    {
        var seen = new List<(string Path, string? Header)>();
        using var handler = new ScriptedHandler(request =>
        {
            seen.Add((
                request.RequestUri!.AbsolutePath,
                request.Headers.TryGetValues("x-trace", out IEnumerable<string>? values) ? values.Single() : null));
            return request.RequestUri.AbsolutePath switch
            {
                "/a" => Redirect(HttpStatusCode.Redirect, "https://test.com/b"),
                "/b" => Redirect(HttpStatusCode.PermanentRedirect, "/c"),
                _ => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"id":3,"name":"C"}""") }
            };
        });
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://test.com/a"));
        request.Headers.Add("x-trace", "t1");

        Result<TestDto> result = await client.SendWithRedirectsAsResultAsync<TestDto>(request);

        result.Value.Should().Be(new TestDto(3, "C"));
        seen.Should().Equal(("/a", "t1"), ("/b", "t1"), ("/c", "t1"));
    }

    private static HttpResponseMessage Redirect(HttpStatusCode status, string location)
    {
        var response = new HttpResponseMessage(status);
        response.Headers.Location = new Uri(location, UriKind.RelativeOrAbsolute);
        return response;
    }

    private sealed record TestDto(int Id, string Name);

    private sealed class ScriptedHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(respond(request));
    }
}
