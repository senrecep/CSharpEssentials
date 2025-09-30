namespace CSharpEssentials.EntityFrameworkCore.Pagination.Responses;

public record CursorPaginationResponse<T, TCursor>(List<T> Items, TCursor? Cursor = default, bool HasMore = false);