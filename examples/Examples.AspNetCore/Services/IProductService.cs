using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace Examples.AspNetCore.Services;

/// <summary>
/// Product service contract demonstrating the Result pattern.
/// Every operation returns a Result so callers explicitly handle success/failure.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// Returns Result.Failure if the product does not exist.
    /// </summary>
    Result<Product> GetById(Guid id);

    /// <summary>
    /// Creates a new product.
    /// Returns Result.Failure with validation errors if the input is invalid.
    /// </summary>
    Result<Product> Create(CreateProductRequest request);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    Result<Product> Update(Guid id, UpdateProductRequest request);

    /// <summary>
    /// Deletes a product by id.
    /// </summary>
    Result Delete(Guid id);

    /// <summary>
    /// Searches products by name.
    /// Returns an empty list (success) when nothing matches.
    /// </summary>
    Result<IReadOnlyList<Product>> Search(string? name);
}

// ---------------------------------------------------------------------------
// Domain models for the service layer
// ---------------------------------------------------------------------------

public sealed record Product(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    ProductCategory Category,
    DateTime CreatedAt
);

public enum ProductCategory
{
    Electronics,
    Clothing,
    Food,
    Books,
    Home
}

public sealed record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    ProductCategory Category
);

public sealed record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int StockQuantity
);
