using CSharpEssentials.Endpoints;
using Examples.Endpoints.Services;

namespace Examples.Endpoints.Todos.Admin;

[EndpointGroup<TodoAdminGroup>]
public sealed class GetTodoStats : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/stats", (ITodoStore store) =>
        {
            Todo[] todos = store.List();
            return TypedResults.Ok(new TodoStats(todos.Length, todos.Count(todo => todo.IsDone)));
        });
}
