using CSharpEssentials.Endpoints;
using Examples.Endpoints.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Examples.Endpoints.Todos;

[EndpointGroup<TodosGroup>]
public sealed class GetTodo : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/{id:int}", Results<Ok<Todo>, NotFound> (int id, ITodoStore store) =>
            store.Find(id) is { } todo ? TypedResults.Ok(todo) : TypedResults.NotFound());
}
