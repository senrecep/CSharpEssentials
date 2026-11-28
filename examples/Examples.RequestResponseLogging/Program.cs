using System.Net.Http.Json;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.RequestResponseLogging;
using Examples.RequestResponseLogging.Controllers;
using Examples.RequestResponseLogging.Infrastructure;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Net.Http.Headers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEnhancedProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();
ILoggerFactory loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
ILogger httpLogger = loggerFactory.CreateLogger("Http");

app.UseEnhancedProblemDetails();
app.UseRouting();
app.AddRequestResponseLogging(opt =>
{
    LoggingOptions logging = LoggingOptions.CreateAllFields();
    logging.HeaderKeys.Add(HeaderNames.AcceptLanguage);
    opt.UseLogger(loggerFactory, logging);
    opt.UseHandler(context => StructuredJsonLogWriter.WriteAsync(context, httpLogger));
    opt.IgnorePaths("/health");
});

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/minimal/export", () => Enumerable.Range(1, 3))
    .WithMetadata(new SkipResponseLoggingAttribute());

if (args.Contains("--serve"))
{
    await app.RunAsync();
    return;
}

app.Urls.Add("http://127.0.0.1:0");
await app.StartAsync();
string baseUrl = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
ILogger tour = loggerFactory.CreateLogger("Tour");

await Send("GET with no body", HttpMethod.Get, "/api/demo/hello?lang=en");
await Send("POST body and the Accept-Language header are logged", HttpMethod.Post, "/api/demo/echo", new EchoRequest("Hello", 3), language: "tr");
await Send("404 problem response body is logged", HttpMethod.Get, "/api/demo/notfound");
await Send("Exception: logged with the exception type and message, then rethrown", HttpMethod.Get, "/api/demo/error");
await Send("ResponseTiming shows the slow handler", HttpMethod.Get, "/api/demo/slow");
await Send("[SkipRequestLogging]: the password body is not logged", HttpMethod.Post, "/api/demo/login", new LoginRequest("ada", "hunter2"));
await Send("[SkipResponseLogging] on an MVC action", HttpMethod.Get, "/api/demo/export");
await Send("[SkipRequestResponseLogging]: neither body is logged", HttpMethod.Post, "/api/demo/secret", new LoginRequest("ada", "hunter2"));
await Send("SkipResponseLoggingAttribute as Minimal API metadata", HttpMethod.Get, "/minimal/export");
await Send("IgnorePaths: nothing is logged for /health", HttpMethod.Get, "/health");

await Task.Delay(200);
await app.StopAsync();

async Task Send(string title, HttpMethod method, string path, object? body = null, string? language = null)
{
    tour.LogInformation("# {Title}: {Method} {Path}", title, method, path);
    using var request = new HttpRequestMessage(method, path);
    if (language is not null)
        request.Headers.Add(HeaderNames.AcceptLanguage, language);
    if (body is not null)
        request.Content = JsonContent.Create(body);
    using HttpResponseMessage response = await http.SendAsync(request);
    tour.LogInformation("  -> {Status}", (int)response.StatusCode);
}
