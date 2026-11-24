using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Tests.Mediator;
using FluentAssertions;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

/// <summary>
/// A 5xx problem built from errors must not carry exception messages, stack traces or exception metadata unless
/// <see cref="EnhancedProblemDetailsOptions.ExposeExceptionDetails"/> is on.
/// </summary>
public class ServerErrorRedactionTests
{
    private const string GenericDetail = "An unexpected error occurred.";

    public static TheoryData<HostKind, bool> HostsAndFields() => new()
    {
        { HostKind.Minimal, false },
        { HostKind.Minimal, true },
        { HostKind.ApiController, false },
        { HostKind.ApiController, true },
        { HostKind.PlainController, false },
        { HostKind.PlainController, true },
    };

    public static TheoryData<bool> AllFields() => new() { false, true };

    [Fact]
    public void ToProblemDetails_Should_RedactDetailErrorsAndExtensions_When_StatusIs5xx()
    {
        Error error = Error.Exception(new InvalidOperationException(ProblemScenarios.SecretMessage),
            metadata: new ErrorMetadata("orderId", 7));
        var extensions = new ErrorMetadata("tenant", "t1").AddExceptionDetailed(new InvalidOperationException(ProblemScenarios.SecretMessage));

        EnhancedProblemDetails problem = error.ToProblemDetails(extensions);

        problem.Status.Should().Be(500);
        problem.Detail.Should().Be(GenericDetail);
        problem.Extensions.Should().ContainKey("tenant").And.NotContainKeys("exceptionMessage", "exceptionType", "exceptionStackTrace");
        Error redacted = problem.Errors.Should().ContainSingle().Subject;
        redacted.Code.Should().Be(error.Code);
        redacted.Description.Should().Be(GenericDetail);
        redacted.Metadata.Should().ContainKey("orderId").And.HaveCount(1);
        error.Metadata.Should().ContainKey("exceptionMessage", "the caller's metadata must not be changed");
        JsonSerializer.Serialize(problem).Should().NotContain(ProblemScenarios.Secret);
    }

    [Fact]
    public void ToProblemDetails_Should_RedactDetail_When_StatusIsOverriddenTo5xx()
    {
        EnhancedProblemDetails problem = Error.Validation("name.invalid", ProblemScenarios.SecretMessage).ToProblemDetails(statusCode: 503);

        problem.Detail.Should().Be(GenericDetail);
        problem.Errors.Should().ContainSingle().Which.Description.Should().Be(ProblemScenarios.SecretMessage,
            "only errors that map to 5xx themselves get the fixed description");
    }

    [Fact]
    public void ToProblemDetails_Should_StripExceptionMetadataOnly_When_ClientErrorIsPartOf5xxProblem()
    {
        Error[] errors =
        [
            Error.Exception(new InvalidOperationException(ProblemScenarios.SecretMessage)),
            Error.Exception(new FormatException(ProblemScenarios.SecretMessage), ErrorType.Validation),
        ];

        EnhancedProblemDetails problem = errors.ToProblemDetails();

        problem.Status.Should().Be(500);
        problem.Errors[1].Description.Should().Be(ProblemScenarios.SecretMessage);
        problem.Errors[1].Metadata.Should().BeNull();
        problem.Errors[0].Metadata.Should().BeNull();
    }

    [Fact]
    public void ToProblemDetails_Should_KeepDescription_When_StatusIs4xx()
    {
        Error error = Error.Exception(new FormatException("Email format is incorrect."), ErrorType.Validation);

        EnhancedProblemDetails problem = error.ToProblemDetails();

        problem.Status.Should().Be(400);
        problem.Detail.Should().Be("Email format is incorrect.");
        problem.Errors.Should().ContainSingle().Which.Should().Be(error);
    }

    [Theory]
    [MemberData(nameof(HostsAndFields))]
    public async Task ProblemResult_Should_NotLeakExceptionMessage_When_StatusIs5xx(HostKind kind, bool allFields)
    {
        ProblemResponse response = await SendAsync(kind, "/problem?s=secret", allFields);

        AssertRedacted(response);
    }

    [Theory]
    [MemberData(nameof(AllFields))]
    public async Task ResultEndpointFilter_Should_NotLeakExceptionMessage_When_StatusIs5xx(bool allFields)
    {
        ProblemResponse result = await SendAsync(HostKind.Minimal, "/result?s=secret", allFields);
        ProblemResponse resultOfT = await SendAsync(HostKind.Minimal, "/result-of-t?s=secret", allFields);

        AssertRedacted(result);
        AssertRedacted(resultOfT);
    }

    [Theory]
    [MemberData(nameof(AllFields))]
    public async Task MediatorExceptionHandlingBehavior_Should_NotLeakExceptionMessage_When_ResultIsWrittenAsProblem(bool allFields)
    {
        ProblemResponse response = await SendAsync(HostKind.Minimal, "/mediator", allFields, MapMediatorEndpoint);

        AssertRedacted(response);
    }

    [Theory]
    [MemberData(nameof(HostsAndFields))]
    public async Task GlobalExceptionHandler_Should_NotLeakExceptionMessage_When_DefaultMapperHandlesException(HostKind kind, bool allFields)
    {
        ProblemResponse response = await SendAsync(kind, "/throw?e=secret", allFields);

        response.StatusCode.Should().Be(500);
        response.Body.Should().NotContain(ProblemScenarios.Secret);
    }

    [Theory]
    [MemberData(nameof(HostsAndFields))]
    public async Task GlobalExceptionHandler_Should_NotLeakExceptionMessage_When_MapperReturnsExceptionErrors(HostKind kind, bool allFields)
    {
        ProblemResponse response = await SendAsync(kind, "/throw?e=secret", allFields,
            configureServices: services => services.AddExceptionProblemMapper<ExceptionErrorMapper>());

        AssertRedacted(response);
    }

    [Theory]
    [MemberData(nameof(AllFields))]
    public async Task ProblemResult_Should_KeepExceptionDetails_When_ExposeExceptionDetailsIsOn(bool allFields)
    {
        ProblemResponse response = await SendAsync(HostKind.Minimal, "/problem?s=secret", allFields, expose: true);

        response.StatusCode.Should().Be(500);
        response.Json["detail"]!.GetValue<string>().Should().Be(ProblemScenarios.SecretMessage);
        if (allFields)
        {
            JsonNode error = response.Json["errors"]!.AsArray().Single()!;
            error["description"]!.GetValue<string>().Should().Be(ProblemScenarios.SecretMessage);
            error["metadata"]!["exceptionMessage"]!.GetValue<string>().Should().Be(ProblemScenarios.SecretMessage);
        }
    }

    [Fact]
    public async Task GlobalExceptionHandler_Should_KeepExceptionDetails_When_ExposeExceptionDetailsIsOn()
    {
        ProblemResponse response = await SendAsync(HostKind.Minimal, "/throw?e=secret", allFields: true, expose: true,
            configureServices: services => services.AddExceptionProblemMapper<ExceptionErrorMapper>());

        response.StatusCode.Should().Be(500);
        response.Json["detail"]!.GetValue<string>().Should().Be(ProblemScenarios.SecretMessage);
        response.Json["exception"]!["message"]!.GetValue<string>().Should().Be(ProblemScenarios.SecretMessage);
    }

    private static void AssertRedacted(ProblemResponse response)
    {
        response.StatusCode.Should().Be(500);
        response.Body.Should().NotContain(ProblemScenarios.Secret).And.NotContain("exceptionStackTrace");
        response.Json["detail"]!.GetValue<string>().Should().Be(GenericDetail);
        response.Json["errorCodes"]!.AsArray().Select(n => n!.GetValue<string>()).Should().Equal(nameof(InvalidOperationException));
    }

    private static void MapMediatorEndpoint(WebApplication app) =>
        app.MapGet("/mediator", async () =>
            await new ExceptionHandlingBehavior<TestExceptionCommand, Result>().Handle(
                new TestExceptionCommand("redaction"),
                static (_, _) => throw new InvalidOperationException(ProblemScenarios.SecretMessage),
                CancellationToken.None))
            .AddEndpointFilter<ResultEndpointFilter>();

    private static async Task<ProblemResponse> SendAsync(
        HostKind kind,
        string path,
        bool allFields,
        Action<WebApplication>? mapExtra = null,
        bool expose = false,
        Action<IServiceCollection>? configureServices = null)
    {
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(
            kind,
            services =>
            {
                services.AddEnhancedProblemDetails(o =>
                {
                    o.ExposeExceptionDetails = expose;
                    if (allFields)
                        o.ErrorFields = ProblemErrorFields.All;
                });
                services.AddExceptionHandler<GlobalExceptionHandler>();
                configureServices?.Invoke(services);
            },
            app => app.UseEnhancedProblemDetails(),
            mapExtra);
        return await host.SendAsync(path);
    }

    private sealed class ExceptionErrorMapper : IExceptionProblemMapper
    {
        public bool TryMap(HttpContext httpContext, Exception exception, [NotNullWhen(true)] out ExceptionProblem? problem)
        {
            problem = new ExceptionProblem(Errors: [Error.Exception(exception)]);
            return true;
        }
    }
}
