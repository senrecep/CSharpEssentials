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

## Enum schemas

```csharp
builder.Services.AddEnumConventions();
builder.Services.AddSwaggerGen(o => o.AddEnumConventions()); // AddSwagger calls it
```

Enums handled by the enum conventions (`[StringEnum]` or generated metadata) are described the way they are written: one component per enum with the wire names in `enum`, `x-enum-varnames`, `x-enum-descriptions`, `x-enum-numeric-values` and a value table appended to your description (obsolete members `(deprecated)`, the `[EnumFallback]` member `Response only:`). `default` is the wire name, `[Flags]` enums are arrays with `uniqueItems: true`, and nullable is written where the enum is used (`allOf` + `nullable: true`, OpenAPI 3.0).

A document whose operations all write numbers describes the enums as `integer`; a document that mixes formats describes strings and marks each number operation with `x-enum-wire-format: number` and a note; it also logs one warning per enum used in both formats (once per document). Header selected operations get `x-enum-wire-format-header` and a note; a selector that throws for a request without the header is described with the global `WriteAs` format and logged once as a warning. Enums without `[StringEnum]` or metadata keep the Swashbuckle schema.

The schemas match those of `CSharpEssentials.AspNetCore.OpenApi` (both are compared with the same golden files).

### From 4.x

The Swagger code moved here from `CSharpEssentials.AspNetCore`; namespaces and the `AddSwagger` API are unchanged. `EnumSchemaFilter` now takes an `IServiceProvider` (Swashbuckle creates it); register it with `AddEnumConventions()` instead of `SchemaFilter<EnumSchemaFilter>()`.
