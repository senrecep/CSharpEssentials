using CSharpEssentials.Endpoints;
using Examples.Endpoints.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Examples.Endpoints.Todos;

[EndpointGroup<TodosGroup>]
public sealed class TodoCommands : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/{id:int}/done", Results<NoContent, NotFound> (int id, ITodoStore store) =>
            store.MarkDone(id) ? TypedResults.NoContent() : TypedResults.NotFound());

        app.MapDelete("/{id:int}", Results<NoContent, NotFound> (int id, ITodoStore store) =>
            store.Remove(id) ? TypedResults.NoContent() : TypedResults.NotFound());
    }
}
