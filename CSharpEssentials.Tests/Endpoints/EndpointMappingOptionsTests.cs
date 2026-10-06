using CSharpEssentials.Endpoints;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace CSharpEssentials.Tests.Endpoints;

public class EndpointMappingOptionsTests
{
    [Fact]
    public void CreateOptions_Should_Apply_Configure_Callback()
    {
        EndpointMappingOptions options = EndpointMapper.CreateOptions(static o => o.AutoTagFromGroup = true);

        options.AutoTagFromGroup.Should().BeTrue();
    }

    [Fact]
    public void CreateOptions_Should_Return_Defaults_When_Configure_Is_Null()
    {
        EndpointMappingOptions options = EndpointMapper.CreateOptions(null);

        options.LogDiscovered.Should().BeFalse();
        options.AutoTagFromGroup.Should().BeFalse();
        options.OperationNaming.Should().BeSameAs(OperationNaming.None);
    }

    [Fact]
    public void MapEndpoint_Should_Add_EndpointTypeMetadata()
    {
        using WebApplication app = EndpointTestApp.Create();

        EndpointMapper.MapEndpoint<OptionsEndpoints.Ping>(app, null, new EndpointMappingOptions());

        RouteEndpoint endpoint = EndpointTestApp.Single(app);
        endpoint.RoutePattern.RawText.Should().Be("/ping");
        endpoint.Metadata.GetMetadata<EndpointTypeMetadata>()!.EndpointType.Should().Be<OptionsEndpoints.Ping>();
    }

    [Fact]
    public void MapGroup_Should_Use_Prefix_And_Apply_Configure()
    {
        using WebApplication app = EndpointTestApp.Create();

        RouteGroupBuilder group = EndpointMapper.MapGroup<OptionsEndpoints.UsersGroup>(app);
        EndpointMapper.MapEndpoint<OptionsEndpoints.ListUsers>(group, typeof(OptionsEndpoints.UsersGroup), new EndpointMappingOptions());

        RouteEndpoint endpoint = EndpointTestApp.Single(app);
        endpoint.RoutePattern.RawText.Should().Be("users/");
        endpoint.Metadata.GetMetadata<OrderMarker>()!.Name.Should().Be("group");
    }

    [Fact]
    public void MapGroup_Should_Use_Default_Configure_When_Group_Does_Not_Override_It()
    {
        using WebApplication app = EndpointTestApp.Create();

        RouteGroupBuilder group = EndpointMapper.MapGroup<OptionsEndpoints.Reports>(app);
        EndpointMapper.MapEndpoint<OptionsEndpoints.DailyReport>(group, typeof(OptionsEndpoints.Reports), new EndpointMappingOptions());

        EndpointTestApp.Single(app).RoutePattern.RawText.Should().Be("reports/daily");
    }

    [Fact]
    public void Filter_Should_Skip_Endpoint_When_Predicate_Returns_False()
    {
        using WebApplication app = EndpointTestApp.Create();
        EndpointMappingOptions options = new EndpointMappingOptions().Filter(static type => type != typeof(OptionsEndpoints.Ping));

        EndpointMapper.MapEndpoint<OptionsEndpoints.Ping>(app, null, options);

        EndpointTestApp.Endpoints(app).Should().BeEmpty();
    }

    [Fact]
    public void Filter_Should_Combine_Predicates_With_And()
    {
        using WebApplication app = EndpointTestApp.Create();
        EndpointMappingOptions options = new EndpointMappingOptions()
            .Filter(static _ => true)
            .Filter(static type => type != typeof(OptionsEndpoints.DailyReport));

        EndpointMapper.MapEndpoint<OptionsEndpoints.Ping>(app, null, options);
        EndpointMapper.MapEndpoint<OptionsEndpoints.DailyReport>(app, null, options);

        EndpointTestApp.Endpoints(app).Select(static e => e.RoutePattern.RawText).Should().Equal("/ping");
    }

    [Fact]
    public void Filter_Should_Be_Called_Once_Per_Endpoint_Type()
    {
        using WebApplication app = EndpointTestApp.Create();
        List<Type> calls = [];
        EndpointMappingOptions options = new EndpointMappingOptions().Filter(type =>
        {
            calls.Add(type);
            return true;
        });

        bool any = EndpointMapper.ShouldMapAny(app, options, typeof(OptionsEndpoints.ListUsers));
        EndpointMapper.MapEndpoint<OptionsEndpoints.ListUsers>(app, null, options);

        any.Should().BeTrue();
        calls.Should().Equal(typeof(OptionsEndpoints.ListUsers));
    }

    [Fact]
    public void ShouldMapAny_Should_Return_False_When_Every_Type_Is_Filtered()
    {
        using WebApplication app = EndpointTestApp.Create();
        EndpointMappingOptions options = new EndpointMappingOptions().Filter(static _ => false);

        bool any = EndpointMapper.ShouldMapAny(app, options, typeof(OptionsEndpoints.ListUsers), typeof(OptionsEndpoints.NamedUser));

        any.Should().BeFalse();
    }

    [Fact]
    public void ShouldMapAny_Should_Return_True_When_One_Type_Passes()
    {
        using WebApplication app = EndpointTestApp.Create();
        EndpointMappingOptions options = new EndpointMappingOptions().Filter(static type => type == typeof(OptionsEndpoints.NamedUser));

        bool any = EndpointMapper.ShouldMapAny(app, options, typeof(OptionsEndpoints.ListUsers), typeof(OptionsEndpoints.NamedUser));

        any.Should().BeTrue();
    }

    [Fact]
    public void OperationNaming_None_Should_Leave_Endpoint_Unnamed()
    {
        using WebApplication app = EndpointTestApp.Create();

        EndpointMapper.MapEndpoint<OptionsEndpoints.Ping>(app, null, new EndpointMappingOptions());

        EndpointTestApp.Single(app).Metadata.GetMetadata<IEndpointNameMetadata>().Should().BeNull();
    }

    [Fact]
    public void OperationNaming_TypeName_Should_Name_Endpoint_After_Type_And_Containing_Types()
    {
        using WebApplication app = EndpointTestApp.Create();
        EndpointMappingOptions options = new() { OperationNaming = OperationNaming.TypeName };

        EndpointMapper.MapEndpoint<OptionsEndpoints.Ping>(app, null, options);

        RouteEndpoint endpoint = EndpointTestApp.Single(app);
        endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()!.EndpointName.Should().Be("OptionsEndpoints_Ping");
        endpoint.Metadata.GetMetadata<IRouteNameMetadata>()!.RouteName.Should().Be("OptionsEndpoints_Ping");
    }

    [Fact]
    public void OperationNaming_Should_Not_Override_Explicit_Name()
    {
        using WebApplication app = EndpointTestApp.Create();
        EndpointMappingOptions options = new() { OperationNaming = OperationNaming.TypeName };

        EndpointMapper.MapEndpoint<OptionsEndpoints.NamedUser>(app, null, options);

        EndpointTestApp.Single(app).Metadata.GetOrderedMetadata<IEndpointNameMetadata>()
            .Select(static m => m.EndpointName).Should().Equal("ExplicitName");
    }

    [Fact]
    public void OperationNaming_Custom_Should_Use_Type_And_Builder()
    {
        using WebApplication app = EndpointTestApp.Create();
        EndpointMappingOptions options = new()
        {
            OperationNaming = OperationNaming.Custom(static (type, builder) =>
                $"{type.Name}_{((RouteEndpointBuilder)builder).RoutePattern.RawText}"),
        };

        EndpointMapper.MapEndpoint<OptionsEndpoints.Ping>(app, null, options);

        EndpointTestApp.Single(app).Metadata.GetMetadata<IEndpointNameMetadata>()!.EndpointName.Should().Be("Ping_/ping");
    }

    [Fact]
    public void OperationNaming_Custom_Should_Leave_Endpoint_Unnamed_When_Factory_Returns_Null()
    {
        using WebApplication app = EndpointTestApp.Create();
        EndpointMappingOptions options = new() { OperationNaming = OperationNaming.Custom(static (_, _) => null) };

        EndpointMapper.MapEndpoint<OptionsEndpoints.Ping>(app, null, options);

        EndpointTestApp.Single(app).Metadata.GetMetadata<IEndpointNameMetadata>().Should().BeNull();
    }

    [Fact]
    public void OperationNaming_Custom_Should_Throw_When_Factory_Is_Null()
    {
        Action act = static () => OperationNaming.Custom(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AutoTagFromGroup_Should_Tag_With_Group_Name_Without_Group_Suffix()
    {
        using WebApplication app = EndpointTestApp.Create();
        EndpointMappingOptions options = new() { AutoTagFromGroup = true };

        EndpointMapper.MapEndpoint<OptionsEndpoints.ListUsers>(app, typeof(OptionsEndpoints.UsersGroup), options);

        EndpointTestApp.Single(app).Metadata.GetMetadata<ITagsMetadata>()!.Tags.Should().Equal("Users");
    }

    [Fact]
    public void AutoTagFromGroup_Should_Keep_Group_Name_When_It_Has_No_Group_Suffix()
    {
        using WebApplication app = EndpointTestApp.Create();
        EndpointMappingOptions options = new() { AutoTagFromGroup = true };

        EndpointMapper.MapEndpoint<OptionsEndpoints.DailyReport>(app, typeof(OptionsEndpoints.Reports), options);

        EndpointTestApp.Single(app).Metadata.GetMetadata<ITagsMetadata>()!.Tags.Should().Equal("Reports");
    }

    [Fact]
    public void AutoTagFromGroup_Should_Not_Tag_When_Endpoint_Has_Explicit_Tags()
    {
        using WebApplication app = EndpointTestApp.Create();
        EndpointMappingOptions options = new() { AutoTagFromGroup = true };

        EndpointMapper.MapEndpoint<OptionsEndpoints.TaggedUser>(app, typeof(OptionsEndpoints.UsersGroup), options);

        EndpointTestApp.Single(app).Metadata.GetOrderedMetadata<ITagsMetadata>()
            .SelectMany(static m => m.Tags).Should().Equal("Explicit");
    }

    [Fact]
    public void AutoTagFromGroup_Should_Not_Tag_When_Endpoint_Has_No_Group()
    {
        using WebApplication app = EndpointTestApp.Create();
        EndpointMappingOptions options = new() { AutoTagFromGroup = true };

        EndpointMapper.MapEndpoint<OptionsEndpoints.Ping>(app, null, options);

        EndpointTestApp.Single(app).Metadata.GetMetadata<ITagsMetadata>().Should().BeNull();
    }

    [Fact]
    public void AutoTagFromGroup_Should_Not_Tag_When_Disabled()
    {
        using WebApplication app = EndpointTestApp.Create();

        EndpointMapper.MapEndpoint<OptionsEndpoints.ListUsers>(app, typeof(OptionsEndpoints.UsersGroup), new EndpointMappingOptions());

        EndpointTestApp.Single(app).Metadata.GetMetadata<ITagsMetadata>().Should().BeNull();
    }

    [Fact]
    public void ConfigureEach_Should_Run_After_Group_And_Type_Metadata_In_Registration_Order()
    {
        using WebApplication app = EndpointTestApp.Create();
        List<Type> seen = [];
        EndpointMappingOptions options = new EndpointMappingOptions()
            .ConfigureEach((builder, type) =>
            {
                seen.Add(type);
                builder.WithMetadata(new OrderMarker("first"));
            })
            .ConfigureEach(static (builder, _) => builder.WithMetadata(new OrderMarker("second")));

        RouteGroupBuilder group = EndpointMapper.MapGroup<OptionsEndpoints.UsersGroup>(app);
        EndpointMapper.MapEndpoint<OptionsEndpoints.ListUsers>(group, typeof(OptionsEndpoints.UsersGroup), options);

        IReadOnlyList<object> metadata = EndpointTestApp.Single(app).Metadata;
        int groupIndex = IndexOf(metadata, static m => m is OrderMarker { Name: "group" });
        int typeIndex = IndexOf(metadata, static m => m is EndpointTypeMetadata);
        int firstIndex = IndexOf(metadata, static m => m is OrderMarker { Name: "first" });
        int secondIndex = IndexOf(metadata, static m => m is OrderMarker { Name: "second" });
        seen.Should().Equal(typeof(OptionsEndpoints.ListUsers));
        groupIndex.Should().BeLessThan(typeIndex);
        typeIndex.Should().BeLessThan(firstIndex);
        firstIndex.Should().BeLessThan(secondIndex);
    }

    [Fact]
    public void LogDiscovered_Should_Log_Mapped_And_Filtered_Types_At_Debug()
    {
        using CapturingLoggerProvider provider = new();
        using WebApplication app = EndpointTestApp.Create(provider);
        EndpointMappingOptions options = new EndpointMappingOptions { LogDiscovered = true }
            .Filter(static type => type == typeof(OptionsEndpoints.Ping));

        EndpointMapper.MapEndpoint<OptionsEndpoints.Ping>(app, null, options);
        EndpointMapper.MapEndpoint<OptionsEndpoints.DailyReport>(app, null, options);

        (string Category, LogLevel Level, string Message)[] entries = [.. provider.Entries.Where(static e => e.Category == "CSharpEssentials.Endpoints")];
        entries.Should().HaveCount(2);
        entries.Should().OnlyContain(static e => e.Level == LogLevel.Debug);
        entries[0].Message.Should().Contain("Mapped").And.Contain(nameof(OptionsEndpoints.Ping));
        entries[1].Message.Should().Contain("filter").And.Contain(nameof(OptionsEndpoints.DailyReport));
    }

    [Fact]
    public void LogDiscovered_Should_Not_Log_When_Disabled()
    {
        using CapturingLoggerProvider provider = new();
        using WebApplication app = EndpointTestApp.Create(provider);

        EndpointMapper.MapEndpoint<OptionsEndpoints.Ping>(app, null, new EndpointMappingOptions());

        provider.Entries.Should().NotContain(static e => e.Category == "CSharpEssentials.Endpoints");
    }

    [Fact]
    public void ConfigureEach_Should_Throw_When_Callback_Is_Null()
    {
        Action act = static () => new EndpointMappingOptions().ConfigureEach(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Filter_Should_Throw_When_Predicate_Is_Null()
    {
        Action act = static () => new EndpointMappingOptions().Filter(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    private static int IndexOf(IReadOnlyList<object> metadata, Func<object, bool> predicate)
    {
        for (int i = 0; i < metadata.Count; i++)
        {
            if (predicate(metadata[i]))
            {
                return i;
            }
        }

        return -1;
    }
}
