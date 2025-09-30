using System;

namespace CSharpEssentials.EntityFrameworkCore.Pagination.Requests;

public interface IPaginationRequest
{
    string? Search { get; set; }
    int PageNumber { get; set; }
    int PageSize { get; set; }

    int SkipCount() => Math.Max((PageNumber - 1) * PageSize, 0);
    void Normalize()
    {
        Search = Search?.Trim();
        PageNumber = Math.Max(PageNumber, 1);
        PageSize = Math.Max(PageSize, 1);
    }
}

public record PaginationRequest : IPaginationRequest
{
    public string? Search { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
