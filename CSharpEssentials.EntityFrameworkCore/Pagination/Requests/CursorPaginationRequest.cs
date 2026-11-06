namespace CSharpEssentials.EntityFrameworkCore.Pagination.Requests;


public interface ICursorPaginationRequest<TCursor>
{
    string? Search { get; set; }
    TCursor? Cursor { get; set; }

    /// <summary>
    /// Whether <see cref="Cursor"/> holds a position to continue after. When <see langword="false"/>, pagination starts at
    /// the first page. A cursor equal to <c>default(TCursor)</c> (<c>0</c>, <see cref="Guid.Empty"/>) is a real position.
    /// </summary>
    bool HasCursor { get; }

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

    /// <summary>The position to continue after. Setting it, including to <c>default(TCursor)</c>, sets <see cref="HasCursor"/>; setting <see langword="null"/> clears it.</summary>
    public TCursor? Cursor
    {
        get;
        set
        {
            field = value;
            HasCursor = value is not null;
        }
    }

    /// <inheritdoc />
    public bool HasCursor { get; private set; }

    public int Limit { get; set; } = 10;
}
