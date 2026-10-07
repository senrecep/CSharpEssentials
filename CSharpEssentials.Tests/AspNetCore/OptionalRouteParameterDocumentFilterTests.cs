using System.Text.Json.Nodes;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.AspNetCore.Swagger.Filters;
using CSharpEssentials.Tests.Endpoints;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Reader;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CSharpEssentials.Tests.AspNetCore;

public class OptionalRouteParameterDocumentFilterTests
{
    [Fact]
    public async Task SplitPaths_Should_Describe_Post_Without_And_With_Key_When_Action_Template_Is_Optional()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        OpenApiOperation shortForm = Operation(document, "/api/keys", HttpMethod.Post);
        OpenApiOperation longForm = Operation(document, "/api/keys/{key}", HttpMethod.Post);
        shortForm.OperationId.Should().Be("PostKeyWithoutKey");
        shortForm.Parameters.Should().BeNullOrEmpty();
        longForm.OperationId.Should().Be("PostKey");
        AssertRequiredPathParameters(longForm, "key");
    }

    [Fact]
    public async Task SplitPaths_Should_Split_When_Optional_Parameter_Comes_From_Route_Attribute()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        Operation(document, "/api/routed", HttpMethod.Get).OperationId.Should().Be("GetRoutedWithoutId");
        AssertRequiredPathParameters(Operation(document, "/api/routed/{id}", HttpMethod.Get), "id");
    }

    [Fact]
    public async Task SplitPaths_Should_Split_When_Trailing_Optional_Parameter_Comes_From_Controller_Route()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        Operation(document, "/api/scoped", HttpMethod.Get).OperationId.Should().Be("GetScopedWithoutTenant");
        AssertRequiredPathParameters(Operation(document, "/api/scoped/{tenant}", HttpMethod.Get), "tenant");
    }

    [Fact]
    public async Task SplitPaths_Should_Only_Mark_Required_When_Optional_Parameter_Is_Not_Trailing()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        AssertRequiredPathParameters(Operation(document, "/api/{tenant}/Tenants", HttpMethod.Get), "tenant");
        document.Paths.Keys.Should().NotContain(key => key.EndsWith("/Tenants", StringComparison.Ordinal) && key != "/api/{tenant}/Tenants");
    }

    [Fact]
    public async Task SplitPaths_Should_Split_When_Optional_Parameter_Has_Constraint()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        Operation(document, "/api/constrained", HttpMethod.Get).OperationId.Should().Be("GetConstrainedWithoutId");
        AssertRequiredPathParameters(Operation(document, "/api/constrained/{id}", HttpMethod.Get), "id");
    }

    [Fact]
    public async Task SplitPaths_Should_Split_When_Parameter_Has_Default_Value()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        OpenApiOperation shortForm = Operation(document, "/api/pages", HttpMethod.Get);
        shortForm.OperationId.Should().Be("GetPageWithoutPage");
        shortForm.Parameters.Should().BeNullOrEmpty();
        AssertRequiredPathParameters(Operation(document, "/api/pages/{page}", HttpMethod.Get), "page");
    }

    [Fact]
    public async Task SplitPaths_Should_Describe_Each_Form_Shortest_First_When_Two_Trailing_Parameters_Are_Optional()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        document.Paths.Keys.Where(static key => key.StartsWith("/api/archive", StringComparison.Ordinal))
            .Should().Equal("/api/archive", "/api/archive/{year}", "/api/archive/{year}/{month}");
        Operation(document, "/api/archive", HttpMethod.Get).OperationId.Should().Be("GetArchiveWithoutYearAndMonth");
        Operation(document, "/api/archive/{year}", HttpMethod.Get).OperationId.Should().Be("GetArchiveWithoutMonth");
        AssertRequiredPathParameters(Operation(document, "/api/archive/{year}", HttpMethod.Get), "year");
        AssertRequiredPathParameters(Operation(document, "/api/archive/{year}/{month}", HttpMethod.Get), "year", "month");
    }

    [Fact]
    public async Task SplitPaths_Should_Only_Mark_Required_When_More_Than_Three_Trailing_Parameters_Are_Optional()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        document.Paths.Keys.Where(static key => key.StartsWith("/api/deep", StringComparison.Ordinal)).Should().Equal("/api/deep/{a}/{b}/{c}/{d}");
        AssertRequiredPathParameters(Operation(document, "/api/deep/{a}/{b}/{c}/{d}", HttpMethod.Get), "a", "b", "c", "d");
    }

    [Fact]
    public async Task SplitPaths_Should_Warn_Once_When_More_Than_Three_Trailing_Parameters_Are_Optional()
    {
        using var logs = new CapturingLoggerProvider();
        await using WebApplication app = await OptionalRouteParameterHost.StartAsync(null, OptionalRouteParameterControllers.Compliant, logs);
        ISwaggerProvider provider = app.Services.GetRequiredService<ISwaggerProvider>();

        provider.GetSwagger("v1");
        provider.GetSwagger("v1");

        logs.Entries.Where(static entry => entry.Category.EndsWith(".OptionalRouteParameterDocumentFilter", StringComparison.Ordinal))
            .Should().ContainSingle()
            .Which.Should().Match<(string Category, LogLevel Level, string Message)>(static entry =>
                entry.Level == LogLevel.Warning
                && entry.Message.StartsWith("OpenAPI document 'v1': GET /api/deep/{a}/{b}/{c}/{d} ends with 4 optional route parameters, more than the 3 ", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RequiredOnly_Should_Not_Warn_When_More_Than_Three_Trailing_Parameters_Are_Optional()
    {
        using var logs = new CapturingLoggerProvider();

        await OptionalRouteParameterHost.GetDocumentAsync(static options => options.AddOptionalRouteParameters(OptionalRouteParameterMode.RequiredOnly), loggerProvider: logs);

        logs.Entries.Should().NotContain(static entry => entry.Category.EndsWith(".OptionalRouteParameterDocumentFilter", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SplitPaths_Should_Not_Split_When_Parameter_Is_Catch_All()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        document.Paths.Keys.Should().NotContain(["/minimal-all", "/api/files"]);
        AssertRequiredPathParameters(Operation(document, "/minimal-all/{rest}", HttpMethod.Get), "rest");
        AssertRequiredPathParameters(Operation(document, "/api/files/{path}", HttpMethod.Get), "path");
    }

    [Fact]
    public async Task SplitPaths_Should_Keep_Existing_Operation_When_Short_Form_Collides_With_Another_Action()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        Operation(document, "/api/orders", HttpMethod.Get).OperationId.Should().Be("ListOrders");
        Operation(document, "/api/orders/{id}", HttpMethod.Get).OperationId.Should().Be("GetOrder");
        AllOperationIds(document).Should().NotContain("GetOrderWithoutId");
    }

    [Fact]
    public async Task SplitPaths_Should_Throw_When_Generated_OperationId_Is_Already_Used()
    {
        Func<Task> act = () => OptionalRouteParameterHost.GetDocumentAsync(controllers: [typeof(OptionalRouteParameterControllers.ClashController)]);

        (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage("*'GetClashWithoutId'*");
    }

    [Fact]
    public async Task SplitPaths_Should_Leave_OperationId_Empty_When_Original_Has_None()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        Operation(document, "/api/anonymous", HttpMethod.Get).OperationId.Should().BeNull();
        Operation(document, "/api/anonymous/{id}", HttpMethod.Get).OperationId.Should().BeNull();
    }

    [Fact]
    public async Task SplitPaths_Should_Use_OperationId_Selector_When_Given()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync(static options =>
            options.AddOptionalRouteParameters(operationIdSelector: static (operationId, omitted) => operationId + "_" + string.Join("_", omitted)));

        Operation(document, "/api/archive", HttpMethod.Get).OperationId.Should().Be("GetArchive_year_month");
        Operation(document, "/api/archive/{year}", HttpMethod.Get).OperationId.Should().Be("GetArchive_month");
    }

    [Fact]
    public async Task SplitPaths_Should_Split_Minimal_Api_Routes()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        Operation(document, "/minimal", HttpMethod.Get).OperationId.Should().Be("GetMinimalWithoutId");
        Operation(document, "/minimal-default", HttpMethod.Get).OperationId.Should().Be("GetMinimalDefaultWithoutPage");
        AssertRequiredPathParameters(Operation(document, "/minimal/{id}", HttpMethod.Get), "id");
        AssertRequiredPathParameters(Operation(document, "/minimal-default/{page}", HttpMethod.Get), "page");
    }

    [Fact]
    public async Task SplitPaths_Should_Copy_Operation_So_Forms_Do_Not_Share_State()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync(static options => options.OperationFilter<DecorateOperationFilter>());
        OpenApiOperation shortForm = Operation(document, "/api/keys", HttpMethod.Post);
        OpenApiOperation longForm = Operation(document, "/api/keys/{key}", HttpMethod.Post);

        shortForm.Tags!.Clear();
        shortForm.Security![0].Values.Single().Add("changed");
        ((JsonNodeExtension)shortForm.Extensions!["x-test"]).Node["value"] = "changed";
        var shortResponse = (OpenApiResponse)shortForm.Responses!["200"];
        shortResponse.Description = "changed";
        shortResponse.Content!.Values.First().Example = "changed";
        ((OpenApiRequestBody)shortForm.RequestBody!).Description = "changed";

        longForm.Tags.Should().ContainSingle();
        longForm.Security![0].Values.Single().Should().Equal("scope");
        ((JsonNodeExtension)longForm.Extensions!["x-test"]).Node["value"]!.GetValue<string>().Should().Be("original");
        var longResponse = (OpenApiResponse)longForm.Responses!["200"];
        longResponse.Description.Should().Be("OK");
        longResponse.Content!.Values.First().Example.Should().BeNull();
        longForm.RequestBody!.Description.Should().Be("original");
    }

    [Fact]
    public async Task SplitPaths_Should_Describe_Every_Form_When_Three_Trailing_Parameters_Are_Optional()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        document.Paths.Keys.Where(static key => key.StartsWith("/api/triple", StringComparison.Ordinal))
            .Should().Equal("/api/triple", "/api/triple/{a}", "/api/triple/{a}/{b}", "/api/triple/{a}/{b}/{c}");
        Operation(document, "/api/triple", HttpMethod.Get).OperationId.Should().Be("GetTripleWithoutAAndBAndC");
        Operation(document, "/api/triple/{a}", HttpMethod.Get).OperationId.Should().Be("GetTripleWithoutBAndC");
        Operation(document, "/api/triple/{a}/{b}", HttpMethod.Get).OperationId.Should().Be("GetTripleWithoutC");
        AssertRequiredPathParameters(Operation(document, "/api/triple/{a}/{b}", HttpMethod.Get), "a", "b");
        AssertRequiredPathParameters(Operation(document, "/api/triple/{a}/{b}/{c}", HttpMethod.Get), "a", "b", "c");
    }

    [Fact]
    public async Task SplitPaths_Should_Describe_Root_Path_When_Only_Segment_Is_Optional()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        OpenApiOperation shortForm = Operation(document, "/", HttpMethod.Get);
        shortForm.OperationId.Should().Be("GetRootWithoutSlug");
        shortForm.Parameters.Should().BeNullOrEmpty();
        AssertRequiredPathParameters(Operation(document, "/{slug}", HttpMethod.Get), "slug");
    }

    [Fact]
    public async Task SplitPaths_Should_Put_Every_Method_In_One_Path_Item_When_Methods_Share_The_Optional_Route()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        document.Paths["/api/notes"].Operations!.Keys.Should().BeEquivalentTo([HttpMethod.Get, HttpMethod.Post]);
        Operation(document, "/api/notes", HttpMethod.Get).OperationId.Should().Be("GetNoteWithoutId");
        Operation(document, "/api/notes", HttpMethod.Post).OperationId.Should().Be("PostNoteWithoutId");
    }

    [Fact]
    public async Task SplitPaths_Should_Add_Short_Form_To_Existing_Path_Item_When_It_Has_Another_Method()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        Operation(document, "/api/items", HttpMethod.Post).OperationId.Should().Be("CreateItem");
        Operation(document, "/api/items", HttpMethod.Get).OperationId.Should().Be("GetItemWithoutId");
        AssertRequiredPathParameters(Operation(document, "/api/items/{id}", HttpMethod.Get), "id");
    }

    [Fact]
    public async Task SplitPaths_Should_Only_Mark_Required_When_Optional_Parameter_Is_In_Complex_Segment()
    {
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync();

        document.Paths.Keys.Where(static key => key.StartsWith("/api/downloads", StringComparison.Ordinal)).Should().Equal("/api/downloads/{name}.{ext}");
        AssertRequiredPathParameters(Operation(document, "/api/downloads/{name}.{ext}", HttpMethod.Get), "name", "ext");
    }

    [Fact]
    public async Task SplitPaths_Should_Throw_When_Selector_Returns_Same_OperationId_For_Two_Forms()
    {
        Func<Task> act = () => OptionalRouteParameterHost.GetDocumentAsync(
            static options => options.AddOptionalRouteParameters(operationIdSelector: static (operationId, _) => operationId + "Short"),
            [typeof(OptionalRouteParameterControllers.TripleController)]);

        (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage("*'GetTripleShort'*");
    }

    [Fact]
    public async Task RequiredOnly_Should_Mark_Required_Without_Adding_Paths()
    {
        OpenApiDocument split = await OptionalRouteParameterHost.GetDocumentAsync();
        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync(static options => options.AddOptionalRouteParameters(OptionalRouteParameterMode.RequiredOnly));

        document.Paths.Keys.Should().NotContain(["/api/keys", "/api/archive", "/minimal"]);
        document.Paths.Count.Should().BeLessThan(split.Paths.Count);
        AssertRequiredPathParameters(Operation(document, "/api/keys/{key}", HttpMethod.Post), "key");
        AssertRequiredPathParameters(Operation(document, "/api/archive/{year}/{month}", HttpMethod.Get), "year", "month");
    }

    [Theory]
    [InlineData(OptionalRouteParameterMode.SplitPaths, OpenApiSpecVersion.OpenApi3_0)]
    [InlineData(OptionalRouteParameterMode.SplitPaths, OpenApiSpecVersion.OpenApi3_1)]
    [InlineData(OptionalRouteParameterMode.RequiredOnly, OpenApiSpecVersion.OpenApi3_0)]
    [InlineData(OptionalRouteParameterMode.RequiredOnly, OpenApiSpecVersion.OpenApi3_1)]
    public async Task Document_Should_Pass_OpenApi_Validation(OptionalRouteParameterMode mode, OpenApiSpecVersion version)
    {
        string json = await OptionalRouteParameterHost.GetJsonAsync(version, options => options.AddOptionalRouteParameters(mode));

        IReadOnlyList<OpenApiError> errors = Validate(json);

        errors.Should().BeEmpty();
    }

    [Fact]
    public async Task AddOptionalRouteParameters_Should_Replace_Earlier_Registration()
    {
        SwaggerGenOptions? registered = null;

        OpenApiDocument document = await OptionalRouteParameterHost.GetDocumentAsync(options =>
        {
            options.AddOptionalRouteParameters().AddOptionalRouteParameters(OptionalRouteParameterMode.RequiredOnly);
            registered = options;
        });

        registered!.DocumentFilterDescriptors.Should().ContainSingle(static descriptor => descriptor.Type.Name == "OptionalRouteParameterDocumentFilter");
        registered.OperationFilterDescriptors.Should().NotContain(static descriptor => descriptor.Type == typeof(ReApplyOptionalRouteParameterOperationFilter));
        document.Paths.Keys.Should().NotContain("/api/keys");
    }

    [Fact]
    public void AddOptionalRouteParameters_Should_Throw_When_Mode_Is_Unknown()
    {
        var options = new SwaggerGenOptions();

        Action act = () => options.AddOptionalRouteParameters((OptionalRouteParameterMode)42);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("mode");
    }

    internal static IReadOnlyList<OpenApiError> Validate(string json)
    {
        ReadResult result = OpenApiDocument.Parse(json, OpenApiConstants.Json);
        List<OpenApiError> errors = [.. result.Diagnostic?.Errors ?? []];
        if (result.Document is { } document)
            errors.AddRange(document.Validate(ValidationRuleSet.GetDefaultRuleSet()) ?? []);
        return errors;
    }

    private static OpenApiOperation Operation(OpenApiDocument document, string path, HttpMethod method)
    {
        document.Paths.Should().ContainKey(path);
        document.Paths[path].Operations.Should().ContainKey(method);
        return document.Paths[path].Operations![method];
    }

    private static IEnumerable<string?> AllOperationIds(OpenApiDocument document) =>
        document.Paths.Values.SelectMany(static item => item.Operations!.Values).Select(static operation => operation.OperationId);

    private static void AssertRequiredPathParameters(OpenApiOperation operation, params string[] names)
    {
        List<OpenApiParameter> parameters = [.. (operation.Parameters ?? []).OfType<OpenApiParameter>().Where(static parameter => parameter.In == ParameterLocation.Path)];
        parameters.Select(static parameter => parameter.Name).Should().Equal(names);
        parameters.Should().OnlyContain(static parameter => parameter.Required && !parameter.AllowEmptyValue);
        parameters.Select(static parameter => parameter.Schema).OfType<OpenApiSchema>()
            .Should().OnlyContain(static schema => schema.Default == null && (schema.Type == null || !schema.Type.Value.HasFlag(JsonSchemaType.Null)));
    }

    private sealed class DecorateOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer")] = ["scope"] }];
            operation.Extensions ??= new Dictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
            operation.Extensions["x-test"] = new JsonNodeExtension(new JsonObject { ["value"] = "original" });
            operation.RequestBody = new OpenApiRequestBody
            {
                Description = "original",
                Content = new Dictionary<string, OpenApiMediaType>(StringComparer.Ordinal) { ["text/plain"] = new() },
            };
        }
    }
}
