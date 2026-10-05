using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

/// <summary>
/// Framework-generated 404/405 responses from <c>UseStatusCodePages</c> (wired by <c>UseEnhancedProblemDetails</c>)
/// go through the same enrichment as library-generated problems.
/// </summary>
public class StatusCodePagesProblemTests
{
    public static TheoryData<HostKind, string, string, int> Cases()
    {
        var data = new TheoryData<HostKind, string, string, int>();
        foreach (HostKind kind in Enum.GetValues<HostKind>())
        {
            data.Add(kind, "GET", "/does-not-exist", 404);
            data.Add(kind, "POST", "/problem", 405);
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task StatusCodePages_Should_WriteProblemContentType_When_EnhancedProblemDetailsAreUsed(
        HostKind kind, string method, string path, int status)
    {
        ProblemResponse response = await SendAsync(kind, method, path);

        response.StatusCode.Should().Be(status);
        response.MediaType.Should().Be("application/problem+json", "body: {0}", response.Body);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task StatusCodePages_Should_WriteW3CTraceId_When_EnhancedProblemDetailsAreUsed(
        HostKind kind, string method, string path, int status)
    {
        ProblemResponse response = await SendAsync(kind, method, path);

        response.Json["status"]!.GetValue<int>().Should().Be(status);
        response.Json["traceId"]!.GetValue<string>().Should().Be(ProblemScenarios.TraceId);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task StatusCodePages_Should_RunCustomEnricher_When_EnricherIsRegistered(
        HostKind kind, string method, string path, int status)
    {
        ProblemResponse response = await SendAsync(kind, method, path);

        response.StatusCode.Should().Be(status);
        response.Json[MarkerEnricher.Key]!.GetValue<string>().Should().Be("yes");
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task StatusCodePages_Should_UsePathInstance_When_EnhancedProblemDetailsAreUsed(
        HostKind kind, string method, string path, int status)
    {
        ProblemResponse response = await SendAsync(kind, method, path);

        response.StatusCode.Should().Be(status);
        response.Json["instance"]!.GetValue<string>().Should().Be(path);
    }

    private static async Task<ProblemResponse> SendAsync(HostKind kind, string method, string path)
    {
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(
            kind,
            services => services.AddEnhancedProblemDetails().AddProblemDetailsEnricher<MarkerEnricher>(),
            app => app.UseEnhancedProblemDetails());
        return await host.SendAsync(path, new HttpMethod(method));
    }
}
