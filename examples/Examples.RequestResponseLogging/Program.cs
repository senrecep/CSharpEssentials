using CSharpEssentials.RequestResponseLogging;
using Examples.RequestResponseLogging.Infrastructure;
using Microsoft.Net.Http.Headers;

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
app.AddRequestResponseLogging(opt =>
{
    opt.IgnorePaths("/health");
    var loggingOptions = LoggingOptions.CreateAllFields();
    loggingOptions.HeaderKeys.Add(HeaderNames.AcceptLanguage);
    opt.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), loggingOptions);
});
app.UseRouting();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
