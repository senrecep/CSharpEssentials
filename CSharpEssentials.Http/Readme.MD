# CSharpEssentials.Http

`HttpClient` extensions that return `Result<T>` instead of throwing exceptions. Built-in status-code-to-error mapping, fluent request builder, Polly resilience, and redirect handling.

## Features

- **Result-based HTTP**: `GetFromJsonAsResultAsync`, `PostAsJsonAsResultAsync`, `PutAsJsonAsResultAsync`, `PatchAsJsonAsResultAsync`, `DeleteAsResultAsync`.
- **Automatic Error Mapping**: HTTP codes mapped to `ErrorType` (Validation, Unauthorized, NotFound, Conflict, etc.).
- **Fluent Request Builder**: Chain headers, query params, JSON content, and redirects.
- **Polly Integration**: Retry, timeout, and circuit breaker pipelines.
- **Query String Helpers**: Build URIs from dictionaries or anonymous objects.

## Installation

```bash
dotnet add package CSharpEssentials.Http
```

## Usage

### GET, POST, PUT, PATCH, DELETE as Result

```csharp
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;

HttpClient client = new() { BaseAddress = new Uri("https://api.example.com") };

Result<User> user = await client.GetFromJsonAsResultAsync<User>(new Uri("/users/1", UriKind.Relative));

Result<User> created = await client.PostAsJsonAsResultAsync<User>(
    new Uri("/users", UriKind.Relative),
    new { Name = "Alice", Age = 30 });

Result deleted = await client.DeleteAsResultAsync(new Uri("/users/1", UriKind.Relative));
```

### HttpRequestBuilder

```csharp
Result<User> result = await HttpRequestBuilder
    .Get("/users/1")
    .WithHeader("Accept", "application/json")
    .WithQuery("include", "profile")
    .AsResultAsync<User>(client);

Result created = await HttpRequestBuilder
    .Post("/users")
    .WithJsonContent(new { Name = "Bob" })
    .AsResultAsync(client);
```

### Enums in route, query and body

Enums with `[StringEnum]` are written as wire names; `WithEnumConventions(conventions, EnumWireFormat.Number)` writes numbers for servers that accept only integers. Flags and enum collections become repeated query keys, and an undefined value is never sent.

```csharp
Result<Order> result = await HttpRequestBuilder
    .Get("https://api.example.com/orders/{status}")
    .WithEnumConventions(EnumConventions.Default)
    .WithRoute("status", OrderStatus.PendingApproval)          // /orders/pending_approval
    .WithQuery("permissions", Permissions.Read | Permissions.Write) // ?permissions=read&permissions=write
    .AsResultAsync<Order>(client, options);
```

Build `options` once with `new JsonSerializerOptions(JsonSerializerDefaults.Web).AddEnumConventions(conventions, EnumReadMode.Data, writeAs)`. Responses are then read tolerantly: wire names, member names, aliases and numbers, with the `[EnumFallback]` member for values a newer server adds. Plain enums without `[StringEnum]` keep the framework behavior.

Roll out consumers before producers: a server moves its output from `Number` to `String` only after every client reads names.

### Query String Builder

```csharp
Uri uri = new Uri("https://api.example.com/search");
Uri withQuery = uri.WithQueryString("q", "csharp").Value;
Uri withDict = uri.WithQueryString(new Dictionary<string, string?> { { "page", "1" } }).Value;
```

### Polly Resilience

```csharp
var pipeline = HttpClientResilienceExtensions.CreateResiliencePipeline(
    maxRetryAttempts: 3,
    timeout: TimeSpan.FromSeconds(30));

Result<User> result = await pipeline.ExecuteAsResultAsync(async token =>
    await client.GetFromJsonAsResultAsync<User>(new Uri("/users/1", UriKind.Relative), cancellationToken: token));
```

### Redirect Following

```csharp
Result<User> result = await HttpRequestBuilder
    .Get("/legacy-url")
    .FollowRedirects(maxRedirects: 3)
    .AsResultAsync<User>(client);
```
