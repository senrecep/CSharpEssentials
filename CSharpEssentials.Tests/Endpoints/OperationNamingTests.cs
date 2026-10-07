using System.Reflection;
using CSharpEssentials.Endpoints;
using CSharpEssentials.Tests.Generators;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;

namespace CSharpEssentials.Tests.Endpoints;

public class OperationNamingTests
{
    private const string DuplicateSource = """
        using CSharpEssentials.Endpoints;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Routing;

        namespace Sample.Duplicate;

        public sealed class Ping : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/ROUTE", () => "pong");
        }
        """;

    private const string ReservedSource = """
        using CSharpEssentials.Endpoints;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Routing;

        namespace Sample.Reserved;

        public sealed class Items : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/items", () => "items");
        }

        public sealed class Archive : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/archive", () => "archive").WithName("ArchiveItems");
        }

        public static class Startup
        {
            public static void Configure(IEndpointRouteBuilder app)
            {
                app.MapSampleReservedEndpoints(static options => options.OperationNaming = OperationNaming.TypeName);
                app.MapGet("/x", () => "x").WithName("Items");
            }
        }
        """;

    [Fact]
    public void TypeName_Should_Suffix_Http_Method_When_Type_Maps_Several_Routes()
    {
        using WebApplication app = EndpointTestApp.Create();

        Map<NamingEndpoints.Items>(app);

        Names(app).Should().Equal(
            ("/naming/items", "NamingEndpoints_Items_Get"),
            ("/naming/items", "NamingEndpoints_Items_Post"));
    }

    [Fact]
    public void TypeName_Should_Number_Routes_When_Http_Methods_Repeat()
    {
        using WebApplication app = EndpointTestApp.Create();

        Map<NamingEndpoints.Reports>(app);

        Names(app).Should().Equal(
            ("/naming/reports/daily", "NamingEndpoints_Reports_Get_1"),
            ("/naming/reports/weekly", "NamingEndpoints_Reports_Get_2"));
    }

    [Fact]
    public void TypeName_Should_Include_Containing_Types_When_Endpoint_Is_Nested()
    {
        using WebApplication app = EndpointTestApp.Create();

        Map<NamingEndpoints.Orders.Endpoint>(app);
        Map<NamingEndpoints.Users.Endpoint>(app);

        Names(app).Should().Equal(
            ("/naming/orders/{id:int}", "NamingEndpoints_Orders_Endpoint"),
            ("/naming/users/{id:int}", "NamingEndpoints_Users_Endpoint"));
    }

    [Fact]
    public void TypeName_Should_Qualify_With_Namespace_When_Type_Names_Collide()
    {
        using WebApplication app = EndpointTestApp.Create();

        Map<NamingA.Lookup>(app);
        Map<NamingB.Lookup>(app);

        Names(app).Should().Equal(
            ("/naming/a/lookup", "CSharpEssentials_Tests_Endpoints_NamingA_Lookup"),
            ("/naming/b/lookup", "CSharpEssentials_Tests_Endpoints_NamingB_Lookup"));
    }

    [Fact]
    public void TypeName_Should_Disambiguate_When_Name_Is_Taken_By_Explicit_Name()
    {
        using WebApplication app = EndpointTestApp.Create();

        Map<NamingEndpoints.EchoClash>(app);
        Map<NamingEndpoints.Echo>(app);

        Names(app).Should().Equal(
            ("/naming/echo-clash", "NamingEndpoints_Echo"),
            ("/naming/echo", "NamingEndpoints_Echo_2"));
    }

    [Fact]
    public void TypeName_Should_Keep_Names_Stable_When_Endpoints_Are_Enumerated_Again()
    {
        using WebApplication app = EndpointTestApp.Create();
        Map<NamingEndpoints.Items>(app);

        (string Route, string Name)[] first = Names(app);

        Names(app).Should().Equal(first);
    }

    [Fact]
    public void TypeName_Should_Throw_When_Different_Types_Share_Full_Name()
    {
        Assembly first = Load("Sample.Duplicate.First", DuplicateSource.Replace("ROUTE", "first", StringComparison.Ordinal));
        Assembly second = Load("Sample.Duplicate.Second", DuplicateSource.Replace("ROUTE", "second", StringComparison.Ordinal));
        using WebApplication app = EndpointTestApp.Create();

        Action map = () => app.MapEndpointsFromAssemblies(static options => options.OperationNaming = OperationNaming.TypeName, first, second);

        map.Should().Throw<InvalidOperationException>()
            .Which.Message.Should().Contain("Sample.Duplicate.First").And.Contain("Sample.Duplicate.Second").And.Contain("Sample.Duplicate.Ping");
    }

    [Fact]
    public async Task TypeName_Should_Produce_Distinct_ApiExplorer_And_OpenApi_Operation_Ids()
    {
        await using WebApplication app = CreateDocumentedApp();
        MapAll(app);
        await app.StartAsync();

        string?[] apiExplorerNames = [.. app.Services.GetRequiredService<IApiDescriptionGroupCollectionProvider>()
            .ApiDescriptionGroups.Items.SelectMany(static group => group.Items)
            .Select(static description => description.ActionDescriptor.EndpointMetadata.OfType<IEndpointNameMetadata>().LastOrDefault()?.EndpointName)];
        string[] operationIds = [.. app.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1")
            .Paths.Values.SelectMany(static path => path.Operations!.Values).Select(static operation => operation.OperationId!)];

        apiExplorerNames.Should().HaveCount(10).And.OnlyHaveUniqueItems().And.NotContainNulls();
        operationIds.Should().BeEquivalentTo(apiExplorerNames);
        operationIds.Should().Contain(
            ["NamingEndpoints_Items_Get", "NamingEndpoints_Items_Post", "NamingEndpoints_Orders_Endpoint", "NamingEndpoints_Users_Endpoint"]);
    }

    [Fact]
    public async Task TypeName_Should_Generate_Links_By_Generated_Name()
    {
        await using WebApplication app = CreateDocumentedApp();
        MapAll(app);
        await app.StartAsync();
        LinkGenerator links = app.Services.GetRequiredService<LinkGenerator>();

        links.GetPathByName("NamingEndpoints_Orders_Endpoint", new RouteValueDictionary { ["id"] = 5 }).Should().Be("/naming/orders/5");
        links.GetPathByName("NamingEndpoints_Users_Endpoint", new RouteValueDictionary { ["id"] = 7 }).Should().Be("/naming/users/7");
        links.GetPathByRouteValues("NamingEndpoints_Items_Post", new RouteValueDictionary()).Should().Be("/naming/items");
    }

    [Fact]
    public async Task TypeName_Should_Suffix_Generated_Name_When_Plain_Endpoint_Name_Is_Reserved_At_Build_Time()
    {
        Assembly assembly = LoadGenerated("Sample.Reserved", ReservedSource);
        await using WebApplication app = CreateDocumentedApp();
        assembly.GetType("Sample.Reserved.Startup", throwOnError: true)!.GetMethod("Configure")!.Invoke(null, [app]);
        await app.StartAsync();
        LinkGenerator links = app.Services.GetRequiredService<LinkGenerator>();

        Names(app).Should().BeEquivalentTo([("/archive", "ArchiveItems"), ("/items", "Items_2"), ("/x", "Items")]);
        links.GetPathByName("Items", new RouteValueDictionary()).Should().Be("/x");
        links.GetPathByName("Items_2", new RouteValueDictionary()).Should().Be("/items");
        links.GetPathByName("ArchiveItems", new RouteValueDictionary()).Should().Be("/archive");
    }

    [Fact]
    public void ReserveEndpointNames_Should_Ignore_Names_When_OperationNaming_Is_Not_TypeName()
    {
        using WebApplication app = EndpointTestApp.Create();

        EndpointMapper.ReserveEndpointNames(app, new EndpointMappingOptions(), ["NamingEndpoints_Echo"]);
        Map<NamingEndpoints.Echo>(app);

        Names(app).Should().Equal(("/naming/echo", "NamingEndpoints_Echo"));
    }

    [Fact]
    public void ReserveEndpointNames_Should_Suffix_Generated_Name_When_Name_Is_Reserved()
    {
        using WebApplication app = EndpointTestApp.Create();

        Map<NamingEndpoints.Echo>(app);
        EndpointMapper.ReserveEndpointNames(app, new EndpointMappingOptions { OperationNaming = OperationNaming.TypeName }, ["NamingEndpoints_Echo"]);

        Names(app).Should().Equal(("/naming/echo", "NamingEndpoints_Echo_2"));
    }

    private static void Map<TEndpoint>(IEndpointRouteBuilder app)
        where TEndpoint : IEndpoint =>
        EndpointMapper.MapEndpoint<TEndpoint>(app, null, new EndpointMappingOptions { OperationNaming = OperationNaming.TypeName });

    private static void MapAll(IEndpointRouteBuilder app)
    {
        Map<NamingEndpoints.Items>(app);
        Map<NamingEndpoints.Reports>(app);
        Map<NamingEndpoints.Echo>(app);
        Map<NamingEndpoints.EchoClash>(app);
        Map<NamingEndpoints.Orders.Endpoint>(app);
        Map<NamingEndpoints.Users.Endpoint>(app);
        Map<NamingA.Lookup>(app);
        Map<NamingB.Lookup>(app);
    }

    private static (string Route, string Name)[] Names(IEndpointRouteBuilder app) =>
        [.. EndpointTestApp.Endpoints(app).Select(static endpoint => (
            endpoint.RoutePattern.RawText!,
            endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()!.EndpointName))];

    private static WebApplication CreateDocumentedApp()
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(static options => options.SwaggerDoc("v1", new OpenApiInfo { Title = "Naming", Version = "v1" }));
        return builder.Build();
    }

    private static Assembly LoadGenerated(string assemblyName, string source)
    {
        using MemoryStream stream = new();
        GeneratorRun run = EndpointCompilations.Run(EndpointCompilations.Create(assemblyName, OutputKind.DynamicallyLinkedLibrary, source));
        EmitResult result = run.OutputCompilation.Emit(stream);
        result.Success.Should().BeTrue(string.Join(Environment.NewLine, result.Diagnostics));
        return Assembly.Load(stream.ToArray());
    }

    private static Assembly Load(string assemblyName, string source)
    {
        using MemoryStream stream = new();
        EmitResult result = EndpointCompilations.Create(assemblyName, OutputKind.DynamicallyLinkedLibrary, source).Emit(stream);
        result.Success.Should().BeTrue(string.Join(Environment.NewLine, result.Diagnostics));
        return Assembly.Load(stream.ToArray());
    }
}
