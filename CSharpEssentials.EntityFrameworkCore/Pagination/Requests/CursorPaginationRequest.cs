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
}

public record CursorPaginationRequest<TCursor>
{
    public string? Search { get; set; }
    public TCursor? Cursor { get; set; }
    public int Limit { get; set; }
}
