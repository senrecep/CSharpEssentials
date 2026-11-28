using System.Net.Http.Json;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;
using CSharpEssentials.Idempotency;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Validation.Extensions;
using Examples.AspNetCore;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEnhancedProblemDetails(o => o.ExposeExceptionDetails = builder.Environment.IsDevelopment());
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.ConfigureInvalidModelStateResponse();
builder.Services.AddControllers();

builder.Services.AddEnumConventions();
builder.Services.AddAndConfigureApiVersioning();
builder.Services.AddConditionalRequests();
builder.Services.AddIdempotency();

builder.Services.AddValidator<CreateOrder, CreateOrderValidator>();
builder.Services.AddSingleton<OrderStore>();

WebApplication app = builder.Build();

app.UseEnhancedProblemDetails();
app.UseRouting();
app.UseEnumBinding();
app.UseIdempotency();

RouteGroupBuilder v1 = app.MapVersionedGroup(1);

v1.MapGet("/orders", (OrderStatus? status, OrderStore store) => store.List(status));

v1.MapGet("/orders/{id:guid}", (Guid id, OrderStore store) => store.Find(id).ToHttpResult())
    .WithConditionalGet();

v1.MapPost("/orders", (CreateOrder command, OrderStore store) =>
    {
        Order order = store.Add(command);
        return Results.Created($"/v1/orders/{order.Id}", order);
    })
    .WithValidation<CreateOrder>()
    .WithIdempotency(o => o.UnscopedKeyBehavior = UnscopedKeyBehavior.Global);

v1.MapPut("/orders/{id:guid}", (Guid id, UpdateOrder body, HttpContext http, OrderStore store) =>
        store.Update(id, body, http.GetPreconditions()))
    .AddEndpointFilter<ResultEndpointFilter>()
    .WithIfMatch(required: true);

app.MapGroup("/legacy")
    .WithEnumWireFormat(EnumWireFormat.Number)
    .MapGet("/orders", (OrderStore store) => store.List(null));

app.MapGet("/boom", string () => throw new InvalidOperationException("Secret connection string"));

app.MapControllers();

if (args.Contains("--serve"))
{
    await app.RunAsync();
    return;
}

app.Urls.Add("http://127.0.0.1:0");
await app.StartAsync();
string baseUrl = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };
string order = $"/v1/orders/{OrderStore.SeedId}";

await Show("Enum binding: any spelling of the enum binds", HttpMethod.Get, "/v1/orders?status=PENDING_APPROVAL");
await Show("Invalid enum value: 400 listing the allowed values", HttpMethod.Get, "/v1/orders?status=lost");
await Show("Typed result: 200 with an ETag", HttpMethod.Get, order);
await Show("Conditional GET: 304 when If-None-Match matches", HttpMethod.Get, order, h => h.TryAddWithoutValidation("If-None-Match", "\"1\""));
await Show("Typed result: NotFoundError becomes a 404 problem", HttpMethod.Get, $"/v1/orders/{Guid.Empty}");
await Show("If-Match missing: 428", HttpMethod.Put, order, body: new UpdateOrder(OrderStatus.Shipped));
await Show("If-Match stale: 412", HttpMethod.Put, order, h => h.TryAddWithoutValidation("If-Match", "\"7\""), new UpdateOrder(OrderStatus.Shipped));
await Show("If-Match current: 200 through ResultEndpointFilter", HttpMethod.Put, order, h => h.TryAddWithoutValidation("If-Match", "\"1\""), new UpdateOrder(OrderStatus.Shipped));
await Show("Validation filter: 400 before the handler runs", HttpMethod.Post, "/v1/orders", body: new CreateOrder("", 0));
await Show("Idempotency: first request runs the handler", HttpMethod.Post, "/v1/orders", h => h.Add("Idempotency-Key", "order-1"), new CreateOrder("bob", 1));
await Show("Idempotency: same key, same body: stored response replayed", HttpMethod.Post, "/v1/orders", h => h.Add("Idempotency-Key", "order-1"), new CreateOrder("bob", 1));
await Show("Idempotency: same key, other body: 422", HttpMethod.Post, "/v1/orders", h => h.Add("Idempotency-Key", "order-1"), new CreateOrder("eve", 3));
await Show("Wire format: the legacy group writes enums as numbers", HttpMethod.Get, "/legacy/orders");
await Show("Unhandled exception: 500 problem without the message", HttpMethod.Get, "/boom");
await Show("Unknown route: status code pages write a 404 problem", HttpMethod.Get, "/nothing-here");
await Show("MVC: this.Problem(error) and [ConditionalGet]", HttpMethod.Get, $"/mvc/orders/{Guid.Empty}");
await Show("MVC: invalid model state as an enhanced problem", HttpMethod.Post, "/mvc/orders", body: new { quantity = 0 });

await app.StopAsync();

async Task Show(string title, HttpMethod method, string path, Action<System.Net.Http.Headers.HttpRequestHeaders>? headers = null, object? body = null)
{
    using var request = new HttpRequestMessage(method, path);
    headers?.Invoke(request.Headers);
    if (body is not null)
        request.Content = JsonContent.Create(body);
    using HttpResponseMessage response = await http.SendAsync(request);
    string extra = string.Join("", new[] { "ETag", "Idempotency-Replayed" }
        .Where(response.Headers.Contains)
        .Select(h => $" {h}: {string.Join(",", response.Headers.GetValues(h))}"));
    Console.WriteLine($"# {title}");
    Console.WriteLine($"{method} {path} -> {(int)response.StatusCode}{extra}");
    string text = await response.Content.ReadAsStringAsync();
    if (text.Length > 0)
        Console.WriteLine(text);
    Console.WriteLine();
}
