using System.Text.Json.Serialization;
using Examples.Endpoints.Todos;

namespace Examples.Endpoints;

[JsonSerializable(typeof(Todo))]
[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(CreateTodoRequest))]
internal sealed partial class AppJsonSerializerContext : JsonSerializerContext;
