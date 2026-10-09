# CSharpEssentials.Endpoints + DependencyInjection Native AOT Example

A minimal Native AOT web app that uses the source-generated registries of `CSharpEssentials.Endpoints` and `CSharpEssentials.DependencyInjection`. CI publishes it with `PublishAot=true` and fails on any trim or AOT warning.

## What it shows

| Feature | File | Description |
|---------|------|-------------|
| Endpoint | `Health.cs` | `IEndpoint` with a static `Map` method |
| Endpoint group | `Todos/TodosGroup.cs` | `IEndpointGroup` with the `todos` prefix and a tag |
| Grouped endpoints | `Todos/*.cs` | `[EndpointGroup<TodosGroup>]` on list, get and create endpoints |
| Attribute registration | `Services/TodoStore.cs` | `[RegisterSingleton]`, registered as `ITodoStore` by the `I{TypeName}` rule |
| Decorator | `Services/LoggingTodoStore.cs` | `[Decorates(typeof(ITodoStore))]`, built by a generated factory |
| Generated aggregates | `Program.cs` | `AddAllServices()` and `MapAllEndpoints()`, both reflection-free; `MapAllEndpoints` sets `OperationNaming.TypeName` so every endpoint gets a unique name such as `ListTodos` |

## Running

```bash
dotnet run --project examples/Examples.Endpoints
```

## Publishing with Native AOT

```bash
dotnet publish examples/Examples.Endpoints -c Release -r osx-arm64   # or linux-x64, win-x64
./examples/Examples.Endpoints/bin/Release/net10.0/osx-arm64/publish/Examples.Endpoints
```

The project sets `TreatWarningsAsErrors`, which the Native AOT compiler also applies to trim and AOT warnings, so a warning fails the publish.

## Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/health` | Returns `healthy` |
| GET | `/todos/` | Lists todos |
| GET | `/todos/{id}` | Gets a todo, or 404 |
| POST | `/todos/` | Creates a todo from `{ "title": "..." }` |
