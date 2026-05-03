using Asp.Versioning;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.AspNetCore.Swagger.Filters;
using Examples.AspNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// CSharpEssentials.AspNetCore - SERVICE CONFIGURATION
// ============================================================================

// 1. API VERSIONING
//    Adds versioning support with URL segment or header-based versioning.
//    Enables consumers to call /v1/products or /v2/products.
builder.Services.AddAndConfigureApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

// 2. SWAGGER / OPENAPI
//    Configures Swagger with enum schema filtering so enum values display
//    as friendly strings (e.g. "Validation" instead of "2") in Swagger UI.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CSharpEssentials.AspNetCore Example API",
        Version = "v1",
        Description = "Demonstrates Result pattern, ProblemDetails, API versioning, and exception handling."
    });

    // Add enum schema filter from CSharpEssentials.AspNetCore
    options.SchemaFilter<EnumSchemaFilter>();

    // Use custom schema ID factory to avoid conflicts
    options.CustomSchemaIds(new SwashbuckleSchemaIdFactory().GetSchemaId);

    // Include XML comments for Swagger documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// 3. GLOBAL EXCEPTION HANDLER
//    Registers the built-in GlobalExceptionHandler that converts ALL unhandled
//    exceptions into RFC 7807 ProblemDetails responses.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// 4. ENHANCED PROBLEM DETAILS
//    Configures ProblemDetails with custom mappings for CSharpEssentials errors.
builder.Services.AddEnhancedProblemDetails();

// 5. MODEL VALIDATION RESPONSE
//    Configures model validation to return EnhancedProblemDetails
builder.Services.ConfigureModelValidatorResponse();

// 6. JSON CONFIGURATION
//    Configures System.Text.Json with enhanced options from CSharpEssentials.Json
builder.Services.ConfigureSystemTextJson(configureOptions: options =>
{
    options.WriteIndented = true;
});

// 7. CONTROLLERS
builder.Services.AddControllers();

// 8. APPLICATION SERVICES (using Result pattern internally)
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

// ============================================================================
// CSharpEssentials.AspNetCore - MIDDLEWARE PIPELINE
// ============================================================================

// 1. EXCEPTION HANDLER
//    Must be early in the pipeline to catch exceptions from downstream middleware.
app.UseExceptionHandler();

// 2. SWAGGER UI (development only)
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
