using System.Diagnostics;
using System.Text.RegularExpressions;
using CSharpEssentials.Errors;
using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

/// <summary>
/// <see cref="TraceIdFormat.W3CTraceId"/> always yields a trace id: the W3C activity trace id when there is one,
/// otherwise <see cref="HttpContext.TraceIdentifier"/>.
/// </summary>
public static partial class TraceIdFallbackTests
{
    private const string Fallback = "connection-id:0001";

    [GeneratedRegex("^[0-9a-f]{32}$")]
    private static partial Regex HexTraceId();

    [Fact]
    public static void W3CTraceId_Should_FallBackToTraceIdentifier_When_NoActivityExists()
    {
        Activity.Current = null;

        string? traceId = TraceIdOf(CreateContext());

        traceId.Should().Be(Fallback);
    }

    [Fact]
    public static void W3CTraceId_Should_FallBackToTraceIdentifier_When_ActivityUsesHierarchicalId()
    {
        using Activity activity = new("hierarchical");
        activity.SetIdFormat(ActivityIdFormat.Hierarchical);
        activity.Start();

        string? traceId = TraceIdOf(CreateContext());

        traceId.Should().Be(Fallback);
    }

    [Fact]
    public static void W3CTraceId_Should_WriteHexTraceId_When_W3CActivityExists()
    {
        using Activity activity = new("w3c");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.Start();

        string? traceId = TraceIdOf(CreateContext());

        traceId.Should().Be(activity.TraceId.ToHexString());
        traceId.Should().MatchRegex(HexTraceId());
    }

    [Fact]
    public static void None_Should_OmitTraceId_When_NoActivityExists()
    {
        Activity.Current = null;

        string? traceId = TraceIdOf(CreateContext(), TraceIdFormat.None);

        traceId.Should().BeNull();
    }

    private static DefaultHttpContext CreateContext() => new() { TraceIdentifier = Fallback };

    private static string? TraceIdOf(HttpContext httpContext, TraceIdFormat? format = null)
    {
        httpContext.RequestServices = new ServiceCollection()
            .AddEnhancedProblemDetails(o => o.TraceId = format ?? TraceIdFormat.W3CTraceId)
            .BuildServiceProvider();
        IActionResult result = Error.Validation("code", "description").ToActionResult(httpContext);
        ProblemDetails problem = result.Should().BeAssignableTo<ObjectResult>().Subject.Value.Should().BeAssignableTo<ProblemDetails>().Subject;
        return problem.Extensions.TryGetValue("traceId", out object? value) ? value as string : null;
    }
}
