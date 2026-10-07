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

    [Fact]
    public void Normalize_WithoutCap_ShouldNotLowerLargePageSize()
    {
        var request = new PaginationRequest { PageNumber = 1, PageSize = 100_000 };
        ((IPaginationRequest)request).Normalize();
        request.PageSize.Should().Be(100_000);
    }

    [Fact]
    public void Normalize_WithCap_ShouldKeepPageSizeAtCap()
    {
        var request = new PaginationRequest { PageNumber = 1, PageSize = 50 };
        ((IPaginationRequest)request).Normalize(50);
        request.PageSize.Should().Be(50);
    }

    [Fact]
    public void Normalize_WithCap_ShouldLowerPageSizeAboveCap()
    {
        var request = new PaginationRequest { PageNumber = -1, PageSize = 500, Search = " x " };
        ((IPaginationRequest)request).Normalize(100);
        request.PageSize.Should().Be(100);
        request.PageNumber.Should().Be(1);
        request.Search.Should().Be("x");
    }

    [Fact]
    public void Normalize_WithCap_ShouldStillRaiseNegativePageSize()
    {
        var request = new PaginationRequest { PageNumber = 1, PageSize = -5 };
        ((IPaginationRequest)request).Normalize(100);
        request.PageSize.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Normalize_WithInvalidCap_ShouldThrow(int cap)
    {
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };
        var act = () => ((IPaginationRequest)request).Normalize(cap);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
