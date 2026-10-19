using System.Text.Json.Nodes;
using System.Threading.RateLimiting;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Idempotency;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.Extensions.DependencyInjection;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>
/// How a held key is settled: whether the handler started, the status, and failures decide between storing the
/// response, releasing the key and keeping it in flight until its lease expires.
/// </summary>
public sealed class IdempotencyExecutionTests
{
    private const string Stored = "stored";
    private const string Released = "released";
    private const string InFlight = "in-flight";

    private static readonly TimeSpan PastLease = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(1);

    public static TheoryData<string, string, string, string> Matrix()
    {
        TheoryData<string, string, string, string> data = [];
        foreach (string api in new[] { "minimal", "mvc" })
        {
            foreach (string stage in new[] { "handler", "middleware" })
            {
                foreach (string outcome in new[] { "201", "400", "500", "503", "504", IdempotencyOutcomes.Throw, IdempotencyOutcomes.Abort })
                {
                    string expected = stage == "middleware"
                        ? Released
                        : outcome switch
                        {
                            "201" or "500" => Stored,
                            "400" => Released,
                            _ => InFlight,
                        };
                    data.Add(api, stage, outcome, expected);
                }
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Matrix))]
    public async Task Outcome_Should_Settle_The_Key(string api, string stage, string outcome, string expected)
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();
        string path = api == "mvc" ? $"/mvc/orders/matrix?outcome={outcome}" : $"/matrix?outcome={outcome}";
        string? stageHeader = stage == "middleware" ? stage : null;

        IdempotencyResponse? first = await SendAsync(host, path, stageHeader);
        IdempotencyResponse? retry = await SendAsync(host, path, stageHeader);

        if (outcome == IdempotencyOutcomes.Abort)
            first.Should().BeNull("the request was aborted");
        else
            first!.StatusCode.Should().Be(outcome == IdempotencyOutcomes.Throw ? StatusCodes.Status500InternalServerError : int.Parse(outcome, System.Globalization.CultureInfo.InvariantCulture));

        switch (expected)
        {
            case Stored:
                retry!.Replayed.Should().BeTrue();
                retry.StatusCode.Should().Be(first.StatusCode);
                retry.Body.Should().Be(first.Body);
                host.Probe.Executions.Should().Be(1);
                break;
            case Released:
                if (retry is not null)
                {
                    retry.Replayed.Should().BeFalse();
                    retry.StatusCode.Should().NotBe(StatusCodes.Status409Conflict);
                }

                host.Probe.Executions.Should().Be(2);
                break;
            default:
                retry!.StatusCode.Should().Be(StatusCodes.Status409Conflict);
                Code(retry).Should().Be(IdempotencyErrorCodes.InFlight);
                host.Probe.Executions.Should().Be(1);
                host.Time.Advance(PastLease);
                IdempotencyResponse? rerun = await SendAsync(host, path, stageHeader);
                rerun?.StatusCode.Should().NotBe(StatusCodes.Status409Conflict);
                host.Probe.Executions.Should().Be(2, "the handler runs exactly once more after the lease");
                break;
        }
    }

    [Fact]
    public async Task Rate_Limiter_Rejection_Should_Release_The_Key()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            configureServices: services => services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    context.Request.Headers.ContainsKey("X-Limited")
                        ? RateLimitPartition.GetFixedWindowLimiter("closed", _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 1,
                            Window = TimeSpan.FromHours(1),
                            QueueLimit = 0,
                            AutoReplenishment = false,
                        })
                        : RateLimitPartition.GetNoLimiter("open"));
            }),
            configureInnerPipeline: app => app.UseRateLimiter());
        using HttpRequestMessage spend = IdempotencyTestHost.CreateRequest("/unmapped", key: null);
        spend.Headers.TryAddWithoutValidation("X-Limited", "1");
        using HttpRequestMessage limited = IdempotencyTestHost.CreateRequest("/orders");
        limited.Headers.TryAddWithoutValidation("X-Limited", "1");
        await host.SendSettledAsync(spend);

        IdempotencyResponse? rejected = await host.SendSettledAsync(limited);
        IdempotencyResponse retry = await host.PostAsync("/orders");

        rejected!.StatusCode.Should().Be(StatusCodes.Status429TooManyRequests);
        retry.StatusCode.Should().Be(StatusCodes.Status201Created);
        retry.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Load_Shedding_503_Should_Release_The_Key()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            configureInnerPipeline: app => app.Use((context, next) =>
            {
                if (!context.Request.Headers.ContainsKey("X-Shed"))
                    return next(context);
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                return Task.CompletedTask;
            }));
        using HttpRequestMessage shed = IdempotencyTestHost.CreateRequest("/orders");
        shed.Headers.TryAddWithoutValidation("X-Shed", "1");

        IdempotencyResponse? first = await host.SendSettledAsync(shed);
        IdempotencyResponse retry = await host.PostAsync("/orders");

        first!.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        retry.StatusCode.Should().Be(StatusCodes.Status201Created);
        host.Probe.Executions.Should().Be(1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Request_Timeout_Mid_Handler_Should_Keep_The_Key_In_Flight_Until_The_Lease_Expires(bool timeoutsInside)
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            configureServices: services => services.AddRequestTimeouts(),
            configurePipeline: app =>
            {
                if (!timeoutsInside)
                    app.UseRequestTimeouts();
            },
            configureInnerPipeline: app =>
            {
                if (timeoutsInside)
                    app.UseRequestTimeouts();
            });

        IdempotencyResponse? first = await SendAsync(host, "/timeout");
        IdempotencyResponse? retry = await SendAsync(host, "/timeout");
        host.Time.Advance(PastLease);
        IdempotencyResponse? rerun = await SendAsync(host, "/timeout");

        first!.StatusCode.Should().Be(StatusCodes.Status504GatewayTimeout);
        retry!.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        rerun!.StatusCode.Should().Be(StatusCodes.Status504GatewayTimeout);
        host.Probe.Executions.Should().Be(2);
    }

    [Fact]
    public async Task Handler_500_Should_Be_Replayed_For_ServerErrorRetention_While_A_Success_Keeps_Replaying()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync("/fail?status=500", key: "failed");
        await host.PostAsync("/orders", key: "created");
        host.Time.Advance(TimeSpan.FromMinutes(9));
        IdempotencyResponse failedBefore = await host.PostAsync("/fail?status=500", key: "failed");
        IdempotencyResponse createdBefore = await host.PostAsync("/orders", key: "created");
        host.Time.Advance(TimeSpan.FromMinutes(2));
        IdempotencyResponse failedAfter = await host.PostAsync("/fail?status=500", key: "failed");
        IdempotencyResponse createdAfter = await host.PostAsync("/orders", key: "created");

        failedBefore.Replayed.Should().BeTrue();
        failedBefore.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        createdBefore.Replayed.Should().BeTrue();
        failedAfter.Replayed.Should().BeFalse("a stored 5xx expires after ServerErrorRetention (10 minutes)");
        createdAfter.Replayed.Should().BeTrue();
        host.Probe.Executions.Should().Be(3);
    }

    [Fact]
    public async Task Handler_3xx_Should_Be_Stored_By_Default()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync("/fail?status=303");
        IdempotencyResponse retry = await host.PostAsync("/fail?status=303");

        retry.StatusCode.Should().Be(StatusCodes.Status303SeeOther);
        retry.Replayed.Should().BeTrue();
        host.Probe.Executions.Should().Be(1);
    }

    [Theory]
    [InlineData(303)]
    [InlineData(500)]
    public async Task SuccessOnly_Should_Release_3xx_And_5xx(int status)
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            options => options.ShouldStore = IdempotencyStorePolicies.SuccessOnly);

        await host.PostAsync($"/fail?status={status}");
        IdempotencyResponse retry = await host.PostAsync($"/fail?status={status}");
        IdempotencyResponse success = await host.PostAsync("/orders", key: "key-2");
        IdempotencyResponse successRetry = await host.PostAsync("/orders", key: "key-2");

        retry.Replayed.Should().BeFalse();
        successRetry.Replayed.Should().BeTrue();
        success.StatusCode.Should().Be(StatusCodes.Status201Created);
        host.Probe.Executions.Should().Be(3);
    }

    [Fact]
    public async Task NeverStoreStatusCodes_Should_Be_Configurable()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options =>
        {
            options.NeverStoreStatusCodes.Remove(StatusCodes.Status503ServiceUnavailable);
            options.NeverStoreStatusCodes.Add(StatusCodes.Status502BadGateway);
        });

        await host.PostAsync("/fail?status=503", key: "unavailable");
        IdempotencyResponse unavailable = await host.PostAsync("/fail?status=503", key: "unavailable");
        await host.PostAsync("/fail?status=502", key: "bad-gateway");
        IdempotencyResponse badGateway = await host.PostAsync("/fail?status=502", key: "bad-gateway");

        unavailable.Replayed.Should().BeTrue();
        badGateway.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        host.Probe.Executions.Should().Be(2);
    }

    [Fact]
    public async Task ShouldStore_Should_Receive_The_Outcome_Of_A_Started_Handler()
    {
        List<IdempotencyOutcome> outcomes = [];
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.ShouldStore = outcome =>
        {
            outcomes.Add(outcome);
            return false;
        });

        await host.PostAsync("/fail?status=202");
        using HttpRequestMessage notStarted = IdempotencyTestHost.CreateRequest("/matrix?outcome=201", key: "key-2");
        notStarted.Headers.TryAddWithoutValidation(IdempotencyOutcomes.StageHeader, "middleware");
        await host.SendSettledAsync(notStarted);
        await host.PostAsync("/fail?status=503", key: "key-3");

        outcomes.Should().Equal(new IdempotencyOutcome { StatusCode = StatusCodes.Status202Accepted, HandlerStarted = true });
    }

    [Fact]
    public async Task Handler_Abort_Should_Return_409_Then_Run_Once_More_After_The_Lease()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        IdempotencyResponse? first = await SendAsync(host, "/matrix?outcome=abort");
        IdempotencyResponse? retry = await SendAsync(host, "/matrix?outcome=abort");
        host.Time.Advance(PastLease);
        await SendAsync(host, "/matrix?outcome=abort");

        first.Should().BeNull();
        retry!.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        host.Probe.Executions.Should().Be(2);
        host.Logs.Named("Idempotency.KeptInFlight").Should().HaveCount(2);
    }

    [Fact]
    public async Task Handler_That_Ignores_The_Abort_Should_Have_Its_Response_Stored()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await SendAsync(host, "/abort-ignored");
        IdempotencyResponse? retry = await SendAsync(host, "/abort-ignored");

        retry!.Replayed.Should().BeTrue();
        retry.StatusCode.Should().Be(StatusCodes.Status201Created);
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Failure_Before_The_Handler_Started_Should_Release_Even_When_The_Store_Fails()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            options => options.UseStore<ThrowingIdempotencyStore>(ServiceLifetime.Singleton));
        using HttpRequestMessage request = IdempotencyTestHost.CreateRequest("/matrix?outcome=throw");
        request.Headers.TryAddWithoutValidation(IdempotencyOutcomes.StageHeader, "middleware");

        IdempotencyResponse? response = await host.SendSettledAsync(request);

        response!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError, "the request's own exception surfaces");
        host.Logs.Named("Idempotency.ReleaseFailed").Should().ContainSingle();
    }

    [Theory]
    [InlineData("encoded", 201, InFlight)]
    [InlineData("encoded", 500, InFlight)]
    [InlineData("encoded", 422, Released)]
    [InlineData("event-stream", 200, InFlight)]
    [InlineData("event-stream", 502, InFlight)]
    [InlineData("event-stream", 409, Released)]
    [InlineData("large", 201, InFlight)]
    [InlineData("large", 500, InFlight)]
    [InlineData("large", 400, Released)]
    public async Task Response_That_Cannot_Be_Stored_Should_Release_Only_On_4xx(string kind, int status, string expected)
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options =>
        {
            options.MaxResponseBodySize = 1024;
            options.ShouldStore = _ => true;
        });
        string path = $"/not-storable?kind={kind}&status={status}";

        IdempotencyResponse first = await host.PostAsync(path);
        IdempotencyResponse retry = await host.PostAsync(path);

        first.StatusCode.Should().Be(status);
        if (kind == "large")
            first.Body.Should().HaveLength(4096);
        if (expected == Released)
        {
            retry.StatusCode.Should().Be(status);
            host.Probe.Executions.Should().Be(2);
            host.Logs.Named("Idempotency.ResponseNotStorable").Should().HaveCount(2);
            return;
        }

        retry.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        host.Probe.Executions.Should().Be(1);
        host.Logs.Named("Idempotency.KeptInFlight").Should().ContainSingle();
        host.Time.Advance(PastLease);
        IdempotencyResponse rerun = await host.PostAsync(path);
        rerun.StatusCode.Should().Be(status);
        host.Probe.Executions.Should().Be(2);
    }

    [Fact]
    public async Task Response_Started_Before_Its_First_Write_Should_Be_Replayed_Through_Outer_Compression()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            configureServices: services => services.AddResponseCompression(options => options.EnableForHttps = true),
            configurePipeline: app => app.UseResponseCompression());

        using HttpRequestMessage first = IdempotencyTestHost.CreateRequest("/start-first");
        first.Headers.AcceptEncoding.ParseAdd("gzip");
        using HttpRequestMessage second = IdempotencyTestHost.CreateRequest("/start-first");
        second.Headers.AcceptEncoding.ParseAdd("gzip");
        using HttpResponseMessage firstResponse = await host.Client.SendAsync(first);
        using HttpResponseMessage secondResponse = await host.Client.SendAsync(second);

        firstResponse.Content.Headers.ContentEncoding.Should().Equal("gzip");
        secondResponse.Content.Headers.ContentEncoding.Should().Equal("gzip");
        secondResponse.Headers.GetValues("Idempotency-Replayed").Should().Equal("true");
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Binding_Failure_Should_Release_The_Key_Because_The_Handler_Did_Not_Start()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.ShouldStore = _ => true);

        IdempotencyResponse first = await host.PostAsync("/fail");
        IdempotencyResponse retry = await host.PostAsync("/fail");

        first.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        retry.Replayed.Should().BeFalse();
    }

    [Fact]
    public async Task Mvc_Model_Validation_Failure_Should_Release_The_Key_Because_The_Action_Did_Not_Start()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.ShouldStore = _ => true);

        IdempotencyResponse first = await host.PostAsync("/mvc/orders/matrix");
        IdempotencyResponse retry = await host.PostAsync("/mvc/orders/matrix");

        first.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        retry.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(0);
    }

    [Fact]
    public async Task Filter_Before_WithIdempotency_That_Short_Circuits_Should_Release_The_Key()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        IdempotencyResponse first = await host.PostAsync("/short-circuit");
        IdempotencyResponse retry = await host.PostAsync("/short-circuit");

        first.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        retry.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(2);
    }

    [Fact]
    public async Task Filter_After_WithIdempotency_Should_Run_After_The_Handler_Is_Marked_As_Started()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync("/inner-short-circuit");
        IdempotencyResponse retry = await host.PostAsync("/inner-short-circuit");

        retry.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        retry.Replayed.Should().BeTrue();
        host.Probe.Executions.Should().Be(1);
    }

    [Theory]
    [InlineData("/raw")]
    [InlineData("/group/fail?status=500")]
    public async Task WithIdempotency_Should_Install_The_Filter_On_Request_Delegates_And_Groups(string path)
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync(path);
        IdempotencyResponse retry = await host.PostAsync(path);

        retry.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        retry.Replayed.Should().BeTrue("a 500 is stored only when the handler was marked as started");
        host.Probe.Executions.Should().Be(1);
        host.Logs.Named("Idempotency.ExecutionFilterMissing").Should().BeEmpty();
    }

    [Fact]
    public async Task Endpoint_Without_The_Filter_Should_Release_Every_Outcome_And_Warn_Once()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync("/no-filter?status=201");
        IdempotencyResponse retry = await host.PostAsync("/no-filter?status=201");

        retry.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(2);
        host.Logs.Named("Idempotency.ExecutionFilterMissing").Should().ContainSingle()
            .Which.Message.Should().Contain("/no-filter");
    }

    [Fact]
    public async Task RequireExecutionFilter_Should_Throw_For_An_Endpoint_Without_The_Filter()
    {
        InvalidOperationException? thrown = null;
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            options => options.RequireExecutionFilter = true,
            configurePipeline: app => app.Use(async (context, next) =>
            {
                try
                {
                    await next(context);
                }
                catch (InvalidOperationException exception)
                {
                    thrown = exception;
                    throw;
                }
            }));

        IdempotencyResponse response = await host.PostAsync("/no-filter?status=201");
        IdempotencyResponse filtered = await host.PostAsync("/orders", key: "key-2");

        response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        thrown.Should().NotBeNull();
        thrown.Message.Should().Contain("/no-filter").And.Contain("WithIdempotency");
        host.Probe.Executions.Should().Be(1);
        filtered.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task Mvc_Action_Opted_In_By_WithIdempotency_Without_The_Attribute_Should_Warn()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            configureEndpoints: app => app.MapControllers().WithIdempotency());

        await host.PostAsync("/mvc/plain");
        IdempotencyResponse retry = await host.PostAsync("/mvc/plain");
        await host.PostAsync("/mvc/orders", key: "key-2");
        IdempotencyResponse attributed = await host.PostAsync("/mvc/orders", key: "key-2");

        retry.Replayed.Should().BeFalse();
        attributed.Replayed.Should().BeTrue();
        host.Probe.Executions.Should().Be(3);
        host.Logs.Named("Idempotency.ExecutionFilterMissing").Should().ContainSingle()
            .Which.Message.Should().Contain(nameof(PlainOrdersController));
    }

    [Fact]
    public async Task Request_Timeout_Longer_Than_The_Lease_Should_Warn_Once_Per_Endpoint()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            configureServices: services => services.AddRequestTimeouts(options =>
                options.DefaultPolicy = new RequestTimeoutPolicy { Timeout = TimeSpan.FromMinutes(2) }),
            configureEndpoints: app => app.MapPost("/untimed", (IdempotencyProbe probe) => HttpResults.Ok(new { execution = probe.Execute() }))
                .WithIdempotency()
                .DisableRequestTimeout());

        await host.PostAsync("/orders");
        await host.PostAsync("/orders", key: "key-2");
        await host.PostAsync("/untimed", key: "key-3");
        await host.PostAsync("/fail?status=201", key: "key-4");

        host.Logs.Named("Idempotency.RequestTimeoutExceedsLease").Select(entry => entry.Message).Should().HaveCount(2)
            .And.Contain(message => message.Contains("/orders"))
            .And.Contain(message => message.Contains("/fail"));
    }

    [Fact]
    public async Task Request_Timeout_Within_The_Lease_Should_Not_Warn()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            configureServices: services => services.AddRequestTimeouts(options =>
                options.DefaultPolicy = new RequestTimeoutPolicy { Timeout = TimeSpan.FromSeconds(30) }));

        await host.PostAsync("/orders");

        host.Logs.Named("Idempotency.RequestTimeoutExceedsLease").Should().BeEmpty();
    }

    [Fact]
    public async Task Client_Scope_Should_Separate_Client_Credentials_Callers()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.ScopeResolver = IdempotencyScopes.Client);

        IdempotencyResponse first = await PostAsClientAsync(host, "billing");
        IdempotencyResponse sameClient = await PostAsClientAsync(host, "billing");
        IdempotencyResponse otherClient = await PostAsClientAsync(host, "reporting");

        first.StatusCode.Should().Be(StatusCodes.Status201Created);
        sameClient.Replayed.Should().BeTrue();
        otherClient.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(2);
    }

    [Fact]
    public async Task Unscoped_Key_Should_Run_Without_Idempotency_And_Warn_By_Default()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync("/orders", key: "secret-key-value", user: null);
        IdempotencyResponse second = await host.PostAsync("/orders", key: "secret-key-value", user: null);

        second.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(2);
        IReadOnlyList<CapturedLog> warnings = host.Logs.Named(IdempotencyErrorCodes.UnscopedKeySkipped);
        warnings.Should().HaveCount(2).And.OnlyContain(entry => entry.Level == Microsoft.Extensions.Logging.LogLevel.Warning);
        warnings.Should().OnlyContain(entry => entry.Message.Contains("/orders") && !entry.Message.Contains("secret-key-value"));
    }

    [Fact]
    public async Task UnscopedKeyBehavior_Reject_Should_Return_400_ScopeRequired()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.UnscopedKeyBehavior = UnscopedKeyBehavior.Reject);

        IdempotencyResponse response = await host.PostAsync("/orders", user: null);
        IdempotencyResponse scoped = await host.PostAsync("/orders", user: "alice");

        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        Code(response).Should().Be(IdempotencyErrorCodes.ScopeRequired);
        response.Header("Content-Type").Should().StartWith("application/problem+json");
        scoped.StatusCode.Should().Be(StatusCodes.Status201Created);
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Retry_After_Should_Be_Replayed()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            configureEndpoints: app => app.MapPost("/accepted", (HttpContext context, IdempotencyProbe probe) =>
            {
                context.Response.Headers.RetryAfter = "5";
                return HttpResults.Accepted("/jobs/1", new { execution = probe.Execute() });
            }).WithIdempotency());

        await host.PostAsync("/accepted");
        IdempotencyResponse retry = await host.PostAsync("/accepted");

        retry.Replayed.Should().BeTrue();
        retry.Header("Retry-After").Should().Be("5");
    }

    private static async Task<IdempotencyResponse> PostAsClientAsync(IdempotencyTestHost host, string client)
    {
        using HttpRequestMessage request = IdempotencyTestHost.CreateRequest("/orders", user: null);
        request.Headers.TryAddWithoutValidation(IdempotencyTestHost.ClientHeader, client);
        using HttpResponseMessage response = await host.Client.SendAsync(request);
        return await IdempotencyResponse.ReadAsync(response);
    }

    private static async Task<IdempotencyResponse?> SendAsync(IdempotencyTestHost host, string path, string? stage = null)
    {
        using HttpRequestMessage request = IdempotencyTestHost.CreateRequest(path);
        if (stage is not null)
            request.Headers.TryAddWithoutValidation(IdempotencyOutcomes.StageHeader, stage);
        return await host.SendSettledAsync(request);
    }

    private static string? Code(IdempotencyResponse response) =>
        JsonNode.Parse(response.Body)?["code"]?.GetValue<string>();
}
