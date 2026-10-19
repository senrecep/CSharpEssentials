using CSharpEssentials.AspNetCore;
using CSharpEssentials.Idempotency;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

/// <summary>
/// Per-endpoint settings and named profiles: each setting given by the endpoint wins over the profile <c>p</c>,
/// which wins over <see cref="IdempotencyOptions"/>, on Minimal API endpoints and MVC actions alike.
/// </summary>
public sealed class IdempotencyOverridesTests
{
    private const string Global = "global";
    private const string Profile = "profile";
    private const string Endpoint = "endpoint";

    private const string Retention = "retention";
    private const string Lease = "lease";
    private const string ServerErrorRetention = "server-error-retention";
    private const string RequireKey = "require-key";
    private const string Unscoped = "unscoped";
    private const string Store = "store";
    private const string ShouldStore = "should-store";

    public static TheoryData<string, string, string> Precedence()
    {
        TheoryData<string, string, string> data = [];
        foreach (string setting in new[] { Retention, Lease, ServerErrorRetention, RequireKey, Unscoped, Store, ShouldStore })
        {
            foreach (string level in new[] { Global, Profile, Endpoint })
            {
                data.Add("minimal", setting, level);
                if (setting != ShouldStore)
                    data.Add("mvc", setting, level);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Precedence))]
    public async Task Setting_Should_Come_From_The_Most_Specific_Level(string api, string setting, string level)
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            options => ConfigureLevels(options, setting),
            services =>
            {
                services.AddKeyedSingleton<IIdempotencyRecordStore>("profile-store", (provider, _) => new CountingIdempotencyStore(provider.GetRequiredService<TimeProvider>()));
                services.AddKeyedSingleton<IIdempotencyRecordStore>("endpoint-store", (provider, _) => new CountingIdempotencyStore(provider.GetRequiredService<TimeProvider>()));
            },
            configureEndpoints: app => MapLevel(app, setting, level));
        string action = level == Endpoint ? setting : level;
        string path = api == "mvc" ? $"/mvc/overrides/{action}" : "/overrides";

        await AssertEffectiveAsync(host, path, setting, level);
    }

    private static void ConfigureLevels(IdempotencyOptions options, string setting)
    {
        switch (setting)
        {
            case Retention:
                options.RetentionPeriod = TimeSpan.FromHours(1);
                options.AddProfile("p", p => p.RetentionPeriod = TimeSpan.FromHours(2));
                break;
            case Lease:
                options.AddProfile("p", p => p.InFlightTimeout = TimeSpan.FromMinutes(2));
                break;
            case ServerErrorRetention:
                options.AddProfile("p", p => p.ServerErrorRetention = TimeSpan.FromMinutes(20));
                break;
            case RequireKey:
                options.RequireKey = true;
                options.AddProfile("p", p => p.RequireKey = false);
                break;
            case Unscoped:
                options.AddProfile("p", p => p.UnscopedKeyBehavior = UnscopedKeyBehavior.Reject);
                break;
            case Store:
                options.AddProfile("p", p => p.StoreKey = "profile-store");
                break;
            default:
                options.ShouldStore = IdempotencyStorePolicies.SuccessOnly;
                options.AddProfile("p", p => p.ShouldStore = IdempotencyStorePolicies.Default);
                break;
        }
    }

    private static void MapLevel(WebApplication app, string setting, string level)
    {
        RouteHandlerBuilder endpoint = app.MapPost("/overrides", async (HttpContext context, string outcome, IdempotencyProbe probe) =>
        {
            probe.Execute();
            await IdempotencyOutcomes.RunAsync(context, outcome);
        });
        if (level == Global)
        {
            endpoint.WithIdempotency();
            return;
        }

        if (level == Profile)
        {
            endpoint.WithIdempotency("p");
            return;
        }

        endpoint.WithIdempotency(o =>
        {
            o.Profile = "p";
            switch (setting)
            {
                case Retention:
                    o.RetentionPeriod = TimeSpan.FromHours(3);
                    break;
                case Lease:
                    o.InFlightTimeout = TimeSpan.FromMinutes(3);
                    break;
                case ServerErrorRetention:
                    o.ServerErrorRetention = TimeSpan.FromMinutes(30);
                    break;
                case RequireKey:
                    o.RequireKey = true;
                    break;
                case Unscoped:
                    o.UnscopedKeyBehavior = UnscopedKeyBehavior.Global;
                    break;
                case Store:
                    o.StoreKey = "endpoint-store";
                    break;
                default:
                    o.ShouldStore = IdempotencyStorePolicies.SuccessOnly;
                    break;
            }
        });
    }

    private static int Pick(string level, int global, int profile, int endpoint) =>
        level switch
        {
            Global => global,
            Profile => profile,
            _ => endpoint,
        };

    private static async Task AssertEffectiveAsync(IdempotencyTestHost host, string path, string setting, string level)
    {
        switch (setting)
        {
            case Retention:
                await AssertExpiresAfterAsync(host, $"{path}?outcome=201", TimeSpan.FromHours(Pick(level, 1, 2, 3)));
                break;
            case ServerErrorRetention:
                await AssertExpiresAfterAsync(host, $"{path}?outcome=500", TimeSpan.FromMinutes(Pick(level, 10, 20, 30)));
                break;
            case Lease:
                await AssertLeaseAsync(host, $"{path}?outcome=503", TimeSpan.FromMinutes(Pick(level, 1, 2, 3)));
                break;
            case RequireKey:
                IdempotencyResponse withoutKey = await host.PostAsync($"{path}?outcome=201", key: null);
                withoutKey.StatusCode.Should().Be(level == Profile ? StatusCodes.Status201Created : StatusCodes.Status400BadRequest);
                break;
            case Unscoped:
                IdempotencyResponse first = await host.PostAsync($"{path}?outcome=201", user: null);
                IdempotencyResponse second = await host.PostAsync($"{path}?outcome=201", user: null);
                switch (level)
                {
                    case Global:
                        second.Replayed.Should().BeFalse();
                        host.Probe.Executions.Should().Be(2, "Skip runs the handler without idempotency");
                        break;
                    case Profile:
                        first.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
                        host.Probe.Executions.Should().Be(0, "Reject refuses an unscoped key");
                        break;
                    default:
                        second.Replayed.Should().BeTrue("Global shares the key between anonymous callers");
                        host.Probe.Executions.Should().Be(1);
                        break;
                }

                break;
            case Store:
                IdempotencyResponse stored = await host.PostAsync($"{path}?outcome=201");
                stored.StatusCode.Should().Be(StatusCodes.Status201Created);
                Reservations(host, "profile-store").Should().Be(level == Profile ? 1 : 0);
                Reservations(host, "endpoint-store").Should().Be(level == Endpoint ? 1 : 0);
                break;
            default:
                await host.PostAsync($"{path}?outcome=500");
                IdempotencyResponse retry = await host.PostAsync($"{path}?outcome=500");
                retry.Replayed.Should().Be(level == Profile, "only the profile stores 5xx responses");
                host.Probe.Executions.Should().Be(level == Profile ? 1 : 2);
                break;
        }
    }

    private static async Task AssertExpiresAfterAsync(IdempotencyTestHost host, string path, TimeSpan retention, string key = "key-1")
    {
        int executions = host.Probe.Executions;
        await host.PostAsync(path, key);
        host.Time.Advance(retention - TimeSpan.FromMinutes(1));
        IdempotencyResponse before = await host.PostAsync(path, key);
        host.Time.Advance(TimeSpan.FromMinutes(2));
        IdempotencyResponse after = await host.PostAsync(path, key);

        before.Replayed.Should().BeTrue($"the record is kept for {retention}");
        after.Replayed.Should().BeFalse($"the record expires after {retention}");
        host.Probe.Executions.Should().Be(executions + 2);
    }

    private static async Task AssertLeaseAsync(IdempotencyTestHost host, string path, TimeSpan lease, string key = "key-1")
    {
        int executions = host.Probe.Executions;
        await host.PostAsync(path, key);
        host.Time.Advance(lease - TimeSpan.FromSeconds(10));
        IdempotencyResponse before = await host.PostAsync(path, key);
        host.Time.Advance(TimeSpan.FromSeconds(20));
        IdempotencyResponse after = await host.PostAsync(path, key);

        before.StatusCode.Should().Be(StatusCodes.Status409Conflict, $"the key is held for {lease}");
        after.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable, $"the lease ends after {lease}");
        host.Probe.Executions.Should().Be(executions + 2);
    }

    private static int Reservations(IdempotencyTestHost host, string key) =>
        ((CountingIdempotencyStore)host.Services.GetRequiredKeyedService<IIdempotencyRecordStore>(key)).Reservations;

    [Fact]
    public async Task Endpoint_Settings_Should_Merge_Over_The_Route_Group()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            options => options.AddProfile("p", p => p.InFlightTimeout = TimeSpan.FromMinutes(2)),
            configureEndpoints: app =>
            {
                RouteGroupBuilder group = app.MapGroup("/profiled").WithIdempotency("p");
                group.MapPost("/inherited", Run);
                group.MapPost("/overridden", Run).WithIdempotency(o => o.InFlightTimeout = TimeSpan.FromMinutes(3));
            });

        await AssertLeaseAsync(host, "/profiled/inherited?outcome=503", TimeSpan.FromMinutes(2), "key-1");
        await AssertLeaseAsync(host, "/profiled/overridden?outcome=503", TimeSpan.FromMinutes(3), "key-2");
    }

    [Fact]
    public async Task Action_Settings_Should_Merge_Over_The_Controller()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            options => options.AddProfile("p", p => p.RetentionPeriod = TimeSpan.FromHours(2)));

        await AssertLeaseAsync(host, "/mvc/profiled?outcome=503", TimeSpan.FromMinutes(3), "key-1");
        await AssertExpiresAfterAsync(host, "/mvc/profiled?outcome=201", TimeSpan.FromHours(2), "key-2");
    }

    [Fact]
    public async Task Overrides_Should_Not_Change_Other_Endpoints()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            options => options.AddProfile("p", p => p.InFlightTimeout = TimeSpan.FromMinutes(2)),
            configureEndpoints: app => app.MapPost("/overrides", Run).WithIdempotency("p"));

        await AssertLeaseAsync(host, "/overrides?outcome=503", TimeSpan.FromMinutes(2), "key-1");
        await AssertLeaseAsync(host, "/matrix?outcome=503", TimeSpan.FromMinutes(1), "key-2");
    }

    [Theory]
    [InlineData("/overrides")]
    [InlineData("/mvc/overrides/unknown-profile")]
    public async Task Unknown_Profile_Should_Fail_Every_Request(string path)
    {
        List<string> errors = [];
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            options => options.AddProfile("p", _ => { }),
            configurePipeline: app => app.Use(async (context, next) =>
            {
                try
                {
                    await next(context);
                }
                catch (InvalidOperationException exception)
                {
                    errors.Add(exception.Message);
                    throw;
                }
            }),
            configureEndpoints: app => app.MapPost("/overrides", Run).WithIdempotency("missing"));

        IdempotencyResponse first = await host.PostAsync($"{path}?outcome=201");
        IdempotencyResponse second = await host.PostAsync($"{path}?outcome=201", key: "key-2");

        first.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        second.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        host.Probe.Executions.Should().Be(0);
        errors.Should().HaveCount(2).And.AllSatisfy(message => message.Should()
            .Contain("'missing'").And.Contain("not registered").And.Contain("Registered profiles: 'p'"));
    }

    [Fact]
    public async Task Invalid_Endpoint_Settings_Should_Fail_Every_Request()
    {
        List<string> errors = [];
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            configurePipeline: app => app.Use(async (context, next) =>
            {
                try
                {
                    await next(context);
                }
                catch (InvalidOperationException exception)
                {
                    errors.Add(exception.Message);
                    throw;
                }
            }),
            configureEndpoints: app => app.MapPost("/overrides", Run).WithIdempotency(o => o.InFlightTimeout = TimeSpan.FromMinutes(20)));

        await host.PostAsync("/overrides?outcome=201");
        await host.PostAsync("/overrides?outcome=201", key: "key-2");

        host.Probe.Executions.Should().Be(0);
        errors.Should().HaveCount(2).And.AllSatisfy(message => message.Should()
            .Contain("/overrides").And.Contain("InFlightTimeout").And.Contain("MaxInFlightTimeout"));
    }

    [Fact]
    public async Task Request_Timeout_Warning_Should_Use_The_Effective_Lease()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            options => options.AddProfile("long", p => p.InFlightTimeout = TimeSpan.FromMinutes(5)),
            configureServices: services => services.AddRequestTimeouts(options =>
                options.DefaultPolicy = new RequestTimeoutPolicy { Timeout = TimeSpan.FromMinutes(2) }),
            configureEndpoints: app => app.MapPost("/overrides", Run).WithIdempotency("long"));

        await host.PostAsync("/overrides?outcome=201");
        await host.PostAsync("/orders", key: "key-2");

        host.Logs.Named("Idempotency.RequestTimeoutExceedsLease").Should().ContainSingle()
            .Which.Message.Should().Contain("/orders");
    }

    [Fact]
    public void AddProfile_Should_Reject_Invalid_Profiles()
    {
        Action[] invalid =
        [
            () => new ServiceCollection().AddIdempotency(o => o.AddProfile(" ", _ => { })),
            () => new ServiceCollection().AddIdempotency(o => o.AddProfile("p", _ => { }).AddProfile("P", _ => { })),
            () => new ServiceCollection().AddIdempotency(o => o.AddProfile("p", p => p.Profile = "q")),
        ];

        foreach (Action act in invalid)
            act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddIdempotency_Should_Validate_Profiles_Against_The_Global_Options()
    {
        Action act = () => new ServiceCollection().AddIdempotency(o =>
            o.AddProfile("short", p => p.RetentionPeriod = TimeSpan.FromMinutes(5)));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*profile 'short'*ServerErrorRetention*RetentionPeriod*");
    }

    [Fact]
    public void AddIdempotency_Should_Accept_A_Long_Server_Error_Retention_Profile()
    {
        Action act = () => new ServiceCollection().AddIdempotency(o =>
            o.AddProfile("payments", p => p.ServerErrorRetention = TimeSpan.FromHours(24)));

        act.Should().NotThrow();
    }

    [Fact]
    public async Task WithIdempotency_Should_Reject_An_Empty_Profile_Name()
    {
        await using WebApplication app = WebApplication.CreateBuilder().Build();

        Action act = () => app.MapPost("/x", () => HttpResults.Ok()).WithIdempotency(" ");

        act.Should().Throw<ArgumentException>();
    }

    private static async Task Run(HttpContext context, string outcome, IdempotencyProbe probe)
    {
        probe.Execute();
        await IdempotencyOutcomes.RunAsync(context, outcome);
    }
}
