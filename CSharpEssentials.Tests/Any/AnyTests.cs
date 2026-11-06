using System.Text.Json;
using CSharpEssentials.Any;
using FluentAssertions;

namespace CSharpEssentials.Tests.Any;

public class AnyTests
{
    private static readonly JsonSerializerOptions SnakeCaseOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
    private static readonly JsonSerializerOptions CaseSensitiveOptions = new() { PropertyNameCaseInsensitive = false };

    [Fact]
    public void Create_WithFirstType_ShouldCreateAny()
    {
        var any = CSharpEssentials.Any.Any.Create<int, string>(42);

        any.Index.Should().Be(0);
        any.IsFirst.Should().BeTrue();
        any.IsSecond.Should().BeFalse();
        any.GetFirst().Should().Be(42);
    }

    [Fact]
    public void Create_WithSecondType_ShouldCreateAny()
    {
        var any = CSharpEssentials.Any.Any.Create<int, string>("test");

        any.Index.Should().Be(1);
        any.IsFirst.Should().BeFalse();
        any.IsSecond.Should().BeTrue();
        any.GetSecond().Should().Be("test");
    }

    [Fact]
    public void First_ShouldCreateAnyWithFirstType()
    {
        var any = Any<int, string>.First(42);

        any.Index.Should().Be(0);
        any.GetFirst().Should().Be(42);
    }

    [Fact]
    public void Second_ShouldCreateAnyWithSecondType()
    {
        var any = Any<int, string>.Second("test");

        any.Index.Should().Be(1);
        any.GetSecond().Should().Be("test");
    }

    [Fact]
    public void ImplicitConversion_FromFirstType_ShouldWork()
    {
        Any<int, string> any = 42;

        any.Index.Should().Be(0);
        any.GetFirst().Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromSecondType_ShouldWork()
    {
        Any<int, string> any = "test";

        any.Index.Should().Be(1);
        any.GetSecond().Should().Be("test");
    }

    [Fact]
    public void Switch_WithFirstValue_ShouldExecuteFirstAction()
    {
        Any<int, string> any = 42;
        bool executed = false;
        AnyActionStatus status = any.Switch(first: _ => executed = true);

        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WithSecondValue_ShouldExecuteSecondAction()
    {
        Any<int, string> any = "test";
        bool executed = false;
        AnyActionStatus status = any.Switch(second: _ => executed = true);

        executed.Should().BeTrue();
        status.Should().Be(AnyActionStatus.Executed);
    }

    [Fact]
    public void Switch_WithNoMatchingAction_ShouldReturnNotExecuted()
    {
        Any<int, string> any = 42;
        AnyActionStatus status = any.Switch(second: _ => { });

        status.Should().Be(AnyActionStatus.NotExecuted);
    }

    [Fact]
    public void Match_WithFirstValue_ShouldReturnFirstResult()
    {
        Any<int, string> any = 42;
        AnyActionResult<int> result = any.Match(first: x => x * 2, second: _ => 0);

        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(84);
    }

    [Fact]
    public void Match_WithSecondValue_ShouldReturnSecondResult()
    {
        Any<int, string> any = "test";
        AnyActionResult<int> result = any.Match(first: _ => 0, second: x => x.Length);

        result.Status.Should().Be(AnyActionStatus.Executed);
        result.Result.Should().Be(4);
    }

    [Fact]
    public void Match_WithNoMatchingFunction_ShouldReturnNotExecuted()
    {
        Any<int, string> any = 42;
        AnyActionResult<int> result = any.Match(second: _ => 0);

        result.Status.Should().Be(AnyActionStatus.NotExecuted);
    }

    [Fact]
    public void GetFirst_WithFirstValue_ShouldReturnValue()
    {
        Any<int, string> any = 42;
        int value = any.GetFirst();

        value.Should().Be(42);
    }

    [Fact]
    public void GetFirst_WithSecondValue_ShouldThrow()
    {
        Any<int, string> any = "test";

        Assert.Throws<InvalidOperationException>(() => any.GetFirst());
    }

    [Fact]
    public void GetSecond_WithSecondValue_ShouldReturnValue()
    {
        Any<int, string> any = "test";
        string value = any.GetSecond();

        value.Should().Be("test");
    }

    [Fact]
    public void GetSecond_WithFirstValue_ShouldThrow()
    {
        Any<int, string> any = 42;

        Assert.Throws<InvalidOperationException>(() => any.GetSecond());
    }

    [Fact]
    public void JsonSerialization_ShouldWork()
    {
        Any<int, string> any = 42;
        string json = JsonSerializer.Serialize(any);
        Any<int, string> deserialized = JsonSerializer.Deserialize<Any<int, string>>(json);

        deserialized.Index.Should().Be(0);
        deserialized.GetFirst().Should().Be(42);
    }

    [Fact]
    public void ToString_ShouldReturnTheValueToString()
    {
        Any<int, string> any = 42;
        string str = any.ToString();

        str.Should().Be("42");
    }

    [Fact]
    public void ToString_WithNullValue_ShouldReturnEmptyString()
    {
        Any<int, string> any = default;

        any.ToString().Should().BeEmpty();
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        Any<int, string> any1 = 42;
        Any<int, string> any2 = 42;

        any1.Should().Be(any2);
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        Any<int, string> any1 = 42;
        Any<int, string> any2 = 43;

        any1.Should().NotBe(any2);
    }

    [Fact]
    public void JsonRoundTrip_WithNullReferenceBranch_ShouldKeepIndexAndNullValue()
    {
        Any<int, string> any = Any<int, string>.Second(null!);

        string json = JsonSerializer.Serialize(any);
        Any<int, string> deserialized = JsonSerializer.Deserialize<Any<int, string>>(json);

        deserialized.Index.Should().Be(1);
        deserialized.Value.Should().BeNull();
    }

    [Fact]
    public void JsonRoundTrip_WithNullNullableBranch_ShouldKeepIndexAndNullValue()
    {
        Any<int?, string> any = Any<int?, string>.First(null);

        string json = JsonSerializer.Serialize(any);
        Any<int?, string> deserialized = JsonSerializer.Deserialize<Any<int?, string>>(json);

        deserialized.Index.Should().Be(0);
        deserialized.GetFirst().Should().BeNull();
        deserialized.Value.Should().BeNull();
    }

    [Fact]
    public void JsonDeserialize_WithNullValueTypeBranch_ShouldThrow()
    {
        Action act = () => JsonSerializer.Deserialize<Any<int, string>>("""{"Index":0,"Value":null}""");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void JsonRoundTrip_WithSnakeCaseOptions_ShouldKeepInnerProperties()
    {
        Any<int, SnakeCasePerson> any = new SnakeCasePerson { FirstName = "Ada" };

        string json = JsonSerializer.Serialize(any, SnakeCaseOptions);
        Any<int, SnakeCasePerson> deserialized = JsonSerializer.Deserialize<Any<int, SnakeCasePerson>>(json, SnakeCaseOptions);

        json.Should().Contain("first_name");
        deserialized.GetSecond().FirstName.Should().Be("Ada");
    }

    [Fact]
    public void JsonDeserialize_ShouldUseCallerOptionsForInnerValue()
    {
        Any<int, SnakeCasePerson> deserialized = JsonSerializer.Deserialize<Any<int, SnakeCasePerson>>(
            """{"Index":1,"Value":{"firstName":"Ada"}}""", CaseSensitiveOptions);

        deserialized.GetSecond().FirstName.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithJsonElementValue_ShouldDeserializeWithDefaultOptions()
    {
        using JsonDocument document = JsonDocument.Parse("""{"firstName":"Ada"}""");

        var any = new Any<int, SnakeCasePerson>(1, document.RootElement);

        any.GetSecond().FirstName.Should().Be("Ada");
    }

    public sealed class SnakeCasePerson
    {
        public string? FirstName { get; set; }
    }
}
