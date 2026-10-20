using System.Text.RegularExpressions;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Idempotency;
using CSharpEssentials.Tests.Idempotency;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

public sealed partial class IdempotencyModeTests
{
    [Fact]
    public async Task Observe_Should_Answer_Exactly_Like_Off_For_Every_Decision()
    {
        (IReadOnlyList<IdempotencyResponse> off, _) = await RunDecisionsAsync(IdempotencyMode.Off);

        (IReadOnlyList<IdempotencyResponse> observe, _) = await RunDecisionsAsync(IdempotencyMode.Observe);

        observe.Select(Wire).Should().Equal(off.Select(Wire));
        observe.Should().OnlyContain(response => !response.Replayed && response.Header("Retry-After") == null);
        observe.Select(response => response.StatusCode).Should().Equal(201, 201, 201, 201, 201, 201, 200, 200);
    }

    [Fact]
    public async Task Observe_Should_Log_What_Enforce_Would_Have_Done()
    {
        (_, IReadOnlyList<CapturedLog> logs) = await RunDecisionsAsync(IdempotencyMode.Observe);

        logs.Select(entry => (entry.EventId.Id, entry.EventId.Name)).Should().Contain(
        [
            (7010, "Idempotency.Observe.WouldReplay"),
            (7012, "Idempotency.Observe.WouldRejectKeyReused"),
            (7016, "Idempotency.Observe.WouldRejectKeyMissing"),
            (7017, "Idempotency.Observe.WouldRejectKeyInvalid"),
            (7013, "Idempotency.Observe.WouldRejectScopeRequired"),
            (7011, "Idempotency.Observe.WouldRejectInFlight"),
        ]);
    }

    [Fact]
    public async Task Off_Should_Log_Nothing_About_Idempotency()
    {
        (_, IReadOnlyList<CapturedLog> logs) = await RunDecisionsAsync(IdempotencyMode.Off);

        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task Enforce_Should_Block_Every_Decision_Observe_Lets_Through()
    {
        (IReadOnlyList<IdempotencyResponse> enforce, _) = await RunDecisionsAsync(IdempotencyMode.Enforce);

        enforce.Select(response => response.StatusCode).Should().Equal(201, 201, 422, 400, 400, 400, 409, 200);
        enforce[1].Replayed.Should().BeTrue();
        enforce[6].Header("Retry-After").Should().Be("1");
    }

    [Fact]
    public async Task Off_Should_Neither_Resolve_The_Key_Nor_Call_The_Store()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options =>
        {
            options.Mode = IdempotencyMode.Off;
            options.RequireKey = true;
            options.ScopeResolver = new ThrowingScopeResolver();
            options.UseStore<UnusedIdempotencyStore>(ServiceLifetime.Singleton);
        });

        IdempotencyResponse first = await host.PostAsync("/orders");
        IdempotencyResponse duplicate = await host.PostAsync("/orders");
        IdempotencyResponse missing = await host.PostAsync("/orders", key: null);

        new[] { first, duplicate, missing }.Select(response => response.StatusCode).Should().Equal(201, 201, 201);
        host.Probe.Executions.Should().Be(3);
    }

    [Fact]
    public async Task A_Mode_Reload_Should_Replay_What_Observe_Stored_And_Reject_What_Is_In_Flight()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Mode"] = "Observe" })
            .Build();
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            configureServices: services => services.Configure<IdempotencyOptions>(configuration));

        IdempotencyResponse? observed = await SettleAsync(host, "/orders");
        IdempotencyResponse? observedAgain = await SettleAsync(host, "/orders");
        SetMode(configuration, IdempotencyMode.Enforce);
        IdempotencyResponse replayed = await host.PostAsync("/orders");

        observedAgain!.Replayed.Should().BeFalse();
        replayed.Replayed.Should().BeTrue();
        replayed.Body.Should().Be(observed!.Body);
        host.Probe.Executions.Should().Be(2);

        SetMode(configuration, IdempotencyMode.Observe);
        Task<IdempotencyResponse> slow = host.PostAsync("/slow", key: "slow-key");
        await host.Probe.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        SetMode(configuration, IdempotencyMode.Enforce);
        IdempotencyResponse inFlight = await host.PostAsync("/slow", key: "slow-key");
        host.Probe.Gate.TrySetResult();

        inFlight.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        (await slow).StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Theory]
    [InlineData(IdempotencyMode.Off, "off")]
    [InlineData(IdempotencyMode.Observe, "observe")]
    [InlineData(IdempotencyMode.Enforce, "enforce")]
    public async Task UseIdempotency_Should_Count_Each_Decision_With_Its_Mode_And_Route(IdempotencyMode mode, string modeTag)
    {
        string route = $"/mode-metrics-{modeTag}";
        using IdempotencyDecisionListener listener = new(route);
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            options => options.Mode = mode,
            configureEndpoints: app => app.MapPost(route, (int status, IdempotencyProbe probe) =>
            {
                probe.Execute();
                return HttpResults.StatusCode(status);
            }).WithIdempotency(endpoint => endpoint.ShouldStore = outcome => outcome.StatusCode < 400));

        await SettleAsync(host, $"{route}?status=201", key: "m1");
        await SettleAsync(host, $"{route}?status=201", key: "m1");
        await SettleAsync(host, $"{route}?status=201", key: "m1", body: "other");
        await SettleAsync(host, $"{route}?status=201", key: "m2", user: null);
        await SettleAsync(host, $"{route}?status=400", key: "m3");

        string[] expected = mode == IdempotencyMode.Off
            ? []
            : ["reserved", "stored", "replay", "key_reused", "unscoped", "reserved", "released"];
        listener.Decisions.Select(decision => decision.Decision).Should().Equal(expected);
        listener.Decisions.Should().OnlyContain(decision =>
            decision.Mode == modeTag && decision.Source == "http" && decision.TagCount == 4);
    }

    [Fact]
    public async Task Observe_Should_Run_The_Request_Once_And_Warn_When_The_Scope_Resolver_Throws()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options =>
        {
            options.Mode = IdempotencyMode.Observe;
            options.ScopeResolver = new ThrowingScopeResolver();
        });

        IdempotencyResponse response = await host.PostAsync("/orders");

        response.StatusCode.Should().Be(StatusCodes.Status201Created);
        host.Probe.Executions.Should().Be(1);
        host.Logs.Named("Idempotency.Observe.ResolutionFailed").Should().ContainSingle()
            .Which.Should().Match<CapturedLog>(entry => entry.Level == LogLevel.Warning && entry.EventId.Id == 7014);
    }

    [Fact]
    public async Task Enforce_Should_Rethrow_When_The_Scope_Resolver_Throws()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options =>
            options.ScopeResolver = new ThrowingScopeResolver());

        IdempotencyResponse response = await host.PostAsync("/orders");

        response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        host.Probe.Executions.Should().Be(0);
        host.Logs.Named("Idempotency.Observe.ResolutionFailed").Should().BeEmpty();
    }

    [Fact]
    public async Task Idempotency_Logs_Should_Carry_A_Key_Hash_But_Never_The_Key_Or_The_Scope()
    {
        const string key = "raw-key-7f3a";
        const string throwingKey = "raw-key-2e8b";
        const string user = "raw-user-9c1d";
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.Mode = IdempotencyMode.Observe);

        await SettleAsync(host, "/orders", key, user: user);
        await SettleAsync(host, "/orders", key, user: user);
        await SettleAsync(host, "/orders", key, body: "other", user: user);
        await SettleAsync(host, "/throw", throwingKey, user: user);

        IReadOnlyList<CapturedLog> logs = [.. host.Logs.Entries.Where(entry => entry.EventId.Name?.StartsWith("Idempotency.", StringComparison.Ordinal) == true)];
        logs.Select(entry => entry.EventId.Id).Should().Contain([7010, 7012, 7003]);
        logs.Should().OnlyContain(entry => !entry.Message.Contains(key) && !entry.Message.Contains(throwingKey) && !entry.Message.Contains(user));
        logs.Where(entry => entry.EventId.Id is 7010 or 7012 or 7003)
            .Should().OnlyContain(entry => KeyHash().IsMatch(entry.Message));
    }

    [Fact]
    public async Task An_Endpoint_Mode_Should_Win_Over_Its_Profile_And_The_Global_Mode()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            options => options.AddProfile("observing", profile => profile.Mode = IdempotencyMode.Observe),
            configureEndpoints: app =>
            {
                app.MapPost("/mode-off", (IdempotencyProbe probe) => HttpResults.Ok(new { execution = probe.Execute() }))
                    .WithIdempotency(endpoint => endpoint.Mode = IdempotencyMode.Off);
                app.MapPost("/mode-profile", (IdempotencyProbe probe) => HttpResults.Ok(new { execution = probe.Execute() }))
                    .WithIdempotency("observing");
                app.MapPost("/mode-profile-enforced", (IdempotencyProbe probe) => HttpResults.Ok(new { execution = probe.Execute() }))
                    .WithIdempotency(endpoint =>
                    {
                        endpoint.Profile = "observing";
                        endpoint.Mode = IdempotencyMode.Enforce;
                    });
            });

        await SettleAsync(host, "/mode-off", "mode-off");
        IdempotencyResponse? offAgain = await SettleAsync(host, "/mode-off", "mode-off");
        await SettleAsync(host, "/mode-profile", "mode-profile");
        IdempotencyResponse? profileAgain = await SettleAsync(host, "/mode-profile", "mode-profile");
        await SettleAsync(host, "/mode-profile-enforced", "mode-profile-enforced");
        IdempotencyResponse? enforcedAgain = await SettleAsync(host, "/mode-profile-enforced", "mode-profile-enforced");

        offAgain!.Replayed.Should().BeFalse();
        profileAgain!.Replayed.Should().BeFalse();
        enforcedAgain!.Replayed.Should().BeTrue();
        host.Probe.Executions.Should().Be(5);
        host.Logs.Named("Idempotency.Observe.WouldReplay").Should().ContainSingle();
    }

    [Theory]
    [InlineData(IdempotencyMode.Observe, 0, 2)]
    [InlineData(IdempotencyMode.Enforce, 2, 0)]
    public async Task A_Transactional_Store_Should_Be_Used_Through_Its_Detached_View_Only_In_Observe(IdempotencyMode mode, int attached, int detached)
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options =>
        {
            options.Mode = mode;
            options.UseStore<TransactionalCountingIdempotencyStore>(ServiceLifetime.Singleton);
        });
        TransactionalCountingIdempotencyStore store = (TransactionalCountingIdempotencyStore)host.Services.GetRequiredService<IIdempotencyRecordStore>();

        await SettleAsync(host, "/orders");
        IdempotencyResponse? again = await SettleAsync(host, "/orders");

        again!.Replayed.Should().Be(mode == IdempotencyMode.Enforce);
        (store.AttachedReservations, store.DetachedReservations).Should().Be((attached, detached));
    }

    [Fact]
    public async Task An_Endpoint_In_Enforce_Should_Replay_When_The_Global_Mode_Is_Off()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            options => options.Mode = IdempotencyMode.Off,
            configureEndpoints: app => app.MapPost("/mode-enforce", (IdempotencyProbe probe) => HttpResults.Ok(new { execution = probe.Execute() }))
                .WithIdempotency(endpoint => endpoint.Mode = IdempotencyMode.Enforce));

        await SettleAsync(host, "/mode-enforce");
        IdempotencyResponse? again = await SettleAsync(host, "/mode-enforce");

        again!.Replayed.Should().BeTrue();
        host.Probe.Executions.Should().Be(1);
    }

    private static async Task<IdempotencyResponse?> SettleAsync(
        IdempotencyTestHost host,
        string path,
        string? key = "key-1",
        string body = """{"item":"book"}""",
        string? user = "alice")
    {
        using HttpRequestMessage request = IdempotencyTestHost.CreateRequest(path, key, body, user);
        return await host.SendSettledAsync(request);
    }

    /// <summary>
    /// Sends one request per decision (stored, replay, key reused, key missing, key invalid, scope required, and an
    /// in-flight pair on <c>/slow</c>) to a host in <paramref name="mode"/>, with keys required and unscoped keys rejected.
    /// </summary>
    private static async Task<(IReadOnlyList<IdempotencyResponse> Responses, IReadOnlyList<CapturedLog> Logs)> RunDecisionsAsync(IdempotencyMode mode)
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options =>
        {
            options.Mode = mode;
            options.RequireKey = true;
            options.UnscopedKeyBehavior = UnscopedKeyBehavior.Reject;
        });

        List<IdempotencyResponse> responses = [];
        foreach (HttpRequestMessage request in new[]
                 {
                     IdempotencyTestHost.CreateRequest("/orders", "k1"),
                     IdempotencyTestHost.CreateRequest("/orders", "k1"),
                     IdempotencyTestHost.CreateRequest("/orders", "k1", body: """{"item":"pen"}"""),
                     IdempotencyTestHost.CreateRequest("/orders", key: null),
                     IdempotencyTestHost.CreateRequest("/orders", "bad key"),
                     IdempotencyTestHost.CreateRequest("/orders", "k2", user: null),
                 })
        {
            using (request)
                responses.Add((await host.SendSettledAsync(request))!);
        }

        int executions = host.Probe.Executions;
        Task<IdempotencyResponse> first = host.PostAsync("/slow", "k3");
        await host.Probe.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Task<IdempotencyResponse> second = host.PostAsync("/slow", "k3");
        await Task.WhenAny(second, WaitForExecutionsAsync(host.Probe, executions + 2)).WaitAsync(TimeSpan.FromSeconds(10));
        host.Probe.Gate.TrySetResult();
        responses.Add(await second);
        responses.Add(await first);

        IReadOnlyList<CapturedLog> logs = [.. host.Logs.Entries.Where(entry => entry.EventId.Name?.StartsWith("Idempotency.", StringComparison.Ordinal) == true)];
        return (responses, logs);
    }

    private static async Task WaitForExecutionsAsync(IdempotencyProbe probe, int count)
    {
        while (probe.Executions < count)
            await Task.Delay(10);
    }

    private static void SetMode(IConfigurationRoot configuration, IdempotencyMode mode)
    {
        configuration["Mode"] = mode.ToString();
        configuration.Reload();
    }

    /// <summary>Everything a client sees, except the <c>Date</c> header.</summary>
    private static string Wire(IdempotencyResponse response) =>
        $"{response.StatusCode}|{string.Join(";", response.Headers.Where(header => !header.Key.Equals("Date", StringComparison.OrdinalIgnoreCase)).OrderBy(header => header.Key, StringComparer.OrdinalIgnoreCase).Select(header => $"{header.Key}={string.Join(",", header.Value)}"))}|{response.Body}";

    [GeneratedRegex("key hash [0-9a-f]{16}")]
    private static partial Regex KeyHash();
}
