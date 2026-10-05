using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

/// <summary>
/// Enrichers, the delegate overload of <c>AddEnhancedProblemDetails</c> and the application's own
/// <see cref="ProblemDetailsOptions.CustomizeProblemDetails"/>.
/// </summary>
public class ProblemDetailsCustomizationTests
{
    private const string Path = "/problem?s=mixed&ext=true";

    public static TheoryData<HostKind> Hosts() => new() { HostKind.Minimal, HostKind.ApiController, HostKind.PlainController };

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task AddProblemDetailsEnricher_Should_RunEnricher_When_ProblemIsWritten(HostKind kind)
    {
        ProblemResponse response = await SendAsync(
            kind, services => services.AddEnhancedProblemDetails().AddProblemDetailsEnricher<MarkerEnricher>());

        response.Json[MarkerEnricher.Key]!.GetValue<string>().Should().Be("yes");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task AddProblemDetailsEnricher_Should_RunEnricher_When_RegisteredBeforeAddEnhancedProblemDetails(HostKind kind)
    {
        ProblemResponse response = await SendAsync(
            kind, services => services.AddProblemDetailsEnricher<MarkerEnricher>().AddEnhancedProblemDetails());

        response.Json[MarkerEnricher.Key]!.GetValue<string>().Should().Be("yes");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task AddEnhancedProblemDetails_Should_RunDelegate_When_DelegateOverloadIsUsed(HostKind kind)
    {
        ProblemResponse response = await SendAsync(
            kind,
            services => services.AddEnhancedProblemDetails((problem, httpContext) =>
                problem.Extensions["fromDelegate"] = httpContext.Request.Path.Value));

        response.Json["fromDelegate"]!.GetValue<string>().Should().Be("/problem");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task AddEnhancedProblemDetails_Should_KeepSecureDefaults_When_DelegateOverloadIsUsed(HostKind kind)
    {
        ProblemResponse response = await SendAsync(
            kind, services => services.AddEnhancedProblemDetails((problem, _) => problem.Extensions["fromDelegate"] = true));

        response.Json["traceId"]!.GetValue<string>().Should().Be(ProblemScenarios.TraceId);
        response.Json.ContainsKey("requestId").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task CustomizeProblemDetails_Should_RunAfterEnrichment_When_RegisteredBeforeAddEnhancedProblemDetails(HostKind kind)
    {
        ProblemResponse response = await SendAsync(kind, services =>
        {
            services.AddProblemDetails(o => o.CustomizeProblemDetails = OverrideTraceId);
            services.AddEnhancedProblemDetails();
        });

        response.Json["traceId"]!.GetValue<string>().Should().Be("from-user");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task CustomizeProblemDetails_Should_RunAfterEnrichment_When_RegisteredAfterAddEnhancedProblemDetails(HostKind kind)
    {
        ProblemResponse response = await SendAsync(kind, services =>
        {
            services.AddEnhancedProblemDetails();
            services.AddProblemDetails(o => o.CustomizeProblemDetails = OverrideTraceId);
        });

        response.Json["traceId"]!.GetValue<string>().Should().Be("from-user");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task CustomizeProblemDetails_Should_KeepEnrichment_When_UserDelegateAddsOtherMembers(HostKind kind)
    {
        ProblemResponse response = await SendAsync(kind, services =>
        {
            services.AddProblemDetails(o => o.CustomizeProblemDetails = c => c.ProblemDetails.Extensions["fromUser"] = true);
            services.AddEnhancedProblemDetails().AddProblemDetailsEnricher<MarkerEnricher>();
        });

        response.Json["fromUser"]!.GetValue<bool>().Should().BeTrue();
        response.Json[MarkerEnricher.Key]!.GetValue<string>().Should().Be("yes");
        response.Json["traceId"]!.GetValue<string>().Should().Be(ProblemScenarios.TraceId);
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task Problem_Should_RunEnricherExactlyOnce_When_ProblemIsWritten(HostKind kind)
    {
        ProblemResponse response = await SendAsync(
            kind, services => services.AddEnhancedProblemDetails().AddProblemDetailsEnricher<CountingEnricher>());

        response.Json[CountingEnricher.Key]!.GetValue<int>().Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task Problem_Should_RunEnricherExactlyOnce_When_OnlyPlainAddProblemDetailsIsRegistered(HostKind kind)
    {
        ProblemResponse response = await SendAsync(
            kind, services => services.AddProblemDetails().AddProblemDetailsEnricher<CountingEnricher>());

        response.Json[CountingEnricher.Key]!.GetValue<int>().Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task GlobalExceptionHandler_Should_RunEnricherExactlyOnce_When_ExceptionIsHandled(HostKind kind)
    {
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(
            kind,
            services =>
            {
                services.AddEnhancedProblemDetails().AddProblemDetailsEnricher<CountingEnricher>();
                services.AddExceptionHandler<GlobalExceptionHandler>();
            },
            app => app.UseEnhancedProblemDetails());

        ProblemResponse response = await host.SendAsync("/throw?e=invalid");

        response.StatusCode.Should().Be(500);
        response.Json[CountingEnricher.Key]!.GetValue<int>().Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task CustomizeProblemDetails_Should_RunOnceAfterEnrichment_When_OnlyPlainAddProblemDetailsIsRegistered(HostKind kind)
    {
        ProblemResponse response = await SendAsync(kind, services =>
            services.AddProblemDetails(o => o.CustomizeProblemDetails = OverrideTraceIdAndCount));

        response.Json["traceId"]!.GetValue<string>().Should().Be("from-user");
        response.Json["customizeCount"]!.GetValue<int>().Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task CustomizeProblemDetails_Should_RunOnceAfterEnrichment_When_GlobalExceptionHandlerHandlesWithPlainAddProblemDetails(HostKind kind)
    {
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(
            kind,
            services =>
            {
                services.AddProblemDetails(o => o.CustomizeProblemDetails = OverrideTraceIdAndCount);
                services.AddExceptionHandler<GlobalExceptionHandler>();
            },
            app => app.UseEnhancedProblemDetails());

        ProblemResponse response = await host.SendAsync("/throw?e=invalid");

        response.StatusCode.Should().Be(500);
        response.Json["traceId"]!.GetValue<string>().Should().Be("from-user");
        response.Json["customizeCount"]!.GetValue<int>().Should().Be(1);
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task CustomizeProblemDetails_Should_RunOnce_When_EnhancedProblemDetailsAreRegistered(HostKind kind)
    {
        ProblemResponse response = await SendAsync(kind, services =>
            services.AddEnhancedProblemDetails().AddProblemDetails(o => o.CustomizeProblemDetails = OverrideTraceIdAndCount));

        response.Json["customizeCount"]!.GetValue<int>().Should().Be(1);
    }

    [Fact]
    public async Task ToActionResult_Should_RunEnricherOnce_When_ExecutedWithHttpContext()
    {
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(
            HostKind.PlainController, services => services.AddEnhancedProblemDetails().AddProblemDetailsEnricher<CountingEnricher>());
        var httpContext = new DefaultHttpContext { RequestServices = host.Services };
        httpContext.Response.Body = new MemoryStream();

        IActionResult result = ProblemScenarios.Get("mixed").ToActionResult(httpContext);
        await result.ExecuteResultAsync(new ActionContext(httpContext, new RouteData(), new ActionDescriptor()));

        httpContext.Items[CountingEnricher.Key].Should().Be(1);
    }

    [Fact]
    public async Task AddErrorStatusCodeMapper_Should_ApplyMapper_When_RegisteredBeforeAddEnhancedProblemDetails()
    {
        ProblemResponse response = await SendAsync(
            HostKind.Minimal,
            services => services.AddErrorStatusCodeMapper<AuthStatusCodeMapper>().AddEnhancedProblemDetails(),
            "/problem?s=unauthorized");

        response.StatusCode.Should().Be(401);
    }

    private static void OverrideTraceIdAndCount(ProblemDetailsContext context)
    {
        int count = context.HttpContext.Items["customizeCount"] is int previous ? previous + 1 : 1;
        context.HttpContext.Items["customizeCount"] = count;
        context.ProblemDetails.Extensions["customizeCount"] = count;
        context.ProblemDetails.Extensions["traceId"] = "from-user";
    }

    private static void OverrideTraceId(ProblemDetailsContext context) =>
        context.ProblemDetails.Extensions["traceId"] = "from-user";

    private static async Task<ProblemResponse> SendAsync(HostKind kind, Action<IServiceCollection> configureServices, string path = Path)
    {
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(kind, configureServices);
        ProblemResponse response = await host.SendAsync(path);
        response.MediaType.Should().Be("application/problem+json");
        return response;
    }
}
