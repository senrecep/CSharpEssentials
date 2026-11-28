using CSharpEssentials.Endpoints;

namespace Examples.Endpoints.Todos.Admin;

[EndpointGroup<TodosGroup>]
public sealed class TodoAdminGroup : IEndpointGroup
{
    public static string Prefix => "admin";

    public static void Configure(RouteGroupBuilder group) => group.RequireRoles("admin");
}
