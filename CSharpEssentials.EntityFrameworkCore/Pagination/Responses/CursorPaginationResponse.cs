namespace CSharpEssentials.EntityFrameworkCore.Pagination.Responses;

public record CursorPaginationResponse<T, TCursor>(IReadOnlyList<T> Items, TCursor? Next = default, bool HasMore = false);