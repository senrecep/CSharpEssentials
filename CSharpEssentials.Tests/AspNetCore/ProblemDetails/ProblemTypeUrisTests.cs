using CSharpEssentials.AspNetCore;
using FluentAssertions;

namespace CSharpEssentials.Tests.AspNetCore.ProblemDetailsIntegration;

public class ProblemTypeUrisTests
{
    [Theory]
    [InlineData(400, "15.5.1")]
    [InlineData(401, "15.5.2")]
    [InlineData(403, "15.5.4")]
    [InlineData(404, "15.5.5")]
    [InlineData(405, "15.5.6")]
    [InlineData(406, "15.5.7")]
    [InlineData(408, "15.5.9")]
    [InlineData(409, "15.5.10")]
    [InlineData(410, "15.5.11")]
    [InlineData(411, "15.5.12")]
    [InlineData(412, "15.5.13")]
    [InlineData(413, "15.5.14")]
    [InlineData(414, "15.5.15")]
    [InlineData(415, "15.5.16")]
    [InlineData(416, "15.5.17")]
    [InlineData(417, "15.5.18")]
    [InlineData(422, "15.5.21")]
    [InlineData(426, "15.5.22")]
    [InlineData(500, "15.6.1")]
    [InlineData(501, "15.6.2")]
    [InlineData(502, "15.6.3")]
    [InlineData(503, "15.6.4")]
    [InlineData(504, "15.6.5")]
    [InlineData(505, "15.6.6")]
    public void Rfc9110_Should_ReturnSectionLink_When_StatusCodeIsDefined(int statusCode, string section)
    {
        ProblemTypeUris.Rfc9110(statusCode).Should().Be($"https://tools.ietf.org/html/rfc9110#section-{section}");
    }

    [Theory]
    [InlineData(200)]
    [InlineData(418)]
    [InlineData(429)]
    [InlineData(599)]
    public void Rfc9110_Should_ReturnNull_When_StatusCodeHasNoMappedSection(int statusCode)
    {
        ProblemTypeUris.Rfc9110(statusCode).Should().BeNull();
    }

    [Theory]
    [InlineData(400, "rfc7231#section-6.5.1")]
    [InlineData(401, "rfc7235#section-3.1")]
    [InlineData(403, "rfc7231#section-6.5.3")]
    [InlineData(404, "rfc7231#section-6.5.4")]
    [InlineData(405, "rfc7231#section-6.5.5")]
    [InlineData(409, "rfc7231#section-6.5.8")]
    [InlineData(501, "rfc7231#section-6.6.2")]
    [InlineData(500, "rfc7231#section-6.6.1")]
    public void Rfc7231_Should_ReturnSectionLink_When_StatusCodeIsMapped(int statusCode, string link)
    {
        ProblemTypeUris.Rfc7231(statusCode).Should().Be($"https://tools.ietf.org/html/{link}");
    }

    [Theory]
    [InlineData(422)]
    [InlineData(429)]
    [InlineData(503)]
    public void Rfc7231_Should_FallBackToServerErrorLink_When_StatusCodeIsNotMapped(int statusCode)
    {
        ProblemTypeUris.Rfc7231(statusCode).Should().Be("https://tools.ietf.org/html/rfc7231#section-6.6.1");
    }
}
