using System.Text.Json.Serialization;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

public class ProblemDetailsSourceGenTests
{
    [Theory]
    [InlineData(Registration.None)]
    [InlineData(Registration.ProblemDetails)]
    [InlineData(Registration.EnhancedProblemDetails)]
    public async Task ProblemResult_Should_WriteProblemJson_When_JsonOptionsOnlyHaveAnAppSourceGeneratedContext(Registration registration)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.Configure<HttpJsonOptions>(o =>
        {
            o.SerializerOptions.TypeInfoResolverChain.Clear();
            o.SerializerOptions.TypeInfoResolverChain.Add(AppJsonContext.Default);
        });
        if (registration == Registration.ProblemDetails)
            services.AddProblemDetails();
        else if (registration == Registration.EnhancedProblemDetails)
            services.AddEnhancedProblemDetails();
        await using ServiceProvider provider = services.BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = provider };
        using var body = new MemoryStream();
        context.Response.Body = body;

        await Error.Conflict("order.conflict", "Order exists").ToProblemResult().ExecuteAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        body.Position = 0;
        using var reader = new StreamReader(body);
        string json = await reader.ReadToEndAsync();
        json.Should().Contain("\"title\"").And.Contain("order.conflict");
    }

    public enum Registration
    {
        None,
        ProblemDetails,
        EnhancedProblemDetails,
    }
}

[JsonSerializable(typeof(int))]
internal sealed partial class AppJsonContext : JsonSerializerContext;
