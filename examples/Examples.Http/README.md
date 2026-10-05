# CSharpEssentials.Http Example

Console application demonstrating `CSharpEssentials.Http` (Result-based `HttpClient` helpers) and a custom `IResultErrorMapper` from `CSharpEssentials.AspNetCore`.

## Features Demonstrated

| Feature | Description |
|---------|-------------|
| **Status code mapping** | `HttpStatusCodeMapper.ToErrorType` / `ToError` |
| **HttpClient Result extensions** | `GetFromJsonAsResultAsync`, `PostAsJsonAsResultAsync`, `PutAsJsonAsResultAsync`, `DeleteAsResultAsync` |
| **Query string builder** | `Uri.WithQueryString(...)` |
| **HttpRequestBuilder** | Fluent request building with `AsResultAsync<T>` |
| **Resilience pipeline** | Retry + timeout around Result-returning calls |
| **Redirect following** | `SendWithRedirectsAsResultAsync`, `FollowRedirects` |
| **Custom `IResultErrorMapper`** | `ApiErrorMapper` maps `Error[]` to a status code and JSON envelope |

## Custom IResultErrorMapper

`ResultEndpointFilter` converts a failed `Result`/`Result<T>` into an HTTP response. It resolves `IResultErrorMapper` per request from `HttpContext.RequestServices`; when none is registered it falls back to a ProblemDetails response.

```csharp
builder.Services.AddSingleton<IResultErrorMapper, ApiErrorMapper>();

app.MapGet("/users/{id}", GetUser).AddEndpointFilter<ResultEndpointFilter>();
```

This example is a console app, so it references `CSharpEssentials.AspNetCore` (plus `<FrameworkReference Include="Microsoft.AspNetCore.App" />`) and executes the mapper's `IResult` against a `DefaultHttpContext` to print the status code and body. See `Examples.AspNetCore` for a full web app.

## Running

```bash
cd examples/Examples.Http
dotnet run
```
