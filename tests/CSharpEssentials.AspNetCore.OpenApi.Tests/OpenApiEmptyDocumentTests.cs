using FluentAssertions;
using Microsoft.OpenApi;

namespace CSharpEssentials.AspNetCore.OpenApi.Tests;

public sealed class OpenApiEmptyDocumentTests
{
    [Fact]
    public async Task OptionalRouteParameters_Should_LeaveTheDocumentEmpty_When_NoOperationIsIncluded()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync(static options =>
        {
            options.AddOptionalRouteParameters();
            options.ShouldInclude = static _ => false;
        });

        document.Paths.Should().BeEmpty();
    }

    [Fact]
    public async Task EnumConventions_Should_AddNoComponents_When_TheDocumentHasOnlyPrimitiveSchemas()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync(
            static options => options.AddEnumConventions(), controllers: []);

        document.Paths.Keys.Should().Contain("/minimal/{id}");
        document.Components?.Schemas.Should().BeNullOrEmpty();
    }
}
