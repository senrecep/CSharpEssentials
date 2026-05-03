# CSharpEssentials.AspNetCore Example

This project demonstrates the complete feature set of `CSharpEssentials.AspNetCore` integrated with `CSharpEssentials.Results` and `CSharpEssentials.Errors`.

## Features Demonstrated

| Feature | File | Description |
|---------|------|-------------|
| **Result Pattern** | `Services/*.cs` | Business logic returns `Result<T>` instead of throwing exceptions |
| **ProblemDetails** | `Program.cs` | Automatic RFC 7807 error responses for all failures |
| **Swagger Enum Filter** | `Program.cs` | Enum values display as readable strings in Swagger UI |
| **API Versioning** | `Program.cs`, `Controllers/*.cs` | URL-segment versioning (`/api/v1/products`) |
| **Global Exception Handling** | `Program.cs` | Unhandled exceptions become ProblemDetails automatically |
| **Result Chaining** | `OrderService.cs` | `Then()` composes multiple validation steps |

## Running the Project

```bash
cd examples/Examples.AspNetCore
dotnet run
```

Open `https://localhost:5001/swagger` to explore the API.

## API Endpoints

### Products

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/products?name={name}` | Search products (omit name for all) |
| GET | `/api/v1/products/{id}` | Get product by id |
| POST | `/api/v1/products` | Create a new product |
| PUT | `/api/v1/products/{id}` | Update a product |
| DELETE | `/api/v1/products/{id}` | Delete a product |

### Orders

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/orders` | Place an order |

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

    return Result.Success(product);
}
```

### Controller Mapping

```csharp
[HttpGet("{id:guid}")]
public IActionResult GetById(Guid id)
{
    return _productService.GetById(id).Match(
        onSuccess: product => Ok(product),
        onFailure: errors => errors.ToActionResult() // ProblemDetails automatically
    );
}
```

## Result Chaining

The `OrderService.PlaceOrder` method chains multiple business rules:

```csharp
return _productService.GetById(productId)
    .Then(product => ValidateQuantity(product, quantity))   // Must have stock
    .Then(product => ReserveStock(product, quantity))       // Must be under $10k
    .Then(product => CreateOrder(product, quantity));       // Persist order
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

The `EnumSchemaFilter` from `CSharpEssentials.AspNetCore` transforms this:

```json
"category": { "type": "integer", "enum": [0, 1, 2, 3, 4] }
```

Into this:

```json
"category": { "type": "string", "enum": ["Electronics", "Clothing", "Food", "Books", "Home"] }
```

Making the API self-documenting.
