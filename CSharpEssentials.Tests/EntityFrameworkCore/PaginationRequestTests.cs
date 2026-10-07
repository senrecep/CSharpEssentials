using CSharpEssentials.EntityFrameworkCore.Pagination;
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

    [Theory]
    [InlineData(int.MaxValue, 100)]
    [InlineData(int.MaxValue, int.MaxValue)]
    [InlineData(21_474_838, 100)]
    [InlineData(2, int.MaxValue)]
    public void SkipCount_Should_ClampToIntMaxValue_When_ProductOverflowsInt(int pageNumber, int pageSize)
    {
        var request = new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize };

        int skip = ((IPaginationRequest)request).SkipCount();

        skip.Should().Be(int.MaxValue);
    }

    [Theory]
    [InlineData(21_474_836, 100, 2_147_483_500)]
    [InlineData(1, 100, 0)]
    [InlineData(0, 100, 0)]
    [InlineData(int.MinValue, int.MaxValue, 0)]
    public void SkipCount_Should_ReturnExactProduct_When_ProductFitsInt(int pageNumber, int pageSize, int expected)
    {
        var request = new PaginationRequest { PageNumber = pageNumber, PageSize = pageSize };

        int skip = ((IPaginationRequest)request).SkipCount();

        skip.Should().Be(expected);
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
    public void MaxPageSize_Should_Be100()
    {
        int max = PaginationDefaults.MaxPageSize;

        max.Should().Be(100);
    }

    [Fact]
    public void Normalize_Should_LowerPageSizeToDefaultMax_When_NoCapIsGiven()
    {
        var request = new PaginationRequest { PageNumber = 1, PageSize = 100_000 };

        ((IPaginationRequest)request).Normalize();

        request.PageSize.Should().Be(PaginationDefaults.MaxPageSize);
    }

    [Theory]
    [InlineData(99)]
    [InlineData(100)]
    public void Normalize_Should_KeepPageSize_When_AtOrBelowDefaultMax(int pageSize)
    {
        var request = new PaginationRequest { PageNumber = 1, PageSize = pageSize };

        ((IPaginationRequest)request).Normalize();

        request.PageSize.Should().Be(pageSize);
    }

    [Fact]
    public void Normalize_Should_LowerPageSizeToDefaultMax_When_PageSizeIs101()
    {
        var request = new PaginationRequest { PageNumber = 1, PageSize = 101 };

        ((IPaginationRequest)request).Normalize();

        request.PageSize.Should().Be(100);
    }

    [Fact]
    public void Normalize_WithCapAboveDefault_Should_KeepPageSizeAboveDefault()
    {
        var request = new PaginationRequest { PageNumber = 1, PageSize = 500 };

        ((IPaginationRequest)request).Normalize(1_000);

        request.PageSize.Should().Be(500);
    }

    [Fact]
    public void Normalize_WithIntMaxValueCap_Should_NotLowerLargePageSize()
    {
        var request = new PaginationRequest { PageNumber = 1, PageSize = 100_000 };

        ((IPaginationRequest)request).Normalize(int.MaxValue);

        request.PageSize.Should().Be(100_000);
    }

    [Fact]
    public void Normalize_WithCapOfOne_Should_LowerPageSizeToOne()
    {
        var request = new PaginationRequest { PageNumber = 1, PageSize = 20 };

        ((IPaginationRequest)request).Normalize(1);

        request.PageSize.Should().Be(1);
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
