using System.Text.Json.Serialization;
using CSharpEssentials.AspNetCore;
using CSharpEssentials.ResultPattern;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpEssentials.Tests.AspNetCore;

public class ResultEndpointFilterTypedOkTests
{
    [Fact]
    public async Task Success_WithExactRuntimeType_Should_Return_TypedOk_With_SameBody()
    {
        var order = new OrderDto("order-42", "pending", 3);

        object? result = await InvokeAsync(Result<OrderDto>.Success(order));

        result.Should().BeOfType<Ok<OrderDto>>();
        await AssertSameResponseAsync(result, order);
    }

    [Fact]
    public async Task Success_WithValueType_Should_Return_TypedOk_With_SameBody()
    {
        object? result = await InvokeAsync(Result<int>.Success(7));

        result.Should().BeOfType<Ok<int>>();
        await AssertSameResponseAsync(result, 7);
    }

    [Fact]
    public async Task Success_WithDerivedValueOfPolymorphicBase_Should_Return_ObjectOk_With_SameBody()
    {
        Shape circle = new Circle(2);

        object? result = await InvokeAsync(Result<Shape>.Success(circle));

        result.Should().BeOfType<Ok<object>>();
        await AssertSameResponseAsync(result, circle);
    }

    [Fact]
    public async Task Success_WithNullValue_Should_Return_ObjectOk_With_SameBody()
    {
        object? result = await InvokeAsync(default(Result<string>));

        result.Should().BeOfType<Ok<object>>();
        await AssertSameResponseAsync(result, null);
    }

    [Fact]
    public async Task Success_WithNullableValueType_Should_Return_ObjectOk_With_SameBody()
    {
        object? result = await InvokeAsync(Result<int?>.Success(5));

        result.Should().BeOfType<Ok<object>>();
        await AssertSameResponseAsync(result, 5);
    }

    [Fact]
    public async Task Success_WithDerivedValueOfPlainBase_Should_Return_ObjectOk_With_DerivedProperties()
    {
        PlainBase derived = new PlainDerived("base-1", "extra-1");

        object? result = await InvokeAsync(Result<PlainBase>.Success(derived));

        result.Should().BeOfType<Ok<object>>();
        byte[] body = await AssertSameResponseAsync(result, derived);
        System.Text.Encoding.UTF8.GetString(body).Should().Contain("extra-1");
    }

    [Fact]
    public async Task Success_WithInterfaceDeclaredType_Should_Return_ObjectOk_With_SameBody()
    {
        INamed named = new NamedDto("named-1", 9);

        object? result = await InvokeAsync(Result<INamed>.Success(named));

        result.Should().BeOfType<Ok<object>>();
        byte[] body = await AssertSameResponseAsync(result, named);
        System.Text.Encoding.UTF8.GetString(body).Should().Contain("\"rank\":9");
    }

    [Fact]
    public async Task Success_WithNestedGenericList_Should_Return_TypedOk_With_SameBody()
    {
        List<OrderDto> orders = [new("order-1", "pending", 1), new("order-2", "shipped", 2)];

        object? result = await InvokeAsync(Result<List<OrderDto>>.Success(orders));

        result.Should().BeOfType<Ok<List<OrderDto>>>();
        await AssertSameResponseAsync(result, orders);
    }

    [Fact]
    public async Task Success_WithNestedGenericDictionary_Should_Return_TypedOk_With_SameBody()
    {
        Dictionary<string, OrderDto> orders = new() { ["a"] = new("order-1", "pending", 1), ["b"] = new("order-2", "shipped", 2) };

        object? result = await InvokeAsync(Result<Dictionary<string, OrderDto>>.Success(orders));

        result.Should().BeOfType<Ok<Dictionary<string, OrderDto>>>();
        await AssertSameResponseAsync(result, orders);
    }

    [Fact]
    public async Task Success_WithString_Should_Return_TypedOk_With_SameBody()
    {
        object? result = await InvokeAsync(Result<string>.Success("hello \"world\""));

        result.Should().BeOfType<Ok<string>>();
        await AssertSameResponseAsync(result, "hello \"world\"");
    }

    [Fact]
    public async Task Success_WithConfiguredJsonOptions_Should_Return_TypedOk_With_SameBody()
    {
        var shipment = new ShipmentDto("shipment-7", ShipmentState.InTransit);
        Action<IServiceCollection> configure = services => services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        object? result = await InvokeAsync(Result<ShipmentDto>.Success(shipment));

        result.Should().BeOfType<Ok<ShipmentDto>>();
        byte[] body = await AssertSameResponseAsync(result, shipment, configure);
        System.Text.Encoding.UTF8.GetString(body).Should().Be("{\"tracking_id\":\"shipment-7\",\"state\":\"InTransit\"}");
    }

    private static async Task<object?> InvokeAsync(object returned)
    {
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext());
        return await new ResultEndpointFilter().InvokeAsync(context, _ => new ValueTask<object?>(returned));
    }

    private static async Task<byte[]> AssertSameResponseAsync(object? actual, object? value, Action<IServiceCollection>? configure = null)
    {
        (int status, string? contentType, byte[] body) typed = await ExecuteAsync((Microsoft.AspNetCore.Http.IResult)actual!, configure);
        (int status, string? contentType, byte[] body) boxed = await ExecuteAsync(TypedResults.Ok(value), configure);

        typed.status.Should().Be(boxed.status);
        typed.contentType.Should().Be(boxed.contentType);
        typed.body.Should().Equal(boxed.body);
        return typed.body;
    }

    private static async Task<(int Status, string? ContentType, byte[] Body)> ExecuteAsync(Microsoft.AspNetCore.Http.IResult result, Action<IServiceCollection>? configure)
    {
        var collection = new ServiceCollection().AddLogging().AddOptions();
        configure?.Invoke(collection);
        var services = collection.BuildServiceProvider();
        var httpContext = new DefaultHttpContext { RequestServices = services };
        using var body = new MemoryStream();
        httpContext.Response.Body = body;

        await result.ExecuteAsync(httpContext);

        return (httpContext.Response.StatusCode, httpContext.Response.ContentType, body.ToArray());
    }

    public sealed record OrderDto(string Id, string Status, int Lines);

    [JsonPolymorphic]
    [JsonDerivedType(typeof(Circle), "circle")]
    public abstract record Shape;

    public sealed record Circle(double Radius) : Shape;

    public record PlainBase(string Id);

    public sealed record PlainDerived(string Id, string Extra) : PlainBase(Id);

    public interface INamed
    {
        string Name { get; }
    }

    public sealed record NamedDto(string Name, int Rank) : INamed;

    public enum ShipmentState
    {
        Pending,
        InTransit,
    }

    public sealed record ShipmentDto(string TrackingId, ShipmentState State);
}
