using System.Text;
using System.Text.Json;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

namespace CSharpEssentials.Tests.AspNetCore;

public class HttpResultTests
{
    public sealed record OrderDto(int Id);

    public sealed record OrderLocked(int Id) : ConflictError("Order.Locked", $"Order {Id} is locked.");

    public sealed record CreateOrder(int Quantity);

    private static readonly NotFoundError OrderNotFound = new("Order.NotFound", "The order was not found.");

    private static Result<OrderDto, NotFoundError, OrderLocked> GetOrder(int id) => id switch
    {
        0 => OrderNotFound,
        7 => new OrderLocked(id),
        _ => new OrderDto(id),
    };

    [Theory]
    [InlineData(3, StatusCodes.Status200OK, "{\"id\":3}")]
    [InlineData(0, StatusCodes.Status404NotFound, "Order.NotFound")]
    [InlineData(7, StatusCodes.Status409Conflict, "Order.Locked")]
    public async Task ToHttpResult_Should_SendTheValueOrTheProblem(int id, int status, string bodyPart)
    {
        await using WebApplication app = Build(map: app => app.MapGet("/orders/{id:int}", (int id) => GetOrder(id).ToHttpResult()));

        Response response = await InvokeAsync(app, "/orders/{id:int}", id);

        response.Exception.Should().BeNull();
        response.Status.Should().Be(status);
        response.Body.Should().Contain(bodyPart);
        if (status != StatusCodes.Status200OK)
            response.ContentType.Should().StartWith("application/problem+json");
    }

    [Fact]
    public void ToHttpResult_Should_BuildTheSameProblemAsTheClassicResult()
    {
        HttpResult<Ok<OrderDto>, NotFoundError, OrderLocked> typed = GetOrder(7).ToHttpResult();

        var problem = (EnhancedProblemHttpResult)typed.Result;
        var classic = (EnhancedProblemHttpResult)Result<OrderDto>.Failure(new OrderLocked(7).ToError()).ToProblemResult();
        problem.ProblemDetails.Should().BeEquivalentTo(classic.ProblemDetails);
    }

    [Fact]
    public async Task ToHttpResult_Should_Throw_When_TheResultIsDefault()
    {
        Action sync = () => default(Result<OrderDto, NotFoundError>).ToHttpResult();
        Func<Task> task = () => Task.FromResult(default(Result<OrderDto, NotFoundError, OrderLocked>)).ToHttpResult();
        Func<Task> valueTask = async () => await new ValueTask<Result<OrderDto, NotFoundError, OrderLocked, ValidationError>>(default(Result<OrderDto, NotFoundError, OrderLocked, ValidationError>)).ToOk();

        sync.Should().Throw<InvalidOperationException>();
        await task.Should().ThrowAsync<InvalidOperationException>();
        await valueTask.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Conversions_Should_SendTheirSuccessResponse()
    {
        Result<OrderDto, NotFoundError> found = new OrderDto(5);

        (await Task.FromResult(found).ToOk()).Result.Should().BeOfType<Ok<OrderDto>>();
        Created<OrderDto> created = (Created<OrderDto>)(await new ValueTask<Result<OrderDto, NotFoundError>>(found).ToCreated(o => $"/orders/{o.Id}")).Result;
        created.Location.Should().Be("/orders/5");
        created.Value.Should().Be(new OrderDto(5));
        found.ToNoContent().Result.Should().BeOfType<NoContent>();
        found.ToHttpResult(o => TypedResults.Accepted($"/orders/{o.Id}", o)).Result.Should().BeOfType<Accepted<OrderDto>>();
        GetOrder(0).ToCreated(o => $"/orders/{o.Id}").Result.Should().BeOfType<EnhancedProblemHttpResult>();
    }

    [Fact]
    public void Metadata_Should_ListTheSuccessAndOneProblemPerErrorSlot()
    {
        using WebApplication app = Build(map: app =>
        {
            app.MapGet("/sync/{id:int}", (int id) => GetOrder(id).ToHttpResult());
            app.MapGet("/task/{id:int}", (int id) => Task.FromResult(GetOrder(id)).ToHttpResult());
            app.MapGet("/valuetask/{id:int}", (int id) => new ValueTask<Result<OrderDto, NotFoundError, OrderLocked>>(GetOrder(id)).ToHttpResult());
            app.MapPost("/created", (CreateOrder order) => GetOrder(order.Quantity).ToCreated(o => $"/orders/{o.Id}"));
            app.MapGet("/untyped", () => ((Result<OrderDto, Error>)new OrderDto(1)).ToHttpResult());
        });

        foreach (string route in (string[])["/sync/{id:int}", "/task/{id:int}", "/valuetask/{id:int}"])
            Statuses(app, route).Should().Equal([200, 404, 409], route);
        Statuses(app, "/created").Should().Equal(201, 404, 409);
        Statuses(app, "/untyped").Should().Equal(200);

        IProducesResponseTypeMetadata notFound = Produces(app, "/sync/{id:int}").Single(m => m.StatusCode == 404);
        notFound.Type.Should().Be<EnhancedProblemDetails>();
        notFound.ContentTypes.Should().Equal("application/problem+json");
    }

    [Fact]
    public void Metadata_Should_UseTheDeclaredStatusesOfTheRegisteredMapper()
    {
        using WebApplication app = Build(
            services => services.AddErrorStatusCodeMapper<GoneMapper>(ServiceLifetime.Scoped),
            app => app.MapGet("/orders/{id:int}", (int id) => GetOrder(id).ToHttpResult()),
            validateScopes: true);

        Statuses(app, "/orders/{id:int}").Should().Equal(200, 404, 410, 409);
    }

    [Fact]
    public async Task Metadata_Should_FollowAMapperThatMapsTheKindElsewhere_When_ItKeepsTheDefaultDeclaration()
    {
        await using WebApplication app = Build(
            services => services.AddErrorStatusCodeMapper<NotFoundAsGoneMapper>(),
            app => app.MapGet("/orders/{id:int}", (int id) => GetOrder(id).ToHttpResult()));

        Statuses(app, "/orders/{id:int}").Should().Equal(200, 410, 409);
        (await InvokeAsync(app, "/orders/{id:int}", 0)).Status.Should().Be(StatusCodes.Status410Gone);
    }

    private static readonly Dictionary<string, int> CodeMappedStatuses = new(StringComparer.Ordinal)
    {
        [ConditionalRequestErrors.PreconditionFailedCode] = StatusCodes.Status412PreconditionFailed,
        [CSharpEssentials.Idempotency.IdempotencyErrorCodes.KeyReused] = StatusCodes.Status422UnprocessableEntity,
        [CSharpEssentials.Idempotency.IdempotencyErrorCodes.InFlight] = StatusCodes.Status409Conflict,
        [CSharpEssentials.Idempotency.IdempotencyErrorCodes.KeyMissing] = StatusCodes.Status400BadRequest,
        [CSharpEssentials.Idempotency.IdempotencyErrorCodes.KeyInvalid] = StatusCodes.Status400BadRequest,
        [CSharpEssentials.Idempotency.IdempotencyErrorCodes.ScopeRequired] = StatusCodes.Status400BadRequest,
    };

    [Fact]
    public void GetDeclaredStatusCodes_Should_LeaveTheCodeMappedStatusesUndeclared()
    {
        IEnumerable<int> declared = DefaultErrorStatusCodeMapper.Instance.GetDeclaredStatusCodes(typeof(ConflictError));

        declared.Should().Equal(StatusCodes.Status409Conflict);
        DefaultErrorStatusCodeMapper.Instance.GetStatusCode(new ConflictError(CSharpEssentials.Idempotency.IdempotencyErrorCodes.KeyReused, "Reused.").ToError())
            .Should().Be(StatusCodes.Status422UnprocessableEntity);
    }

    public static TheoryData<Type> ErrorKinds() =>
    [
        .. typeof(TypedError).Assembly.GetTypes().Where(static type => !type.IsAbstract && type.IsSubclassOf(typeof(TypedError))),
        typeof(OrderLocked),
    ];

    [Theory]
    [MemberData(nameof(ErrorKinds))]
    public void GetDeclaredStatusCodes_Should_DeclareEveryStatusTheMapperSends(Type kind)
    {
        foreach (IErrorStatusCodeMapper mapper in (IErrorStatusCodeMapper[])[DefaultErrorStatusCodeMapper.Instance, new GoneMapper(), new NotFoundAsGoneMapper()])
        {
            foreach (string code in (string[])["Order.NotFound", "Order.Gone", "Some.Code", .. CodeMappedStatuses.Keys])
            {
                IError error = kind == typeof(OrderLocked)
                    ? new OrderLocked(1)
                    : (IError)Activator.CreateInstance(kind, code, "Description.", null)!;
                Error untyped = Error.Create(error.Code, error.Description, error.Type, error.Metadata);
                int status = mapper.GetStatusCode(untyped);

                if (!mapper.GetDeclaredStatusCodes(kind).Contains(status))
                    CodeMappedStatuses.Should().Contain(error.Code, status, $"only the default code-mapped statuses may go undeclared: {mapper.GetType().Name} sends {status} for {kind.Name} '{error.Code}'");
            }
        }
    }

    [Fact]
    public void GetDeclaredStatusCodes_Should_DeclareNothing_When_TheTypeHasNoKind()
    {
        DefaultErrorStatusCodeMapper.Instance.GetDeclaredStatusCodes(typeof(Error)).Should().BeEmpty();
        ((IErrorStatusCodeMapper)new NotFoundAsGoneMapper()).GetDeclaredStatusCodes(typeof(string)).Should().BeEmpty();
    }

    [Fact]
    public void WithValidation_Should_Declare400Once()
    {
        using WebApplication app = Build(map: app =>
        {
            app.MapPost("/plain", (CreateOrder order) => order.Quantity).WithValidation<CreateOrder>();
            app.MapPost("/typed", (CreateOrder order) => ((Result<OrderDto, ValidationError>)new OrderDto(order.Quantity)).ToHttpResult())
                .WithValidation<CreateOrder>();
        });

        Statuses(app, "/plain").Should().Equal(200, 400);
        Statuses(app, "/typed").Should().Equal(200, 400);
        Produces(app, "/plain").Single(m => m.StatusCode == 400).Type.Should().Be<EnhancedProblemDetails>();
    }

    [Theory]
    [InlineData("/raw/{id:int}")]
    [InlineData("/ok/{id:int}")]
    public async Task RawReturn_Should_ThrowBeforeWriting(string route)
    {
        await using WebApplication app = Build(map: app =>
        {
            app.MapGet("/raw/{id:int}", (int id) => GetOrder(id));
            app.MapGet("/ok/{id:int}", (int id) => TypedResults.Ok(GetOrder(id)));
        });

        foreach (int id in (int[])[3, 7])
        {
            Response response = await InvokeAsync(app, route, id);

            response.Exception.Should().BeOfType<InvalidOperationException>().Which.Message.Should().Contain("ToHttpResult()");
            response.HasStarted.Should().BeFalse();
            response.Body.Should().BeEmpty();
        }
    }

    [Fact]
    public void Guard_Should_ApplyToMvcAndLeaveOtherSerializerOptionsAlone()
    {
        using WebApplication app = Build();
        Result<OrderDto, NotFoundError, OrderLocked> locked = GetOrder(7);
        JsonSerializerOptions http = app.Services.GetRequiredService<IOptions<HttpJsonOptions>>().Value.SerializerOptions;
        JsonSerializerOptions mvc = app.Services.GetRequiredService<IOptions<MvcJsonOptions>>().Value.JsonSerializerOptions;

        FluentActions.Invoking(() => JsonSerializer.Serialize(locked, http)).Should().Throw<InvalidOperationException>();
        FluentActions.Invoking(() => JsonSerializer.Serialize(locked, mvc)).Should().Throw<InvalidOperationException>();
        string json = JsonSerializer.Serialize(locked, JsonSerializerOptions.Web);
        json.Should().Contain("\"case\":\"OrderLocked\"");
        JsonSerializer.Deserialize<Result<OrderDto, NotFoundError, OrderLocked>>(json, http).Should().Be(locked);
    }

    public sealed record OrderBatch(Result<OrderDto, NotFoundError, OrderLocked> First);

    [Fact]
    public void Guard_Should_WriteANestedTypedResultAsTheEnvelope()
    {
        using WebApplication app = Build();
        JsonSerializerOptions http = app.Services.GetRequiredService<IOptions<HttpJsonOptions>>().Value.SerializerOptions;

        string list = JsonSerializer.Serialize(new List<Result<OrderDto, NotFoundError, OrderLocked>> { GetOrder(7), GetOrder(3) }, http);
        string record = JsonSerializer.Serialize(new OrderBatch(GetOrder(0)), http);

        list.Should().Be(JsonSerializer.Serialize(new List<Result<OrderDto, NotFoundError, OrderLocked>> { GetOrder(7), GetOrder(3) }, JsonSerializerOptions.Web));
        list.Should().Contain("\"case\":\"OrderLocked\"");
        JsonSerializer.Deserialize<OrderBatch>(record, http).Should().Be(new OrderBatch(GetOrder(0)));
    }

    [Fact]
    public async Task Guard_Should_LetAnEndpointReturnTypedResultsNestedInTheBody()
    {
        await using WebApplication app = Build(map: app =>
            app.MapGet("/orders/{id:int}", (int id) => new List<Result<OrderDto, NotFoundError, OrderLocked>> { GetOrder(id) }));

        Response response = await InvokeAsync(app, "/orders/{id:int}", 7);

        response.Exception.Should().BeNull();
        response.Status.Should().Be(StatusCodes.Status200OK);
        response.Body.Should().Contain("\"case\":\"OrderLocked\"");
    }

    [Fact]
    public void AddEnhancedProblemDetails_Should_RegisterTheGuardOnce()
    {
        using WebApplication app = Build(services => services.AddEnhancedProblemDetails());

        app.Services.GetRequiredService<IOptions<HttpJsonOptions>>().Value.SerializerOptions.Converters
            .Count(static converter => converter.GetType().Name == "TypedResultHttpJsonGuard").Should().Be(1);
    }

    [Theory]
    [InlineData(3, StatusCodes.Status200OK, "{\"id\":3}")]
    [InlineData(0, StatusCodes.Status404NotFound, "Order.NotFound")]
    [InlineData(7, StatusCodes.Status409Conflict, "Order.Locked")]
    [InlineData(-1, StatusCodes.Status500InternalServerError, "Result.Default")]
    public async Task ResultEndpointFilter_Should_ConvertTheTypedResult(int id, int status, string bodyPart)
    {
        await using WebApplication app = Build(map: app => app
            .MapGet("/orders/{id:int}", (int id) => id < 0 ? default : GetOrder(id))
            .AddEndpointFilter<ResultEndpointFilter>());

        Response response = await InvokeAsync(app, "/orders/{id:int}", id);

        response.Exception.Should().BeNull();
        response.Status.Should().Be(status);
        response.Body.Should().Contain(bodyPart);
    }

    private sealed class GoneMapper : DefaultErrorStatusCodeMapper
    {
        public override int GetStatusCode(Error error) =>
            error is { Type: ErrorType.NotFound, Code: "Order.Gone" } ? StatusCodes.Status410Gone : base.GetStatusCode(error);

        public override IEnumerable<int> GetDeclaredStatusCodes(Type errorType) =>
            typeof(NotFoundError).IsAssignableFrom(errorType)
                ? [.. base.GetDeclaredStatusCodes(errorType), StatusCodes.Status410Gone]
                : base.GetDeclaredStatusCodes(errorType);
    }

    private sealed class NotFoundAsGoneMapper : IErrorStatusCodeMapper
    {
        public int GetStatusCode(Error error) =>
            error.Type == ErrorType.NotFound ? StatusCodes.Status410Gone : DefaultErrorStatusCodeMapper.Instance.GetStatusCode(error);

        public Error SelectPrimaryError(IReadOnlyList<Error> errors) => errors[0];

        public string? GetTitle(Error primaryError, int statusCode) => null;
    }

    private sealed record Response(int Status, string Body, string? ContentType, bool HasStarted, Exception? Exception);

    private static WebApplication Build(Action<IServiceCollection>? services = null, Action<WebApplication>? map = null, bool validateScopes = false)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.WebHost.UseDefaultServiceProvider(options => options.ValidateScopes = validateScopes);
        builder.Logging.ClearProviders();
        builder.Services.AddEnhancedProblemDetails();
        services?.Invoke(builder.Services);
        WebApplication app = builder.Build();
        map?.Invoke(app);
        return app;
    }

    private static RouteEndpoint Endpoint(WebApplication app, string route) =>
        ((IEndpointRouteBuilder)app).DataSources.SelectMany(static source => source.Endpoints).OfType<RouteEndpoint>()
            .Single(endpoint => endpoint.RoutePattern.RawText == route);

    private static IProducesResponseTypeMetadata[] Produces(WebApplication app, string route) =>
        [.. Endpoint(app, route).Metadata.OfType<IProducesResponseTypeMetadata>()];

    private static int[] Statuses(WebApplication app, string route) =>
        [.. Produces(app, route).Select(static metadata => metadata.StatusCode).Distinct()];

    private static async Task<Response> InvokeAsync(WebApplication app, string route, int id)
    {
        using IServiceScope scope = app.Services.CreateScope();
        var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = route.Replace("{id:int}", id.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
        context.Request.RouteValues["id"] = id.ToString(System.Globalization.CultureInfo.InvariantCulture);
        using var body = new MemoryStream();
        context.Response.Body = body;
        Exception? exception = null;
        try
        {
            await Endpoint(app, route).RequestDelegate!(context);
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        return new Response(context.Response.StatusCode, Encoding.UTF8.GetString(body.ToArray()), context.Response.ContentType, context.Response.HasStarted, exception);
    }
}
