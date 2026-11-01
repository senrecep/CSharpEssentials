using Carter;
using Carter.ModelBinding;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CSharpEssentials.Endpoints.Benchmarks.Throughput.CarterHost;

// https://github.com/CarterCommunity/Carter/blob/10.0.0/samples/CarterSample/Features/CastMembers/CastMemberModule.cs
public sealed class CreateUserModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app) =>
        app.MapPost("/users", async (HttpRequest req, CreateUserRequest user) =>
        {
            FluentValidation.Results.ValidationResult result = await req.ValidateAsync(user).ConfigureAwait(false);
            return result.IsValid
                ? Results.Ok(new UserResponse(1, user.Name!))
                : Results.ValidationProblem(result.GetValidationProblems());
        });
}
