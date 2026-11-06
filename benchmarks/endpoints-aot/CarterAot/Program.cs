// https://github.com/CarterCommunity/Carter/blob/10.0.0/README.md
// https://github.com/CarterCommunity/Carter/blob/10.0.0/src/Carter/DependencyContextAssemblyCatalog.cs#L48
// https://github.com/CarterCommunity/Carter/blob/10.0.0/samples/CarterSample/Features/CastMembers/CastMemberModule.cs
using System.Text.Json.Serialization;
using Carter;
using Carter.ModelBinding;
using FluentValidation;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default));
builder.Services.AddCarter(new DependencyContextAssemblyCatalog([typeof(GetUserModule).Assembly]));

WebApplication app = builder.Build();

app.MapCarter();

await app.RunAsync();

public sealed class GetUserModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapGet("/users/{id:int}", (int id) => TypedResults.Ok(new UserResponse(id, $"user{id}")));
}

public sealed class CreateUserModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapPost("/users", async (HttpRequest req, CreateUserRequest request) =>
        {
            FluentValidation.Results.ValidationResult result = await req.ValidateAsync(request);
            return result.IsValid
                ? Results.Created("/users/1", new UserResponse(1, request.Name))
                : Results.ValidationProblem(result.GetValidationProblems());
        });
}

public sealed class DeleteUserModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapDelete("/users/{id:int}", (int id) => TypedResults.NoContent());
}

public sealed record UserResponse(int Id, string Name);

public sealed record CreateUserRequest(string Name, string Email, int Age);

public sealed class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(1, 100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).InclusiveBetween(18, 120);
    }
}

[JsonSerializable(typeof(UserResponse))]
[JsonSerializable(typeof(CreateUserRequest))]
[JsonSerializable(typeof(HttpValidationProblemDetails))]
internal sealed partial class AppJsonContext : JsonSerializerContext;
