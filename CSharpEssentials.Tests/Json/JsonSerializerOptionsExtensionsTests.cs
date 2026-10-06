using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Enums;
using CSharpEssentials.Json;
using FluentAssertions;

namespace CSharpEssentials.Tests.Json;

public class JsonSerializerOptionsExtensionsTests
{
    private static readonly JsonOrder Order = new() { Status = JsonOrderStatus.PendingApproval, Permissions = JsonPermissions.ReadWrite };

    private const string OrderJson = "{\"Status\":\"pending_approval\",\"Permissions\":[\"read\",\"write\"]}";

    [Fact]
    public void AddEnumConventions_Should_Insert_The_Factory_First_And_Return_The_Same_Options()
    {
        JsonSerializerOptions options = new() { Converters = { new MultiFormatDateTimeConverterFactory() } };

        JsonSerializerOptions result = options.AddEnumConventions(EnumConventions.Default, EnumReadMode.Input, EnumWireFormat.Number);

        result.Should().BeSameAs(options);
        EnumConverterFactory factory = options.Converters[0].Should().BeOfType<EnumConverterFactory>().Subject;
        factory.Mode.Should().Be(EnumReadMode.Input);
        factory.WriteAs.Should().Be(EnumWireFormat.Number);
        factory.Conventions.Should().BeSameAs(EnumConventions.Default);
        options.Converters.Should().HaveCount(2);
    }

    [Fact]
    [Obsolete("Creates the obsolete 4.x converter to check that it is removed.")]
    public void AddEnumConventions_Should_Remove_Other_Enum_Converters()
    {
        JsonSerializerOptions options = new()
        {
            Converters =
            {
                new JsonStringEnumConverter(),
                new JsonStringEnumConverter<JsonOrderStatus>(),
                new ConditionalStringEnumConverter(),
                new EnumConverterFactory(EnumConventions.Default, EnumReadMode.Input),
            },
        };

        options.AddEnumConventions(EnumConventions.Default);

        EnumConverterFactory factory = options.Converters.Should().ContainSingle().Which.Should().BeOfType<EnumConverterFactory>().Subject;
        factory.Mode.Should().Be(EnumReadMode.Data);
    }

    [Fact]
    public void AddEnumConventions_Should_Win_Over_A_JsonStringEnumConverter_Added_Later_By_Order()
    {
        JsonSerializerOptions options = new JsonSerializerOptions().AddEnumConventions(EnumConventions.Default);
        options.Converters.Add(new JsonStringEnumConverter());

        string json = JsonSerializer.Serialize(JsonOrderStatus.PendingApproval, options);

        json.Should().Be("\"pending_approval\"");
    }

    [Fact]
    public void Converter_Should_Win_Over_Source_Generated_Metadata()
    {
        JsonSerializerOptions options = new JsonSerializerOptions { TypeInfoResolver = EnumConventionsJsonContext.Default }
            .AddEnumConventions(EnumConventions.Default);

        string json = JsonSerializer.Serialize(Order, options);

        json.Should().Be(OrderJson);
    }

    [Fact]
    public void Converter_Should_Win_Over_A_Context_With_UseStringEnumConverter_When_Its_Options_Are_Copied()
    {
        JsonSerializerOptions options = new JsonSerializerOptions(EnumConventionsStringEnumJsonContext.Default.Options)
            .AddEnumConventions(EnumConventions.Default);

        string json = JsonSerializer.Serialize(Order, options);
        JsonOrder? back = JsonSerializer.Deserialize<JsonOrder>("{\"Status\":\"Approval\",\"Permissions\":\"read, write\"}", options);

        json.Should().Be(OrderJson);
        back!.Status.Should().Be(JsonOrderStatus.PendingApproval);
        back.Permissions.Should().Be(JsonPermissions.ReadWrite);
    }

    [Fact]
    public void Converter_Should_Win_Over_A_Context_With_UseStringEnumConverter_Created_With_The_Options()
    {
        EnumConventionsStringEnumJsonContext context = new(new JsonSerializerOptions().AddEnumConventions(EnumConventions.Default));

        string json = JsonSerializer.Serialize(Order, context.JsonOrder);

        json.Should().Be(OrderJson);
    }

    [Fact]
    public void A_Context_With_UseStringEnumConverter_Should_Write_Member_Names_Without_The_Conventions()
    {
        string json = JsonSerializer.Serialize(Order, EnumConventionsStringEnumJsonContext.Default.JsonOrder);

        json.Should().Be("{\"Status\":\"PendingApproval\",\"Permissions\":\"ReadWrite\"}");
    }
}

[JsonSerializable(typeof(JsonOrder))]
internal sealed partial class EnumConventionsJsonContext : JsonSerializerContext;

[JsonSourceGenerationOptions(UseStringEnumConverter = true)]
[JsonSerializable(typeof(JsonOrder))]
internal sealed partial class EnumConventionsStringEnumJsonContext : JsonSerializerContext;
