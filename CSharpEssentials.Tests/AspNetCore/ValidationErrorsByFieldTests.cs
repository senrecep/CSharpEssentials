using System.Net;
using System.Net.Http.Json;
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
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.AspNetCore;

public class ValidationErrorsByFieldTests
{
    public sealed record CreateUser(string? Name, string? Email) : ICommand<Result>;

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

    private static Microsoft.AspNetCore.Mvc.ProblemDetails Problem(Error[] errors, Action<EnhancedProblemDetailsOptions> configure)
    {
        ServiceProvider services = new ServiceCollection().AddEnhancedProblemDetails(configure).BuildServiceProvider();
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
            ["Name"] = ["'Name' must not be empty.", "'Name' is too short."],
            ["Email"] = ["'Email' must not be empty."],
            ["Order.Total"] = ["The total is too high."],
        });
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
            ["Name"] = ["'Name' must be at least 3 characters. You entered 2 characters."],
            ["Email"] = ["'Email' must not be empty."],
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
        errors.Select(p => p.Key).Should().Equal("Name", "Email");
        errors["Name"]!.AsArray().Select(n => n!.GetValue<string>()).Should().HaveCount(2);
        errors["Email"]!.AsArray().Select(n => n!.GetValue<string>()).Should().Equal("'Email' must not be empty.");
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

    private static async Task<WebApplication> StartAsync(Action<EnhancedProblemDetailsOptions> configure)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddEnhancedProblemDetails(configure);
        builder.Services.AddValidator<CreateUser, CreateUserValidator>();
        WebApplication app = builder.Build();
        app.MapPost("/users", (CreateUser user) => user.Name).WithValidation<CreateUser>();
        await app.StartAsync();
        return app;
    }
}
