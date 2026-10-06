using CSharpEssentials.Endpoints;
using Examples.Endpoints.Services;

namespace Examples.Endpoints.Todos;

[EndpointGroup<TodosGroup>]
public sealed class CreateTodo : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapPost("/", (CreateTodoRequest request, ITodoStore store) =>
        {
            Todo todo = store.Add(request.Title);
            return TypedResults.Created($"/todos/{todo.Id}", todo);
        });
}
