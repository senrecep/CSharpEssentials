using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Any;
using FluentAssertions;

namespace CSharpEssentials.Tests.Any;

public partial class AnyJsonSourceGenerationTests
{
    [Fact]
    public void JsonRoundTrip_WithSourceGeneratedSnakeCaseContext_ShouldKeepInnerProperties()
    {
        Any<int, GeneratedPerson> any = new GeneratedPerson { FirstName = "Ada" };

        string json = JsonSerializer.Serialize(any, SnakeCaseContext.Default.AnyInt32GeneratedPerson);
        Any<int, GeneratedPerson> deserialized = JsonSerializer.Deserialize(json, SnakeCaseContext.Default.AnyInt32GeneratedPerson);

        json.Should().Be("""{"index":1,"value":{"first_name":"Ada"}}""");
        deserialized.GetSecond().FirstName.Should().Be("Ada");
    }

    [Fact]
    public void Deserialize_WithSourceGeneratedContextMissingBranchType_ShouldThrowNotSupportedException()
    {
        Action act = () => JsonSerializer.Deserialize(
            """{"index":1,"value":{"first_name":"Ada"}}""",
            MissingBranchContext.Default.AnyInt32UnregisteredPerson);

        act.Should().Throw<NotSupportedException>();
    }

    public sealed class GeneratedPerson
    {
        public string? FirstName { get; set; }
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
    [JsonSerializable(typeof(Any<int, GeneratedPerson>))]
    [JsonSerializable(typeof(GeneratedPerson))]
    [JsonSerializable(typeof(int))]
    internal sealed partial class SnakeCaseContext : JsonSerializerContext;

    public sealed class UnregisteredPerson
    {
        public string? FirstName { get; set; }
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
    [JsonSerializable(typeof(Any<int, UnregisteredPerson>))]
    [JsonSerializable(typeof(int))]
    internal sealed partial class MissingBranchContext : JsonSerializerContext;
}
