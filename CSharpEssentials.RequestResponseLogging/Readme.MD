# CSharpEssentials.RequestResponseLogging

- **NuGet Package RequestResponseLogging**: [CSharpEssentials.RequestResponseLogging](https://www.nuget.org/packages/CSharpEssentials.RequestResponseLogging) [![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.RequestResponseLogging.svg)](https://www.nuget.org/packages/CSharpEssentials.RequestResponseLogging)

A powerful and flexible request/response logging middleware for ASP.NET Core applications that enables comprehensive HTTP traffic monitoring with minimal setup.

## Features

- 🚀 Easy integration with ASP.NET Core applications
- 📝 Configurable logging fields (Request, Response, Headers, Timing, etc.)
- 🎯 Path-based filtering for ignored routes
- 🔧 Customizable header logging
- ⚡ High performance with minimal overhead
- 🎨 Flexible logging options
- 🔄 Support for custom request/response handlers

## Installation

```bash
dotnet add package CSharpEssentials.RequestResponseLogging
```

## Quick Start

```csharp
using CSharpEssentials.RequestResponseLogging;

public void Configure(IApplicationBuilder app)
{
    app.AddRequestResponseLogging(opt =>
    {
        // Basic configuration
        opt.UseLogger(app.Services.GetRequiredService<ILoggerFactory>());
    });
}
```

## Advanced Configuration

```csharp
app.AddRequestResponseLogging(opt =>
{
    // Ignore specific paths
    opt.IgnorePaths("/health");
    opt.IgnorePaths("/swagger");

    // Configure logging options
    var loggingOptions = LoggingOptions.CreateAllFields();

    // Add specific headers to log
    loggingOptions.HeaderKeys.Add("Accept-Language");
    loggingOptions.HeaderKeys.Add("X-Tenant-Id");

    // Configure logging level and fields
    loggingOptions.LoggingLevel = LogLevel.Information;
    loggingOptions.UseSeparateContext = true;

    // Use the configured logger
    opt.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), loggingOptions);
});
```

## Available Logging Fields

The library supports logging the following fields:

- `Request`: Full request body
- `Response`: Full response body
- `HostName`: Server hostname
- `Path`: Request path
- `Method`: HTTP method
- `QueryString`: URL query parameters
- `Headers`: HTTP headers (configurable)
- `ResponseTiming`: Request processing duration
- `RequestLength`: Request body length
- `ResponseLength`: Response body length

## Configuration Options

### LoggingOptions

```csharp
var options = new LoggingOptions
{
    LoggingLevel = LogLevel.Information,
    UseSeparateContext = true,
    LoggerCategoryName = "CustomRequestResponseLogger",
    LoggingFields = new List<LogFields>
    {
        LogFields.Method,
        LogFields.Path,
        LogFields.Headers,
        LogFields.ResponseTiming
    }
};
```

### Ignoring Paths

You can ignore specific paths to prevent logging:

```csharp
opt.IgnorePaths("/health");
opt.IgnorePaths("/metrics");
opt.IgnorePaths("/swagger");
```

### Custom Header Logging

Configure specific headers to be logged:

```csharp
loggingOptions.HeaderKeys.Add("Authorization");
loggingOptions.HeaderKeys.Add("X-Correlation-Id");
loggingOptions.HeaderKeys.Add("Custom-Header");
```

## Use Cases

1. **API Monitoring**

   ```csharp
   app.AddRequestResponseLogging(opt =>
   {
       var loggingOptions = LoggingOptions.CreateAllFields();
       loggingOptions.LoggingLevel = LogLevel.Debug;
       opt.UseLogger(loggerFactory, loggingOptions);
   });
   ```

2. **Performance Monitoring**

   ```csharp
   app.AddRequestResponseLogging(opt =>
   {
        opt.IgnorePaths("/health");
        opt.IgnorePaths("/swagger");
        var loggingOptions = LoggingOptions.CreateAllFields();
        loggingOptions.HeaderKeys.Add(HeaderNames.AcceptLanguage);
        loggingOptions.HeaderKeys.Add(CustomHeaderNames.TenantId);
        opt.UseLogger(app.Services.GetRequiredService<ILoggerFactory>(), loggingOptions);
   });
   ```

3. **Security Monitoring**
   ```csharp
   app.AddRequestResponseLogging(opt =>
   {
       var loggingOptions = new LoggingOptions
       {
           LoggingFields = new List<LogFields>
           {
               LogFields.Headers,
               LogFields.Method,
               LogFields.Path
           }
       };
       loggingOptions.HeaderKeys.Add("Authorization");
       loggingOptions.HeaderKeys.Add("X-API-Key");
       opt.UseLogger(loggerFactory, loggingOptions);
   });
   ```

## Best Practices

1. Always ignore health check and swagger endpoints to reduce noise
2. Configure appropriate logging levels based on environment
3. Be selective with header logging to avoid security issues
4. Use separate context for better log organization
5. Consider performance impact when logging full request/response bodies

## Acknowledgments

This project is based on and enhances [TechBuddySolution](https://github.com/salihcantekin/TechBuddySolution) by Salih Cantekin. We've built upon the original codebase to add additional features, improve performance, and enhance the overall functionality while maintaining the core concepts of the original implementation.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
