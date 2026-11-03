using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace CSharpEssentials.Tests.AspNetCore;

public sealed class OptionalRouteParameterEmptyDocumentTests
{
    [Fact]
    public async Task Document_Should_HaveNoPaths_When_TheInclusionPredicateExcludesEveryOperation()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync(
            static options => options.DocInclusionPredicate(static (_, _) => false));

        document.Paths.Should().BeEmpty();
    }
}
