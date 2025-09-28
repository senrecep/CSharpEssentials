# CSharpEssentials.AspNetCore

[![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.AspNetCore.svg)](https://www.nuget.org/packages/CSharpEssentials.AspNetCore) [![NuGet](https://img.shields.io/nuget/dt/CSharpEssentials.AspNetCore.svg)](https://www.nuget.org/packages/CSharpEssentials.AspNetCore) [![GitHub](https://img.shields.io/github/stars/senrecep/CSharpEssentials.svg)](https://github.com/senrecep/CSharpEssentials)

A comprehensive ASP.NET Core library that provides essential building blocks and utilities for modern web applications, focusing on API development best practices and common patterns.

## Features

- 🔄 **API Versioning**: Simplified API versioning setup with built-in support for URL and header-based versioning
- 🛡️ **Global Exception Handling**: Centralized exception handling with enhanced problem details
- 📝 **Swagger Integration**: Advanced Swagger/OpenAPI configuration with versioning support
- ⚡ **Problem Details**: Enhanced problem details implementation with validation support
- 🔧 **JSON Configuration**: Extended JSON serialization options and utilities

## Installation

```bash
dotnet add package CSharpEssentials.AspNetCore
```

## Usage

### Complete Setup Example

```csharp
// In Program.cs or Startup.cs ConfigureServices
public void ConfigureServices(IServiceCollection services)
{
    services
            .AddExceptionHandler<GlobalExceptionHandler>()
            .ConfigureModelValidatorResponse()
            .ConfigureSystemTextJson()
            .AddEnhancedProblemDetails()
            .AddAndConfigureApiVersioning()
            .AddSwagger<DefaultConfigureSwaggerOptions>(
                SecuritySchemes.JwtBearerTokenSecurity,
                Assembly.GetExecutingAssembly()
            );
}

// In Program.cs or Startup.cs Configure
public void Configure(IApplicationBuilder app)
{
    app.UseVersionableSwagger();
    app.UseExceptionHandler();
    app.UseStatusCodePages();
}
```

### API Versioning Setup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add API versioning with enhanced configuration
    services.AddAndConfigureApiVersioning();
}
```

### Global Exception Handling and Problem Details

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
            .AddExceptionHandler<GlobalExceptionHandler>()
            .ConfigureModelValidatorResponse()
            .AddEnhancedProblemDetails();
}

public void Configure(IApplicationBuilder app)
{
    app.UseExceptionHandler();
    app.UseStatusCodePages();
}
```

### Swagger Configuration with Security

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddSwagger<DefaultConfigureSwaggerOptions>(
        SecuritySchemes.JwtBearerTokenSecurity,  // Use JWT Bearer authentication
        Assembly.GetExecutingAssembly()
    );
}

public void Configure(IApplicationBuilder app)
{
    app.UseVersionableSwagger();
}
```

### JSON Configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers()
        .ConfigureSystemTextJson();  // Applies optimized JSON settings
}
```

## Best Practices

1. Always use the global exception handler for consistent error responses
2. Implement API versioning from the start of your project
3. Use the `ValidateModel` attribute on controllers that require model validation
4. Configure Swagger documentation for better API discoverability
5. Utilize the enhanced problem details for standardized error responses
6. Configure request/response logging early in the pipeline
7. Use appropriate security schemes based on your authentication requirements

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the terms of the license included in the repository.

## Support

For support, please open an issue in the GitHub repository or contact the maintainers.
