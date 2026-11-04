using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using CSharpEssentials.Endpoints;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default));
builder.Services.AddValidation();

WebApplication app = builder.Build();

app.MapCseGeneratedAotEndpoints();

await app.RunAsync();

public sealed class GetUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/users/{id:int}", (int id) => TypedResults.Ok(new UserResponse(id, $"user{id}")));
}

public sealed class CreateUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/users", (CreateUserRequest request) => TypedResults.Created("/users/1", new UserResponse(1, request.Name)));
}

public sealed class DeleteUser : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapDelete("/users/{id:int}", (int id) => TypedResults.NoContent());
}

public sealed record UserResponse(int Id, string Name);

public sealed record CreateUserRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Name,
    [property: Required, EmailAddress] string Email,
    [property: Range(18, 120)] int Age);

[JsonSerializable(typeof(UserResponse))]
[JsonSerializable(typeof(CreateUserRequest))]
[JsonSerializable(typeof(HttpValidationProblemDetails))]
internal sealed partial class AppJsonContext : JsonSerializerContext;
