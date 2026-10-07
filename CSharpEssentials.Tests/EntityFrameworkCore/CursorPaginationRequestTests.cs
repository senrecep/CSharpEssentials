using CSharpEssentials.EntityFrameworkCore.Pagination.Requests;
using FluentAssertions;

namespace CSharpEssentials.Tests.EntityFrameworkCore;

public class CursorPaginationRequestTests
{
    [Fact]
    public void Normalize_ShouldClampNegativeLimit()
    {
        var request = new CursorPaginationRequest<int> { Limit = -5, Search = "  test  " };
        ((ICursorPaginationRequest<int>)request).Normalize();
        request.Limit.Should().Be(1);
        request.Search.Should().Be("test");
    }

    [Fact]
    public void Normalize_ShouldNotAlterValidValues()
    {
        var request = new CursorPaginationRequest<DateTime> { Limit = 25, Search = "hello" };
        ((ICursorPaginationRequest<DateTime>)request).Normalize();
        request.Limit.Should().Be(25);
        request.Search.Should().Be("hello");
    }

    [Fact]
    public void DefaultLimit_ShouldBeTen()
    {
        var request = new CursorPaginationRequest<Guid>();
        request.Limit.Should().Be(10);
    }

    [Fact]
    public void Normalize_WithoutCap_ShouldNotLowerLargeLimit()
    {
        var request = new CursorPaginationRequest<int> { Limit = 100_000 };
        ((ICursorPaginationRequest<int>)request).Normalize();
        request.Limit.Should().Be(100_000);
    }

    [Fact]
    public void Normalize_WithCap_ShouldKeepLimitAtCap()
    {
        var request = new CursorPaginationRequest<int> { Limit = 50 };
        ((ICursorPaginationRequest<int>)request).Normalize(50);
        request.Limit.Should().Be(50);
    }

    [Fact]
    public void Normalize_WithCap_ShouldLowerLimitAboveCap()
    {
        var request = new CursorPaginationRequest<int> { Limit = 500, Search = " x " };
        ((ICursorPaginationRequest<int>)request).Normalize(100);
        request.Limit.Should().Be(100);
        request.Search.Should().Be("x");
    }

    [Fact]
    public void Normalize_WithCap_ShouldStillRaiseNegativeLimit()
    {
        var request = new CursorPaginationRequest<int> { Limit = -5 };
        ((ICursorPaginationRequest<int>)request).Normalize(100);
        request.Limit.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Normalize_WithInvalidCap_ShouldThrow(int cap)
    {
        var request = new CursorPaginationRequest<int> { Limit = 10 };
        var act = () => ((ICursorPaginationRequest<int>)request).Normalize(cap);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
