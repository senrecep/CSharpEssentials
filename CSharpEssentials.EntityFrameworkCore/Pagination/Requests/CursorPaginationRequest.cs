namespace CSharpEssentials.EntityFrameworkCore.Pagination.Requests;


public interface ICursorPaginationRequest<TCursor>
{
    string? Search { get; set; }
    TCursor? Cursor { get; set; }
    int Limit { get; set; }
    void Normalize()
    {
        Search = Search?.Trim();
        Limit = Math.Max(Limit, 1);
    }

    /// <summary>
    /// Normalizes the request like <see cref="Normalize()"/> and also lowers <see cref="Limit"/> to
    /// <paramref name="maxLimit"/>, the same way keyset pagination clamps to <c>KeysetPaginationOptions.MaxLimit</c>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLimit"/> is less than 1.</exception>
    void Normalize(int maxLimit)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLimit, 1);
        Normalize();
        Limit = Math.Min(Limit, maxLimit);
    }
}

public record CursorPaginationRequest<TCursor> : ICursorPaginationRequest<TCursor>
{
    public string? Search { get; set; }
    public TCursor? Cursor { get; set; }
    public int Limit { get; set; } = 10;
}
