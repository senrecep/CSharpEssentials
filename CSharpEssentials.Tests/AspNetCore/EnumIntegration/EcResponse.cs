using System.Text.Json;
using FluentAssertions;

namespace CSharpEssentials.Tests.AspNetCore.EnumIntegration;

internal sealed record EcResponse(int Status, string Body, string? ContentType, IReadOnlyList<string> Vary, string? Location)
{
    /// <summary>Asserts a 400 problem response and returns its errors.</summary>
    public IReadOnlyList<(string Code, string Description)> ShouldBeProblem()
    {
        Status.Should().Be(400, Body);
        ContentType.Should().Be("application/problem+json");
        using JsonDocument document = JsonDocument.Parse(Body);
        return [.. document.RootElement.GetProperty("errors").EnumerateArray()
            .Select(e => (e.GetProperty("code").GetString()!, e.GetProperty("description").GetString()!))];
    }

    /// <summary>Asserts a 200 response and returns the JSON value of <paramref name="property"/> as raw text.</summary>
    public string Json(string property)
    {
        Status.Should().Be(200, Body);
        using JsonDocument document = JsonDocument.Parse(Body);
        return document.RootElement.GetProperty(property).GetRawText();
    }
}
