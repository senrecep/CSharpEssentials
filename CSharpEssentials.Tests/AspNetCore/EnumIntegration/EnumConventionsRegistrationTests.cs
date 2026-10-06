using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore.EnumIntegration;

/// <summary>
/// <c>AddEnumConventions()</c> registration: required by <c>UseEnumBinding()</c> and <c>WithEnumWireFormat</c>, plain enums left
/// to the framework, the reflection opt-in and request body errors.
/// </summary>
public class EnumConventionsRegistrationTests
{
    public static TheoryData<string> PlainRequests => new()
    {
        "/min/plain?plain=InProgress",
        "/min/plain?plain=1",
        "/min/plain?plain=inprogress",
        "/min/plain?plain=in_progress",
        "/min/plain?plain=garbage",
        "/min/plain-order",
        "/mvc/plain?plain=InProgress",
        "/mvc/plain?plain=1",
        "/mvc/plain?plain=in_progress",
        "/mvc/plain?plain=garbage",
        "/mvc/plain-order",
    };

    [Fact]
    public void UseEnumBinding_Should_Throw_When_EnumConventionsAreNotRegistered()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        using WebApplication app = builder.Build();

        Action act = () => app.UseEnumBinding();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("UseEnumBinding requires the enum conventions. Call services.AddEnumConventions() when registering services.");
    }

    [Fact]
    public async Task WithEnumWireFormat_Should_Throw_When_EnumConventionsAreNotRegistered()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartAsync(
            register: _ => { },
            map: app => app.MapGet("/order", () => EcEcho.Order).WithEnumWireFormat(EnumWireFormat.Number),
            useEnumBinding: false);

        EcResponse response = await host.GetAsync("/order");

        response.Status.Should().Be(500);
    }

    [Fact]
    public void AddEnumConventions_Should_Throw_When_CallbackReturnsNull()
    {
        ServiceCollection services = new();

        Action act = () => services.AddEnumConventions(_ => null!);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddEnumConventions_Should_ReplaceConventions_When_CalledTwice()
    {
        ServiceCollection services = new();
        services.AddEnumConventions();
        services.AddEnumConventions(EnumConventionsHost.WriteAs(EnumWireFormat.Number));

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<EnumConventions>().WriteAs.Should().Be(EnumWireFormat.Number);
    }

    [Theory]
    [MemberData(nameof(PlainRequests))]
    public async Task PlainEnum_Should_MatchStockBindingAndOutput_When_WriteAsIsNumber(string path)
    {
        await using EnumConventionsHost stock = await EnumConventionsHost.StartMatrixAsync(_ => { }, useEnumBinding: false);
        await using EnumConventionsHost conventions = await EnumConventionsHost.StartMatrixAsync(
            s => s.AddEnumConventions(EnumConventionsHost.WriteAs(EnumWireFormat.Number)));

        EcResponse expected = await stock.GetAsync(path);
        EcResponse actual = await conventions.GetAsync(path);

        (actual.Status, WithoutTraceId(actual.Body)).Should().Be((expected.Status, WithoutTraceId(expected.Body)));
    }

    [Theory]
    [MemberData(nameof(PlainRequests))]
    public async Task PlainEnum_Should_MatchStockBindingAndOutput_When_WriteAsIsString(string path)
    {
        await using EnumConventionsHost stock = await EnumConventionsHost.StartMatrixAsync(_ => { }, useEnumBinding: false);
        await using EnumConventionsHost conventions = await EnumConventionsHost.StartMatrixAsync();

        EcResponse expected = await stock.GetAsync(path);
        EcResponse actual = await conventions.GetAsync(path);

        (actual.Status, WithoutTraceId(actual.Body)).Should().Be((expected.Status, WithoutTraceId(expected.Body)));
    }

    [Fact]
    public void CanHandle_Should_ReturnFalse_When_EnumHasNoMetadata()
    {
        bool canHandle = EnumConventions.Default.CanHandle(typeof(EcPlain));

        canHandle.Should().BeFalse();
    }

    [Theory]
    [InlineData("/min/plain?plain=in_progress", 200, "InProgress")]
    [InlineData("/mvc/plain?plain=IN_PROGRESS", 200, "InProgress")]
    [InlineData("/min/plain?plain=garbage", 400, "")]
    [InlineData("/mvc/plain?plain=garbage", 400, "")]
    public async Task ReflectionOptIn_Should_BindPlainEnum_When_CanHandleSelectsIt(string path, int status, string body)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync(
            s => s.AddEnumConventionsWithReflection(c => c with { CanHandle = t => t == typeof(EcPlain) }));

        EcResponse response = await host.GetAsync(path);

        if (status == 400)
            response.ShouldBeProblem().Should().ContainSingle().Which.Description.Should().Be(
                "'garbage' is not a valid EcPlain. Allowed values: active, in_progress.");
        else
            (response.Status, response.Body).Should().Be((status, body));
    }

    [Theory]
    [InlineData("/min/plain-order")]
    [InlineData("/mvc/plain-order")]
    public async Task ReflectionOptIn_Should_WritePlainEnumAsString_When_CanHandleSelectsIt(string path)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync(
            s => s.AddEnumConventionsWithReflection(c => c with { CanHandle = t => t == typeof(EcPlain) }));

        EcResponse response = await host.GetAsync(path);

        response.Json("plain").Should().Be("\"in_progress\"");
    }

    [Theory]
    [InlineData("""{"status":"PENDING_APPROVAL"}""", 200, "PendingApproval")]
    [InlineData("""{"status":"pending_approval"}""", 200, "PendingApproval")]
    public async Task JsonBody_Should_BindWithSameRules_When_ValueIsAccepted(string json, int status, string body)
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse minimal = await host.PostJsonAsync("/min/body", json);
        EcResponse mvc = await host.PostJsonAsync("/mvc/body", json);

        (minimal.Status, minimal.Body, mvc.Status, mvc.Body).Should().Be((status, body, status, body));
    }

    [Fact]
    public async Task MinimalApiJsonBody_Should_Return400ListingAllowedValues_When_ValueIsInvalid()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await host.PostJsonAsync("/min/body", """{"status":"99"}""");

        response.ShouldBeProblem().Should().ContainSingle().Which.Should().Be(
            ("status", "'99' is not a valid EcStatus. Allowed values: pending, pending_approval, shipped."));
    }

    [Fact]
    public async Task MvcJsonBody_Should_Return400_When_ValueIsInvalid()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync();

        EcResponse response = await host.PostJsonAsync("/mvc/body", """{"status":"99"}""");

        response.Status.Should().Be(400);
    }

    [Fact]
    public async Task ConfigureErrors_Should_CreateBodyError_When_MinimalApiJsonBodyIsInvalid()
    {
        await using EnumConventionsHost host = await EnumConventionsHost.StartMatrixAsync(s => s.AddEnumConventions()
            .ConfigureErrors((error, key) => CSharpEssentials.Errors.Error.Validation(code: $"enum.{key}", description: error.Value ?? "")));

        EcResponse response = await host.PostJsonAsync("/min/body", """{"status":"garbage"}""");

        response.ShouldBeProblem().Should().ContainSingle().Which.Should().Be(("enum.status", "garbage"));
    }

    private static string WithoutTraceId(string body) =>
        System.Text.RegularExpressions.Regex.Replace(body, "\"traceId\":\"[^\"]*\"", "\"traceId\":\"\"");
}
