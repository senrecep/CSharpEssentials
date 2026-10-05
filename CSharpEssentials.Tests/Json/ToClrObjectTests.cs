using System.Text.Json;
using CSharpEssentials.Json;
using FluentAssertions;

namespace CSharpEssentials.Tests.Json;

public class ToClrObjectTests
{
    private static object? Convert(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.ToClrObject();
    }

    [Fact]
    public void ToClrObject_Object_ShouldReturnDictionaryWithClrValues()
    {
        object? result = Convert("""{"name":"Alice","age":30,"active":true,"manager":null}""");

        var dictionary = result.Should().BeOfType<Dictionary<string, object?>>().Subject;
        dictionary["name"].Should().Be("Alice");
        dictionary["age"].Should().Be(30);
        dictionary["active"].Should().Be(true);
        dictionary["manager"].Should().BeNull();
    }

    [Fact]
    public void ToClrObject_Array_ShouldReturnListWithClrValues()
    {
        object? result = Convert("""[1,"two",false,null]""");

        result.Should().BeOfType<List<object?>>()
            .Which.Should().Equal(1, "two", false, null);
    }

    [Fact]
    public void ToClrObject_Nested_ShouldConvertRecursively()
    {
        object? result = Convert("""{"items":[{"id":1},{"id":2}]}""");

        var root = (Dictionary<string, object?>)result!;
        var items = root["items"].Should().BeOfType<List<object?>>().Subject;
        items.Should().HaveCount(2);
        items[1].Should().BeOfType<Dictionary<string, object?>>()
            .Which["id"].Should().Be(2);
    }

    [Theory]
    [InlineData("42", typeof(int))]
    [InlineData("-2147483648", typeof(int))]
    [InlineData("2147483648", typeof(long))]
    [InlineData("9223372036854775808", typeof(decimal))]
    [InlineData("0.1", typeof(decimal))]
    [InlineData("1e300", typeof(double))]
    public void ToClrObject_Number_ShouldPickNarrowestFittingType(string json, Type expectedType)
    {
        Convert(json).Should().BeOfType(expectedType);
    }

    [Fact]
    public void ToClrObject_Decimal_ShouldPreservePrecision()
    {
        Convert("0.1").Should().Be(0.1m);
    }

    [Fact]
    public void ToClrObject_DuplicateKeys_ShouldKeepLastValue()
    {
        var result = (Dictionary<string, object?>)Convert("""{"a":1,"a":2}""")!;

        result["a"].Should().Be(2);
    }

    [Fact]
    public void ToClrObject_DefaultElement_ShouldReturnNull()
    {
        default(JsonElement).ToClrObject().Should().BeNull();
    }

    [Fact]
    public void ToClrObject_DeserializedDictionary_ShouldUnwrapJsonElements()
    {
        var raw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>("""{"count":3,"tags":["a"]}""")!;

        var result = raw.ToDictionary(pair => pair.Key, pair => pair.Value.ToClrObject());

        result["count"].Should().Be(3);
        result["tags"].Should().BeOfType<List<object?>>().Which.Should().Equal("a");
    }

    [Fact]
    public void ToClrObject_NumberBeyondDoubleRange_ShouldReturnInfinity()
    {
        Convert("1e400").Should().Be(double.PositiveInfinity);
    }
}
