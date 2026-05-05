using System.Net;
using CSharpEssentials.Errors;
using CSharpEssentials.Http;
using CSharpEssentials.ResultPattern;

Console.WriteLine("=== CSharpEssentials.Http Examples ===");
Console.WriteLine();

// ============================================================================
// STATUS CODE MAPPING
// ============================================================================
Console.WriteLine("--- Status Code Mapping ---");

ErrorType badRequestType = HttpStatusCodeMapper.ToErrorType(HttpStatusCode.BadRequest);
Console.WriteLine($"400 -> {badRequestType}");

ErrorType notFoundType = HttpStatusCodeMapper.ToErrorType(HttpStatusCode.NotFound);
Console.WriteLine($"404 -> {notFoundType}");

ErrorType tooManyRequestsType = HttpStatusCodeMapper.ToErrorType(HttpStatusCode.TooManyRequests);
Console.WriteLine($"429 -> {tooManyRequestsType}");

ErrorType serverErrorType = HttpStatusCodeMapper.ToErrorType(HttpStatusCode.InternalServerError);
Console.WriteLine($"500 -> {serverErrorType}");

Error error = HttpStatusCodeMapper.ToError(HttpStatusCode.Forbidden);
Console.WriteLine($"403 Error: {error.Code} - {error.Description}");
Console.WriteLine();

// ============================================================================
// MOCK HTTP CLIENT WITH RESULT
// ============================================================================
Console.WriteLine("--- HttpClient Result Extensions ---");

var mockHandler = new MockHttpHandler();
HttpClient client = new(mockHandler) { BaseAddress = new Uri("https://api.example.com") };

Result<User> userResult = await client.GetFromJsonAsResultAsync<User>(new Uri("/users/1", UriKind.Relative));
userResult.Switch(
    onSuccess: u => Console.WriteLine($"GET User: {u.Name} ({u.Email})"),
    onError: errs => Console.WriteLine($"GET Failed: {errs[0].Description}")
);

Result<User> postResult = await client.PostAsJsonAsResultAsync<User>(
    new Uri("/users", UriKind.Relative),
    new { Name = "Alice", Email = "alice@example.com" });
postResult.Switch(
    onSuccess: u => Console.WriteLine($"POST User: {u.Name}"),
    onError: errs => Console.WriteLine($"POST Failed: {errs[0].Description}")
);

Result<User> putResult = await client.PutAsJsonAsResultAsync<User>(
    new Uri("/users/1", UriKind.Relative),
    new { Name = "Alice Updated", Email = "alice@example.com" });
putResult.Switch(
    onSuccess: u => Console.WriteLine($"PUT User: {u.Name}"),
    onError: errs => Console.WriteLine($"PUT Failed: {errs[0].Description}")
);

Result deleteResult = await client.DeleteAsResultAsync(new Uri("/users/1", UriKind.Relative));
deleteResult.Switch(
    onSuccess: () => Console.WriteLine("DELETE succeeded"),
    onFailure: errs => Console.WriteLine($"DELETE Failed: {errs[0].Description}")
);

Console.WriteLine();

// ============================================================================
// QUERY STRING BUILDER
// ============================================================================
Console.WriteLine("--- Query String Builder ---");

Uri baseUri = new("https://api.example.com/search");
var queryUriResult = baseUri.WithQueryString("q", "csharp").WithQueryString("page", "1");
queryUriResult.Switch(
    onSuccess: uri => Console.WriteLine($"Query URI: {uri}"),
    onFailure: errs => Console.WriteLine($"Query Failed: {errs[0].Description}")
);

var dict = new Dictionary<string, string?> { { "sort", "desc" }, { "limit", "10" } };
var dictUriResult = baseUri.WithQueryString(dict);
dictUriResult.Switch(
    onSuccess: uri => Console.WriteLine($"Dict URI: {uri}"),
    onFailure: errs => Console.WriteLine($"Dict Failed: {errs[0].Description}")
);

Console.WriteLine();

// ============================================================================
// HTTP REQUEST BUILDER
// ============================================================================
Console.WriteLine("--- HttpRequestBuilder ---");

Result<User> builderResult = await HttpRequestBuilder
    .Get("/users/1")
    .WithHeader("Accept", "application/json")
    .AsResultAsync<User>(client);

builderResult.Switch(
    onSuccess: u => Console.WriteLine($"Builder GET: {u.Name}"),
    onError: errs => Console.WriteLine($"Builder GET Failed: {errs[0].Description}")
);

Console.WriteLine();

// ============================================================================
// RESILIENCE PIPELINE
// ============================================================================
Console.WriteLine("--- Resilience Pipeline ---");

var pipeline = HttpClientResilienceExtensions.CreateResiliencePipeline(
    maxRetryAttempts: 2,
    timeout: TimeSpan.FromSeconds(5),
    retryDelay: TimeSpan.FromMilliseconds(100));

Result<User> resilientResult = await pipeline.ExecuteAsResultAsync(async token =>
    await client.GetFromJsonAsResultAsync<User>(new Uri("/users/1", UriKind.Relative), cancellationToken: token));

resilientResult.Switch(
    onSuccess: u => Console.WriteLine($"Resilient GET: {u.Name}"),
    onError: errs => Console.WriteLine($"Resilient GET Failed: {errs[0].Description}")
);

Console.WriteLine();
Console.WriteLine("=== Done ===");

public sealed record User(string Name, string Email);

public sealed class MockHttpHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = request.Method.Method switch
        {
            "GET" => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"name":"Bob","email":"bob@example.com"}""")
            },
            "POST" => new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent("""{"name":"Alice","email":"alice@example.com"}""")
            },
            "PUT" => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"name":"Alice Updated","email":"alice@example.com"}""")
            },
            "DELETE" => new HttpResponseMessage(HttpStatusCode.NoContent),
            _ => new HttpResponseMessage(HttpStatusCode.NotFound)
        };
        return Task.FromResult(response);
    }
}
