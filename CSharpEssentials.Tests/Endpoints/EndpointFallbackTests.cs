using System.Reflection;
using CSharpEssentials.Endpoints;
using CSharpEssentials.Tests.Fixtures.EndpointsA;
using CSharpEssentials.Tests.Fixtures.EndpointsB;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.Endpoints;

public class EndpointFallbackTests
{
    private const string LogCategory = "CSharpEssentials.Endpoints";

    private const string Usings = """
        using CSharpEssentials.Endpoints;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Http;
        using Microsoft.AspNetCore.Routing;

        namespace Sample.Fallback;

        """;

    private const string MixedSource = """
        public sealed class AppsGroup : IEndpointGroup
        {
            public static string Prefix => "apps";
        }

        [EndpointGroup<AppsGroup>]
        public sealed class ListApps : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", () => "apps");
        }

        public static class Container
        {
            private sealed class PrivateEndpoint : IEndpoint
            {
                public static void Map(IEndpointRouteBuilder app) => app.MapGet("/private", () => "private");
            }
        }

        file sealed class FileEndpoint : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/file", () => "file");
        }

        [EndpointGroup<SecondGroup>]
        public sealed class FirstGroup : IEndpointGroup
        {
            public static string Prefix => "first";
        }

        [EndpointGroup(typeof(FirstGroup))]
        public sealed class SecondGroup : IEndpointGroup
        {
            public static string Prefix => "second";
        }

        [EndpointGroup<FirstGroup>]
        public sealed class InCycle : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", () => "cycle");
        }

        public sealed class UsersGroup : IEndpointGroup
        {
            public static string Prefix => "users";
        }

        [EndpointGroup<AppsGroup>]
        [EndpointGroup(typeof(UsersGroup))]
        public sealed class Conflicting : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", () => "conflict");
        }

        public sealed class NotAGroup
        {
        }

        [EndpointGroup(typeof(NotAGroup))]
        public sealed class InNonGroup : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", () => "non-group");
        }

        public abstract class AbstractEndpoint : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/abstract", () => "abstract");
        }

        public sealed class GenericEndpoint<T> : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/generic", () => typeof(T).Name);
        }

        [ExcludeFromMapping]
        public sealed class Excluded : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/excluded", () => "excluded");
        }

        [ExcludeFromMapping]
        public sealed class HiddenGroup : IEndpointGroup
        {
            public static string Prefix => "hidden";
        }

        [EndpointGroup<HiddenGroup>]
        public sealed class InHiddenGroup : IEndpoint
        {
            public static void Map(IEndpointRouteBuilder app) => app.MapGet("/", () => "hidden");
        }
        """;

    [Fact]
    public void MapEndpointsFromAssemblies_Should_Match_Generated_Registry()
    {
        using WebApplication generated = EndpointTestApp.Create();
        generated.MapCSharpEssentialsTestsEndpoints();
        using WebApplication fallback = EndpointTestApp.Create();

        fallback.MapEndpointsFromAssemblies(typeof(EndpointFallbackTests).Assembly);

        Describe(fallback).Should().Equal(Describe(generated));
    }

    [Fact]
    public void MapEndpointsFromAssemblies_Should_Match_Generated_Aggregate_When_Scanning_Referenced_Assemblies()
    {
        using WebApplication generated = EndpointTestApp.Create();
        generated.MapAllEndpoints();
        using WebApplication fallback = EndpointTestApp.Create();

        fallback.MapEndpointsFromAssemblies(
            typeof(EndpointFallbackTests).Assembly,
            typeof(FixtureAPing).Assembly,
            typeof(FixtureBPing).Assembly);

        string[] mapped = Describe(fallback);
        mapped.Should().Equal(Describe(generated));
        mapped.Should().ContainSingle(static shape => shape.StartsWith(typeof(FixtureBInAGroup).FullName!, StringComparison.Ordinal));
    }

    [Fact]
    public void MapEndpointsFromAssemblies_Should_Map_Each_Assembly_Once_When_Assembly_Is_Repeated()
    {
        using WebApplication app = EndpointTestApp.Create();

        app.MapEndpointsFromAssemblies(typeof(FixtureAPing).Assembly, typeof(FixtureAPing).Assembly);

        EndpointTypes(app).Should().Equal(CSharpEssentialsTestsFixturesEndpointsAEndpointRegistry.EndpointTypes);
    }

    [Fact]
    public void MapEndpointsFromAssemblies_Should_Apply_Options_When_Configured()
    {
        using WebApplication app = EndpointTestApp.Create();

        app.MapEndpointsFromAssemblies(
            static options => options.Filter(static type => type == typeof(RegistryEndpoints.ListOrderItems)),
            typeof(EndpointFallbackTests).Assembly);

        RouteEndpoint endpoint = EndpointTestApp.Single(app);
        endpoint.RoutePattern.RawText.Should().Be("orders/{orderId:int}/items/");
        endpoint.Metadata.GetMetadata<EndpointTypeMetadata>()!.EndpointType.Should().Be<RegistryEndpoints.ListOrderItems>();
    }

    [Fact]
    public void MapEndpointsFromAssemblies_Should_Skip_Invalid_Types_And_Log_Warnings()
    {
        Assembly assembly = Load("Sample.Fallback.Mixed", Usings + MixedSource);
        using CapturingLoggerProvider provider = new();
        using WebApplication app = EndpointTestApp.Create(provider);

        app.MapEndpointsFromAssemblies(assembly);

        EndpointTypes(app).Select(static type => type.Name).Should().Equal("ListApps");
        string[] warnings = [.. provider.Entries
            .Where(static entry => entry.Category == LogCategory && entry.Level == LogLevel.Warning)
            .Select(static entry => entry.Message)];
        warnings.Should().HaveCount(5);
        warnings.Should().ContainSingle(static message => message.Contains("PrivateEndpoint", StringComparison.Ordinal) && message.Contains("CSE1001", StringComparison.Ordinal));
        warnings.Should().ContainSingle(static message => message.Contains("FileEndpoint", StringComparison.Ordinal) && message.Contains("CSE1001", StringComparison.Ordinal));
        warnings.Should().ContainSingle(static message => message.Contains("InCycle", StringComparison.Ordinal) && message.Contains("CSE1002", StringComparison.Ordinal));
        warnings.Should().ContainSingle(static message => message.Contains("Conflicting", StringComparison.Ordinal) && message.Contains("CSE1003", StringComparison.Ordinal));
        warnings.Should().ContainSingle(static message => message.Contains("InNonGroup", StringComparison.Ordinal) && message.Contains("CSE1007", StringComparison.Ordinal));
    }

    [Fact]
    public void MapEndpointsFromAssemblies_Should_Skip_Assembly_When_Assembly_Is_Excluded()
    {
        Assembly assembly = Load(
            "Sample.Fallback.Excluded",
            "[assembly: CSharpEssentials.Endpoints.ExcludeFromMapping]",
            Usings + MixedSource);
        using CapturingLoggerProvider provider = new();
        using WebApplication app = EndpointTestApp.Create(provider);

        app.MapEndpointsFromAssemblies(assembly);

        EndpointTestApp.Endpoints(app).Should().BeEmpty();
        provider.Entries.Should().NotContain(static entry => entry.Level == LogLevel.Warning);
    }

    [Fact]
    public void MapEndpointsFromAssemblies_Should_Log_Warning_When_Types_Cannot_Be_Loaded()
    {
        Assembly assembly = PartiallyLoadableAssembly.Create("Sample.Fallback.Partial");
        using CapturingLoggerProvider provider = new();
        using WebApplication app = EndpointTestApp.Create(provider);

        Action map = () => app.MapEndpointsFromAssemblies(assembly);

        map.Should().NotThrow();
        EndpointTestApp.Endpoints(app).Should().BeEmpty();
        provider.Entries.Should().ContainSingle(static entry =>
            entry.Category == LogCategory &&
            entry.Level == LogLevel.Warning &&
            entry.Message.Contains("Pending", StringComparison.Ordinal) &&
            entry.Message.Contains("Sample.Fallback.Partial", StringComparison.Ordinal));
    }

    private static Assembly Load(string assemblyName, params string[] sources)
    {
        using MemoryStream stream = new();
        EmitResult result = EndpointCompilations.Create(assemblyName, OutputKind.DynamicallyLinkedLibrary, sources).Emit(stream);
        result.Success.Should().BeTrue(string.Join(Environment.NewLine, result.Diagnostics));
        return Assembly.Load(stream.ToArray());
    }

    private static Type[] EndpointTypes(IEndpointRouteBuilder app) =>
        [.. EndpointTestApp.Endpoints(app).Select(static endpoint => endpoint.Metadata.GetMetadata<EndpointTypeMetadata>()!.EndpointType)];

    private static string[] Describe(IEndpointRouteBuilder app) =>
        [.. EndpointTestApp.Endpoints(app).Select(static endpoint =>
            $"{endpoint.Metadata.GetMetadata<EndpointTypeMetadata>()!.EndpointType.FullName} {endpoint.RoutePattern.RawText} {string.Join(',', endpoint.Metadata.Select(static item => item.GetType().Name))}")];
}
