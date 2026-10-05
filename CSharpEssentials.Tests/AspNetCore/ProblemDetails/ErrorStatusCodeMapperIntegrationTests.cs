using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

/// <summary>
/// A custom <see cref="IErrorStatusCodeMapper"/> registered with <c>AddErrorStatusCodeMapper</c> drives the status of
/// Minimal API, MVC and <see cref="ResultEndpointFilter"/> responses.
/// </summary>
public class ErrorStatusCodeMapperIntegrationTests
{
    public static TheoryData<HostKind, string, string, int, string> Cases()
    {
        var data = new TheoryData<HostKind, string, string, int, string>();
        foreach ((string scenario, int status, string title) in new[] { ("unauthorized", 401, "Unauthorized"), ("forbidden", 403, "Forbidden") })
        {
            data.Add(HostKind.Minimal, "/problem", scenario, status, title);
            data.Add(HostKind.Minimal, "/result", scenario, status, title);
            data.Add(HostKind.Minimal, "/result-of-t", scenario, status, title);
            data.Add(HostKind.ApiController, "/problem", scenario, status, title);
        }
        return data;
    }

    public static TheoryData<HostKind, string, string, int> StatusCases()
    {
        var data = new TheoryData<HostKind, string, string, int>();
        foreach (object[] row in Cases())
            data.Add((HostKind)row[0], (string)row[1], (string)row[2], (int)row[3]);
        return data;
    }

    [Theory]
    [MemberData(nameof(StatusCases))]
    public async Task Problem_Should_UseCustomStatusCode_When_ErrorStatusCodeMapperIsRegistered(
        HostKind kind, string path, string scenario, int status)
    {
        ProblemResponse response = await SendAsync(kind, $"{path}?s={scenario}");

        response.StatusCode.Should().Be(status);
        response.Json["status"]!.GetValue<int>().Should().Be(status);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task Problem_Should_UseReasonPhraseTitle_When_CustomStatusDiffersFromErrorType(
        HostKind kind, string path, string scenario, int status, string title)
    {
        ProblemResponse response = await SendAsync(kind, $"{path}?s={scenario}");

        response.StatusCode.Should().Be(status);
        response.Json["title"]!.GetValue<string>().Should().Be(title);
    }

    [Theory]
    [InlineData("unauthorized", 401)]
    [InlineData("forbidden", 403)]
    public async Task PlainControllerToActionResult_Should_UseCustomStatusCode_When_ErrorStatusCodeMapperIsRegistered(string scenario, int status)
    {
        ProblemResponse response = await SendAsync(HostKind.PlainController, $"/problem?s={scenario}");

        response.StatusCode.Should().Be(status);
    }

    [Fact]
    public async Task Problem_Should_KeepDefaultStatusCode_When_MapperDoesNotKnowTheCode()
    {
        ProblemResponse response = await SendAsync(HostKind.Minimal, "/problem?s=notfound");

        response.StatusCode.Should().Be(404);
    }

    private static async Task<ProblemResponse> SendAsync(HostKind kind, string path)
    {
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(
            kind,
            services => services.AddEnhancedProblemDetails().AddErrorStatusCodeMapper<AuthStatusCodeMapper>());
        return await host.SendAsync(path);
    }
}
