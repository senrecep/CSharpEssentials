using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;
using FluentAssertions;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class PaginationRequestTests
{
    [Fact]
    public void SkipCount_ShouldCalculateCorrectly()
    {
        var request = new PaginationRequest { PageNumber = 3, PageSize = 10 };
        ((IPaginationRequest)request).SkipCount().Should().Be(20);
    }

    [Fact]
    public void Normalize_ShouldClampNegativeValues()
    {
        var request = new PaginationRequest { PageNumber = -2, PageSize = -5, Search = "  test  " };
        ((IPaginationRequest)request).Normalize();
        request.PageNumber.Should().Be(1);
        request.PageSize.Should().Be(1);
        request.Search.Should().Be("test");
    }

    [Fact]
    public void Normalize_ShouldNotAlterValidValues()
    {
        var request = new PaginationRequest { PageNumber = 2, PageSize = 20, Search = "hello" };
        ((IPaginationRequest)request).Normalize();
        request.PageNumber.Should().Be(2);
        request.PageSize.Should().Be(20);
        request.Search.Should().Be("hello");
    }

    [Fact]
    public void SkipCount_AfterNormalize_ShouldBeZeroForFirstPage()
    {
        var request = new PaginationRequest { PageNumber = 0, PageSize = 0 };
        ((IPaginationRequest)request).Normalize();
        ((IPaginationRequest)request).SkipCount().Should().Be(0);
    }
}
