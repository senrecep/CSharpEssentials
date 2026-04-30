using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
using FluentAssertions;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class CursorPaginationResponseTests
{
    [Fact]
    public void CursorPaginationResponse_ShouldStoreItems()
    {
        List<int> items = [1, 2, 3, 4, 5];
        CursorPaginationResponse<int, string> response = new(items, "cursor123", true);

        response.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void CursorPaginationResponse_ShouldStoreNextCursor()
    {
        List<int> items = [1, 2, 3];
        string cursor = "next-cursor-abc";
        CursorPaginationResponse<int, string> response = new(items, cursor, true);

        response.Next.Should().Be(cursor);
    }

    [Fact]
    public void CursorPaginationResponse_ShouldStoreHasMore()
    {
        List<int> items = [1, 2, 3];
        CursorPaginationResponse<int, string> response = new(items, "cursor", true);

        response.HasMore.Should().BeTrue();
    }

    [Fact]
    public void CursorPaginationResponse_WithNoMoreItems_ShouldHaveHasMoreFalse()
    {
        List<int> items = [1, 2, 3];
        CursorPaginationResponse<int, string> response = new(items, null, false);

        response.HasMore.Should().BeFalse();
    }

    [Fact]
    public void CursorPaginationResponse_WithNullCursor_ShouldWork()
    {
        List<int> items = [1, 2, 3];
        CursorPaginationResponse<int, string> response = new(items);

        response.Next.Should().BeNull();
        response.HasMore.Should().BeFalse();
    }

    [Fact]
    public void CursorPaginationResponse_WithDefaultValues_ShouldWork()
    {
        List<int> items = [1, 2, 3];
        CursorPaginationResponse<int, int> response = new(items);

        response.Items.Should().BeEquivalentTo(items);
        response.Next.Should().Be(default(int));
        response.HasMore.Should().BeFalse();
    }

    [Fact]
    public void CursorPaginationResponse_WithGuidCursor_ShouldWork()
    {
        List<string> items = ["Item1", "Item2"];
        var cursor = Guid.NewGuid();
        CursorPaginationResponse<string, Guid> response = new(items, cursor, true);

        response.Next.Should().Be(cursor);
    }

    [Fact]
    public void CursorPaginationResponse_WithDateTimeCursor_ShouldWork()
    {
        List<string> items = ["Item1", "Item2"];
        DateTime cursor = DateTime.UtcNow;
        CursorPaginationResponse<string, DateTime> response = new(items, cursor, true);

        response.Next.Should().Be(cursor);
    }

    [Fact]
    public void CursorPaginationResponse_WithEmptyItems_ShouldWork()
    {
        List<int> items = [];
        CursorPaginationResponse<int, string> response = new(items, null, false);

        response.Items.Should().BeEmpty();
        response.HasMore.Should().BeFalse();
    }
}

