using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

/// <summary>
/// <see cref="GlobalExceptionHandler"/> registered with <c>AddExceptionHandler</c> and <c>UseEnhancedProblemDetails</c>,
/// for exceptions thrown from Minimal API endpoints and from MVC actions.
/// </summary>
public class GlobalExceptionHandlerIntegrationTests
{
    public static TheoryData<HostKind> Hosts() => new() { HostKind.Minimal, HostKind.ApiController, HostKind.PlainController };

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task TryHandleAsync_Should_Return500WithGenericBody_When_OperationIsCanceledServerSide(HostKind kind)
    {
        ProblemResponse response = await ThrowAsync(kind, "canceled");

        response.StatusCode.Should().Be(500);
        response.Json["title"]!.GetValue<string>().Should().Be("An unexpected error occurred.");
        response.Body.Should().NotContain("canceled");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task TryHandleAsync_Should_UseBadHttpRequestStatusAndDetail_When_BadHttpRequestIsThrown(HostKind kind)
    {
        ProblemResponse response = await ThrowAsync(kind, "badrequest");

        response.StatusCode.Should().Be(413);
        response.Json["detail"]!.GetValue<string>().Should().Be("Request body too large");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task TryHandleAsync_Should_UseErrorStatusAndCode_When_DomainExceptionIsThrown(HostKind kind)
    {
        ProblemResponse response = await ThrowAsync(kind, "domain");

        response.StatusCode.Should().Be(409);
        response.Json["detail"]!.GetValue<string>().Should().Be("Order already shipped");
        response.Json["errorCodes"]!.AsArray().Select(n => n!.GetValue<string>()).Should().Equal("order.conflict");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task TryHandleAsync_Should_Return400WithValidationErrors_When_EnhancedValidationExceptionIsThrown(HostKind kind)
    {
        ProblemResponse response = await ThrowAsync(kind, "validation");

        response.StatusCode.Should().Be(400);
        JsonArray errors = response.Json["errors"]!.AsArray();
        errors.Select(n => n!["code"]!.GetValue<string>()).Should().Equal("email.invalid");
        errors.Select(n => n!["description"]!.GetValue<string>()).Should().Equal("Email is invalid");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task TryHandleAsync_Should_Return500_When_UnknownExceptionIsThrown(HostKind kind)
    {
        ProblemResponse response = await ThrowAsync(kind, "invalid");

        response.StatusCode.Should().Be(500);
        response.MediaType.Should().Be("application/problem+json");
        response.Json["title"]!.GetValue<string>().Should().Be("An unexpected error occurred.");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task TryHandleAsync_Should_NotLeakExceptionMessage_When_UnknownExceptionIsThrown(HostKind kind)
    {
        ProblemResponse response = await ThrowAsync(kind, "invalid");

        response.Body.Should().NotContainEquivalentOf("secret");
        response.Json.ContainsKey("exception").Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task TryHandleAsync_Should_WriteTraceIdAndPathInstance_When_UnknownExceptionIsThrown(HostKind kind)
    {
        ProblemResponse response = await ThrowAsync(kind, "invalid");

        response.Json["traceId"]!.GetValue<string>().Should().Be(ProblemScenarios.TraceId);
        response.Json["instance"]!.GetValue<string>().Should().Be("/throw");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task TryHandleAsync_Should_WriteExceptionTypeAndMessage_When_ExposeExceptionDetailsIsEnabled(HostKind kind)
    {
        ProblemResponse response = await ThrowAsync(
            kind, "invalid", services => services.AddEnhancedProblemDetails(o => o.ExposeExceptionDetails = true));

        JsonObject exception = response.Json["exception"]!.AsObject();
        exception["type"]!.GetValue<string>().Should().Be(typeof(InvalidOperationException).FullName);
        exception["message"]!.GetValue<string>().Should().Be("secret");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task TryHandleAsync_Should_UseCustomMapper_When_MapperHandlesException(HostKind kind)
    {
        ProblemResponse response = await ThrowAsync(
            kind, "invalid", services => services.AddEnhancedProblemDetails().AddExceptionProblemMapper<InvalidOperationProblemMapper>());

        response.StatusCode.Should().Be(422);
        response.Json["title"]!.GetValue<string>().Should().Be(InvalidOperationProblemMapper.Title);
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task TryHandleAsync_Should_UseFirstMatchingCustomMapper_When_SeveralMappersAreRegistered(HostKind kind)
    {
        ProblemResponse response = await ThrowAsync(
            kind,
            "timeout",
            services => services.AddEnhancedProblemDetails()
                .AddExceptionProblemMapper<InvalidOperationProblemMapper>()
                .AddExceptionProblemMapper<TimeoutProblemMapper>());

        response.StatusCode.Should().Be(504);
        response.Body.Should().NotContainEquivalentOf("secret");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task TryHandleAsync_Should_FallBackToDefaultMapping_When_CustomMappersDecline(HostKind kind)
    {
        ProblemResponse response = await ThrowAsync(
            kind, "notsupported", services => services.AddEnhancedProblemDetails().AddExceptionProblemMapper<TimeoutProblemMapper>());

        response.StatusCode.Should().Be(500);
        response.Body.Should().NotContainEquivalentOf("secret");
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task TryHandleAsync_Should_UseScopedExceptionMapper_When_ScopeValidationIsEnabled(HostKind kind)
    {
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(
            kind,
            services =>
            {
                services.AddEnhancedProblemDetails().AddExceptionProblemMapper<ScopedInstanceMapper>(ServiceLifetime.Scoped);
                services.AddExceptionHandler<GlobalExceptionHandler>();
            },
            app => app.UseEnhancedProblemDetails());

        ProblemResponse first = await host.SendAsync("/throw?e=invalid");
        ProblemResponse second = await host.SendAsync("/throw?e=invalid");

        first.StatusCode.Should().Be(422);
        second.StatusCode.Should().Be(422);
        first.Json["title"]!.GetValue<string>().Should().StartWith(ScopedInstanceMapper.TitlePrefix);
        first.Json["title"]!.GetValue<string>().Should().NotBe(second.Json["title"]!.GetValue<string>());
    }

    [Theory]
    [MemberData(nameof(Hosts))]
    public async Task TryHandleAsync_Should_UseScopedErrorStatusCodeMapper_When_ScopeValidationIsEnabled(HostKind kind)
    {
        ScopedStatusCodeMapper.Created.Clear();
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(
            kind,
            services =>
            {
                services.AddEnhancedProblemDetails().AddErrorStatusCodeMapper<ScopedStatusCodeMapper>(ServiceLifetime.Scoped);
                services.AddExceptionHandler<GlobalExceptionHandler>();
            },
            app => app.UseEnhancedProblemDetails());

        ProblemResponse first = await host.SendAsync("/throw?e=domain");
        ProblemResponse second = await host.SendAsync("/throw?e=domain");

        first.StatusCode.Should().Be(ScopedStatusCodeMapper.Status);
        second.StatusCode.Should().Be(ScopedStatusCodeMapper.Status);
        ScopedStatusCodeMapper.Created.Distinct().Should().HaveCountGreaterThanOrEqualTo(2);
    }

    private static async Task<ProblemResponse> ThrowAsync(
        HostKind kind,
        string exceptionKind,
        Action<IServiceCollection>? configureServices = null)
    {
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(
            kind,
            services =>
            {
                (configureServices ?? (s => s.AddEnhancedProblemDetails()))(services);
                services.AddExceptionHandler<GlobalExceptionHandler>();
            },
            app => app.UseEnhancedProblemDetails());
        return await host.SendAsync($"/throw?e={exceptionKind}");
    }
}
