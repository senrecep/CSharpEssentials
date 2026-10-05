using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

/// <summary>
/// The same <c>Error[]</c> must produce the same problem response from a Minimal API endpoint (<c>ToProblemResult</c>),
/// an <c>[ApiController]</c> action and a plain controller action (<c>ToActionResult</c>).
/// </summary>
public class ProblemDetailsParityTests
{
    private static readonly Dictionary<string, Action<EnhancedProblemDetailsOptions>> OptionCases = new()
    {
        ["defaults"] = _ => { },
        ["legacy"] = o => o.UseLegacyDefaults(),
        ["trace-w3c"] = o => o.TraceId = TraceIdFormat.W3CTraceId,
        ["trace-traceparent"] = o => o.TraceId = TraceIdFormat.TraceparentHeader,
        ["trace-none"] = o => o.TraceId = TraceIdFormat.None,
        ["include-all"] = o =>
        {
            o.IncludeUser = true;
            o.IncludeSpanIds = true;
            o.IncludeRequestId = true;
        },
        ["include-spans-traceparent"] = o =>
        {
            o.IncludeSpanIds = true;
            o.TraceId = TraceIdFormat.TraceparentHeader;
        },
        ["instance-path"] = o => o.Instance = ProblemInstanceFormat.Path,
        ["instance-method-path"] = o => o.Instance = ProblemInstanceFormat.MethodAndPath,
        ["instance-none"] = o => o.Instance = ProblemInstanceFormat.None,
        ["fields-none"] = o => o.ErrorFields = ProblemErrorFields.None,
        ["fields-codes"] = o => o.ErrorFields = ProblemErrorFields.Codes,
        ["fields-validation"] = o => o.ErrorFields = ProblemErrorFields.ValidationErrors,
        ["fields-messages"] = o => o.ErrorFields = ProblemErrorFields.Messages,
        ["fields-allerrors"] = o => o.ErrorFields = ProblemErrorFields.AllErrors,
        ["fields-all"] = o => o.ErrorFields = ProblemErrorFields.All,
        ["validation-list"] = o => o.ValidationErrorsFormat = ValidationErrorsFormat.List,
        ["validation-dictionary"] = o => o.ValidationErrorsFormat = ValidationErrorsFormat.Dictionary,
        ["type-rfc7231"] = o => o.TypeUriResolver = ProblemTypeUris.Rfc7231,
        ["type-null"] = o => o.TypeUriResolver = _ => null,
    };

    public static TheoryData<string, string> Cases()
    {
        var data = new TheoryData<string, string>();
        foreach (string option in OptionCases.Keys)
            foreach (string path in new[] { "/problem?s=mixed&ext=true", "/problem?s=validation", "/problem?s=notfound" })
                data.Add(option, path);
        return data;
    }

    private static readonly HostKind[] AllKinds = [HostKind.Minimal, HostKind.ApiController, HostKind.PlainController];

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task AllEndpointStyles_Should_WriteIdenticalProblem_When_SameErrorsAndOptions(string option, string path)
    {
        Action<EnhancedProblemDetailsOptions> configure = OptionCases[option];

        ProblemResponse[] responses = await SendAsync(
            services => services.AddEnhancedProblemDetails(configure), path, "application/json", AllKinds);

        AssertAllIdentical(responses);
        responses[0].Json.ContainsKey("errorCodes").Should().Be(
            ShouldWriteErrorCodes(option), "errorCodes presence follows ErrorFields. Body: {0}", responses[0].Body);
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task AllEndpointStyles_Should_WriteIdenticalProblem_When_OnlyPlainAddProblemDetailsIsRegistered(string option, string path)
    {
        Action<EnhancedProblemDetailsOptions> configure = OptionCases[option];

        ProblemResponse[] responses = await SendAsync(
            services =>
            {
                services.AddProblemDetails();
                services.Configure(configure);
            },
            path, "application/json", AllKinds);

        AssertAllIdentical(responses);
    }

    [Theory]
    [InlineData("application/json")]
    [InlineData("application/problem+json")]
    [InlineData(null)]
    [InlineData("text/html")]
    public async Task AllEndpointStyles_Should_WriteIdenticalProblem_When_AcceptHeaderVaries(string? accept)
    {
        ProblemResponse[] responses = await SendAsync(
            services => services.AddEnhancedProblemDetails(), "/problem?s=mixed&ext=true", accept, AllKinds);

        AssertAllIdentical(responses);
    }

    [Theory]
    [InlineData("/problem?s=mixed&ext=true")]
    [InlineData("/problem?s=validation")]
    [InlineData("/problem?s=notfound")]
    public async Task AllEndpointStyles_Should_WriteIdenticalProblem_When_NothingIsRegistered(string path)
    {
        ProblemResponse[] responses = await SendAsync(_ => { }, path, "application/json", AllKinds);

        AssertAllIdentical(responses);
    }

    [Theory]
    [InlineData("defaults", "/problem?s=mixed&ext=true")]
    [InlineData("legacy", "/problem?s=mixed&ext=true")]
    [InlineData("trace-traceparent", "/problem?s=validation")]
    public async Task AllEndpointStyles_Should_WriteIdenticalProblem_When_LibraryJsonConfigurationIsNotApplied(string option, string path)
    {
        Action<EnhancedProblemDetailsOptions> configure = OptionCases[option];
        var responses = new List<ProblemResponse>();
        foreach (HostKind kind in AllKinds)
        {
            await using ProblemTestHost host = await ProblemTestHost.StartAsync(
                kind, services => services.AddEnhancedProblemDetails(configure), useLibraryJson: false);
            responses.Add(await host.SendAsync(path));
        }

        AssertAllIdentical([.. responses]);
    }

    private static bool ShouldWriteErrorCodes(string option) => option is not ("fields-none" or "fields-messages" or "fields-validation" or "fields-allerrors");

    private static void AssertAllIdentical(ProblemResponse[] responses)
    {
        for (int i = 1; i < responses.Length; i++)
            AssertIdentical(responses[0], responses[i]);
    }

    internal static async Task<ProblemResponse[]> SendAsync(
        Action<IServiceCollection> configureServices,
        string path,
        string? accept,
        params HostKind[] kinds)
    {
        var responses = new List<ProblemResponse>();
        foreach (HostKind kind in kinds)
            responses.Add(await SendOneAsync(kind, configureServices, path, accept));
        return [.. responses];
    }

    private static async Task<ProblemResponse> SendOneAsync(
        HostKind kind,
        Action<IServiceCollection> configureServices,
        string path,
        string? accept)
    {
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(kind, configureServices);
        return await host.SendAsync(path, accept: accept);
    }

    internal static void AssertIdentical(ProblemResponse expected, ProblemResponse actual)
    {
        AssertSameEnvelope(expected, actual);
        ProblemJson.Canonical(actual.Json).Should().Be(ProblemJson.Canonical(expected.Json));
    }

    private static void AssertSameEnvelope(ProblemResponse expected, ProblemResponse actual)
    {
        expected.MediaType.Should().Be("application/problem+json", "expected body: {0}", expected.Body);
        actual.StatusCode.Should().Be(expected.StatusCode);
        actual.MediaType.Should().Be(expected.MediaType, "actual body: {0}", actual.Body);
    }
}
