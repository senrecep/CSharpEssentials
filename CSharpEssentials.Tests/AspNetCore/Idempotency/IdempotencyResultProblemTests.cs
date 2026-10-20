using System.Security.Claims;
using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.Idempotency;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;
using CSharpEssentials.Tests.Mediator;
using FluentAssertions;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

public sealed class IdempotencyResultProblemTests
{
    private static Task<IdempotencyTestHost> StartAsync(Action<IdempotencyOptions>? configure = null) =>
        IdempotencyTestHost.StartAsync(configure, configureEndpoints: app =>
        {
            app.MapPost("/result/in-flight", () =>
                Result<int>.Failure(Error.Conflict(IdempotencyErrorCodes.InFlight, "Still processing.")).ToProblemResult());
            app.MapPost("/result/business", () =>
                Result<int>.Failure(Error.Conflict("Order.Paid", "The order is already paid.")).ToProblemResult());
        });

    private static Task<IdempotencyTestHost> StartMediatorAsync() =>
        IdempotencyTestHost.StartAsync(
            configureServices: services => services.AddMediatorIdempotencyBehavior(),
            configureEndpoints: app => app.MapPost("/mediator/orders", async (HttpContext context) =>
            {
                using StreamReader reader = new(context.Request.Body);
                int amount = (await reader.ReadToEndAsync()).Length;
                IPipelineBehavior<IdemCommand, Result<int>> behavior =
                    context.RequestServices.GetServices<IPipelineBehavior<IdemCommand, Result<int>>>().Single();
                Result<int> result = await behavior.Handle(
                    new IdemCommand(context.Request.Headers["Idempotency-Key"].ToString(), amount),
                    (command, _) => new ValueTask<Result<int>>(command.Amount),
                    context.RequestAborted);
                return result.IsSuccess ? Microsoft.AspNetCore.Http.Results.Ok(result.Value) : result.ToProblemResult();
            }));

    [Fact]
    public async Task Same_Key_With_Different_Payload_Should_Return_422_With_The_Same_Code_Over_Middleware_And_Mediator()
    {
        await using IdempotencyTestHost middlewareHost = await IdempotencyTestHost.StartAsync();
        await using IdempotencyTestHost mediatorHost = await StartMediatorAsync();

        await middlewareHost.PostAsync("/orders");
        IdempotencyResponse middleware = await middlewareHost.PostAsync("/orders", body: """{"item":"pen"}""");
        await mediatorHost.PostAsync("/mediator/orders");
        IdempotencyResponse mediator = await mediatorHost.PostAsync("/mediator/orders", body: """{"item":"notebook"}""");

        middleware.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        mediator.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        JsonNode.Parse(middleware.Body)!.AsObject()["code"]!.GetValue<string>().Should().Be(IdempotencyErrorCodes.KeyReused);
        JsonNode.Parse(mediator.Body)!.AsObject()["code"]!.GetValue<string>().Should().Be(IdempotencyErrorCodes.KeyReused);
        mediator.Header("Retry-After").Should().BeNull();
    }

    [Theory]
    [InlineData(IdempotencyErrorCodes.KeyReused, StatusCodes.Status422UnprocessableEntity)]
    [InlineData(IdempotencyErrorCodes.InFlight, StatusCodes.Status409Conflict)]
    [InlineData(IdempotencyErrorCodes.ScopeRequired, StatusCodes.Status400BadRequest)]
    [InlineData(IdempotencyErrorCodes.KeyMissing, StatusCodes.Status400BadRequest)]
    [InlineData(IdempotencyErrorCodes.KeyInvalid, StatusCodes.Status400BadRequest)]
    [InlineData("Order.Invalid", StatusCodes.Status400BadRequest)]
    public void GetStatusCode_Should_Map_Idempotency_Codes_To_Middleware_Statuses(string code, int expected)
    {
        Error error = Error.Validation(code, "Idempotency failure.");

        int status = DefaultErrorStatusCodeMapper.Instance.GetStatusCode(error);

        status.Should().Be(expected);
    }

    [Theory]
    [InlineData("/result/in-flight")]
    [InlineData("/mvc/result/in-flight")]
    public async Task InFlight_Result_Should_Return_409_With_Code_And_Retry_After(string path)
    {
        await using IdempotencyTestHost host = await StartAsync(options => options.RetryAfter = TimeSpan.FromSeconds(3));

        IdempotencyResponse response = await host.PostAsync(path, key: null);

        response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        response.Header("Retry-After").Should().Be("3");
        JsonNode.Parse(response.Body)!.AsObject()["code"]!.GetValue<string>().Should().Be(IdempotencyErrorCodes.InFlight);
    }

    [Fact]
    public async Task InFlight_Result_Should_Default_Retry_After_To_One_Second_Without_Idempotency_Options()
    {
        await using ProblemTestHost host = await ProblemTestHost.StartAsync(HostKind.Minimal, mapExtra: app =>
            app.MapPost("/result/in-flight", () =>
                Result<int>.Failure(Error.Conflict(IdempotencyErrorCodes.InFlight, "Still processing.")).ToProblemResult()));

        using HttpResponseMessage message = await host.Client.PostAsync("/result/in-flight", content: null);
        IdempotencyResponse response = await IdempotencyResponse.ReadAsync(message);

        response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        response.Header("Retry-After").Should().Be("1");
        JsonNode.Parse(response.Body)!.AsObject()["code"]!.GetValue<string>().Should().Be(IdempotencyErrorCodes.InFlight);
    }

    [Theory]
    [InlineData("/result/business")]
    [InlineData("/mvc/result/business")]
    public async Task Business_Conflict_Should_Return_409_Without_Code_Or_Retry_After(string path)
    {
        await using IdempotencyTestHost host = await StartAsync(options => options.RetryAfter = TimeSpan.FromSeconds(3));

        IdempotencyResponse response = await host.PostAsync(path, key: null);

        response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        response.Header("Retry-After").Should().BeNull();
        JsonNode.Parse(response.Body)!.AsObject().ContainsKey("code").Should().BeFalse();
    }

    [Fact]
    public void AddIdempotency_Should_Register_A_Scope_Accessor_That_Reads_The_Request_User()
    {
        ServiceCollection services = new();
        services.AddIdempotency();
        using ServiceProvider provider = services.BuildServiceProvider();
        IHttpContextAccessor httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
        IIdempotencyScopeAccessor accessor = provider.GetRequiredService<IIdempotencyScopeAccessor>();
        ClaimsPrincipal user = UserPrincipal("alice");

        httpContextAccessor.HttpContext = new DefaultHttpContext { User = user };
        ClaimsPrincipal? withContext = accessor.Principal;
        httpContextAccessor.HttpContext = null;
        ClaimsPrincipal? withoutContext = accessor.Principal;

        withContext.Should().BeSameAs(user);
        withoutContext.Should().BeNull();
    }

    [Fact]
    public void AddIdempotency_Should_Keep_An_Existing_Scope_Accessor()
    {
        ServiceCollection services = new();
        services.AddSingleton<IIdempotencyScopeAccessor, FixedScopeAccessor>();

        services.AddIdempotency();

        using ServiceProvider provider = services.BuildServiceProvider();
        provider.GetRequiredService<IIdempotencyScopeAccessor>().Should().BeOfType<FixedScopeAccessor>();
    }

    [Fact]
    public async Task Mediator_Behavior_Should_Scope_Keys_To_The_Request_User_When_AddIdempotency_Is_Registered()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddIdempotency();
        services.AddMediatorIdempotencyBehavior();
        await using ServiceProvider provider = services.BuildServiceProvider();
        IHttpContextAccessor httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
        int runs = 0;

        Result<int> alice = await SendAsync(provider, httpContextAccessor, "alice", () => ++runs);
        Result<int> bob = await SendAsync(provider, httpContextAccessor, "bob", () => ++runs);
        Result<int> aliceRetry = await SendAsync(provider, httpContextAccessor, "alice", () => ++runs);

        alice.Value.Should().Be(1);
        bob.Value.Should().Be(2);
        aliceRetry.Value.Should().Be(1);
        runs.Should().Be(2);
    }

    private static async Task<Result<int>> SendAsync(
        IServiceProvider provider,
        IHttpContextAccessor httpContextAccessor,
        string user,
        Func<int> handler)
    {
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        httpContextAccessor.HttpContext = new DefaultHttpContext { User = UserPrincipal(user) };
        try
        {
            IPipelineBehavior<IdemCommand, Result<int>> behavior =
                scope.ServiceProvider.GetServices<IPipelineBehavior<IdemCommand, Result<int>>>().Single();
            return await behavior.Handle(new IdemCommand("k1", 5), (_, _) => new ValueTask<Result<int>>(handler()), CancellationToken.None);
        }
        finally
        {
            httpContextAccessor.HttpContext = null;
        }
    }

    private static ClaimsPrincipal UserPrincipal(string user) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user)], "Test"));

    private sealed class FixedScopeAccessor : IIdempotencyScopeAccessor
    {
        public ClaimsPrincipal? Principal => null;
    }
}
