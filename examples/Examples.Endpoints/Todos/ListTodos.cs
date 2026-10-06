using CSharpEssentials.Endpoints;
using Examples.Endpoints.Services;

namespace Examples.Endpoints.Todos;

[EndpointGroup<TodosGroup>]
public sealed class ListTodos : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) =>
        app.MapGet("/", (ITodoStore store) => TypedResults.Ok(store.List()));
}
