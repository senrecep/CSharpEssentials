namespace CSharpEssentials.EntityFrameworkCore.Pagination;

/// <summary>Default limits of offset pagination.</summary>
public static class PaginationDefaults
{
    /// <summary>
    /// Default upper bound of <see cref="Requests.IPaginationRequest.PageSize"/>: 100. Applied by
    /// <see cref="Requests.IPaginationRequest.Normalize()"/> and by the offset <c>PaginateAsync</c>/<c>Paginate</c>
    /// overloads unless another <c>maxPageSize</c> is passed. Matches <c>KeysetPaginationOptions.DefaultMaxLimit</c>.
    /// </summary>
    public const int MaxPageSize = 100;
}
