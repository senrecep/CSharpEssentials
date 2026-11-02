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
        context.Response.Body = new MemoryStream();

        await Error.Conflict("order.conflict", "Order exists").ToProblemResult().ExecuteAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        context.Response.Body.Position = 0;
        string body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().Contain("\"title\"").And.Contain("order.conflict");
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
