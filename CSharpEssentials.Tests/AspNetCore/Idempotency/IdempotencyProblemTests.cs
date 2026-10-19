using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Idempotency;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.Idempotency;

public sealed class IdempotencyProblemTests
{
    [Theory]
    [InlineData("\"key-1\"", "key-1")]
    [InlineData("key-1", "\"key-1\"")]
    public async Task Quoted_And_Bare_Key_Should_Hit_The_Same_Record(string first, string second)
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        IdempotencyResponse original = await host.PostAsync("/orders", key: first);
        IdempotencyResponse retry = await host.PostAsync("/orders", key: second);

        original.StatusCode.Should().Be(StatusCodes.Status201Created);
        retry.Replayed.Should().BeTrue();
        retry.Body.Should().Be(original.Body);
        host.Probe.Executions.Should().Be(1);
    }

    [Theory]
    [InlineData("\"abc")]
    [InlineData("abc\"")]
    [InlineData("\"")]
    [InlineData("\"\"")]
    [InlineData("\"ab\"c\"")]
    [InlineData("\"abc\"x")]
    [InlineData("\"ab\\\"c\"")]
    [InlineData("\"ab\\\\c\"")]
    [InlineData("\"ab\\c\"")]
    [InlineData("\"abc\\\"")]
    public async Task Malformed_Quoting_Should_Return_400_KeyInvalid(string key)
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        IdempotencyResponse response = await host.PostAsync("/orders", key: key);

        AssertProblem(response, StatusCodes.Status400BadRequest, IdempotencyErrorCodes.KeyInvalid, "Invalid idempotency key");
        host.Probe.Executions.Should().Be(0);
    }

    [Theory]
    [InlineData("key 1")]
    [InlineData("key\t1")]
    [InlineData("anahtar-ğ")]
    [InlineData("a\\b")]
    [InlineData("\"key 1\"")]
    public async Task Key_With_Invalid_Characters_Should_Return_400_KeyInvalid(string key)
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(configurePipeline: app => app.Use((context, next) =>
        {
            context.Request.Headers["Idempotency-Key"] = key;
            return next(context);
        }));

        IdempotencyResponse response = await host.PostAsync("/orders", key: null);

        AssertProblem(response, StatusCodes.Status400BadRequest, IdempotencyErrorCodes.KeyInvalid, "Invalid idempotency key");
        host.Probe.Executions.Should().Be(0);
    }

    [Fact]
    public async Task MaxKeyLength_Should_Apply_To_The_Unquoted_Key()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.MaxKeyLength = 5);

        IdempotencyResponse quoted = await host.PostAsync("/orders", key: "\"12345\"");
        IdempotencyResponse tooLong = await host.PostAsync("/orders", key: "\"123456\"");

        quoted.StatusCode.Should().Be(StatusCodes.Status201Created);
        AssertProblem(tooLong, StatusCodes.Status400BadRequest, IdempotencyErrorCodes.KeyInvalid, "Invalid idempotency key");
    }

    [Fact]
    public async Task Multiple_Key_Headers_Should_Return_KeyInvalid()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();
        using HttpRequestMessage request = IdempotencyTestHost.CreateRequest("/orders", key: null);
        request.Headers.TryAddWithoutValidation("Idempotency-Key", ["a", "a"]);

        using HttpResponseMessage message = await host.Client.SendAsync(request);
        IdempotencyResponse response = await IdempotencyResponse.ReadAsync(message);

        AssertProblem(response, StatusCodes.Status400BadRequest, IdempotencyErrorCodes.KeyInvalid, "Invalid idempotency key");
    }

    [Fact]
    public async Task Missing_Key_Should_Return_KeyMissing()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(options => options.RequireKey = true);

        IdempotencyResponse response = await host.PostAsync("/orders", key: null);

        AssertProblem(response, StatusCodes.Status400BadRequest, IdempotencyErrorCodes.KeyMissing, "Missing idempotency key");
    }

    [Fact]
    public async Task Reused_Key_Should_Return_KeyReused()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        await host.PostAsync("/orders", body: """{"item":"book"}""");
        IdempotencyResponse response = await host.PostAsync("/orders", body: """{"item":"pen"}""");

        AssertProblem(response, StatusCodes.Status422UnprocessableEntity, IdempotencyErrorCodes.KeyReused, "Idempotency key reused");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InFlight_Should_Return_Code_And_Retry_After(bool enhancedProblemDetails)
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            configureServices: services => AddProblemDetails(services, enhancedProblemDetails));

        Task<IdempotencyResponse> first = host.PostAsync("/slow");
        await host.Probe.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        IdempotencyResponse response = await host.PostAsync("/slow");
        host.Probe.Gate.SetResult();
        await first;

        AssertProblem(response, StatusCodes.Status409Conflict, IdempotencyErrorCodes.InFlight, "Request in progress");
        response.Header("Retry-After").Should().Be("1");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Mvc_Problems_Should_Carry_The_Code(bool enhancedProblemDetails)
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync(
            options => options.RequireKey = true,
            configureServices: services => AddProblemDetails(services, enhancedProblemDetails));

        await host.PostAsync("/mvc/orders", body: """{"item":"book"}""");
        IdempotencyResponse reused = await host.PostAsync("/mvc/orders", body: """{"item":"pen"}""");
        IdempotencyResponse missing = await host.PostAsync("/mvc/orders", key: null);

        AssertProblem(reused, StatusCodes.Status422UnprocessableEntity, IdempotencyErrorCodes.KeyReused, "Idempotency key reused");
        AssertProblem(missing, StatusCodes.Status400BadRequest, IdempotencyErrorCodes.KeyMissing, "Missing idempotency key");
    }

    [Fact]
    public async Task Business_409_Should_Not_Carry_An_Idempotency_Code()
    {
        await using IdempotencyTestHost host = await IdempotencyTestHost.StartAsync();

        IdempotencyResponse response = await host.PostAsync("/conflict");

        response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        JsonObject body = JsonNode.Parse(response.Body)!.AsObject();
        body["title"]!.GetValue<string>().Should().Be("Order already shipped");
        body.ContainsKey("code").Should().BeFalse();
        response.Header("Retry-After").Should().BeNull();
    }

    private static void AddProblemDetails(IServiceCollection services, bool enhanced)
    {
        if (enhanced)
            services.AddEnhancedProblemDetails();
    }

    private static void AssertProblem(IdempotencyResponse response, int status, string code, string title)
    {
        response.StatusCode.Should().Be(status);
        response.Header("Content-Type").Should().StartWith("application/problem+json");
        JsonObject body = JsonNode.Parse(response.Body)!.AsObject();
        body["code"]!.GetValue<string>().Should().Be(code);
        body["title"]!.GetValue<string>().Should().Be(title);
        body["status"]!.GetValue<int>().Should().Be(status);
        body["errorCodes"]!.AsArray().Select(node => node!.GetValue<string>()).Should().Equal(code);
        string? type = body["type"]?.GetValue<string>();
        (type is null || !type.Contains("idempotency", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }
}
