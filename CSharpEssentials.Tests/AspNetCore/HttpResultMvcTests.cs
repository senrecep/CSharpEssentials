using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.AspNetCore;

public sealed record MvcOrderDto(int Id);

public sealed record MvcOrderLocked(int Id) : ConflictError("Order.Locked", $"Order {Id} is locked.");

[ApiController]
[Route("orders")]
internal sealed class TypedResultOrdersController : ControllerBase
{
    private static readonly NotFoundError OrderNotFound = new("Order.NotFound", "The order was not found.");

    public static Result<MvcOrderDto, NotFoundError, MvcOrderLocked> GetOrder(int id) => id switch
    {
        0 => OrderNotFound,
        7 => new MvcOrderLocked(id),
        _ => new MvcOrderDto(id),
    };

    [HttpGet("sync/{id:int}")]
    public HttpResult<Ok<MvcOrderDto>, NotFoundError, MvcOrderLocked> Sync(int id) => GetOrder(id).ToHttpResult();

    [HttpGet("task/{id:int}")]
    public Task<HttpResult<Ok<MvcOrderDto>, NotFoundError, MvcOrderLocked>> TaskOf(int id) => Task.FromResult(GetOrder(id)).ToHttpResult();

    [HttpGet("valuetask/{id:int}")]
    public ValueTask<HttpResult<Ok<MvcOrderDto>, NotFoundError, MvcOrderLocked>> ValueTaskOf(int id) =>
        new ValueTask<Result<MvcOrderDto, NotFoundError, MvcOrderLocked>>(GetOrder(id)).ToHttpResult();

    [HttpPost("created/{id:int}")]
    public HttpResult<Created<MvcOrderDto>, NotFoundError, MvcOrderLocked> Create(int id) => GetOrder(id).ToCreated(o => $"/orders/{o.Id}");

    [HttpGet("raw/{id:int}")]
    public Result<MvcOrderDto, NotFoundError, MvcOrderLocked> Raw(int id) => GetOrder(id);

    [HttpGet("ok/{id:int}")]
    public IActionResult OkOf(int id) => Ok(GetOrder(id));

    [HttpGet("actionresult/{id:int}")]
    public ActionResult<Result<MvcOrderDto, NotFoundError, MvcOrderLocked>> ActionResultOf(int id) => GetOrder(id);

    [HttpGet("nested/{id:int}")]
    public List<Result<MvcOrderDto, NotFoundError, MvcOrderLocked>> Nested(int id) => [GetOrder(id)];
}

public class HttpResultMvcTests
{
    private sealed class GuardFailures
    {
        public Exception? Exception { get; set; }

        public bool HasStarted { get; set; }
    }

    private sealed record Response(int Status, string Body, string? MediaType);

    [Theory]
    [InlineData("sync")]
    [InlineData("task")]
    [InlineData("valuetask")]
    public async Task ToHttpResult_Should_SendTheValue_When_TheActionSucceeds(string action)
    {
        await using WebApplication app = await StartAsync();

        Response response = await GetAsync(app, $"/orders/{action}/3");

        response.Status.Should().Be(StatusCodes.Status200OK);
        response.MediaType.Should().Be("application/json");
        response.Body.Should().Be("{\"id\":3}");
    }

    [Theory]
    [InlineData("sync", 0, StatusCodes.Status404NotFound, "Order.NotFound")]
    [InlineData("sync", 7, StatusCodes.Status409Conflict, "Order.Locked")]
    [InlineData("task", 0, StatusCodes.Status404NotFound, "Order.NotFound")]
    [InlineData("valuetask", 7, StatusCodes.Status409Conflict, "Order.Locked")]
    public async Task ToHttpResult_Should_SendTheProblem_When_TheActionFails(string action, int id, int status, string code)
    {
        await using WebApplication app = await StartAsync();

        Response response = await GetAsync(app, $"/orders/{action}/{id}");

        response.Status.Should().Be(status);
        response.MediaType.Should().Be("application/problem+json");
        response.Body.Should().Contain($"\"status\":{status}").And.Contain(code);
    }

    [Fact]
    public async Task ToHttpResult_Should_SendTheSameProblemAsMinimalApi()
    {
        await using WebApplication app = await StartAsync(map: app => app.MapGet("/minimal/{id:int}", (int id) => TypedResultOrdersController.GetOrder(id).ToHttpResult()));

        Response mvc = await GetAsync(app, "/orders/sync/7");
        Response minimal = await GetAsync(app, "/minimal/7");

        PerRequestFieldsRemoved(mvc).Should().Be(PerRequestFieldsRemoved(minimal));
    }

    private static string PerRequestFieldsRemoved(Response response)
    {
        JsonObject json = new ProblemResponse(response.Status, response.MediaType, response.MediaType, response.Body).Json;
        json.Remove("instance");
        json.Remove("traceId");
        return ProblemJson.Canonical(json);
    }

    [Fact]
    public async Task ToCreated_Should_SendCreatedWithTheLocation_When_TheActionSucceeds()
    {
        await using WebApplication app = await StartAsync();
        HttpClient client = app.GetTestClient();

        using HttpResponseMessage response = await client.PostAsync(new Uri("/orders/created/5", UriKind.Relative), null);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        response.Headers.Location.Should().Be(new Uri("/orders/5", UriKind.Relative));
        (await response.Content.ReadAsStringAsync()).Should().Be("{\"id\":5}");
    }

    [Theory]
    [InlineData("raw")]
    [InlineData("ok")]
    [InlineData("actionresult")]
    public async Task RawReturn_Should_ThrowBeforeWriting(string action)
    {
        var failures = new GuardFailures();
        await using WebApplication app = await StartAsync(failures);

        foreach (int id in (int[])[3, 7])
        {
            failures.Exception = null;

            Response response = await GetAsync(app, $"/orders/{action}/{id}");

            failures.Exception.Should().BeOfType<InvalidOperationException>().Which.Message.Should().Contain("ToHttpResult()");
            failures.HasStarted.Should().BeFalse();
            response.Status.Should().Be(StatusCodes.Status500InternalServerError);
            response.Body.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task Guard_Should_LetAnActionReturnTypedResultsNestedInTheBody()
    {
        var failures = new GuardFailures();
        await using WebApplication app = await StartAsync(failures);

        Response response = await GetAsync(app, "/orders/nested/7");

        failures.Exception.Should().BeNull();
        response.Status.Should().Be(StatusCodes.Status200OK);
        response.Body.Should().Contain("\"case\":\"MvcOrderLocked\"");
    }

    [Theory]
    [InlineData("orders/sync/{id:int}", new[] { 200, 404, 409 })]
    [InlineData("orders/task/{id:int}", new[] { 200, 404, 409 })]
    [InlineData("orders/valuetask/{id:int}", new[] { 200, 404, 409 })]
    [InlineData("orders/created/{id:int}", new[] { 201, 404, 409 })]
    public async Task Metadata_Should_ListTheSuccessAndOneProblemPerErrorSlot(string route, int[] statuses)
    {
        await using WebApplication app = await StartAsync();

        IProducesResponseTypeMetadata[] produces =
        [
            .. ((IEndpointRouteBuilder)app).DataSources.SelectMany(static source => source.Endpoints).OfType<RouteEndpoint>()
                .Single(endpoint => endpoint.RoutePattern.RawText == route)
                .Metadata.OfType<IProducesResponseTypeMetadata>(),
        ];

        produces.Select(static metadata => metadata.StatusCode).Distinct().Should().Equal(statuses);
        produces.Single(static metadata => metadata.StatusCode == 404).Type.Should().Be<EnhancedProblemDetails>();
    }

    private static async Task<WebApplication> StartAsync(GuardFailures? failures = null, Action<WebApplication>? map = null)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddEnhancedProblemDetails();
        builder.Services.AddControllers().ConfigureApplicationPartManager(manager =>
        {
            foreach (IApplicationFeatureProvider<ControllerFeature> provider in
                     manager.FeatureProviders.OfType<IApplicationFeatureProvider<ControllerFeature>>().ToList())
                manager.FeatureProviders.Remove(provider);
            manager.FeatureProviders.Add(new OnlyControllersFeatureProvider(typeof(TypedResultOrdersController)));
        });
        WebApplication app = builder.Build();
        app.Use(async (context, next) =>
        {
            try
            {
                await next(context);
            }
            catch (InvalidOperationException exception) when (failures is not null)
            {
                failures.Exception = exception;
                failures.HasStarted = context.Response.HasStarted;
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            }
        });
        app.MapControllers();
        map?.Invoke(app);
        await app.StartAsync();
        return app;
    }

    private static async Task<Response> GetAsync(WebApplication app, string path)
    {
        HttpClient client = app.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        using HttpResponseMessage response = await client.SendAsync(request);
        return new Response((int)response.StatusCode, await response.Content.ReadAsStringAsync(), response.Content.Headers.ContentType?.MediaType);
    }
}
