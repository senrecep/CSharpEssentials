namespace CSharpEssentials.EntityFrameworkCore.Pagination.Requests;

public interface IPaginationRequest
{
    string? Search { get; set; }
    int PageNumber { get; set; }
    int PageSize { get; set; }

    /// <summary>
    /// Returns the number of rows to skip. The product is computed in 64-bit arithmetic and clamped to
    /// <see cref="int.MaxValue"/>, so a page beyond the addressable range yields an empty page instead of overflowing.
    /// </summary>
    int SkipCount() => (int)Math.Clamp(((long)PageNumber - 1) * PageSize, 0, int.MaxValue);
    void Normalize()
    {
        Search = Search?.Trim();
        PageNumber = Math.Max(PageNumber, 1);
        PageSize = Math.Max(PageSize, 1);
    }

    /// <summary>
    /// Normalizes the request like <see cref="Normalize()"/> and also lowers <see cref="PageSize"/> to
    /// <paramref name="maxPageSize"/>, the same way keyset pagination clamps to <c>KeysetPaginationOptions.MaxLimit</c>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxPageSize"/> is less than 1.</exception>
    void Normalize(int maxPageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxPageSize, 1);
        Normalize();
        PageSize = Math.Min(PageSize, maxPageSize);
    }
}

public record PaginationRequest : IPaginationRequest
{
    public string? Search { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
