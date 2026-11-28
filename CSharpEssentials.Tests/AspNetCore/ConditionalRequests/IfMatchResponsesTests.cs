using System.Reflection;
using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.ConditionalRequests;

public sealed class IfMatchResponsesTests
{
    [Fact]
    public void OnProvidersExecuted_Should_AddProblemStatuses_When_TheActionHasTheAttributeAndMvcMetadataIsNotRegistered()
    {
        ApiDescription description = Describe(nameof(Actions.Attributed), "PUT");

        Run(description);

        Statuses(description).Should().Equal(400, 412);
    }

    [Fact]
    public void OnProvidersExecuted_Should_AddPreconditionRequired_When_TheAttributeRequiresTheHeader()
    {
        ApiDescription description = Describe(nameof(Actions.Required), "PATCH");

        Run(description);

        Statuses(description).Should().Equal(400, 412, 428);
    }

    [Fact]
    public void OnProvidersExecuted_Should_AddProblemStatuses_When_TheDescriptionHasNoHttpMethod()
    {
        ApiDescription description = Describe(nameof(Actions.Attributed), httpMethod: null);

        Run(description);

        Statuses(description).Should().Equal(400, 412);
    }

    [Fact]
    public void OnProvidersExecuted_Should_NotAddStatuses_When_TheMethodIsSafe()
    {
        ApiDescription description = Describe(nameof(Actions.Attributed), "GET");

        Run(description);

        Statuses(description).Should().BeEmpty();
    }

    [Fact]
    public void OnProvidersExecuted_Should_NotAddStatuses_When_NeitherTheActionNorAnEndpointSetsIfMatch()
    {
        ApiDescription description = Describe(nameof(Actions.Plain), "PUT");

        Run(description);

        Statuses(description).Should().BeEmpty();
    }

    [Fact]
    public void OnProvidersExecuted_Should_KeepTheDeclaredResponse_When_TheActionAlreadyDeclaresTheStatus()
    {
        ApiDescription description = Describe(nameof(Actions.Attributed), "PUT");
        description.SupportedResponseTypes.Add(new ApiResponseType { StatusCode = 412 });

        Run(description);

        Statuses(description).Should().Equal(412, 400);
    }

    [Fact]
    public void OnProvidersExecuted_Should_Ignore_A_Description_Of_A_Minimal_Endpoint()
    {
        ApiDescription description = new() { HttpMethod = "PUT" };

        Run(description);

        Statuses(description).Should().BeEmpty();
    }

    [Fact]
    public async Task WithIfMatch_Should_DeclareProblemResponses_When_TheEndpointHasNoHttpMethod()
    {
        await using WebApplication app = WebApplication.CreateBuilder().Build();
        app.Map("/any", () => "ok").WithIfMatch();

        Endpoint endpoint = ((IEndpointRouteBuilder)app).DataSources.SelectMany(static source => source.Endpoints).Single();

        endpoint.Metadata.OfType<IProducesResponseTypeMetadata>().Select(static produces => produces.StatusCode).Should().Contain([400, 412]);
    }

    private static void Run(ApiDescription description)
    {
        using ServiceProvider provider = new ServiceCollection().AddConditionalRequests().BuildServiceProvider();
        IApiDescriptionProvider responses = provider.GetServices<IApiDescriptionProvider>().Single(static candidate => candidate.GetType().Name == "IfMatchResponses");
        ApiDescriptionProviderContext context = new([]);
        context.Results.Add(description);

        responses.OnProvidersExecuted(context);
    }

    private static ApiDescription Describe(string method, string? httpMethod) =>
        new()
        {
            HttpMethod = httpMethod,
            ActionDescriptor = new ControllerActionDescriptor
            {
                MethodInfo = typeof(Actions).GetMethod(method)!,
                ControllerTypeInfo = typeof(Actions).GetTypeInfo(),
            },
        };

    private static IEnumerable<int> Statuses(ApiDescription description) =>
        description.SupportedResponseTypes.Select(static response => response.StatusCode);

    private sealed class Actions
    {
        [IfMatch]
        public int Attributed() => 0;

        [IfMatch(Required = true)]
        public int Required() => 0;

        public int Plain() => 0;
    }
}
