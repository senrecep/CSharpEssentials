using Asp.Versioning;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.AspNetCore.Swagger.Filters;
using Examples.AspNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAndConfigureApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CSharpEssentials.AspNetCore Example API",
        Version = "v1",
        Description = "Demonstrates Result pattern, ProblemDetails, API versioning, and exception handling."
    });

    options.SchemaFilter<EnumSchemaFilter>();

    options.CustomSchemaIds(new SwashbuckleSchemaIdFactory().GetSchemaId);

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEnhancedProblemDetails();

builder.Services.ConfigureModelValidatorResponse();

builder.Services.ConfigureSystemTextJson(configureOptions: options =>
{
    options.WriteIndented = true;
});

builder.Services.AddControllers();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CSharpEssentials Example API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
