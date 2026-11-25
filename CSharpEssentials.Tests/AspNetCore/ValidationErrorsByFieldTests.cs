using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.Mediator;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation;
using CSharpEssentials.Validation.Extensions;
using CSharpEssentials.Validation.Validators;
using FluentAssertions;
using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

namespace CSharpEssentials.Tests.AspNetCore;

public class ValidationErrorsByFieldTests
{
    public sealed record CreateUser(string? Name, string? Email) : ICommand<Result>;

    public sealed record RenameUser(string? Name) : ICommand<string>;

    private sealed class RenameUserValidator : Validator<RenameUser>
    {
        protected override ValueTask Configure(RenameUser model, RuleContext<RenameUser> rules, CancellationToken ct = default)
        {
            rules.For(() => model.Name).NotEmpty();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class CreateUserValidator : Validator<CreateUser>
    {
        protected override ValueTask Configure(CreateUser model, RuleContext<CreateUser> rules, CancellationToken ct = default)
        {
            rules.For(() => model.Name).Cascade(CascadeMode.Continue).NotEmpty().MinLength(3);
            rules.For(() => model.Email).NotEmpty();
            return ValueTask.CompletedTask;
        }
    }

    private static Error FieldError(string code, string description, string field) =>
        Error.Validation(code, description, new ErrorMetadata(ErrorMetadata.FieldKey, field));

    private static readonly Error[] Errors =
    [
        FieldError("Name.NotEmpty", "'Name' must not be empty.", "Name"),
        FieldError("Name.MinLength", "'Name' is too short.", "Name"),
        FieldError("Email.NotEmpty", "'Email' must not be empty.", "Email"),
        Error.Validation("Order.Total", "The total is too high."),
        Error.NotFound("User.NotFound", "Not found."),
    ];

    private static Microsoft.AspNetCore.Mvc.ProblemDetails Problem(
        Error[] errors,
        Action<EnhancedProblemDetailsOptions> configure,
        Action<MvcJsonOptions>? json = null)
    {
        ServiceCollection collection = new();
        if (json is not null)
            collection.Configure(json);
        ServiceProvider services = collection.AddEnhancedProblemDetails(configure).BuildServiceProvider();
        DefaultHttpContext httpContext = new() { RequestServices = services };
        return errors.ToActionResult(httpContext).Should().BeAssignableTo<ObjectResult>()
            .Subject.Value.Should().BeAssignableTo<Microsoft.AspNetCore.Mvc.ProblemDetails>().Subject;
    }

    [Fact]
    public void ByField_Should_GroupDescriptionsByField_And_FallBackToCode()
    {
        var problem = Problem(Errors, o => o.ValidationErrorsFormat = ValidationErrorsFormat.ByField);

        problem.Extensions["errors"].Should().BeEquivalentTo(new Dictionary<string, string[]>
        {
            ["name"] = ["'Name' must not be empty.", "'Name' is too short."],
            ["email"] = ["'Email' must not be empty."],
            ["Order.Total"] = ["The total is too high."],
        });
    }

    [Fact]
    public void ByField_Should_KeepPathAsGiven_When_NoNamingPolicy()
    {
        var problem = Problem(
            [FieldError("Lines[1].Sku.NotEmpty", "Empty.", "Lines[1].Sku")],
            o => o.ValidationErrorsFormat = ValidationErrorsFormat.ByField,
            json => json.JsonSerializerOptions.PropertyNamingPolicy = null);

        problem.Extensions["errors"].Should().BeAssignableTo<Dictionary<string, string[]>>()
            .Which.Keys.Should().Equal("Lines[1].Sku");
    }

    [Theory]
    [InlineData("Lines[1].Sku", "lines[1].sku")]
    [InlineData("Address.PostCode", "address.post_code")]
    [InlineData("Items[0][2].UnitPrice", "items[0][2].unit_price")]
    [InlineData("$", "$")]
    public void ByField_Should_ApplyNamingPolicyToEachSegment_And_KeepIndexers(string field, string expected)
    {
        var problem = Problem(
            [FieldError("Code", "Invalid.", field)],
            o => o.ValidationErrorsFormat = ValidationErrorsFormat.ByField,
            json => json.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);

        problem.Extensions["errors"].Should().BeAssignableTo<Dictionary<string, string[]>>()
            .Which.Keys.Should().Equal(expected);
    }

    [Fact]
    public void Default_Should_StayList()
    {
        new EnhancedProblemDetailsOptions().ValidationErrorsFormat.Should().Be(ValidationErrorsFormat.List);

        var problem = Problem(Errors, _ => { });

        problem.Extensions["errors"].Should().BeAssignableTo<List<ProblemValidationError>>()
            .Which.Select(e => e.Code).Should().Equal("Name.NotEmpty", "Name.MinLength", "Email.NotEmpty", "Order.Total");
    }

    [Fact]
    public void Dictionary_Should_StillGroupByCode()
    {
        var problem = Problem(Errors, o => o.ValidationErrorsFormat = ValidationErrorsFormat.Dictionary);

        problem.Extensions["errors"].Should().BeAssignableTo<Dictionary<string, string[]>>()
            .Which.Keys.Should().Equal("Name.NotEmpty", "Name.MinLength", "Email.NotEmpty", "Order.Total");
    }

    [Fact]
    public async Task MediatorValidationBehavior_Should_RenderByField()
    {
        ValidationBehavior<CreateUser, Result> behavior = new([new CreateUserValidator()]);

        Result result = await behavior.Handle(
            new CreateUser("ab", null),
            (_, _) => new ValueTask<Result>(Result.Success()),
            default);

        result.IsFailure.Should().BeTrue();
        var problem = Problem(result.Errors, o => o.ValidationErrorsFormat = ValidationErrorsFormat.ByField);
        problem.Extensions["errors"].Should().BeEquivalentTo(new Dictionary<string, string[]>
        {
            ["name"] = ["'Name' must be at least 3 characters. You entered 2 characters."],
            ["email"] = ["'Email' must not be empty."],
        });
    }

    [Fact]
    public async Task EndpointFilter_Should_WriteErrorsByField_When_ByFieldIsConfigured()
    {
        await using WebApplication app = await StartAsync(o => o.ValidationErrorsFormat = ValidationErrorsFormat.ByField);
        using HttpClient client = app.GetTestClient();

        using HttpResponseMessage response = await client.PostAsJsonAsync(new Uri("/users", UriKind.Relative), new CreateUser("", ""));
        JsonObject json = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        JsonObject errors = json["errors"]!.AsObject();
        errors.Select(p => p.Key).Should().Equal("name", "email");
        errors["name"]!.AsArray().Select(n => n!.GetValue<string>()).Should().HaveCount(2);
        errors["email"]!.AsArray().Select(n => n!.GetValue<string>()).Should().Equal("'Email' must not be empty.");
    }

    [Fact]
    public async Task EndpointFilter_Should_WriteErrorList_By_Default()
    {
        await using WebApplication app = await StartAsync(_ => { });
        using HttpClient client = app.GetTestClient();

        using HttpResponseMessage response = await client.PostAsJsonAsync(new Uri("/users", UriKind.Relative), new CreateUser("", "a@b.c"));
        JsonObject json = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();

        json["errors"]!.AsArray().Select(n => n!["code"]!.GetValue<string>()).Should().Equal("Name.NotEmpty", "Name.MinLength");
    }

    [Fact]
    public void ConfigureInvalidModelStateResponse_Should_SetField_And_SkipModelLevelKey()
    {
        ServiceProvider services = new ServiceCollection().AddEnhancedProblemDetails().ConfigureInvalidModelStateResponse().BuildServiceProvider();
        ActionContext context = new(new DefaultHttpContext { RequestServices = services }, new RouteData(), new ActionDescriptor());
        context.ModelState.AddModelError("Name", "Name is required");
        context.ModelState.AddModelError("", "Model is invalid");

        IActionResult result = services.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value.InvalidModelStateResponseFactory(context);

        result.Should().BeOfType<EnhancedProblemObjectResult>().Which.Value.Should().BeOfType<EnhancedProblemDetails>()
            .Which.Errors.Select(e => (e.Code, Field: e.Metadata?.GetField()))
            .Should().BeEquivalentTo(new (string, string?)[] { ("Name", "Name"), ("request", null) });
    }

    [Fact]
    public async Task AllErrors_Should_WriteFieldMetadata_Of_ValidatorErrors()
    {
        await using WebApplication app = await StartAsync(o => o.ErrorFields = ProblemErrorFields.All);
        using HttpClient client = app.GetTestClient();

        using HttpResponseMessage response = await client.PostAsJsonAsync(new Uri("/users", UriKind.Relative), new CreateUser("", "a@b.c"));
        JsonObject json = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();

        json["errors"]!.AsArray().Select(n => n!["metadata"]!["field"]!.GetValue<string>()).Should().Equal("Name", "Name");
    }

    [Fact]
    public async Task GlobalExceptionHandler_Should_WriteErrorsByField_When_MediatorThrowsValidationException()
    {
        await using WebApplication app = await StartAsync(o => o.ValidationErrorsFormat = ValidationErrorsFormat.ByField);
        using HttpClient client = app.GetTestClient();

        using HttpResponseMessage response = await client.PostAsJsonAsync(new Uri("/rename", UriKind.Relative), new RenameUser(""));
        JsonObject json = JsonNode.Parse(await response.Content.ReadAsStringAsync())!.AsObject();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        JsonObject errors = json["errors"]!.AsObject();
        errors.Select(p => p.Key).Should().Equal("name");
        errors["name"]!.AsArray().Select(n => n!.GetValue<string>()).Should().Equal("'Name' must not be empty.");
    }

    private static async Task<WebApplication> StartAsync(Action<EnhancedProblemDetailsOptions> configure)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddEnhancedProblemDetails(configure);
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddValidator<CreateUser, CreateUserValidator>();
        WebApplication app = builder.Build();
        app.UseEnhancedProblemDetails();
        app.MapPost("/users", (CreateUser user) => user.Name).WithValidation<CreateUser>();
        app.MapPost("/rename", async (RenameUser user) => await new ValidationBehavior<RenameUser, string>([new RenameUserValidator()])
            .Handle(user, (_, _) => new ValueTask<string>("renamed"), default));
        await app.StartAsync();
        return app;
    }
}
