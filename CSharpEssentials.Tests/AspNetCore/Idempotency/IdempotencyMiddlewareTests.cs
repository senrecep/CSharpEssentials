using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

public sealed class IdempotencyMiddlewareTests
{
    [Fact]
    public async Task Retry_Should_Replay_The_Stored_Response_Without_Executing_Again()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        IdempotencyResponse first = await host.PostAsync("/orders");
        IdempotencyResponse second = await host.PostAsync("/orders");

        host.Probe.Executions.Should().Be(1);
        first.StatusCode.Should().Be(StatusCodes.Status201Created);
        first.Replayed.Should().BeFalse();
        second.Replayed.Should().BeTrue();
        second.StatusCode.Should().Be(first.StatusCode);
        second.Body.Should().Be(first.Body);
        second.Header("Location").Should().Be("/orders/1");
        second.Header("Content-Type").Should().Be(first.Header("Content-Type"));
        second.Header("Content-Length").Should().Be(first.Body.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task Handler_Should_Read_The_Body_After_Fingerprinting()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        IdempotencyResponse response = await host.PostAsync("/orders", body: """{"item":"pen"}""");

        JsonNode.Parse(response.Body)!["body"]!.GetValue<string>().Should().Be("""{"item":"pen"}""");
    }

    [Fact]
    public async Task Replay_Should_Not_Include_Headers_Outside_The_Allowlist()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.ReplayedHeaders.Add("Set-Cookie"));

        IdempotencyResponse first = await host.PostAsync("/orders");
        IdempotencyResponse second = await host.PostAsync("/orders");

        first.Header("Set-Cookie").Should().NotBeNull();
        second.Header("Set-Cookie").Should().BeNull("cookies are never replayed, even when allowlisted");
        second.Header("X-Not-Replayed").Should().BeNull();
    }

    [Fact]
    public async Task Same_Key_With_Different_Body_Should_Return_422()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync("/orders");
        IdempotencyResponse second = await host.PostAsync("/orders", body: """{"item":"pen"}""");

        second.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        second.Header("Content-Type").Should().StartWith("application/problem+json");
        JsonNode.Parse(second.Body)!["title"]!.GetValue<string>().Should().Be("Idempotency key reused");
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Same_Key_On_Different_Path_Should_Return_422()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync("/orders");
        IdempotencyResponse second = await host.PostAsync("/group/items");

        second.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    public async Task Same_Key_With_Different_Query_Should_Return_422()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync("/fail?status=200");
        IdempotencyResponse second = await host.PostAsync("/fail?status=201");

        second.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    public async Task Concurrent_Request_Should_Return_409_With_Retry_After()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.RetryAfter = TimeSpan.FromMilliseconds(1500));

        Task<IdempotencyResponse> first = host.PostAsync("/slow");
        await host.Probe.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        IdempotencyResponse second = await host.PostAsync("/slow");
        host.Probe.Gate.SetResult();
        IdempotencyResponse completed = await first;
        IdempotencyResponse third = await host.PostAsync("/slow");

        second.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        second.Header("Retry-After").Should().Be("2");
        JsonNode.Parse(second.Body)!["title"]!.GetValue<string>().Should().Be("Request in progress");
        completed.StatusCode.Should().Be(StatusCodes.Status200OK);
        third.Replayed.Should().BeTrue();
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Concurrent_Request_Without_RetryAfter_Should_Omit_The_Header()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.RetryAfter = null);

        Task<IdempotencyResponse> first = host.PostAsync("/slow");
        await host.Probe.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        IdempotencyResponse second = await host.PostAsync("/slow");
        host.Probe.Gate.SetResult();
        await first;

        second.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        second.Header("Retry-After").Should().BeNull();
    }

    [Theory]
    [InlineData(400)]
    [InlineData(409)]
    [InlineData(500)]
    public async Task Non_Success_Response_Should_Release_The_Key(int status)
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        IdempotencyResponse first = await host.PostAsync($"/fail?status={status}");
        IdempotencyResponse second = await host.PostAsync($"/fail?status={status}");

        first.StatusCode.Should().Be(status);
        second.StatusCode.Should().Be(status);
        second.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(2);
    }

    [Fact]
    public async Task Released_Key_Should_Accept_A_Different_Request()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync("/fail?status=400");
        IdempotencyResponse second = await host.PostAsync("/fail?status=201");

        second.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task Exception_Should_Release_The_Key()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        IdempotencyResponse first = await host.PostAsync("/throw");
        IdempotencyResponse second = await host.PostAsync("/throw");

        first.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        second.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        host.Probe.Executions.Should().Be(2);
    }

    [Fact]
    public async Task Custom_ShouldStore_Should_Decide_What_Is_Replayed()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.ShouldStore = status => status < 500);

        await host.PostAsync("/fail?status=409");
        IdempotencyResponse second = await host.PostAsync("/fail?status=409");

        second.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        second.Replayed.Should().BeTrue();
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Stored_Response_Should_Expire_After_Retention()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.RetentionPeriod = TimeSpan.FromMinutes(10));

        await host.PostAsync("/orders");
        host.Time.Advance(TimeSpan.FromMinutes(9));
        IdempotencyResponse beforeExpiry = await host.PostAsync("/orders");
        host.Time.Advance(TimeSpan.FromMinutes(2));
        IdempotencyResponse afterExpiry = await host.PostAsync("/orders");

        beforeExpiry.Replayed.Should().BeTrue();
        afterExpiry.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(2);
    }

    [Fact]
    public async Task Request_Without_Key_Should_Pass_Through()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync("/orders", key: null);
        IdempotencyResponse second = await host.PostAsync("/orders", key: null);

        second.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(2);
    }

    [Fact]
    public async Task RequireKey_Should_Reject_Request_Without_Key()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.RequireKey = true);

        IdempotencyResponse response = await host.PostAsync("/orders", key: null);

        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        JsonNode.Parse(response.Body)!["title"]!.GetValue<string>().Should().Be("Missing idempotency key");
        host.Probe.Executions.Should().Be(0);
    }

    [Fact]
    public async Task Too_Long_Key_Should_Return_400()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.MaxKeyLength = 8);

        IdempotencyResponse response = await host.PostAsync("/orders", key: "123456789");

        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        JsonNode.Parse(response.Body)!["title"]!.GetValue<string>().Should().Be("Invalid idempotency key");
        host.Probe.Executions.Should().Be(0);
    }

    [Fact]
    public async Task Empty_Key_Should_Return_400()
    {
        // HttpClient drops empty header values, so the empty key is set on the server side.
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(configurePipeline: app => app.Use((context, next) =>
        {
            context.Request.Headers["Idempotency-Key"] = "";
            return next(context);
        }));

        IdempotencyResponse response = await host.PostAsync("/orders", key: null);

        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Multiple_Key_Headers_Should_Return_400()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();
        using HttpRequestMessage request = IdempotencyTestHost.CreateRequest("/orders", key: null);
        request.Headers.TryAddWithoutValidation("Idempotency-Key", ["a", "b"]);

        using HttpResponseMessage response = await host.Client.SendAsync(request);

        ((int)response.StatusCode).Should().Be(StatusCodes.Status400BadRequest);
        host.Probe.Executions.Should().Be(0);
    }

    [Fact]
    public async Task Custom_Header_Names_Should_Be_Used()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options =>
        {
            options.HeaderName = "X-Request-Id";
            options.ReplayedHeaderName = "X-Replayed";
        });
        using HttpRequestMessage first = IdempotencyTestHost.CreateRequest("/orders", key: null);
        first.Headers.TryAddWithoutValidation("X-Request-Id", "abc");
        using HttpRequestMessage second = IdempotencyTestHost.CreateRequest("/orders", key: null);
        second.Headers.TryAddWithoutValidation("X-Request-Id", "abc");

        (await host.Client.SendAsync(first)).Dispose();
        using HttpResponseMessage replay = await host.Client.SendAsync(second);

        replay.Headers.GetValues("X-Replayed").Should().Equal("true");
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Anonymous_Request_Should_Pass_Through_By_Default()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync("/orders", user: null);
        IdempotencyResponse second = await host.PostAsync("/orders", user: null);

        second.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(2);
    }

    [Fact]
    public async Task AllowUnscopedKeys_Should_Replay_Anonymous_Requests()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.AllowUnscopedKeys = true);

        await host.PostAsync("/orders", user: null);
        IdempotencyResponse second = await host.PostAsync("/orders", user: null);

        second.Replayed.Should().BeTrue();
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Same_Key_From_Different_Users_Should_Not_Share_Responses()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        IdempotencyResponse alice = await host.PostAsync("/orders", user: "alice");
        IdempotencyResponse bob = await host.PostAsync("/orders", user: "bob", body: """{"item":"pen"}""");

        bob.Replayed.Should().BeFalse();
        bob.StatusCode.Should().Be(StatusCodes.Status201Created);
        bob.Body.Should().NotBe(alice.Body);
        host.Probe.Executions.Should().Be(2);
    }

    [Fact]
    public async Task Custom_KeyScope_Should_Be_Used()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options =>
            options.KeyScope = context => context.Request.Headers["X-Tenant"].ToString() is { Length: > 0 } tenant ? tenant : null);
        using HttpRequestMessage first = IdempotencyTestHost.CreateRequest("/orders", user: null);
        first.Headers.TryAddWithoutValidation("X-Tenant", "t1");
        using HttpRequestMessage second = IdempotencyTestHost.CreateRequest("/orders", user: null);
        second.Headers.TryAddWithoutValidation("X-Tenant", "t1");

        (await host.Client.SendAsync(first)).Dispose();
        using HttpResponseMessage replay = await host.Client.SendAsync(second);

        replay.Headers.Contains("Idempotency-Replayed").Should().BeTrue();
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Method_Outside_Methods_Should_Pass_Through()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync("/orders", method: HttpMethod.Put);
        IdempotencyResponse second = await host.PostAsync("/orders", method: HttpMethod.Put);

        second.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(2);
    }

    [Fact]
    public async Task Configured_Method_Should_Participate()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.Methods.Add("PUT"));

        await host.PostAsync("/orders", method: HttpMethod.Put);
        IdempotencyResponse second = await host.PostAsync("/orders", method: HttpMethod.Put);

        second.Replayed.Should().BeTrue();
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Endpoint_Without_Metadata_Should_Pass_Through()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync("/plain");
        IdempotencyResponse second = await host.PostAsync("/plain");

        second.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(2);
    }

    [Fact]
    public async Task Route_Group_Metadata_Should_Apply_To_Its_Endpoints()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync("/group/items");
        IdempotencyResponse second = await host.PostAsync("/group/items");

        second.Replayed.Should().BeTrue();
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Mvc_Action_With_Attribute_Should_Be_Replayed()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        IdempotencyResponse first = await host.PostAsync("/mvc/orders");
        IdempotencyResponse second = await host.PostAsync("/mvc/orders");

        first.StatusCode.Should().Be(StatusCodes.Status201Created);
        second.Replayed.Should().BeTrue();
        second.Body.Should().Be(first.Body);
        second.Header("Location").Should().Be("/mvc/orders/1");
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Response_Larger_Than_Limit_Should_Be_Sent_But_Not_Stored()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.MaxResponseBodySize = 1024);

        IdempotencyResponse first = await host.PostAsync("/large");
        IdempotencyResponse second = await host.PostAsync("/large");

        first.Body.Should().HaveLength(4096);
        second.Body.Should().HaveLength(4096);
        second.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(2);
    }

    [Fact]
    public async Task Distributed_Cache_Store_Should_Replay()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            options => options.UseDistributedCacheStore(),
            services => services.AddDistributedMemoryCache());

        await host.PostAsync("/orders");
        IdempotencyResponse second = await host.PostAsync("/orders");

        second.Replayed.Should().BeTrue();
        second.Header("Location").Should().Be("/orders/1");
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Failure_After_The_Response_Started_Should_Keep_The_Key_In_Flight()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        Func<Task> first = async () => await host.PostAsync("/abort");
        await first.Should().ThrowAsync<Exception>();
        IdempotencyResponse retry = await host.PostAsync("/abort");

        retry.StatusCode.Should().Be(StatusCodes.Status409Conflict, "the work may be done, so a retry must not run it again");
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Response_Written_Through_BodyWriter_Should_Be_Replayed()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        IdempotencyResponse first = await host.PostAsync("/pipe");
        IdempotencyResponse second = await host.PostAsync("/pipe");

        first.Body.Should().Be("piped");
        second.Body.Should().Be("piped");
        second.Replayed.Should().BeTrue();
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task Large_Request_Body_Should_Reach_The_Handler_And_Be_Fingerprinted()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();
        string body = new('a', 200_000);

        IdempotencyResponse first = await host.PostAsync("/echo-length", body: body);
        IdempotencyResponse second = await host.PostAsync("/echo-length", body: body);
        IdempotencyResponse changed = await host.PostAsync("/echo-length", body: body[..^1] + "b");

        JsonNode.Parse(first.Body)!["length"]!.GetValue<int>().Should().Be(200_000);
        second.Replayed.Should().BeTrue();
        changed.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        host.Probe.Executions.Should().Be(1);
    }

    [Fact]
    public async Task No_Content_Replay_Should_Not_Send_A_Body_Or_Content_Length()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync("/no-content");
        IdempotencyResponse second = await host.PostAsync("/no-content");

        second.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        second.Replayed.Should().BeTrue();
        second.Body.Should().BeEmpty();
        second.Header("Content-Length").Should().BeNull();
    }

    [Fact]
    public async Task Store_Failure_After_Success_Should_Not_Break_The_Response()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            options => options.UseStore<ThrowingIdempotencyStore>(ServiceLifetime.Singleton));

        IdempotencyResponse response = await host.PostAsync("/orders");
        IdempotencyResponse failed = await host.PostAsync("/fail?status=400", key: "key-2");

        response.StatusCode.Should().Be(StatusCodes.Status201Created);
        JsonNode.Parse(response.Body)!["execution"]!.GetValue<int>().Should().Be(1);
        failed.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Store_Failure_On_Release_After_Exception_Should_Surface_The_Original_Exception()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            options => options.UseStore<ThrowingIdempotencyStore>(ServiceLifetime.Singleton));

        IdempotencyResponse response = await host.PostAsync("/throw");

        response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task Unscoped_Key_Should_Not_Reach_A_Scoped_Entry()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.AllowUnscopedKeys = true);

        await host.PostAsync("/orders", key: "order-42", user: "alice");
        IdempotencyResponse forged = await host.PostAsync("/orders", key: "5:alice:order-42", user: null);

        forged.StatusCode.Should().Be(StatusCodes.Status201Created);
        forged.Replayed.Should().BeFalse();
        host.Probe.Executions.Should().Be(2);
    }

    [Fact]
    public async Task Replay_Should_Be_Compressed_Again_When_Response_Compression_Runs_Outside()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            configureServices: services => services.AddResponseCompression(options => options.EnableForHttps = true),
            configurePipeline: app => app.UseResponseCompression());

        (string firstEncoding, string firstBody) = await SendGzipAsync(host);
        (string secondEncoding, string secondBody) = await SendGzipAsync(host);

        host.Probe.Executions.Should().Be(1);
        firstEncoding.Should().Be("gzip");
        secondEncoding.Should().Be("gzip");
        secondBody.Should().Be(firstBody);
        JsonNode.Parse(secondBody).Should().NotBeNull();
    }

    private static async Task<(string Encoding, string Body)> SendGzipAsync(IdempotencyTestHost host)
    {
        using HttpRequestMessage request = IdempotencyTestHost.CreateRequest("/orders");
        request.Headers.AcceptEncoding.ParseAdd("gzip");
        using HttpResponseMessage response = await host.Client.SendAsync(request);
        await using Stream compressed = await response.Content.ReadAsStreamAsync();
        await using var gzip = new System.IO.Compression.GZipStream(compressed, System.IO.Compression.CompressionMode.Decompress);
        using var reader = new StreamReader(gzip);
        return (string.Join(",", response.Content.Headers.ContentEncoding), await reader.ReadToEndAsync());
    }
}
