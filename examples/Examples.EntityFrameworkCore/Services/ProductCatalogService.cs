using CSharpEssentials.EntityFrameworkCore.Pagination;
using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;
using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
using Examples.EntityFrameworkCore.Data;

namespace Examples.EntityFrameworkCore.Services;

/// <summary>
/// Demonstrates pagination extension methods from CSharpEssentials.EntityFrameworkCore.
/// </summary>
public class ProductCatalogService : IProductCatalogService
{
    private readonly ShopDbContext _dbContext;

    public ProductCatalogService(ShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public PaginationResponse<Product> GetProducts(int pageNumber, int pageSize)
    {
        // -------------------------------------------------------------------
        // PaginateAsync extension from CSharpEssentials.EntityFrameworkCore
        // Automatically handles:
        //   - Total count query
        //   - Skip/Take calculation
        //   - HasPreviousPage / HasNextPage flags
        // -------------------------------------------------------------------
        var request = new PaginationRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return _dbContext.Products
            .OrderBy(p => p.Name)
            .PaginateAsync(request)
            .GetAwaiter()
            .GetResult();
    }

    public void DeleteProduct(Guid id)
    {
        var product = _dbContext.Products.Find(id);
        if (product is not null)
        {
            // Soft delete: mark IsDeleted = true so the query filter hides it,
            // but the row stays in the database for audit / undo purposes.
            product.MarkAsDeleted(DateTimeOffset.UtcNow, "system");
            _dbContext.SaveChanges();
        }
    }
}
