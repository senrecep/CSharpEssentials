using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CSharpEssentials.Json;
using FluentAssertions;

namespace CSharpEssentials.Tests.Json;

public class RequireValueTypeConstructorParametersTests
{
    public enum Size
    {
        Small,
        Large,
    }

    public sealed record Order(int Quantity, Size Size, Guid Id, int? Discount, string? Note, int Priority = 5);

    public sealed class Mutable
    {
        public int Quantity { get; set; }
    }

    public sealed class GetOnly
    {
        public GetOnly(int quantity) => Quantity = quantity;

        public int Quantity { get; }
    }

    public sealed record ExplicitGetOnly(int Quantity)
    {
        public int Quantity { get; } = Quantity;
    }

    private const string Complete = """{"quantity":2,"size":1,"id":"6ba7b810-9dad-11d1-80b4-00c04fd430c8"}""";

    private static JsonSerializerOptions Options() =>
        new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() }
            .RequireValueTypeConstructorParameters();

    [Theory]
    [InlineData("""{"size":1,"id":"6ba7b810-9dad-11d1-80b4-00c04fd430c8"}""", "quantity")]
    [InlineData("""{"quantity":2,"id":"6ba7b810-9dad-11d1-80b4-00c04fd430c8"}""", "size")]
    [InlineData("""{"quantity":2,"size":1}""", "id")]
    public void Deserialize_WithMissingValueTypeParameter_ShouldThrow(string json, string missing)
    {
        Action act = () => JsonSerializer.Deserialize<Order>(json, Options());

        act.Should().Throw<JsonException>().WithMessage($"*{missing}*");
    }

    [Fact]
    public void Deserialize_WithoutNullableReferenceOrDefaultedParameters_ShouldBind()
    {
        Order? order = JsonSerializer.Deserialize<Order>(Complete, Options());

        order.Should().Be(new Order(2, Size.Large, new Guid("6ba7b810-9dad-11d1-80b4-00c04fd430c8"), null, null));
    }

    [Fact]
    public void Deserialize_WithoutTheModifier_ShouldBindMissingValueAsDefault()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

        Order? order = JsonSerializer.Deserialize<Order>("""{"size":1,"id":"6ba7b810-9dad-11d1-80b4-00c04fd430c8"}""", options);

        order!.Quantity.Should().Be(0);
    }

    [Fact]
    public void Deserialize_TypeWithoutConstructorParameters_ShouldStayOptional()
    {
        Mutable? value = JsonSerializer.Deserialize<Mutable>("{}", Options());

        value!.Quantity.Should().Be(0);
    }

    [Fact]
    public void Serialize_ShouldNotChange()
    {
        var order = new Order(2, Size.Large, Guid.Empty, null, null);

        JsonSerializer.Serialize(order, Options()).Should().Be(JsonSerializer.Serialize(order, JsonSerializerOptions.Web));
    }

    [Fact]
    public void Extension_WithoutResolver_ShouldThrowInvalidOperation()
    {
        Action act = () => new JsonSerializerOptions().RequireValueTypeConstructorParameters();

        act.Should().Throw<InvalidOperationException>().WithMessage("*TypeInfoResolver*");
    }

    [Fact]
    public void Modifier_WithNull_ShouldThrow()
    {
        Action act = () => JsonTypeInfoModifiers.RequireValueTypeConstructorParameters(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("typeInfo");
    }

    [Fact]
    public void StrictOptions_ShouldRequireValueTypeConstructorParameters()
    {
        Action act = () => JsonSerializer.Deserialize<Order>("""{"size":1,"id":"6ba7b810-9dad-11d1-80b4-00c04fd430c8"}""", EnhancedJsonSerializerOptions.StrictOptions);

        act.Should().Throw<JsonException>().WithMessage("*quantity*");
        JsonSerializer.Deserialize<Order>(Complete, EnhancedJsonSerializerOptions.StrictOptions)!.Quantity.Should().Be(2);
    }

    [Fact]
    public void StrictOptions_ApplyTo_OptionsWithoutResolver_ShouldCarryTheModifier()
    {
        JsonSerializerOptions target = EnhancedJsonSerializerOptions.StrictOptions.ApplyTo(new JsonSerializerOptions());

        Action act = () => JsonSerializer.Deserialize<Order>("""{"size":1,"id":"6ba7b810-9dad-11d1-80b4-00c04fd430c8"}""", target);

        act.Should().Throw<JsonException>().WithMessage("*quantity*");
    }

    [Fact]
    public void StrictOptions_ApplyTo_OptionsWithOwnResolver_ShouldCarryTheModifier()
    {
        var target = new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

        EnhancedJsonSerializerOptions.StrictOptions.ApplyTo(target);
        Action act = () => JsonSerializer.Deserialize<Order>("""{"size":1,"id":"6ba7b810-9dad-11d1-80b4-00c04fd430c8"}""", target);

        act.Should().Throw<JsonException>().WithMessage("*quantity*");
        JsonSerializer.Deserialize<Order>(Complete, target)!.Quantity.Should().Be(2);
    }

    [Fact]
    public void ApplyTo_FromOptionsWithoutTheModifier_ShouldLeaveTargetOptional()
    {
        var target = new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

        EnhancedJsonSerializerOptions.DefaultOptionsWithoutConverters.ApplyTo(target);

        JsonSerializer.Deserialize<Order>("""{"size":1,"id":"6ba7b810-9dad-11d1-80b4-00c04fd430c8"}""", target)!.Quantity.Should().Be(0);
    }

    [Fact]
    public void GetOnlyConstructorProperty_ShouldStayOptional_AndSerialize()
    {
        foreach (JsonSerializerOptions options in new[] { Options(), EnhancedJsonSerializerOptions.StrictOptions })
        {
            JsonSerializer.Deserialize<GetOnly>("{}", options)!.Quantity.Should().Be(0);
            JsonSerializer.Deserialize<GetOnly>("""{"quantity":3}""", options)!.Quantity.Should().Be(3);
            JsonSerializer.Serialize(new GetOnly(3), options).Should().Contain("3");
        }
    }

    [Fact]
    public void ExplicitGetOnlyRecordProperty_ShouldStayOptional_AndSerialize()
    {
        foreach (JsonSerializerOptions options in new[] { Options(), EnhancedJsonSerializerOptions.StrictOptions })
        {
            JsonSerializer.Deserialize<ExplicitGetOnly>("{}", options)!.Quantity.Should().Be(0);
            JsonSerializer.Serialize(new ExplicitGetOnly(4), options).Should().Contain("4");
        }
    }
}
