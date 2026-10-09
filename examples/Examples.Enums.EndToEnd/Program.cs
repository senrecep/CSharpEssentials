using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;
using Examples.Enums.EndToEnd;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEnumConventions();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<ShopDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Shop")));
builder.Services.AddScoped<OrderService>();

builder.Services.AddOpenApi("v1", o => o.AddEnumConventions());
builder.Services.AddOpenApi("v2", o => o.AddEnumConventions());

WebApplication app = builder.Build();

await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
    await scope.ServiceProvider.GetRequiredService<ShopDbContext>().Database.EnsureCreatedAsync();

app.UseExceptionHandler();
app.UseRouting();
app.UseEnumBinding();

app.MapOpenApi();

app.MapGroup("/api/v1").WithGroupName("v1").WithEnumWireFormat(EnumWireFormat.Number).MapOrders();

app.MapGroup("/api/v2").WithGroupName("v2").MapOrders();

await app.RunAsync();
