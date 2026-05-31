using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.These;
using FluentAssertions;

namespace CSharpEssentials.Tests.These;

public class TheseJsonTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public void Serialize_Should_ProduceCorrectJson_When_IsLeft()
    {
        These<string, string> these = These<string, string>.Left("error");

        string json = JsonSerializer.Serialize(these, Options);

        json.Should().Contain("\"isLeft\":true");
        json.Should().Contain("\"isRight\":false");
        json.Should().Contain("\"left\":\"error\"");
        json.Should().NotContain("\"right\"");
    }

    [Fact]
    public void Serialize_Should_ProduceCorrectJson_When_IsRight()
    {
        These<string, int> these = These<string, int>.Right(42);

        string json = JsonSerializer.Serialize(these, Options);

        json.Should().Contain("\"isLeft\":false");
        json.Should().Contain("\"isRight\":true");
        json.Should().Contain("\"right\":42");
        json.Should().NotContain("\"left\"");
    }

    [Fact]
    public void Serialize_Should_ProduceCorrectJson_When_IsBoth()
    {
        These<string, int> these = These<string, int>.Both("warning", 42);

        string json = JsonSerializer.Serialize(these, Options);

        json.Should().Contain("\"isLeft\":true");
        json.Should().Contain("\"isRight\":true");
        json.Should().Contain("\"left\":\"warning\"");
        json.Should().Contain("\"right\":42");
    }

    [Fact]
    public void Deserialize_Should_RoundTrip_When_IsLeft()
    {
        These<string, int> original = These<string, int>.Left("error");
        string json = JsonSerializer.Serialize(original, Options);

        These<string, int> result = JsonSerializer.Deserialize<These<string, int>>(json, Options);

        result.IsLeft.Should().BeTrue();
        result.GetLeft().Value.Should().Be("error");
    }

    [Fact]
    public void Deserialize_Should_RoundTrip_When_IsRight()
    {
        These<string, int> original = These<string, int>.Right(42);
        string json = JsonSerializer.Serialize(original, Options);

        These<string, int> result = JsonSerializer.Deserialize<These<string, int>>(json, Options);

        result.IsRight.Should().BeTrue();
        result.GetRight().Value.Should().Be(42);
    }

    [Fact]
    public void Deserialize_Should_RoundTrip_When_IsBoth()
    {
        These<string, int> original = These<string, int>.Both("warning", 42);
        string json = JsonSerializer.Serialize(original, Options);

        These<string, int> result = JsonSerializer.Deserialize<These<string, int>>(json, Options);

        result.IsBoth.Should().BeTrue();
        result.GetLeft().Value.Should().Be("warning");
        result.GetRight().Value.Should().Be(42);
    }

    [Fact]
    public void IsBoth_Discriminator_Should_NotAppearInJson()
    {
        These<string, int> these = These<string, int>.Both("w", 1);

        string json = JsonSerializer.Serialize(these, Options);

        // IsBoth is [JsonIgnore] — not serialized
        json.Should().NotContain("\"isBoth\"");
        // Raw backing booleans (HasLeft/HasRight) ARE in JSON as isLeft/isRight
        json.Should().Contain("\"isLeft\":true");
        json.Should().Contain("\"isRight\":true");
    }
}
