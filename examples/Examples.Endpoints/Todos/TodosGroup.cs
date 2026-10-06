using CSharpEssentials.Endpoints;

namespace Examples.Endpoints.Todos;

public sealed class TodosGroup : IEndpointGroup
{
    public static string Prefix => "todos";

    public static void Configure(RouteGroupBuilder group) => group.WithTags("Todos");
}
