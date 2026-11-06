using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace CSharpEssentials.AspNetCore.Comparison.Benchmarks;

/// <summary>
/// Logging set up the same way on every logging host: only the middleware under test logs (Information), everything
/// else, the hosting "Request starting/finished" events included, stays below the Warning floor.
/// </summary>
internal static class LoggingHosts
{
    public const string CseCategory = "RequestResponseLogger";
    public const string HttpLoggingCategory = "Microsoft.AspNetCore.HttpLogging";
    public const string SerilogCategory = "Serilog.AspNetCore.RequestLoggingMiddleware";

    public static CountingLoggerProvider UseCountingLogger(WebApplicationBuilder builder)
    {
        CountingLoggerProvider provider = new();
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(provider);
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        builder.Logging.AddFilter(CseCategory, LogLevel.Information);
        builder.Logging.AddFilter(HttpLoggingCategory, LogLevel.Information);
        return provider;
    }

    public static CountingSerilogSink UseCountingSerilog(WebApplicationBuilder builder)
    {
        CountingSerilogSink sink = new();
        builder.Services.AddSerilog((_, configuration) => configuration
            .MinimumLevel.Warning()
            .MinimumLevel.Override(SerilogCategory, LogEventLevel.Information)
            .WriteTo.Sink(sink));
        return sink;
    }
}
