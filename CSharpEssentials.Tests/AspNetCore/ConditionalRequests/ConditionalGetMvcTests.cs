using System.Net;
using CSharpEssentials.AspNetCore;
using FluentAssertions;

namespace CSharpEssentials.Tests.AspNetCore.ConditionalRequests;

public sealed class ConditionalGetMvcTests
{
    [Fact]
    public async Task ConditionalGetAttribute_Should_SetETag_When_ActionReturnsValue()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/mvc/items/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ETag().Should().Be("\"v1\"");
        (await response.Content.ReadAsStringAsync()).Should().Contain("\"version\":\"v1\"");
    }

    [Theory]
    [InlineData("W/\"v1\"")]
    [InlineData("\"other\", \"v1\"")]
    [InlineData("*")]
    public async Task ConditionalGetAttribute_Should_Return304_When_IfNoneMatchMatches(string ifNoneMatch)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/mvc/items/1", ("If-None-Match", ifNoneMatch));

        response.StatusCode.Should().Be(HttpStatusCode.NotModified);
        response.ETag().Should().Be("\"v1\"");
        (await response.Content.ReadAsByteArrayAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task ConditionalGetAttribute_Should_Return200_When_IfNoneMatchDoesNotMatch()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/mvc/items/1", ("If-None-Match", "\"v2\""));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ConditionalGetAttribute_Should_Return304_When_HeadRequestMatches()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Head, "/mvc/items/1", ("If-None-Match", "\"v1\""));

        response.StatusCode.Should().Be(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task ConditionalGetAttribute_Should_KeepHeadersAndLastModified_When_IfModifiedSinceMatches()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/mvc/docs", ("If-Modified-Since", ConditionalData.LastModified));

        response.StatusCode.Should().Be(HttpStatusCode.NotModified);
        response.ETag().Should().Be("\"doc-7\"");
        response.LastModified().Should().Be(ConditionalData.LastModified);
        response.Headers.CacheControl!.ToString().Should().Be("max-age=60, private");
    }

    [Fact]
    public async Task ConditionalGetAttribute_Should_UseResultValue_When_ActionReturnsSuccessfulResult()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/mvc/result", ("If-None-Match", "\"v1\""));

        response.StatusCode.Should().Be(HttpStatusCode.NotModified);
        response.ETag().Should().Be("\"v1\"");
    }

    [Fact]
    public async Task ConditionalGetAttribute_Should_LeaveResponseUntouched_When_StatusIsNotSuccess()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/mvc/missing", ("If-None-Match", "*"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.ETag().Should().BeNull();
    }

    [Fact]
    public async Task ConditionalGetAttribute_Should_LeaveResponseUntouched_When_MethodIsPost()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Post, "/mvc/items", ("If-None-Match", "*"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ETag().Should().BeNull();
    }

    [Fact]
    public async Task Action_Should_NotSetETag_When_AttributeIsMissing()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/mvc/off", ("If-None-Match", "*"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ETag().Should().BeNull();
    }

    [Fact]
    public async Task ConditionalGetAttribute_Should_ApplyToActions_When_OnController()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/mvc-class", ("If-None-Match", "\"v1\""));

        response.StatusCode.Should().Be(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task WithConditionalGet_Should_ApplyToActions_When_UsedOnMapControllers()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync(
            controllerConventions: controllers => controllers.WithConditionalGet());

        using HttpResponseMessage response = await host.GetAsync("/mvc/off", ("If-None-Match", "\"v1\""));

        response.StatusCode.Should().Be(HttpStatusCode.NotModified);
        response.Headers.GetValues("ETag").Should().Equal("\"v1\"");
    }

    [Fact]
    public async Task ConditionalGetAttribute_Should_KeepResponseHeaders_When_Returning304()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/mvc/docs", ("If-None-Match", "\"doc-7\""));

        response.StatusCode.Should().Be(HttpStatusCode.NotModified);
        response.Headers.CacheControl!.ToString().Should().Be("max-age=60, private");
        response.Headers.Vary.Should().Equal("Accept-Language");
        response.Content.Headers.ContentLocation!.ToString().Should().Be("/docs/1");
        response.ETag().Should().Be("\"doc-7\"");
    }

    [Theory]
    [InlineData("/mvc/result-string")]
    [InlineData("/mvc/problem")]
    public async Task ConditionalGetAttribute_Should_LeaveResponseUntouched_When_ValueIsStringOrProblemDetails(string path)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync(o => o.UseBodyHashFallback = true);

        using HttpResponseMessage response = await host.GetAsync(path, ("If-None-Match", "*"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ETag().Should().BeNull();
    }
}
