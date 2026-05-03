using CSharpEssentials.RequestResponseLogging;
using Examples.RequestResponseLogging.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// CSharpEssentials.RequestResponseLogging - CONFIGURATION
// ============================================================================

// Add controllers for demo endpoints
builder.Services.AddControllers();

// Add a simple health check endpoint (excluded from logging via IgnorePaths)
builder.Services.AddHealthChecks();

var app = builder.Build();

// ============================================================================
// MIDDLEWARE PIPELINE
// ============================================================================

// The request/response logging middleware MUST be placed early in the pipeline
// so it wraps the entire request lifecycle, including all downstream middleware.
app.AddRequestResponseLogging(options =>
{
    // Ignore health check endpoints — no need to log these
    options.IgnorePaths("/health");

    // Use the built-in logger with custom options
    options.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), loggingOptions =>
    {
        loggingOptions.LoggingLevel = LogLevel.Information;
        loggingOptions.UseSeparateContext = true;
        loggingOptions.LoggerCategoryName = "RequestResponseLogger";
    });

    // Alternative: Use a custom handler for full control over logging output
    // options.UseHandler(async context =>
    // {
    //     await StructuredJsonLogWriter.WriteAsync(context, logger);
    // });
});

app.UseRouting();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
