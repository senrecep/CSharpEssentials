using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.ConditionalRequests;

public sealed class IfMatchTests
{
    [Theory]
    [InlineData("/if-match/items")]
    [InlineData("/mvc-ifmatch/items")]
    public async Task WithIfMatch_Should_Return412_When_HandlerReturnsPreconditionFailed(string path)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Put, path, ("If-Match", "\"v1\""));

        response.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        JsonObject problem = (await response.Content.ReadFromJsonAsync<JsonObject>())!;
        problem["status"]!.GetValue<int>().Should().Be(412);
        problem["title"]!.GetValue<string>().Should().Be("Precondition Failed");
    }

    [Theory]
    [InlineData("/if-match/items", "\"v2\"")]
    [InlineData("/if-match/items", "\"v1\", \"v2\"")]
    [InlineData("/if-match/items", "*")]
    [InlineData("/mvc-ifmatch/items", "\"v2\"")]
    [InlineData("/mvc-ifmatch/items", "*")]
    public async Task WithIfMatch_Should_RunHandler_When_IfMatchMatches(string path, string ifMatch)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Put, path, ("If-Match", ifMatch));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("updated");
    }

    [Theory]
    [InlineData("/if-match/items")]
    [InlineData("/mvc-ifmatch/items")]
    public async Task WithIfMatch_Should_RunHandler_When_HeaderIsMissingAndNotRequired(string path)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Put, path);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/if-match/items")]
    [InlineData("/mvc-ifmatch/items")]
    public async Task WithIfMatch_Should_NeverMatch_When_TagIsWeak(string path)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Put, path, ("If-Match", "W/\"v2\""));

        response.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
    }

    [Theory]
    [InlineData("/if-match/required")]
    [InlineData("/mvc-ifmatch/required")]
    [InlineData("/if-match-group/required")]
    public async Task WithIfMatch_Should_Return428_When_RequiredHeaderIsMissing(string path)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Put, path);

        response.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status428PreconditionRequired);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Theory]
    [InlineData("/if-match/required")]
    [InlineData("/mvc-ifmatch/required")]
    public async Task WithIfMatch_Should_RunHandler_When_RequiredHeaderIsPresent(string path)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Put, path, ("If-Match", "\"v9\""));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WithIfMatch_Should_OverrideGroup_When_EndpointIsNotRequired()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Put, "/if-match-group/optional");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("v1")]
    [InlineData("\"v1")]
    [InlineData("\"v1\" \"v2\"")]
    [InlineData("*, \"v1\"")]
    [InlineData(",")]
    public async Task WithIfMatch_Should_Return400_When_HeaderIsMalformed(string ifMatch)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage minimal = await host.SendAsync(HttpMethod.Put, "/if-match/items", ("If-Match", ifMatch));
        using HttpResponseMessage mvc = await host.SendAsync(HttpMethod.Put, "/mvc-ifmatch/items", ("If-Match", ifMatch));

        minimal.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        mvc.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("\"v2\"", true, false, 1, "v2", true)]
    [InlineData("\"42\"", true, false, 1, "42", false)]
    [InlineData("\"\"", true, false, 1, "", false)]
    [InlineData("W/\"v2\"", true, false, 1, null, false)]
    [InlineData("\"v1\", \"v2\"", true, false, 2, null, true)]
    [InlineData("*", true, true, 0, null, true)]
    public async Task GetPreconditions_Should_ExposeParsedHeader_When_IfMatchIsPresent(
        string ifMatch, bool hasIfMatch, bool isWildcard, int count, string? version, bool matches)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Put, "/if-match/inspect", ("If-Match", ifMatch));

        PreconditionsView view = (await response.Content.ReadFromJsonAsync<PreconditionsView>())!;
        view.Should().Be(new PreconditionsView(hasIfMatch, isWildcard, count, version, matches));
    }

    [Fact]
    public async Task GetPreconditions_Should_BeEmpty_When_HeaderIsMissing()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Delete, "/if-match/inspect");

        PreconditionsView view = (await response.Content.ReadFromJsonAsync<PreconditionsView>())!;
        view.Should().Be(new PreconditionsView(false, false, 0, null, true));
    }

    [Fact]
    public async Task GetPreconditions_Should_IgnoreHeader_When_MethodIsSafe()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.GetAsync("/if-match/inspect", ("If-Match", "not valid"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PreconditionsView view = (await response.Content.ReadFromJsonAsync<PreconditionsView>())!;
        view.HasIfMatch.Should().BeFalse();
    }

    [Fact]
    public async Task GetPreconditions_Should_Throw_When_EndpointHasNoIfMatch()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        Func<Task> act = () => host.SendAsync(HttpMethod.Put, "/if-match/no-filter", ("If-Match", "\"v1\""));

        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Contain("WithIfMatch");
    }

    [Theory]
    [InlineData("\"v1\"")]
    [InlineData("W/\"v2\"")]
    public async Task WithIfMatchLoader_Should_Return412BeforeHandler_When_CurrentDoesNotMatch(string ifMatch)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Put, "/if-match/loader/1", ("If-Match", ifMatch));

        response.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
        (await response.Content.ReadAsStringAsync()).Should().NotContain("handled");
    }

    [Theory]
    [InlineData("\"v2\"")]
    [InlineData("*")]
    public async Task WithIfMatchLoader_Should_Return412_When_CurrentDoesNotExist(string ifMatch)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Put, "/if-match/loader/0", ("If-Match", ifMatch));

        response.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
    }

    [Theory]
    [InlineData("\"v2\"")]
    [InlineData("\"v1\", \"v2\"")]
    [InlineData("*")]
    public async Task WithIfMatchLoader_Should_RunHandler_When_CurrentMatches(string ifMatch)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Put, "/if-match/loader/1", ("If-Match", ifMatch));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("handled 1");
    }

    [Fact]
    public async Task WithIfMatchLoader_Should_RunHandler_When_HeaderIsMissing()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Put, "/if-match/loader/0");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WithIfMatch_Should_ApplyToActions_When_UsedOnMapControllers()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync(
            controllerConventions: controllers => controllers.WithIfMatch(required: true));

        using HttpResponseMessage missing = await host.SendAsync(HttpMethod.Put, "/mvc-ifmatch/convention");
        using HttpResponseMessage present = await host.SendAsync(HttpMethod.Put, "/mvc-ifmatch/convention", ("If-Match", "\"v1\""));

        ((int)missing.StatusCode).Should().Be(StatusCodes.Status428PreconditionRequired);
        present.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task IfMatchAttribute_Should_OverrideConvention_When_OnAction()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync(
            controllerConventions: controllers => controllers.WithIfMatch(required: true));

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Put, "/mvc-ifmatch/items");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("\"v1\"", HttpStatusCode.PreconditionFailed)]
    [InlineData("\"v2\"", HttpStatusCode.OK)]
    public async Task WithIfMatchLoader_Should_CheckCurrent_When_UsedOnMapControllers(string ifMatch, HttpStatusCode expected)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync(
            controllerConventions: controllers => controllers.WithIfMatch(
                (_, _) => ValueTask.FromResult<VersionedItem?>(new VersionedItem(1, "v2"))));

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Put, "/mvc-ifmatch/convention", ("If-Match", ifMatch));

        response.StatusCode.Should().Be(expected);
    }

    [Theory]
    [InlineData("POST", "/if-match/methods")]
    [InlineData("PATCH", "/if-match/methods")]
    [InlineData("DELETE", "/if-match/methods")]
    [InlineData("POST", "/mvc-ifmatch/required")]
    [InlineData("PATCH", "/mvc-ifmatch/required")]
    [InlineData("DELETE", "/mvc-ifmatch/required")]
    public async Task WithIfMatch_Should_Return428_When_RequiredHeaderIsMissingOnUnsafeMethod(string method, string path)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(new HttpMethod(method), path);

        ((int)response.StatusCode).Should().Be(StatusCodes.Status428PreconditionRequired);
    }

    [Theory]
    [InlineData("POST", "/if-match/methods")]
    [InlineData("PATCH", "/if-match/methods")]
    [InlineData("DELETE", "/if-match/methods")]
    [InlineData("POST", "/mvc-ifmatch/items")]
    [InlineData("PATCH", "/mvc-ifmatch/items")]
    [InlineData("DELETE", "/mvc-ifmatch/items")]
    public async Task WithIfMatch_Should_Return412OrRun_When_UnsafeMethodHasIfMatch(string method, string path)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage stale = await host.SendAsync(new HttpMethod(method), path, ("If-Match", "\"v1\""));
        using HttpResponseMessage current = await host.SendAsync(new HttpMethod(method), path, ("If-Match", "\"v2\""));

        stale.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
        current.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task WithIfMatch_Should_CombineValues_When_HeaderIsSentOnSeveralLines()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();

        using HttpResponseMessage response = await host.SendAsync(
            HttpMethod.Put, "/if-match/inspect", ("If-Match", "\"v1\""), ("If-Match", "\"v2\""));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PreconditionsView view = (await response.Content.ReadFromJsonAsync<PreconditionsView>())!;
        view.Count.Should().Be(2);
        view.Matches.Should().BeTrue();
    }

    [Fact]
    public async Task WithIfMatch_Should_LetMapperDecideStatus_When_ResultErrorMapperIsRegistered()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync(
            configureServices: services => services.AddSingleton<IResultErrorMapper, ConflictErrorMapper>());

        using HttpResponseMessage response = await host.SendAsync(HttpMethod.Put, "/if-match/items", ("If-Match", "\"v1\""));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("42", true)]
    [InlineData("has space", false)]
    public async Task Matches_Should_RoundTripETag_When_ResourceIsVersioned(string version, bool isRawVersion)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();
        string query = Uri.EscapeDataString(version);
        using HttpResponseMessage get = await host.GetAsync($"/version?version={query}");

        using HttpResponseMessage put = await host.SendAsync(HttpMethod.Put, $"/if-match/versioned?version={query}", ("If-Match", get.ETag()!));

        put.StatusCode.Should().Be(HttpStatusCode.OK);
        VersionCheck check = (await put.Content.ReadFromJsonAsync<VersionCheck>())!;
        check.Matches.Should().BeTrue();
        (check.IfMatchVersion == version).Should().Be(isRawVersion);
    }

    [Fact]
    public async Task Matches_Should_Fail_When_VersionedResourceChanged()
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync();
        using HttpResponseMessage get = await host.GetAsync($"/version?version={Uri.EscapeDataString("has space")}");

        using HttpResponseMessage put = await host.SendAsync(HttpMethod.Put, "/if-match/versioned?version=other", ("If-Match", get.ETag()!));

        (await put.Content.ReadFromJsonAsync<VersionCheck>())!.Matches.Should().BeFalse();
    }

    [Theory]
    [InlineData(null, HttpStatusCode.PreconditionFailed)]
    [InlineData("*", HttpStatusCode.OK)]
    public async Task WithIfMatchLoader_Should_Return412_When_CurrentETagIsWeakBodyHash(string? ifMatch, HttpStatusCode expected)
    {
        await using ConditionalRequestsTestHost host = await ConditionalRequestsTestHost.StartAsync(o => o.UseBodyHashFallback = true);
        using HttpResponseMessage get = await host.GetAsync("/plain/a");
        string weak = get.ETag()!;

        using HttpResponseMessage put = await host.SendAsync(HttpMethod.Put, "/if-match/plain-loader", ("If-Match", ifMatch ?? weak));
        using HttpResponseMessage strong = await host.SendAsync(HttpMethod.Put, "/if-match/plain-loader", ("If-Match", ifMatch ?? weak[2..]));

        put.StatusCode.Should().Be(expected);
        strong.StatusCode.Should().Be(expected);
    }
}
