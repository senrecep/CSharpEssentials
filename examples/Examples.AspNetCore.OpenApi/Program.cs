using CSharpEssentials.AspNetCore;
using Examples.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddEnhancedProblemDetails();
builder.Services.AddEnumConventions();
builder.Services.AddIdempotency(o => o.RequireKey = true);
builder.Services.AddConditionalRequests();

builder.Services.AddOpenApi("v1", o => o
    .AddEnumConventions()
    .AddIdempotencyConventions()
    .AddOptionalRouteParameters()
    .AddBearerSecurityScheme(description: "Access token from the identity server."));

WebApplication app = builder.Build();

app.UseRouting();
app.UseEnumBinding();
app.UseIdempotency();

app.MapOpenApi();

app.MapGet("/orders", (OrderStatus? status) => Array.Empty<Order>());

app.MapGet("/orders/archive/{year:int?}", (int? year) => Array.Empty<Order>())
    .WithName("GetArchive");

app.MapPost("/orders", (CreateOrder command) => Results.Created("/orders/1", new Order(1, OrderStatus.Pending)))
    .WithIdempotency()
    .RequireAuthorization();

app.MapPut("/orders/{id:int}", (int id, UpdateOrder body) => Results.NoContent())
    .WithIfMatch(required: true)
    .RequireAuthorization();

app.MapGet("/health", () => "ok");

if (args.Contains("--serve"))
{
    await app.RunAsync();
    return;
}

app.Urls.Add("http://127.0.0.1:0");
await app.StartAsync();
string baseUrl = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
Console.WriteLine(await http.GetStringAsync("/openapi/v1.json"));
await app.StopAsync();
