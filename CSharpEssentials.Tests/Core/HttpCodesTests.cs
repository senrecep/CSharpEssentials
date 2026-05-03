using FluentAssertions;
using CSharpEssentials.Core;

namespace CSharpEssentials.Tests.Core;

public class HttpCodesTests
{
    [Fact]
    public void HttpCodes_ShouldHaveCorrectValues()
    {
        HttpCodes.BadRequest.Should().Be(400);
        HttpCodes.Unauthorized.Should().Be(401);
        HttpCodes.Forbidden.Should().Be(403);
        HttpCodes.NotFound.Should().Be(404);
        HttpCodes.Conflict.Should().Be(409);
        HttpCodes.InternalServerError.Should().Be(500);
    }

    [Fact]
    public void HttpCodes_ShouldBeConstants()
    {
        typeof(HttpCodes).GetFields()
            .Where(f => f.IsPublic && f.IsStatic && f.IsLiteral)
            .Should().NotBeEmpty();
    }
}

