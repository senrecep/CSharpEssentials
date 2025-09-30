using System;

namespace CSharpEssentials.EntityFrameworkCore.Pagination.Responses;

public record PaginationResponse<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => PageSize == 0 ? TotalCount : (int)Math.Abs(Math.Ceiling((double)TotalCount / PageSize));
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

