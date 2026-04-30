using CSharpEssentials.EntityFrameworkCore.Pagination.Responses;
using FluentAssertions;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class PaginationResponseTests
{
    [Fact]
    public void PaginationResponse_ShouldCalculateTotalPages_Correctly()
    {
        List<int> items = [1, 2, 3, 4, 5];
        PaginationResponse<int> response = new(items, 1, 10, 50);

        response.TotalPages.Should().Be(5);
    }

    [Fact]
    public void PaginationResponse_ShouldCalculateTotalPages_WithPartialPage()
    {
        List<int> items = [1, 2, 3, 4, 5];
        PaginationResponse<int> response = new(items, 1, 10, 53);

        response.TotalPages.Should().Be(6);
    }

    [Fact]
    public void PaginationResponse_ShouldCalculateTotalPages_WithZeroPageSize()
    {
        List<int> items = [1, 2, 3];
        PaginationResponse<int> response = new(items, 1, 0, 100);

        response.TotalPages.Should().Be(100);
    }

    [Fact]
    public void HasPreviousPage_ShouldBeFalse_OnFirstPage()
    {
        List<int> items = [1, 2, 3];
        PaginationResponse<int> response = new(items, 1, 10, 100);

        response.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasPreviousPage_ShouldBeTrue_OnSecondPage()
    {
        List<int> items = [1, 2, 3];
        PaginationResponse<int> response = new(items, 2, 10, 100);

        response.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_ShouldBeTrue_WhenNotOnLastPage()
    {
        List<int> items = [1, 2, 3];
        PaginationResponse<int> response = new(items, 1, 10, 100);

        response.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void HasNextPage_ShouldBeFalse_WhenOnLastPage()
    {
        List<int> items = [1, 2, 3];
        PaginationResponse<int> response = new(items, 10, 10, 100);

        response.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_ShouldBeFalse_WhenBeyondLastPage()
    {
        List<int> items = [1, 2, 3];
        PaginationResponse<int> response = new(items, 15, 10, 100);

        response.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void Items_ShouldContainProvidedData()
    {
        List<string> items = ["Item1", "Item2", "Item3"];
        PaginationResponse<string> response = new(items, 1, 10, 100);

        response.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void PaginationResponse_ShouldStorePageNumber()
    {
        List<int> items = [1, 2, 3];
        PaginationResponse<int> response = new(items, 5, 10, 100);

        response.PageNumber.Should().Be(5);
    }

    [Fact]
    public void PaginationResponse_ShouldStorePageSize()
    {
        List<int> items = [1, 2, 3];
        PaginationResponse<int> response = new(items, 1, 25, 100);

        response.PageSize.Should().Be(25);
    }

    [Fact]
    public void PaginationResponse_ShouldStoreTotalCount()
    {
        List<int> items = [1, 2, 3];
        PaginationResponse<int> response = new(items, 1, 10, 150);

        response.TotalCount.Should().Be(150);
    }
}

