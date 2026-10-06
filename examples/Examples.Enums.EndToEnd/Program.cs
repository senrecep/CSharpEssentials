using CSharpEssentials.AspNetCore;
using CSharpEssentials.Enums;
using Examples.Enums.EndToEnd;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// One EnumConventions instance for JSON bodies, route/query/header/form binding, output and EF Core.
builder.Services.AddEnumConventions();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<ShopDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Shop")));
builder.Services.AddScoped<OrderService>();

// One document per API version: v1 describes enums as integers, v2 as wire names.
builder.Services.AddOpenApi("v1", o => o.AddEnumConventions());
builder.Services.AddOpenApi("v2", o => o.AddEnumConventions());

WebApplication app = builder.Build();

// Creates the orders table with its enum check constraints (ck_orders_status_enum, ...). Use migrations in a real app.
await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
    await scope.ServiceProvider.GetRequiredService<ShopDbContext>().Database.EnsureCreatedAsync();

app.UseExceptionHandler();
app.UseRouting();
app.UseEnumBinding();

app.MapOpenApi();

// v1: existing clients keep receiving numbers; they may already send names.
app.MapGroup("/api/v1").WithGroupName("v1").WithEnumWireFormat(EnumWireFormat.Number).MapOrders();

// v2: wire names (EnumConventions.WriteAs, the default).
app.MapGroup("/api/v2").WithGroupName("v2").MapOrders();

await app.RunAsync();
