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
}
