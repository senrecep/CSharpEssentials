using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace CSharpEssentials.Tests.AspNetCore.ConditionalRequests;

public sealed class ConditionalGetMinimalApiTests
{
    [Fact]
    public async Task WithConditionalGet_Should_SetETagAndLastModified_When_SourceHasBoth()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/docs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ETag().Should().Be("\"doc-7\"");
        response.LastModified().Should().Be(ConditionalData.LastModified);
        (await response.Content.ReadAsStringAsync()).Should().Contain("\"revision\":7");
    }

    [Fact]
    public async Task WithConditionalGet_Should_SetStrongETagFromVersion_When_ValueIsVersioned()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/items/1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ETag().Should().Be("\"v1\"");
        response.LastModified().Should().BeNull();
    }

    [Theory]
    [InlineData("\"v1\"")]
    [InlineData("W/\"v1\"")]
    [InlineData("\"other\", W/\"v1\"")]
    [InlineData("\"other\",\"v1\"")]
    [InlineData("*")]
    public async Task WithConditionalGet_Should_Return304_When_IfNoneMatchMatches(string ifNoneMatch)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/items/1", ("If-None-Match", ifNoneMatch));

        response.StatusCode.Should().Be(HttpStatusCode.NotModified);
        response.ETag().Should().Be("\"v1\"");
        (await response.Content.ReadAsByteArrayAsync()).Should().BeEmpty();
    }

    [Theory]
    [InlineData("\"v2\"")]
    [InlineData("W/\"v2\", \"V1\"")]
    public async Task WithConditionalGet_Should_Return200_When_IfNoneMatchDoesNotMatch(string ifNoneMatch)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/items/1", ("If-None-Match", ifNoneMatch));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ETag().Should().Be("\"v1\"");
    }

    [Theory]
    [InlineData(ConditionalData.LastModified)]
    [InlineData("Sat, 03 Jan 2026 00:00:00 GMT")]
    public async Task WithConditionalGet_Should_Return304_When_IfModifiedSinceIsNotOlderThanLastModified(string ifModifiedSince)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/docs", ("If-Modified-Since", ifModifiedSince));

        response.StatusCode.Should().Be(HttpStatusCode.NotModified);
        response.LastModified().Should().Be(ConditionalData.LastModified);
    }

    [Theory]
    [InlineData("Fri, 02 Jan 2026 03:04:04 GMT")]
    [InlineData("not a date")]
    public async Task WithConditionalGet_Should_Return200_When_IfModifiedSinceIsOlderOrUnparsable(string ifModifiedSince)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/docs", ("If-Modified-Since", ifModifiedSince));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WithConditionalGet_Should_IgnoreIfModifiedSince_When_IfNoneMatchIsPresent()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync(
            "/docs",
            ("If-None-Match", "\"doc-6\""),
            ("If-Modified-Since", ConditionalData.LastModified));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WithConditionalGet_Should_KeepResponseHeaders_When_Returning304()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/docs", ("If-None-Match", "\"doc-7\""));

        response.StatusCode.Should().Be(HttpStatusCode.NotModified);
        response.Headers.CacheControl!.ToString().Should().Be("max-age=60, private");
        response.Headers.Vary.Should().Equal("Accept-Language");
        response.Content.Headers.ContentLocation!.ToString().Should().Be("/docs/1");
        response.ETag().Should().Be("\"doc-7\"");
        response.LastModified().Should().Be(ConditionalData.LastModified);
    }

    [Fact]
    public async Task WithConditionalGet_Should_SetValidators_When_RequestIsHead()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Head, "/docs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ETag().Should().Be("\"doc-7\"");
    }

    [Fact]
    public async Task WithConditionalGet_Should_Return304_When_HeadRequestMatches()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Head, "/docs", ("If-None-Match", "\"doc-7\""));

        response.StatusCode.Should().Be(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task WithConditionalGet_Should_LeaveResponseUntouched_When_MethodIsNotGetOrHead()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Post, "/items", ("If-None-Match", "*"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ETag().Should().BeNull();
    }

    [Fact]
    public async Task WithConditionalGet_Should_LeaveResponseUntouched_When_StatusIsNotSuccess()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/not-found", ("If-None-Match", "*"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.ETag().Should().BeNull();
    }

    [Fact]
    public async Task WithConditionalGet_Should_UseValue_When_HandlerReturnsTypedOk()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/typed", ("If-None-Match", "\"v1\""));

        response.StatusCode.Should().Be(HttpStatusCode.NotModified);
        response.ETag().Should().Be("\"v1\"");
    }

    [Fact]
    public async Task WithConditionalGet_Should_LeaveResponseUntouched_When_ValueIsString()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/string", ("If-None-Match", "*"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ETag().Should().BeNull();
    }

    [Fact]
    public async Task Endpoint_Should_NotSetETag_When_WithConditionalGetIsNotUsed()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/off", ("If-None-Match", "*"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ETag().Should().BeNull();
    }

    [Theory]
    [InlineData("/result-outer")]
    [InlineData("/result-inner")]
    public async Task WithConditionalGet_Should_UseResultValue_When_ResultSucceeds(string path)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage ok = await host.GetAsync(path);
        using HttpResponseMessage notModified = await host.GetAsync(path, ("If-None-Match", "\"v1\""));

        ok.StatusCode.Should().Be(HttpStatusCode.OK);
        ok.ETag().Should().Be("\"v1\"");
        (await ok.Content.ReadAsStringAsync()).Should().Contain("\"version\":\"v1\"");
        notModified.StatusCode.Should().Be(HttpStatusCode.NotModified);
    }

    [Theory]
    [InlineData("/result-outer")]
    [InlineData("/result-inner")]
    public async Task WithConditionalGet_Should_PassFailureThrough_When_ResultFails(string path)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync($"{path}?fail=true", ("If-None-Match", "*"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.ETag().Should().BeNull();
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Theory]
    [InlineData("/group/item")]
    [InlineData("/group/twice")]
    public async Task WithConditionalGet_Should_ApplyToEndpoints_When_UsedOnGroup(string path)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync(path, ("If-None-Match", "\"v1\""));

        response.StatusCode.Should().Be(HttpStatusCode.NotModified);
        response.Headers.GetValues("ETag").Should().Equal("\"v1\"");
    }

    [Fact]
    public async Task WithConditionalGet_Should_UseSource_When_SourceIsRegisteredForVersionedType()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/versioned-doc");

        response.ETag().Should().Be("\"custom-3\"");
    }

    [Fact]
    public async Task WithConditionalGet_Should_UseBaseTypeSource_When_RuntimeTypeDerivesFromIt()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/derived");

        response.ETag().Should().Be("\"doc-9\"");
    }

    [Fact]
    public async Task WithConditionalGet_Should_ResolveSourcePerRequest_When_SourceHasScopedDependency()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/tenant-doc", ("If-None-Match", "\"t1-5\""));

        response.StatusCode.Should().Be(HttpStatusCode.NotModified);
        response.ETag().Should().Be("\"t1-5\"");
    }

    [Fact]
    public async Task WithConditionalGet_Should_NotHashBody_When_FallbackIsOff()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/plain/a");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ETag().Should().BeNull();
    }

    [Fact]
    public async Task WithConditionalGet_Should_SetWeakBodyHash_When_FallbackIsOn()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync(o => o.UseBodyHashFallback = true);

        using HttpResponseMessage first = await host.GetAsync("/plain/a");
        using HttpResponseMessage again = await host.GetAsync("/plain/a");
        using HttpResponseMessage other = await host.GetAsync("/plain/b");
        using HttpResponseMessage notModified = await host.GetAsync("/plain/a", ("If-None-Match", first.ETag()!));

        first.Headers.ETag!.IsWeak.Should().BeTrue();
        again.ETag().Should().Be(first.ETag());
        other.ETag().Should().NotBe(first.ETag());
        notModified.StatusCode.Should().Be(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task WithConditionalGet_Should_PreferVersion_When_FallbackIsOn()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync(o => o.UseBodyHashFallback = true);

        using HttpResponseMessage response = await host.GetAsync("/items/1");

        response.ETag().Should().Be("\"v1\"");
    }

    [Theory]
    [InlineData("has space")]
    [InlineData("quote\"d")]
    [InlineData("ünicode")]
    public async Task WithConditionalGet_Should_HashVersion_When_VersionIsNotAValidEntityTag(string version)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync($"/version?version={Uri.EscapeDataString(version)}");

        response.ETag().Should().MatchRegex("^\"[A-Za-z0-9_-]{43}\"$");
    }

    [Fact]
    public async Task WithConditionalGet_Should_NotSetETag_When_VersionIsEmpty()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/version?version=", ("If-None-Match", "*"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ETag().Should().BeNull();
    }

    [Fact]
    public async Task WithConditionalGet_Should_UseRegisteredGenerator_When_RegisteredBeforeAddConditionalRequests()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync(
            configureServices: services => services.AddSingleton<CSharpEssentials.AspNetCore.IETagGenerator, FixedETagGenerator>());

        using HttpResponseMessage response = await host.GetAsync("/plain/a");

        response.ETag().Should().Be("\"fixed\"");
    }

    [Fact]
    public async Task WithConditionalGet_Should_Throw_When_ServicesAreNotRegistered()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync(addConditionalRequests: false);

        Func<Task> act = () => host.GetAsync("/items/1");

        (await act.Should().ThrowAsync<InvalidOperationException>())
            .Which.Message.Should().Contain("AddConditionalRequests");
    }

    [Fact]
    public async Task WithConditionalGet_Should_NotHashOrReenumerate_When_ValueIsAsyncEnumerable()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync(o => o.UseBodyHashFallback = true);

        using HttpResponseMessage response = await host.GetAsync("/async-items");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ETag().Should().BeNull();
        (await response.Content.ReadAsStringAsync()).Should().Contain("\"a\"").And.Contain("\"b\"");
        host.Services.GetRequiredService<EnumerationCounter>().Count.Should().Be(1);
    }

    [Fact]
    public async Task WithConditionalGet_Should_LeaveResponseUntouched_When_HandlerSetETag()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/existing-etag", ("If-None-Match", "\"v1\""));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues("ETag").Should().Equal("\"handler\"");
    }

    [Theory]
    [InlineData("/result-string-outer")]
    [InlineData("/result-string-inner")]
    public async Task WithConditionalGet_Should_LeaveResponseUntouched_When_ResultValueIsString(string path)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync(o => o.UseBodyHashFallback = true);

        using HttpResponseMessage response = await host.GetAsync(path, ("If-None-Match", "*"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.ETag().Should().BeNull();
        (await response.Content.ReadAsStringAsync()).Should().Contain("v1");
    }

    [Fact]
    public async Task WithConditionalGet_Should_ClampLastModifiedToNow_When_ValueIsInTheFuture()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, 500, TimeSpan.Zero));
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync(
            configureServices: services => services.AddSingleton<TimeProvider>(clock));
        const string now = "Thu, 01 Jan 2026 00:00:00 GMT";

        using HttpResponseMessage response = await host.GetAsync("/docs");
        using HttpResponseMessage notModified = await host.GetAsync("/docs", ("If-Modified-Since", now));

        response.LastModified().Should().Be(now);
        notModified.StatusCode.Should().Be(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task WithConditionalGet_Should_KeepLastModified_When_ValueIsInThePast()
    {
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero));
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync(
            configureServices: services => services.AddSingleton<TimeProvider>(clock));

        using HttpResponseMessage response = await host.GetAsync("/docs");

        response.LastModified().Should().Be(ConditionalData.LastModified);
    }
}
