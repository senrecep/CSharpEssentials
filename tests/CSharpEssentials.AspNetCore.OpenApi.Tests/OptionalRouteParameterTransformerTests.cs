using System.Text.Json;
using System.Text.Json.Nodes;
using CSharpEssentials.Tests.AspNetCore;
using CSharpEssentials.Tests.Fixtures.OpenApiSample;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace CSharpEssentials.AspNetCore.OpenApi.Tests;

public class OptionalRouteParameterTransformerTests
{
    [Fact]
    public async Task SplitPaths_Should_Describe_Post_Without_And_With_Key_When_Action_Template_Is_Optional()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();

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
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();

        Operation(document, "/api/routed", HttpMethod.Get).OperationId.Should().Be("GetRoutedWithoutId");
        AssertRequiredPathParameters(Operation(document, "/api/routed/{id}", HttpMethod.Get), "id");
    }

    [Fact]
    public async Task SplitPaths_Should_Split_When_Trailing_Optional_Parameter_Comes_From_Controller_Route()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();

        Operation(document, "/api/scoped", HttpMethod.Get).OperationId.Should().Be("GetScopedWithoutTenant");
        AssertRequiredPathParameters(Operation(document, "/api/scoped/{tenant}", HttpMethod.Get), "tenant");
    }

    [Fact]
    public async Task SplitPaths_Should_Only_Mark_Required_When_Optional_Parameter_Is_Not_Trailing()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();

        document.Paths.Keys.Should().NotContain("/api/Tenants");
        AssertRequiredPathParameters(Operation(document, "/api/{tenant}/Tenants", HttpMethod.Get), "tenant");
    }

    [Fact]
    public async Task SplitPaths_Should_Split_When_Optional_Parameter_Has_Constraint()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();

        Operation(document, "/api/constrained", HttpMethod.Get).OperationId.Should().Be("GetConstrainedWithoutId");
        AssertRequiredPathParameters(Operation(document, "/api/constrained/{id}", HttpMethod.Get), "id");
    }

    [Fact]
    public async Task SplitPaths_Should_Split_When_Parameter_Has_Default_Value()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();

        Operation(document, "/api/pages", HttpMethod.Get).OperationId.Should().Be("GetPageWithoutPage");
        AssertRequiredPathParameters(Operation(document, "/api/pages/{page}", HttpMethod.Get), "page");
    }

    [Fact]
    public async Task SplitPaths_Should_Describe_Every_Form_When_Three_Trailing_Parameters_Are_Optional()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();

        document.Paths.Keys.Where(static key => key.StartsWith("/api/triple", StringComparison.Ordinal))
            .Should().Equal("/api/triple", "/api/triple/{a}", "/api/triple/{a}/{b}", "/api/triple/{a}/{b}/{c}");
        Operation(document, "/api/triple", HttpMethod.Get).OperationId.Should().Be("GetTripleWithoutAAndBAndC");
        Operation(document, "/api/triple/{a}", HttpMethod.Get).OperationId.Should().Be("GetTripleWithoutBAndC");
        Operation(document, "/api/triple/{a}/{b}", HttpMethod.Get).OperationId.Should().Be("GetTripleWithoutC");
        AssertRequiredPathParameters(Operation(document, "/api/triple/{a}/{b}", HttpMethod.Get), "a", "b");
        AssertRequiredPathParameters(Operation(document, "/api/triple/{a}/{b}/{c}", HttpMethod.Get), "a", "b", "c");
    }

    [Fact]
    public async Task SplitPaths_Should_Only_Mark_Required_When_More_Than_Three_Trailing_Parameters_Are_Optional()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();

        document.Paths.Keys.Where(static key => key.StartsWith("/api/deep", StringComparison.Ordinal)).Should().Equal("/api/deep/{a}/{b}/{c}/{d}");
        AssertRequiredPathParameters(Operation(document, "/api/deep/{a}/{b}/{c}/{d}", HttpMethod.Get), "a", "b", "c", "d");
    }

    [Fact]
    public async Task SplitPaths_Should_Warn_Once_When_More_Than_Three_Trailing_Parameters_Are_Optional()
    {
        using var logger = new CapturingLoggerProvider();

        await OptionalRouteParameterOpenApiHost.GetDocumentAsync(controllers: [typeof(OptionalRouteParameterControllers.DeepController)], loggerProvider: logger, passes: 2);

        logger.Messages.Should().ContainSingle().Which.Should().StartWith(
            "OpenAPI document 'v1': GET /api/deep/{a}/{b}/{c}/{d} ends with 4 optional route parameters, more than the 3 ");
    }

    [Fact]
    public async Task RequiredOnly_Should_Not_Warn_When_More_Than_Three_Trailing_Parameters_Are_Optional()
    {
        using var logger = new CapturingLoggerProvider();

        await OptionalRouteParameterOpenApiHost.GetDocumentAsync(
            static options => options.AddOptionalRouteParameters(OpenApiOptionalRouteParameterMode.RequiredOnly),
            [typeof(OptionalRouteParameterControllers.DeepController)],
            logger);

        logger.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task SplitPaths_Should_Not_Split_When_Parameter_Is_Catch_All()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();

        document.Paths.Keys.Should().NotContain(["/minimal-all", "/api/files"]);
        AssertRequiredPathParameters(Operation(document, "/minimal-all/{rest}", HttpMethod.Get), "rest");
        AssertRequiredPathParameters(Operation(document, "/api/files/{path}", HttpMethod.Get), "path");
    }

    [Fact]
    public async Task SplitPaths_Should_Keep_Existing_Operation_When_Short_Form_Collides_With_Another_Action()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();

        Operation(document, "/api/orders", HttpMethod.Get).OperationId.Should().Be("ListOrders");
        Operation(document, "/api/orders/{id}", HttpMethod.Get).OperationId.Should().Be("GetOrder");
        AllOperationIds(document).Should().NotContain("GetOrderWithoutId");
    }

    [Fact]
    public async Task SplitPaths_Should_Throw_When_Generated_OperationId_Is_Already_Used()
    {
        Func<Task> act = () => OptionalRouteParameterOpenApiHost.GetDocumentAsync(controllers: [typeof(OptionalRouteParameterControllers.ClashController)]);

        (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage("*'GetClashWithoutId'*");
    }

    [Fact]
    public async Task SplitPaths_Should_Leave_OperationId_Empty_When_Original_Has_None()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();

        Operation(document, "/api/anonymous", HttpMethod.Get).OperationId.Should().BeNull();
        Operation(document, "/api/anonymous/{id}", HttpMethod.Get).OperationId.Should().BeNull();
    }

    [Fact]
    public async Task SplitPaths_Should_Use_OperationId_Selector_When_Given()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync(static options =>
            options.AddOptionalRouteParameters(operationIdSelector: static (operationId, omitted) => operationId + "_" + string.Join("_", omitted)));

        Operation(document, "/api/archive", HttpMethod.Get).OperationId.Should().Be("GetArchive_year_month");
        Operation(document, "/api/archive/{year}", HttpMethod.Get).OperationId.Should().Be("GetArchive_month");
    }

    [Fact]
    public async Task SplitPaths_Should_Throw_When_Selector_Returns_Same_OperationId_For_Two_Forms()
    {
        Func<Task> act = () => OptionalRouteParameterOpenApiHost.GetDocumentAsync(
            static options => options.AddOptionalRouteParameters(operationIdSelector: static (operationId, _) => operationId + "Short"),
            [typeof(OptionalRouteParameterControllers.TripleController)]);

        (await act.Should().ThrowAsync<InvalidOperationException>()).WithMessage("*'GetTripleShort'*");
    }

    [Fact]
    public async Task SplitPaths_Should_Split_Minimal_Api_Routes_Under_The_Path_Without_Constraints()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();

        Operation(document, "/minimal", HttpMethod.Get).OperationId.Should().Be("GetMinimalWithoutId");
        Operation(document, "/minimal-default", HttpMethod.Get).OperationId.Should().Be("GetMinimalDefaultWithoutPage");
        AssertRequiredPathParameters(Operation(document, "/minimal/{id}", HttpMethod.Get), "id");
        AssertRequiredPathParameters(Operation(document, "/minimal-default/{page}", HttpMethod.Get), "page");
        document.Paths.Keys.Should().NotContain(static key => key.Contains(':', StringComparison.Ordinal) || key.Contains('?', StringComparison.Ordinal) || key.Contains('='));
    }

    [Fact]
    public async Task SplitPaths_Should_Copy_Operation_So_Forms_Do_Not_Share_State()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync(static options =>
        {
            options.AddOperationTransformer(static (operation, _, _) =>
            {
                operation.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer")] = ["scope"] }];
                operation.Extensions ??= new Dictionary<string, IOpenApiExtension>(StringComparer.Ordinal);
                operation.Extensions["x-test"] = new JsonNodeExtension(new JsonObject { ["value"] = "original" });
                operation.RequestBody = new OpenApiRequestBody { Description = "original" };
                return Task.CompletedTask;
            });
            options.AddOptionalRouteParameters();
        }, [typeof(OptionalRouteParameterControllers.KeysController)]);
        OpenApiOperation shortForm = Operation(document, "/api/keys", HttpMethod.Post);
        OpenApiOperation longForm = Operation(document, "/api/keys/{key}", HttpMethod.Post);

        shortForm.Tags!.Clear();
        shortForm.Security![0].Values.Single().Add("changed");
        ((JsonNodeExtension)shortForm.Extensions!["x-test"]).Node["value"] = "changed";
        var shortResponse = (OpenApiResponse)shortForm.Responses!["200"];
        shortResponse.Description = "changed";
        ((OpenApiRequestBody)shortForm.RequestBody!).Description = "changed";

        longForm.Tags.Should().ContainSingle();
        longForm.Security![0].Values.Single().Should().Equal("scope");
        ((JsonNodeExtension)longForm.Extensions!["x-test"]).Node["value"]!.GetValue<string>().Should().Be("original");
        ((OpenApiResponse)longForm.Responses!["200"]).Description.Should().Be("OK");
        longForm.RequestBody!.Description.Should().Be("original");
    }

    [Fact]
    public async Task SplitPaths_Should_Describe_Root_Path_When_Only_Segment_Is_Optional()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();

        OpenApiOperation shortForm = Operation(document, "/", HttpMethod.Get);
        shortForm.OperationId.Should().Be("GetRootWithoutSlug");
        shortForm.Parameters.Should().BeNullOrEmpty();
        AssertRequiredPathParameters(Operation(document, "/{slug}", HttpMethod.Get), "slug");
    }

    [Fact]
    public async Task SplitPaths_Should_Put_Every_Method_In_One_Path_Item_When_Methods_Share_The_Optional_Route()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();

        document.Paths["/api/notes"].Operations!.Keys.Should().BeEquivalentTo([HttpMethod.Get, HttpMethod.Post]);
        Operation(document, "/api/notes", HttpMethod.Get).OperationId.Should().Be("GetNoteWithoutId");
        Operation(document, "/api/notes", HttpMethod.Post).OperationId.Should().Be("PostNoteWithoutId");
    }

    [Fact]
    public async Task SplitPaths_Should_Add_Short_Form_To_Existing_Path_Item_When_It_Has_Another_Method()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();

        Operation(document, "/api/items", HttpMethod.Post).OperationId.Should().Be("CreateItem");
        Operation(document, "/api/items", HttpMethod.Get).OperationId.Should().Be("GetItemWithoutId");
        AssertRequiredPathParameters(Operation(document, "/api/items/{id}", HttpMethod.Get), "id");
    }

    [Fact]
    public async Task SplitPaths_Should_Only_Mark_Required_When_Optional_Parameter_Is_In_Complex_Segment()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();

        document.Paths.Keys.Where(static key => key.StartsWith("/api/downloads", StringComparison.Ordinal)).Should().Equal("/api/downloads/{name}.{ext}");
        AssertRequiredPathParameters(Operation(document, "/api/downloads/{name}.{ext}", HttpMethod.Get), "name", "ext");
    }

    [Fact]
    public async Task RequiredOnly_Should_Mark_Required_Without_Adding_Paths()
    {
        OpenApiDocument split = await OptionalRouteParameterOpenApiHost.GetDocumentAsync();
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync(
            static options => options.AddOptionalRouteParameters(OpenApiOptionalRouteParameterMode.RequiredOnly));

        document.Paths.Keys.Should().NotContain(["/api/keys", "/api/archive", "/minimal"]);
        document.Paths.Count.Should().BeLessThan(split.Paths.Count);
        AssertRequiredPathParameters(Operation(document, "/api/keys/{key}", HttpMethod.Post), "key");
        AssertRequiredPathParameters(Operation(document, "/api/archive/{year}/{month}", HttpMethod.Get), "year", "month");
        AssertRequiredPathParameters(Operation(document, "/minimal/{id}", HttpMethod.Get), "id");
    }

    [Fact]
    public async Task Document_Should_Keep_Framework_Paths_When_Not_Registered()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync(static _ => { });

        document.Paths.Keys.Should().NotContain(["/api/keys", "/api/archive", "/minimal"]);
        document.Paths.Keys.Should().Contain(["/api/keys/{key}", "/api/archive/{year}/{month}", "/minimal/{id}"]);
    }

    [Theory]
    [InlineData(OpenApiOptionalRouteParameterMode.SplitPaths, OpenApiSpecVersion.OpenApi3_0)]
    [InlineData(OpenApiOptionalRouteParameterMode.SplitPaths, OpenApiSpecVersion.OpenApi3_1)]
    [InlineData(OpenApiOptionalRouteParameterMode.RequiredOnly, OpenApiSpecVersion.OpenApi3_0)]
    [InlineData(OpenApiOptionalRouteParameterMode.RequiredOnly, OpenApiSpecVersion.OpenApi3_1)]
    public async Task Document_Should_Pass_OpenApi_Validation(OpenApiOptionalRouteParameterMode mode, OpenApiSpecVersion version)
    {
        string json = await OptionalRouteParameterOpenApiHost.GetJsonAsync(version, options => options.AddOptionalRouteParameters(mode));

        IReadOnlyList<OpenApiError> errors = Validate(json);

        errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(OpenApiOptionalRouteParameterMode.SplitPaths, OpenApiSpecVersion.OpenApi3_0, "openapi-optional-route-split-paths", null)]
    [InlineData(OpenApiOptionalRouteParameterMode.SplitPaths, OpenApiSpecVersion.OpenApi3_1, "openapi-optional-route-split-paths", "openapi31")]
    [InlineData(OpenApiOptionalRouteParameterMode.RequiredOnly, OpenApiSpecVersion.OpenApi3_0, "openapi-optional-route-required-only", null)]
    [InlineData(OpenApiOptionalRouteParameterMode.RequiredOnly, OpenApiSpecVersion.OpenApi3_1, "openapi-optional-route-required-only", "openapi31")]
    public async Task Document_Should_Match_Golden_File(OpenApiOptionalRouteParameterMode mode, OpenApiSpecVersion version, string fixture, string? suffix)
    {
        string json = await OptionalRouteParameterOpenApiHost.GetJsonAsync(version, options => options.AddOptionalRouteParameters(mode));

        Golden.Verify(OpenApiGolden.PathOf(fixture, suffix), WithoutServers(json));
    }

    // The server address has the random port of the run.
    private static string WithoutServers(string json)
    {
        var document = (JsonObject)JsonNode.Parse(json)!;
        document.Remove("servers");
        return document.ToJsonString(new JsonSerializerOptions { WriteIndented = true }).Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    [Fact]
    public async Task SplitPaths_Should_Describe_Each_Document_With_Its_Own_Operations_When_Groups_Share_A_Route()
    {
        IReadOnlyDictionary<string, string> documents = await OptionalRouteParameterOpenApiHost.GetJsonDocumentsAsync(
            OpenApiSpecVersion.OpenApi3_1,
            ["v1", "v2"],
            controllers:
            [
                typeof(OptionalRouteParameterControllers.KeysController),
                typeof(OptionalRouteParameterGroupControllers.SharedV1Controller),
                typeof(OptionalRouteParameterGroupControllers.SharedV2Controller),
                typeof(OptionalRouteParameterGroupControllers.ReportsController),
            ]);

        JsonNode v1 = JsonNode.Parse(documents["v1"])!["paths"]!;
        JsonNode v2 = JsonNode.Parse(documents["v2"])!["paths"]!;
        OperationId(v1, "/api/shared").Should().Be("GetSharedV1WithoutId");
        OperationId(v2, "/api/shared").Should().Be("GetSharedV2WithoutId");
        OperationId(v1, "/api/keys", "post").Should().Be("PostKeyWithoutKey");
        OperationId(v2, "/api/keys", "post").Should().Be("PostKeyWithoutKey");
        OperationId(v2, "/api/reports").Should().Be("GetReportWithoutYear");
        v1.AsObject().Select(static pair => pair.Key).Should().NotContain(static key => key.StartsWith("/api/reports", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(OpenApiSpecVersion.OpenApi3_0)]
    [InlineData(OpenApiSpecVersion.OpenApi3_1)]
    public async Task SplitPaths_Should_Describe_Required_Non_Null_Enum_When_Enum_Route_Parameter_Is_Optional(OpenApiSpecVersion version)
    {
        IReadOnlyDictionary<string, string> documents = await OptionalRouteParameterOpenApiHost.GetJsonDocumentsAsync(
            version,
            [OptionalRouteParameterOpenApiHost.DocumentName],
            static options => options.AddOptionalRouteParameters().AddEnumConventions(),
            [typeof(OptionalRouteParameterControllers.KeysController)],
            static services => services.AddEnumConventions(),
            static app => app.MapGet("/statuses/{status?}", static (SampleStatus? status) => status?.ToString()).WithName("GetByStatus"));
        string json = documents[OptionalRouteParameterOpenApiHost.DocumentName];

        JsonNode paths = JsonNode.Parse(json)!["paths"]!;
        JsonNode parameter = paths["/statuses/{status}"]!["get"]!["parameters"]![0]!;
        OperationId(paths, "/statuses").Should().Be("GetByStatusWithoutStatus");
        parameter["required"]!.GetValue<bool>().Should().BeTrue();
        parameter["schema"]!.ToJsonString().Should().Contain("#/components/schemas/SampleStatus")
            .And.NotContain("\"null\"").And.NotContain("nullable");
        Validate(json).Should().BeEmpty();
    }

    [Fact]
    public async Task AddOptionalRouteParameters_Should_Replace_Earlier_Call()
    {
        OpenApiDocument document = await OptionalRouteParameterOpenApiHost.GetDocumentAsync(static options =>
            options.AddOptionalRouteParameters().AddOptionalRouteParameters(OpenApiOptionalRouteParameterMode.RequiredOnly));

        document.Paths.Keys.Should().NotContain("/api/keys");
        AssertRequiredPathParameters(Operation(document, "/api/keys/{key}", HttpMethod.Post), "key");
    }

    [Fact]
    public void AddOptionalRouteParameters_Should_Throw_When_Mode_Is_Unknown()
    {
        var options = new OpenApiOptions();

        Action act = () => options.AddOptionalRouteParameters((OpenApiOptionalRouteParameterMode)42);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("mode");
    }

    private static List<OpenApiError> Validate(string json)
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

    private static string? OperationId(JsonNode paths, string path, string method = "get") =>
        paths[path]?[method]?["operationId"]?.GetValue<string>();

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
}
