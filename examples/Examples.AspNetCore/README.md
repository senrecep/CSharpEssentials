# CSharpEssentials.AspNetCore Example

This project demonstrates `CSharpEssentials.AspNetCore` with MVC controllers, integrated with `CSharpEssentials.Results` and `CSharpEssentials.Errors`, and uses `CSharpEssentials.AspNetCore.Swashbuckle` for the Swagger document.

## Features Demonstrated

| Feature | File | Description |
|---------|------|-------------|
| **Result Pattern** | `Services/*.cs` | Business logic returns `Result<T>` instead of throwing exceptions |
| **ProblemDetails** | `Program.cs`, `Controllers/*.cs` | `AddEnhancedProblemDetails()` and `errors.ToActionResult()` turn failures into RFC 9457 problem responses; `ToProblemDetails()` and `this.Problem(result)` are shown in `ProductsController.GetProblemDetails` |
| **Global Exception Handling** | `Program.cs` | `AddExceptionHandler<GlobalExceptionHandler>()` + `UseExceptionHandler()`: unhandled exceptions become ProblemDetails |
| **Model Validation Response** | `Program.cs` | `ConfigureModelValidatorResponse()` returns invalid model state as an enhanced problem |
| **JSON Configuration** | `Program.cs` | `ConfigureSystemTextJson(configureOptions: ...)` applies the `CSharpEssentials.Json` options with `WriteIndented` |
| **API Versioning** | `Program.cs`, `Controllers/*.cs` | `AddAndConfigureApiVersioning(...)`; URL-segment versioning (`/api/v1/products`) |
| **Swagger** | `Program.cs` | `AddSwaggerGen` with `EnumSchemaFilter` and `SwashbuckleSchemaIdFactory` (see Swagger Enum Display) |
| **Result Chaining** | `Services/OrderService.cs` | `Then()` composes multiple validation steps |

## Running the Project

```bash
cd examples/Examples.AspNetCore
ASPNETCORE_ENVIRONMENT=Development dotnet run
```

The project has no launch profile, so Kestrel listens on `http://localhost:5000` and the environment is `Production` unless you set it. Swagger is only enabled in `Development`: open `http://localhost:5000/swagger` to explore the API. (`UseHttpsRedirection()` logs `Failed to determine the https port for redirect` without an HTTPS endpoint; it does not break the requests.)

## API Endpoints

### Products

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/products?name={name}` | Search products (omit name for all) |
| GET | `/api/v1/products/{id}` | Get product by id |
| POST | `/api/v1/products` | Create a new product |
| PUT | `/api/v1/products/{id}` | Update a product |
| DELETE | `/api/v1/products/{id}` | Delete a product |

| GET | `/api/v1/products/problem/{id}` | Same lookup as `GET /{id}`, written with `ToProblemDetails()` and `this.Problem(result)` |

### Orders

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/orders` | Place an order (chained business rules) |
| GET | `/api/v1/orders/{id}` | Always returns a 404 problem: the lookup is simulated |

## Result Pattern in Action

### Before (Traditional Exception-Based)

```csharp
public Product GetById(Guid id)
{
    var product = _db.Products.Find(id);
    if (product == null)
        throw new NotFoundException($"Product {id} not found"); // Exception for expected case!
    return product;
}
```

### After (Result Pattern)

```csharp
public Result<Product> GetById(Guid id)
{
    var product = _products.FirstOrDefault(p => p.Id == id);
    if (product is null)
        return Error.NotFound($"Product with id '{id}' was not found."); // Expected failure

    return product;
}
```

### Controller Mapping

```csharp
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        return productService.GetById(id).Match(
            onSuccess: product => Ok(product),
            onError: errors => errors.ToActionResult() // ProblemDetails automatically
        );
    }
}
```

## Result Chaining

The `OrderService.PlaceOrder` method chains multiple business rules:

```csharp
public Result<Order> PlaceOrder(Guid productId, int quantity)
{
    return _productService.GetById(productId)
        .Then(product => ValidateQuantity(product, quantity))   // Must have stock
        .Then(product => ReserveStock(product, quantity))       // Must be under $10k
        .Then(product => CreateOrder(product, quantity));       // Persist order
}
```

If any step fails, the chain short-circuits and returns the first error set.

## Error Types

| Error Type | HTTP Status | Use Case |
|------------|-------------|----------|
| `Error.NotFound()` | 404 | Resource does not exist |
| `Error.Validation()` | 400 | Input validation failure |
| `Error.Conflict()` | 409 | Business rule violation |
| `Error.Unauthorized()` | 401 | Authentication required |
| `Error.Forbidden()` | 403 | Permission denied |

## Swagger Enum Display

`Program.cs` adds `options.SchemaFilter<EnumSchemaFilter>()`. `EnumSchemaFilter` (package `CSharpEssentials.AspNetCore.Swashbuckle`, namespace `CSharpEssentials.AspNetCore`) describes the enums that the enum conventions handle: `[StringEnum]` enums, whose metadata the `CSharpEssentials.Enums` generator creates. `ProductCategory` and `OrderStatus` in this project are plain enums, and the project does not reference the generator, so the filter leaves them alone and Swagger shows the framework schema:

```json
"ProductCategory": { "type": "integer", "format": "int32", "enum": [0, 1, 2, 3, 4] }
```

To get the wire names in the document and in the JSON, mark the enum with `[StringEnum]`, reference `CSharpEssentials.Enums.Generators` as an analyzer (a package reference to `CSharpEssentials.AspNetCore` already brings it), call `builder.Services.AddEnumConventions()` and use `options.AddEnumConventions()` on `SwaggerGenOptions` (`AddSwagger` does that for you). See the [Swashbuckle package README](../../CSharpEssentials.AspNetCore.Swashbuckle/Readme.MD).
