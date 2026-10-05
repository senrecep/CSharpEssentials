using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

/// <summary>
/// The writer decides from endpoint metadata and <see cref="ApiBehaviorOptions"/> whether MVC's problem writer applies,
/// not from <c>Response.HasStarted</c>.
/// </summary>
public class EnhancedProblemDetailsWriterTests
{
    private static readonly HostKind[] AllKinds = [HostKind.Minimal, HostKind.ApiController, HostKind.PlainController];

    [Theory]
    [InlineData("/problem?s=mixed&ext=true")]
    [InlineData("/problem?s=validation")]
    public async Task ApiController_Should_WriteSameProblemAsMinimal_When_SuppressMapClientErrorsIsTrue(string path)
    {
        ProblemResponse[] responses = await ProblemDetailsParityTests.SendAsync(
            services =>
            {
                services.AddEnhancedProblemDetails();
                services.Configure<ApiBehaviorOptions>(o => o.SuppressMapClientErrors = true);
            },
            path, "application/json", AllKinds);

        ProblemDetailsParityTests.AssertIdentical(responses[0], responses[1]);
        ProblemDetailsParityTests.AssertIdentical(responses[0], responses[2]);
        responses[1].Json.ContainsKey("errorCodes").Should().BeTrue(responses[1].Body);
    }

    [Theory]
    [InlineData(HostKind.Minimal)]
    [InlineData(HostKind.ApiController)]
    [InlineData(HostKind.PlainController)]
    public async Task Problem_Should_BeSingleJsonDocument_When_ResponseBodyIsBuffered(HostKind kind)
    {
        ProblemResponse response = await SendBufferedAsync(kind, "/problem?s=mixed&ext=true");

        response.StatusCode.Should().Be(409);
        response.MediaType.Should().Be("application/problem+json");
        response.Json["errorCodes"]!.AsArray().Should().HaveCount(4);
    }

    [Fact]
    public async Task Problem_Should_BeIdenticalAcrossEndpointStyles_When_ResponseBodyIsBuffered()
    {
        var responses = new List<ProblemResponse>();
        foreach (HostKind kind in AllKinds)
            responses.Add(await SendBufferedAsync(kind, "/problem?s=mixed&ext=true"));

        ProblemDetailsParityTests.AssertIdentical(responses[0], responses[1]);
        ProblemDetailsParityTests.AssertIdentical(responses[0], responses[2]);
    }

    [Theory]
    [InlineData(HostKind.Minimal)]
    [InlineData(HostKind.ApiController)]
    [InlineData(HostKind.PlainController)]
    public async Task GlobalExceptionHandler_Should_WriteSingleJsonDocument_When_ResponseBodyIsBuffered(HostKind kind)
    {
        ProblemResponse response = await SendBufferedAsync(kind, "/throw?e=domain", addExceptionHandler: true);

        response.StatusCode.Should().Be(409);
        response.Json["errorCodes"]!.AsArray().Should().HaveCount(1);
    }

    private static async Task<ProblemResponse> SendBufferedAsync(HostKind kind, string path, bool addExceptionHandler = false)
    {
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(
            kind,
            services =>
            {
                services.AddEnhancedProblemDetails();
                if (addExceptionHandler)
                    services.AddExceptionHandler<GlobalExceptionHandler>();
            },
            app =>
            {
                app.Use(BufferResponseBody);
                app.UseEnhancedProblemDetails();
            });
        return await host.SendAsync(path);
    }

    /// <summary>
    /// Replaces the body feature so nothing reaches the client until the request ends: <c>Response.HasStarted</c>
    /// stays false while the problem is written.
    /// </summary>
    private static async Task BufferResponseBody(HttpContext context, RequestDelegate next)
    {
        IHttpResponseBodyFeature original = context.Features.GetRequiredFeature<IHttpResponseBodyFeature>();
        await using var buffer = new MemoryStream();
        context.Features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(buffer));
        try
        {
            await next(context);
        }
        finally
        {
            context.Features.Set(original);
        }

        buffer.Position = 0;
        await buffer.CopyToAsync(original.Stream, context.RequestAborted);
    }
}
