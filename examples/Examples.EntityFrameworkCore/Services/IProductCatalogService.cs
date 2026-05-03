using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
using Examples.EntityFrameworkCore.Data;

namespace Examples.EntityFrameworkCore.Services;

/// <summary>
/// Product catalog service demonstrating pagination and soft-delete operations.
/// </summary>
public interface IProductCatalogService
{
    PaginationResponse<Product> GetProducts(int pageNumber, int pageSize);
    void DeleteProduct(Guid id);
}
