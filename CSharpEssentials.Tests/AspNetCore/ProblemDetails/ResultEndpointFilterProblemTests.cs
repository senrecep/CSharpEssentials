using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

/// <summary>
/// A failed <c>Result</c>/<c>Result&lt;T&gt;</c> turned into a problem by <see cref="ResultEndpointFilter"/> is the same
/// response as returning <c>errors.ToProblemResult()</c> from the endpoint.
/// </summary>
public class ResultEndpointFilterProblemTests
{
    public static TheoryData<string, string, bool> Cases()
    {
        var data = new TheoryData<string, string, bool>();
        foreach (string resultPath in new[] { "/result", "/result-of-t" })
            foreach (string scenario in new[] { "mixed", "validation", "notfound" })
                foreach (bool legacy in new[] { false, true })
                    data.Add(resultPath, scenario, legacy);
        return data;
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task ResultEndpointFilter_Should_WriteSameProblemAsToProblemResult_When_ResultFails(string resultPath, string scenario, bool legacy)
    {
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(
            HostKind.Minimal,
            services => services.AddEnhancedProblemDetails(o =>
            {
                if (legacy)
                    o.UseLegacyDefaults();
            }));

        ProblemResponse fromFilter = await host.SendAsync($"{resultPath}?s={scenario}");
        ProblemResponse fromResult = await host.SendAsync($"/problem?s={scenario}");

        ProblemDetailsParityTests.AssertIdentical(fromResult, WithInstanceOf(fromFilter, resultPath, legacy));
    }

    [Theory]
    [InlineData("/result")]
    [InlineData("/result-of-t")]
    public async Task ResultEndpointFilter_Should_WriteSameProblemAsToProblemResult_When_NothingIsRegistered(string resultPath)
    {
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(HostKind.Minimal);

        ProblemResponse fromFilter = await host.SendAsync($"{resultPath}?s=mixed");
        ProblemResponse fromResult = await host.SendAsync("/problem?s=mixed");

        ProblemDetailsParityTests.AssertIdentical(fromResult, WithInstanceOf(fromFilter, resultPath, legacy: false));
    }

    /// <summary>
    /// The endpoints live at different paths, so <c>instance</c> is checked here and then aligned with <c>/problem</c>.
    /// </summary>
    private static ProblemResponse WithInstanceOf(ProblemResponse response, string resultPath, bool legacy)
    {
        string prefix = legacy ? "GET " : string.Empty;
        System.Text.Json.Nodes.JsonObject json = response.Json;
        json["instance"]!.GetValue<string>().Should().Be(prefix + resultPath);
        json["instance"] = prefix + "/problem";
        return response with { Body = json.ToJsonString() };
    }
}
