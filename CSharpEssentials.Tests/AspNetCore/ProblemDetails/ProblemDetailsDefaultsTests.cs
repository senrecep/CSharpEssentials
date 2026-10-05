using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

/// <summary>
/// The 4.0 defaults are secure; <see cref="EnhancedProblemDetailsOptions.UseLegacyDefaults"/> restores the 3.x shape.
/// </summary>
public class ProblemDetailsDefaultsTests
{
    private const string MixedPath = "/problem?s=mixed&ext=true";

    public static TheoryData<HostKind> Hosts() => new() { HostKind.Minimal, HostKind.ApiController, HostKind.PlainController };

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task Problem_Should_NotExposeRequestIdUserOrSpanIds_When_DefaultsAreUsed(HostKind kind)
    {
        JsonObject json = await GetDefaultsAsync(kind, MixedPath);

        json.Select(p => p.Key).Should().NotContain(["requestId", "user", "spanId", "parentSpanId"]);
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task Problem_Should_WriteW3CTraceId_When_DefaultsAreUsed(HostKind kind)
    {
        JsonObject json = await GetDefaultsAsync(kind, MixedPath);

        json["traceId"]!.GetValue<string>().Should().Be(ProblemScenarios.TraceId);
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task Problem_Should_UseRfc9110TypeAndPathInstance_When_DefaultsAreUsed(HostKind kind)
    {
        JsonObject json = await GetDefaultsAsync(kind, MixedPath);

        json["type"]!.GetValue<string>().Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.10");
        json["instance"]!.GetValue<string>().Should().Be("/problem");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task Problem_Should_WriteErrorCodes_When_DefaultsAreUsed(HostKind kind)
    {
        JsonObject json = await GetDefaultsAsync(kind, MixedPath);

        json["errorCodes"]!.AsArray().Select(n => n!.GetValue<string>())
            .Should().Equal("name.required", "email.invalid", "user.notFound", "user.conflict");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task Problem_Should_ListOnlyValidationErrors_When_ErrorsAreMixed(HostKind kind)
    {
        JsonObject json = await GetDefaultsAsync(kind, MixedPath);

        json["errors"]!.AsArray().Select(n => n!["code"]!.GetValue<string>())
            .Should().Equal("name.required", "name.required", "email.invalid");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task Problem_Should_OmitErrors_When_ThereAreNoValidationErrors(HostKind kind)
    {
        JsonObject json = await GetDefaultsAsync(kind, "/problem?s=notfound");

        json.ContainsKey("errors").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task Problem_Should_NotExposeErrorMessagesOrExceptionDetails_When_DefaultsAreUsed(HostKind kind)
    {
        JsonObject json = await GetDefaultsAsync(kind, MixedPath);

        json.Select(p => p.Key).Should().NotContain(["errorMessages", "exception"]);
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task Problem_Should_WriteTraceparentTraceId_When_LegacyDefaultsAreUsed(HostKind kind)
    {
        JsonObject json = await GetLegacyAsync(kind);

        ProblemJson.IsTraceparent(json["traceId"]!.GetValue<string>(), out string traceId).Should().BeTrue();
        traceId.Should().Be(ProblemScenarios.TraceId);
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task Problem_Should_WriteRequestIdUserAndSpanIds_When_LegacyDefaultsAreUsed(HostKind kind)
    {
        JsonObject json = await GetLegacyAsync(kind);

        json["requestId"]!.GetValue<string>().Should().NotBeNullOrWhiteSpace();
        json["user"]!.GetValue<string>().Should().Be(ProblemScenarios.UserName);
        json["spanId"]!.GetValue<string>().Should().MatchRegex("^[0-9a-f]{16}$");
        json["parentSpanId"]!.GetValue<string>().Should().Be(ProblemScenarios.Traceparent);
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task Problem_Should_UseMethodAndPathInstanceAndRfc7231Type_When_LegacyDefaultsAreUsed(HostKind kind)
    {
        JsonObject json = await GetLegacyAsync(kind);

        json["instance"]!.GetValue<string>().Should().Be("GET /problem");
        json["type"]!.GetValue<string>().Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.8");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task Problem_Should_WriteEveryErrorInErrors_When_LegacyDefaultsAreUsed(HostKind kind)
    {
        JsonObject json = await GetLegacyAsync(kind);

        json["errors"]!.AsArray().Select(n => n!["code"]!.GetValue<string>())
            .Should().Equal("name.required", "name.required", "email.invalid", "user.notFound", "user.conflict");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task Problem_Should_WriteErrorMessages_When_LegacyDefaultsAreUsed(HostKind kind)
    {
        JsonObject json = await GetLegacyAsync(kind);

        json["errorMessages"]!.AsArray().Select(n => n!.GetValue<string>())
            .Should().Equal("Name is required", "Name is too short", "Email is invalid", "User not found", "User already exists");
    }

    [Fact]
    public async Task ToProblemResult_Should_WriteSecureDefaults_When_NothingIsRegistered()
    {
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(HostKind.Minimal);

        ProblemResponse response = await host.SendAsync(MixedPath);

        response.MediaType.Should().Be("application/problem+json");
        JsonObject json = response.Json;
        json.Select(p => p.Key).Should().NotContain(["requestId", "user", "spanId", "parentSpanId", "errorMessages", "exception"]);
        json["traceId"]!.GetValue<string>().Should().Be(ProblemScenarios.TraceId);
        json["instance"]!.GetValue<string>().Should().Be("/problem");
        json["type"]!.GetValue<string>().Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.10");
        json["errorCodes"]!.AsArray().Should().HaveCount(4);
        json["errors"]!.AsArray().Should().HaveCount(3);
    }

    [Theory]
    [InlineData(HostKind.Minimal, 413, "https://tools.ietf.org/html/rfc9110#section-15.5.14")]
    [InlineData(HostKind.ApiController, 413, "https://tools.ietf.org/html/rfc9110#section-15.5.14")]
    [InlineData(HostKind.PlainController, 413, "https://tools.ietf.org/html/rfc9110#section-15.5.14")]
    [InlineData(HostKind.Minimal, 410, "https://tools.ietf.org/html/rfc9110#section-15.5.11")]
    [InlineData(HostKind.ApiController, 415, "https://tools.ietf.org/html/rfc9110#section-15.5.16")]
    [InlineData(HostKind.PlainController, 505, "https://tools.ietf.org/html/rfc9110#section-15.6.6")]
    public async Task Problem_Should_UseRfc9110TypeUri_When_StatusCodeIsOverridden(HostKind kind, int status, string expectedType)
    {
        JsonObject json = await GetJsonAsync(
            kind, services => services.AddEnhancedProblemDetails(), $"/problem?s=notfound&status={status}");

        json["status"]!.GetValue<int>().Should().Be(status);
        json["type"]!.GetValue<string>().Should().Be(expectedType);
    }

    [Theory]
    [InlineData(410, "https://tools.ietf.org/html/rfc9110#section-15.5.11")]
    [InlineData(411, "https://tools.ietf.org/html/rfc9110#section-15.5.12")]
    [InlineData(413, "https://tools.ietf.org/html/rfc9110#section-15.5.14")]
    [InlineData(414, "https://tools.ietf.org/html/rfc9110#section-15.5.15")]
    [InlineData(415, "https://tools.ietf.org/html/rfc9110#section-15.5.16")]
    [InlineData(416, "https://tools.ietf.org/html/rfc9110#section-15.5.17")]
    [InlineData(417, "https://tools.ietf.org/html/rfc9110#section-15.5.18")]
    [InlineData(505, "https://tools.ietf.org/html/rfc9110#section-15.6.6")]
    public void Rfc9110_Should_ReturnSectionLink_When_StatusCodeIsDefined(int status, string expectedType)
    {
        ProblemTypeUris.Rfc9110(status).Should().Be(expectedType);
    }

    private static Task<JsonObject> GetDefaultsAsync(HostKind kind, string path) =>
        GetJsonAsync(kind, services => services.AddEnhancedProblemDetails(), path);

    private static Task<JsonObject> GetLegacyAsync(HostKind kind) =>
        GetJsonAsync(kind, services => services.AddEnhancedProblemDetails(o => o.UseLegacyDefaults()), MixedPath);

    private static async Task<JsonObject> GetJsonAsync(HostKind kind, Action<IServiceCollection> configureServices, string path)
    {
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(kind, configureServices);
        ProblemResponse response = await host.SendAsync(path);
        response.MediaType.Should().Be("application/problem+json");
        return response.Json;
    }
}
