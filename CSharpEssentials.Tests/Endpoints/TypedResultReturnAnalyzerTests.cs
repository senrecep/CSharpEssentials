using System.Collections.Immutable;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using CSharpEssentials.Tests.Generators;
using FluentAssertions;
using Microsoft.CodeAnalysis;

namespace CSharpEssentials.Tests.Endpoints;

public class TypedResultReturnAnalyzerTests
{
    private const string Usings = """
        using System.Threading.Tasks;
        using CSharpEssentials.AspNetCore;
        using CSharpEssentials.Errors;
        using CSharpEssentials.ResultPattern;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Http;
        using Microsoft.AspNetCore.Routing;

        namespace Sample.Api;

        public sealed record OrderDto(int Id);

        public static class Orders
        {
            public static Result<OrderDto, NotFoundError> Get(int id) => new OrderDto(id);

            public static Task<Result<OrderDto, NotFoundError, ConflictError>> GetAsync(int id) =>
                Task.FromResult<Result<OrderDto, NotFoundError, ConflictError>>(new OrderDto(id));

            public static ValueTask<Result<OrderDto, NotFoundError, ConflictError, ValidationError>> GetValueAsync(int id) =>
                new(new OrderDto(id));

            public static Result<OrderDto> GetClassic(int id) => new OrderDto(id);
        }

        """;

    [Fact]
    public async Task Analyzer_Should_ReportCse1013_When_AHandlerReturnsATypedResult()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            public static class Api
            {
                private static Result<OrderDto, NotFoundError> Handler(int id) => Orders.Get(id);

                public static void Map(IEndpointRouteBuilder app)
                {
                    app.MapGet("/lambda/{id}", (int id) => Orders.Get(id));
                    app.MapGet("/task/{id}", (int id) => Orders.GetAsync(id));
                    app.MapPost("/async/{id}", async (int id) => await Orders.GetAsync(id));
                    app.MapPut("/valuetask/{id}", (int id) => Orders.GetValueAsync(id));
                    app.MapGet("/method/{id}", Handler);
                    app.MapMethods("/methods/{id}", ["PATCH"], (int id) => Orders.Get(id));
                }
            }
            """);

        diagnostics.Select(static diagnostic => diagnostic.Id).Should().Equal(Enumerable.Repeat("CSE1013", 6));
        diagnostics.Select(static diagnostic => diagnostic.Severity).Should().AllBeEquivalentTo(DiagnosticSeverity.Warning);
        diagnostics[0].GetMessage(System.Globalization.CultureInfo.InvariantCulture).Should().Contain("Result<OrderDto, NotFoundError>");
    }

    [Fact]
    public async Task Analyzer_Should_NotReport_When_TheHandlerConvertsOrReturnsAnotherType()
    {
        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync("""
            public static class Api
            {
                public static void Map(IEndpointRouteBuilder app)
                {
                    app.MapGet("/converted/{id}", (int id) => Orders.Get(id).ToHttpResult());
                    app.MapGet("/task/{id}", (int id) => Orders.GetAsync(id).ToHttpResult());
                    app.MapGet("/created/{id}", (int id) => Orders.GetValueAsync(id).ToCreated(o => $"/orders/{o.Id}"));
                    app.MapGet("/classic/{id}", (int id) => Orders.GetClassic(id));
                    app.MapGet("/plain", () => "plain");
                    app.MapGet("/void", () => { });
                }
            }
            """);

        diagnostics.Should().BeEmpty();
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source)
    {
        MetadataReference[] references =
        [
            .. new[] { typeof(Result<,>), typeof(IError), typeof(HttpResultExtensions), typeof(CSharpEssentials.Core.HttpCodes) }
                .Select(static type => (MetadataReference)MetadataReference.CreateFromFile(type.Assembly.Location)),
        ];
        Compilation compilation = EndpointCompilations.Create("Sample.Api", OutputKind.DynamicallyLinkedLibrary, references, Usings + source);
        compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        ImmutableArray<Diagnostic> diagnostics = await AnalyzerHarness.GetAnalyzerDiagnosticsAsync(
            compilation, EndpointCompilations.Generators.Value.Analyzers);
        return [.. diagnostics.Where(static diagnostic => diagnostic.Id == "CSE1013")];
    }
}
