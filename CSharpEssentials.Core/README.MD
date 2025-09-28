# CSharpEssentials.Core

[![NuGet](https://img.shields.io/nuget/v/CSharpEssentials.Core.svg)](https://www.nuget.org/packages/CSharpEssentials.Core) [![NuGet](https://img.shields.io/nuget/dt/CSharpEssentials.Core.svg)](https://www.nuget.org/packages/CSharpEssentials.Core) [![GitHub](https://img.shields.io/github/stars/senrecep/CSharpEssentials.svg)](https://github.com/senrecep/CSharpEssentials)

Core utilities and extensions for C# development. This is the foundation package for all CSharpEssentials libraries.

## Features

### String Extensions

- **IsNullOrEmpty/IsNullOrWhiteSpace checks**
- **String manipulation utilities**
- **Validation helpers**

### Collection Extensions

- **Enhanced LINQ operations**
- **Collection manipulation utilities**
- **Safe enumeration helpers**

### General Extensions

- **Object utilities**
- **Type checking helpers**
- **Conversion utilities**

### HTTP Constants

- **Standard HTTP status codes**
- **Common HTTP headers**
- **Content type constants**

### GUID Utilities

- **GUID generation helpers**
- **GUID validation**
- **Short GUID implementations**

## Installation

```bash
dotnet add package CSharpEssentials.Core
```

## Usage

```csharp
using CSharpEssentials.Extensions;
using CSharpEssentials.Constants;
using CSharpEssentials.Guids;

// String extensions
var text = "Hello World";
if (!text.IsNullOrEmpty())
{
    // Safe string operations
}

// Collection extensions
var items = new[] { 1, 2, 3, 4, 5 };
var evenItems = items.WhereNotNull();

// HTTP constants
var statusCode = HttpCodes.OK; // 200
var contentType = HttpCodes.ApplicationJson;

// GUID utilities
var shortGuid = Guider.NewGuid();
```

## Dependencies

- **System.Text.Json** - For JSON serialization support

## Related Packages

This is the foundation package. Other CSharpEssentials packages build on this:

- `CSharpEssentials.Errors` - Error handling
- `CSharpEssentials.Results` - Result pattern
- `CSharpEssentials.Json` - JSON utilities
- And more...

## License

MIT
