// https://fast-endpoints.com/docs/native-aot
// https://fast-endpoints.com/docs/configuration-settings#source-generator-based-startup
// https://fast-endpoints.com/docs/configuration-settings#source-generated-reflection
// https://fast-endpoints.com/docs/model-binding#source-generators
// https://fast-endpoints.com/docs/validation
using System.Text.Json.Serialization;
using FastEndpoints;
using FluentValidation;
using FastEndpointsSourceGenAot;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);
builder.Services.AddFastEndpoints(DiscoveredTypes.All);

WebApplication app = builder.Build();

app.UseFastEndpoints(c => c.Binding.ReflectionCache.AddFromFastEndpointsSourceGenAot());

await app.RunAsync();

public sealed class GetUserRequest
{
    public int Id { get; set; }
}

public sealed class GetUser : Endpoint<GetUserRequest, UserResponse>
{
    public override void Configure()
    {
        Get("/users/{id:int}");
        AllowAnonymous();
        SerializerContext<AppJsonContext>();
    }

    public override Task HandleAsync(GetUserRequest req, CancellationToken ct) =>
        Send.OkAsync(new UserResponse(req.Id, $"user{req.Id}"), ct);
}

public sealed class CreateUser : Endpoint<CreateUserRequest, UserResponse>
{
    public override void Configure()
    {
        Post("/users");
        AllowAnonymous();
        SerializerContext<AppJsonContext>();
    }

    public override Task HandleAsync(CreateUserRequest req, CancellationToken ct) =>
        Send.ResponseAsync(new UserResponse(1, req.Name), 201, ct);
}

public sealed class DeleteUser : EndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/users/{id:int}");
        AllowAnonymous();
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        _ = Route<int>("id");
        return Send.NoContentAsync(ct);
    }
}

public sealed class UserResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = "";

    public UserResponse() { }

    public UserResponse(int id, string name) => (Id, Name) = (id, name);
}

public sealed class CreateUserRequest
{
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public int Age { get; set; }
}

public sealed class CreateUserValidator : Validator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(1, 100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).InclusiveBetween(18, 120);
    }
}

[JsonSerializable(typeof(GetUserRequest))]
[JsonSerializable(typeof(CreateUserRequest))]
[JsonSerializable(typeof(UserResponse))]
[JsonSerializable(typeof(ErrorResponse))]
internal sealed partial class AppJsonContext : JsonSerializerContext;
