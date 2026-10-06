using System.Net;
using System.Net.Http.Json;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Extensions;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.AspNetCore;

public class ValidationEndpointFilterTests
{
    public sealed record CreateItem(string? Name, int Quantity);

    private sealed class NameValidator : Validator<CreateItem>
    {
        protected override ValueTask Configure(CreateItem model, RuleContext<CreateItem> rules, CancellationToken ct = default)
        {
            rules.For(() => model.Name).NotEmpty();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class QuantityValidator : Validator<CreateItem>
    {
        public override int Order => 1;

        protected override ValueTask Configure(CreateItem model, RuleContext<CreateItem> rules, CancellationToken ct = default)
        {
            rules.For(() => model.Quantity).GreaterThan(0);
            return ValueTask.CompletedTask;
        }
    }

    [Fact]
    public async Task WithValidation_Should_Call_Handler_When_Request_Is_Valid()
    {
        await using WebApplication app = await StartAsync(registerValidators: true);
        using HttpClient client = app.GetTestClient();

        using HttpResponseMessage response = await client.PostAsJsonAsync(new Uri("/items", UriKind.Relative), new CreateItem("pen", 2));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("pen:2");
    }

    [Fact]
    public async Task WithValidation_Should_Return_Problem_With_All_Errors_When_Request_Is_Invalid()
    {
        await using WebApplication app = await StartAsync(registerValidators: true);
        using HttpClient client = app.GetTestClient();

        using HttpResponseMessage response = await client.PostAsJsonAsync(new Uri("/items", UriKind.Relative), new CreateItem("", 0));
        string body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        body.Should().Contain("Name").And.Contain("Quantity").And.NotContain("pen");
    }

    [Fact]
    public async Task WithValidation_Should_Call_Handler_When_Optional_Argument_Is_Null()
    {
        await using WebApplication app = await StartAsync(registerValidators: true);
        using HttpClient client = app.GetTestClient();

        using HttpResponseMessage response = await client.PostAsync(new Uri("/optional", UriKind.Relative), null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be("none");
    }

    [Fact]
    public async Task WithValidation_Should_Throw_When_No_Validator_Is_Registered()
    {
        await using WebApplication app = await StartAsync(registerValidators: false);
        using HttpClient client = app.GetTestClient();

        Func<Task> act = () => client.PostAsJsonAsync(new Uri("/items", UriKind.Relative), new CreateItem("pen", 2));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("No IValidator<*CreateItem> is registered*");
    }

    [Fact]
    public async Task WithValidation_Should_Throw_When_Handler_Has_No_Argument_Of_Type()
    {
        await using WebApplication app = CreateApp(registerValidators: true);
        app.MapGet("/plain", () => "plain").WithValidation<CreateItem>();

        Action act = () => _ = ((IEndpointRouteBuilder)app).DataSources.SelectMany(static source => source.Endpoints).ToArray();

        act.Should().Throw<InvalidOperationException>().WithMessage("WithValidation<*CreateItem>() was applied to handler*no parameter of that type.");
    }

    private static async Task<WebApplication> StartAsync(bool registerValidators)
    {
        WebApplication app = CreateApp(registerValidators);
        app.MapPost("/items", (CreateItem item) => $"{item.Name}:{item.Quantity}").WithValidation<CreateItem>();
        app.MapPost("/optional", (CreateItem? item) => item?.Name ?? "none").WithValidation<CreateItem>();
        await app.StartAsync();
        return app;
    }

    private static WebApplication CreateApp(bool registerValidators)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        if (registerValidators)
        {
            builder.Services.AddValidator<CreateItem, QuantityValidator>();
            builder.Services.AddValidator<CreateItem, NameValidator>();
        }

        return builder.Build();
    }
}
