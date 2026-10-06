# CSharpEssentials.AspNetCore.Swashbuckle

Swashbuckle (Swagger) integration for `CSharpEssentials.AspNetCore`: versioned Swagger setup, schema ids and security schemes.

> One host, one OpenAPI package: reference either `CSharpEssentials.AspNetCore.Swashbuckle` (Swashbuckle, `Microsoft.OpenApi` 1.x) or `CSharpEssentials.AspNetCore.OpenApi` (`Microsoft.AspNetCore.OpenApi`, `Microsoft.OpenApi` 2.x), never both.

## Installation

```bash
dotnet add package CSharpEssentials.AspNetCore.Swashbuckle
```

Targets net8.0, net9.0, net10.0 and net11.0 with `Swashbuckle.AspNetCore` 8.1 or later (below 10).

## Swagger with Versioning

```csharp
builder.Services.AddSwagger<DefaultConfigureSwaggerOptions>(securityScheme: SecuritySchemes.JwtBearerTokenSecurity);

var app = builder.Build();
app.UseVersionableSwagger();
```

`AddSwagger` includes the XML documentation file of every assembly that declares an endpoint (controllers and Minimal API handlers), plus the optional `assembly` argument. Missing files are skipped.
