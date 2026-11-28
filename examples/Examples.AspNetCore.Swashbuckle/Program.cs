using CSharpEssentials.AspNetCore;
using Examples.AspNetCore.Swashbuckle;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Swashbuckle.AspNetCore.SwaggerGen;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
builder.Services.AddEnhancedProblemDetails();
builder.Services.AddEnumConventions();
builder.Services.AddIdempotency(o => o.RequireKey = true);
builder.Services.AddConditionalRequests();
builder.Services.AddAndConfigureApiVersioning();

builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>();
builder.Services.Configure<SwaggerGenOptions>(o => o.AddBearerSecurityScheme(description: "Access token from the identity server."));

WebApplication app = builder.Build();

app.UseRouting();
app.UseEnumBinding();
app.UseIdempotency();
app.UseVersionableSwagger();

RouteGroupBuilder v1 = app.MapVersionedGroup(1);
v1.MapGet("/orders", (OrderStatus? status) => Array.Empty<Order>());
v1.MapGet("/orders/archive/{year:int?}", (int? year) => Array.Empty<Order>()).WithName("GetArchive");
v1.MapPost("/orders", (CreateOrder command) => Results.Created("/v1/orders/1", new Order(1, OrderStatus.Pending)))
    .WithIdempotency()
    .RequireAuthorization();
v1.MapPut("/orders/{id:int}", (int id, UpdateOrder body) => Results.NoContent())
    .WithIfMatch(required: true)
    .RequireAuthorization();

RouteGroupBuilder v2 = app.MapVersionedGroup(2);
v2.MapGet("/health", () => "ok");

if (args.Contains("--serve"))
{
    await app.RunAsync();
    return;
}

app.Urls.Add("http://127.0.0.1:0");
await app.StartAsync();
string baseUrl = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
Console.WriteLine(await http.GetStringAsync("/swagger/v1/swagger.json"));
Console.WriteLine(await http.GetStringAsync("/swagger/v2/swagger.json"));
await app.StopAsync();
