using System.Text.Json.Serialization;
using Examples.Endpoints.Todos;
using Examples.Endpoints.Todos.Admin;

namespace Examples.Endpoints;

[JsonSerializable(typeof(Todo))]
[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(CreateTodoRequest))]
[JsonSerializable(typeof(TodoStats))]
[JsonSerializable(typeof(string[]))]
internal sealed partial class AppJsonSerializerContext : JsonSerializerContext;
