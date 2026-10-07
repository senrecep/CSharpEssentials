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

    /// <summary>
    /// Normalizes the request like <see cref="Normalize(int)"/> with
    /// <see cref="PaginationDefaults.MaxPageSize"/> (100) as the cap.
    /// </summary>
    void Normalize() => Normalize(PaginationDefaults.MaxPageSize);

    /// <summary>
    /// Trims <see cref="Search"/>, raises <see cref="PageNumber"/> and <see cref="PageSize"/> to 1 and lowers
    /// <see cref="PageSize"/> to <paramref name="maxPageSize"/>, the same way keyset pagination clamps to
    /// <c>KeysetPaginationOptions.MaxLimit</c>. Pass <see cref="int.MaxValue"/> to turn the cap off.
    /// </summary>
    /// <remarks>
    /// This method does not call <see cref="Normalize()"/>, because <see cref="Normalize()"/> applies the default
    /// cap of <see cref="PaginationDefaults.MaxPageSize"/> and would lower a larger <paramref name="maxPageSize"/>.
    /// The pagination extensions call this overload, so an implementation that adds its own rules must override
    /// this method, not only <see cref="Normalize()"/>.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxPageSize"/> is less than 1.</exception>
    void Normalize(int maxPageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxPageSize, 1);
        Search = Search?.Trim();
        PageNumber = Math.Max(PageNumber, 1);
        PageSize = Math.Clamp(PageSize, 1, maxPageSize);
    }
}

public record PaginationRequest : IPaginationRequest
{
    public string? Search { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
