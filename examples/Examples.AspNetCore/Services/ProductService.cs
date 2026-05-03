using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;

namespace Examples.AspNetCore.Services;

/// <summary>
/// Concrete implementation of IProductService.
/// Shows how Result pattern eliminates exceptions for expected failure cases.
/// </summary>
public sealed class ProductService : IProductService
{
    // In-memory store for demo purposes
    private static readonly List<Product> _products = new()
    {
        new(Guid.NewGuid(), "Laptop", "High-performance laptop", 1499.99m, 10, ProductCategory.Electronics, DateTime.UtcNow.AddDays(-30)),
        new(Guid.NewGuid(), "T-Shirt", "Cotton t-shirt", 19.99m, 100, ProductCategory.Clothing, DateTime.UtcNow.AddDays(-15)),
        new(Guid.NewGuid(), "Pizza", "Frozen pizza", 5.99m, 50, ProductCategory.Food, DateTime.UtcNow.AddDays(-7)),
    };

    public Result<Product> GetById(Guid id)
    {
        // Result.Failure with a domain error — NOT an exception.
        // Exceptions are for exceptional cases; "not found" is expected.
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product is null)
        {
            return Error.NotFound($"Product with id '{id}' was not found.");
        }

        return Result.Success(product);
    }

    public Result<Product> Create(CreateProductRequest request)
    {
        // -------------------------------------------------------------------
        // VALIDATION with Result pattern
        // Instead of throwing ValidationException, collect errors into a Result.
        // -------------------------------------------------------------------
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(Error.Validation("Product.Name", "Name is required."));

        if (request.Name.Length > 100)
            errors.Add(Error.Validation("Product.Name", "Name must not exceed 100 characters."));

        if (request.Price <= 0)
            errors.Add(Error.Validation("Product.Price", "Price must be greater than zero."));

        if (request.StockQuantity < 0)
            errors.Add(Error.Validation("Product.StockQuantity", "Stock quantity cannot be negative."));

        if (errors.Count > 0)
            return Result.Failure<Product>(errors.ToArray());

        var product = new Product(
            Guid.NewGuid(),
            request.Name.Trim(),
            request.Description.Trim(),
            request.Price,
            request.StockQuantity,
            request.Category,
            DateTime.UtcNow
        );

        _products.Add(product);
        return Result.Success(product);
    }

    public Result<Product> Update(Guid id, UpdateProductRequest request)
    {
        var existing = _products.FirstOrDefault(p => p.Id == id);
        if (existing is null)
            return Error.NotFound($"Product with id '{id}' was not found.");

        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add(Error.Validation("Product.Name", "Name is required."));

        if (request.Price <= 0)
            errors.Add(Error.Validation("Product.Price", "Price must be greater than zero."));

        if (errors.Count > 0)
            return Result.Failure<Product>(errors.ToArray());

        var updated = existing with
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity
        };

        _products[_products.IndexOf(existing)] = updated;
        return Result.Success(updated);
    }

    public Result Delete(Guid id)
    {
        var existing = _products.FirstOrDefault(p => p.Id == id);
        if (existing is null)
            return Error.NotFound($"Product with id '{id}' was not found.");

        _products.Remove(existing);
        return Result.Success();
    }

    public Result<IReadOnlyList<Product>> Search(string? name)
    {
        // Empty search returns all products — still a success.
        if (string.IsNullOrWhiteSpace(name))
            return Result.Success<IReadOnlyList<Product>>(_products);

        var filtered = _products
            .Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Result.Success<IReadOnlyList<Product>>(filtered);
    }
}
