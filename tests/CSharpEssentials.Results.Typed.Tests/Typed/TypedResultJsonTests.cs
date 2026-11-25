using System.Text.Json;
using System.Text.Json.Serialization;
using CSharpEssentials.Errors;
using CSharpEssentials.ResultPattern;
using FluentAssertions;

namespace CSharpEssentials.Results.Typed.Tests.Typed;

/// <summary>
/// The envelope converter writes <c>{"case":"&lt;name&gt;","value":...}</c>, reads each case back into its slot by the slot
/// type, and maps <see langword="default"/> to <c>null</c>. It runs on every target framework and on the netstandard2.1 asset.
/// </summary>
public sealed partial class TypedResultJsonTests
{
    private static string Write<TResult>(TResult result) => JsonSerializer.Serialize(result);

    private static TResult Read<TResult>(string json) => JsonSerializer.Deserialize<TResult>(json)!;

    private static TResult RoundTrip<TResult>(TResult result) => Read<TResult>(Write(result));

    [Fact]
    public void Serialize_Should_WriteTheSuccessEnvelope_When_TheResultHoldsAValue()
    {
        Result<int, NotFoundError> result = 5;

        string json = Write(result);

        json.Should().Be("""{"case":"Success","value":5}""");
    }

    [Fact]
    public void Serialize_Should_WriteTheSlotTypeName_When_TheResultHoldsAnError()
    {
        Result<int, NotFoundError, RateLimited> result = new RateLimited("Rate.Limited");

        string json = Write(result);

        json.Should().StartWith("""{"case":"RateLimited","value":{"Code":"Rate.Limited",""");
    }

    [Fact]
    public void Serialize_Should_NameTheDeclaredSlot_When_ADerivedErrorSitsInABaseSlot()
    {
        Result<int, ConflictError> result = new UserErrors.Locked(7);

        string json = Write(result);

        json.Should().StartWith("""{"case":"ConflictError","value":{""");
    }

    [Fact]
    public void RoundTrip_Should_KeepTheValue_When_TheResultIsASuccess()
    {
        Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited> result = new UserDto(3);

        Result<UserDto, NotFoundError, UserErrors.Locked, RateLimited> back = RoundTrip(result);

        back.TryGetValue(out UserDto value).Should().BeTrue();
        value.Id.Should().Be(3);
    }

    [Fact]
    public void RoundTrip_Should_KeepAPredefinedErrorEqual_When_ItHasNoMetadata()
    {
        Result<int, NotFoundError, UserErrors.Locked> result = UserErrors.NotFound;

        Result<int, NotFoundError, UserErrors.Locked> back = RoundTrip(result);

        back.Should().Be(result);
        back.TryGetValue(out NotFoundError error).Should().BeTrue();
        error.Should().NotBeSameAs(UserErrors.NotFound);
    }

    [Fact]
    public void RoundTrip_Should_KeepTheNestedErrorTypeAndItsFields_When_ItHasItsOwnSlot()
    {
        Result<int, NotFoundError, UserErrors.Locked> result = new UserErrors.Locked(7);

        Result<int, NotFoundError, UserErrors.Locked> back = RoundTrip(result);

        back.TryGetValue(out UserErrors.Locked error).Should().BeTrue();
        error.Id.Should().Be(7);
        error.Code.Should().Be(UserErrors.Codes.Locked);
        back.Should().Be(result);
    }

    [Fact]
    public void RoundTrip_Should_ReadTheBaseKind_When_ADerivedErrorSitsInABaseSlot()
    {
        Result<int, ConflictError> result = new UserErrors.Locked(7);

        Result<int, ConflictError> back = RoundTrip(result);

        back.TryGetValue(out ConflictError error).Should().BeTrue();
        error.Should().BeOfType<ConflictError>();
        error.Code.Should().Be(UserErrors.Codes.Locked);
    }

    [Fact]
    public void RoundTrip_Should_KeepAStructError()
    {
        Result<int, NotFoundError, UserErrors.Locked, RateLimited> result = new RateLimited("Rate.Limited");

        Result<int, NotFoundError, UserErrors.Locked, RateLimited> back = RoundTrip(result);

        back.TryGetValue(out RateLimited error).Should().BeTrue();
        error.Code.Should().Be("Rate.Limited");
        back.Should().Be(result);
    }

    [Fact]
    public void RoundTrip_Should_KeepTheCodeButNotEquality_When_TheErrorHasMetadata()
    {
        Result<int, NotFoundError> result = UserErrors.Deleted;

        Result<int, NotFoundError> back = RoundTrip(result);

        back.TryGetValue(out NotFoundError error).Should().BeTrue();
        error.Code.Should().Be(UserErrors.Codes.Deleted);
        error.Metadata!["reason"].Should().BeOfType<JsonElement>().Which.GetString().Should().Be("gdpr");
        back.Should().NotBe(result);
    }

    [Fact]
    public void Serialize_Should_WriteNull_When_TheResultIsDefault()
    {
        Result<int, NotFoundError, ConflictError> result = default;

        string json = Write(result);

        json.Should().Be("null");
    }

    [Fact]
    public void Deserialize_Should_ReturnDefault_When_TheJsonIsNull()
    {
        Result<int, NotFoundError> back = Read<Result<int, NotFoundError>>("null");

        back.HasValue.Should().BeFalse();
        back.Should().Be(default(Result<int, NotFoundError>));
    }

    [Fact]
    public void Deserialize_Should_ReadTheEnvelope_When_TheValueComesBeforeTheCase()
    {
        Result<int, NotFoundError> back = Read<Result<int, NotFoundError>>("""{"value":{"Code":"User.NotFound","Description":"User not found."},"case":"NotFoundError"}""");

        back.Should().Be((Result<int, NotFoundError>)UserErrors.NotFound);
    }

    [Fact]
    public void Deserialize_Should_FillTheMatchingSlot_When_TheJsonComesFromASmallerResult()
    {
        string json = Write<Result<int, NotFoundError>>(UserErrors.NotFound);

        Result<int, ConflictError, NotFoundError> back = Read<Result<int, ConflictError, NotFoundError>>(json);

        back.TryGetValue(out NotFoundError error).Should().BeTrue();
        error.Code.Should().Be(UserErrors.Codes.NotFound);
    }

    [Theory]
    [InlineData("""{"case":"ConflictError","value":{"Code":"c","Description":"d"}}""")]
    [InlineData("""{"case":"NotFoundError"}""")]
    [InlineData("""{"value":5}""")]
    [InlineData("""{"case":"NotFoundError","value":null}""")]
    [InlineData("""{"case":"Success","value":5,"extra":1}""")]
    [InlineData("""{"case":"Success","case":"Success","value":5}""")]
    [InlineData("""{"case":1,"value":5}""")]
    [InlineData("""[5]""")]
    [InlineData("5")]
    public void Deserialize_Should_ThrowJsonException_When_TheEnvelopeIsInvalid(string json)
    {
        Action act = () => Read<Result<int, NotFoundError>>(json);

        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Serialize_Should_UseFullNames_When_TwoSlotTypesShareAName()
    {
        Result<int, UserErrors.Locked, OtherErrors.Locked> result = new OtherErrors.Locked("x");

        string json = Write(result);
        Result<int, UserErrors.Locked, OtherErrors.Locked> back = Read<Result<int, UserErrors.Locked, OtherErrors.Locked>>(json);

        JsonDocument.Parse(json).RootElement.GetProperty("case").GetString().Should().Be(typeof(OtherErrors.Locked).FullName);
        back.TryGetValue(out OtherErrors.Locked error).Should().BeTrue();
        error.Reason.Should().Be("x");
    }

    [Fact]
    public void Serialize_Should_WriteThePropertyNamesOfTheOptions_When_TheOptionsSetANamingPolicy()
    {
        Result<UserDto, NotFoundError> result = new UserDto(3);

        string json = JsonSerializer.Serialize(result, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        json.Should().Be("""{"case":"Success","value":{"id":3}}""");
    }

    [Fact]
    public void RoundTrip_Should_UseTheRegisteredConverter_When_TheContextIsSourceGenerated()
    {
        Result<int, NotFoundError, RateLimited> result = UserErrors.NotFound;

        string json = JsonSerializer.Serialize(result, TypedResultJsonContext.Default.ResultInt32NotFoundErrorRateLimited);
        Result<int, NotFoundError, RateLimited> back = JsonSerializer.Deserialize(json, TypedResultJsonContext.Default.ResultInt32NotFoundErrorRateLimited);

        json.Should().StartWith("""{"case":"NotFoundError","value":{""").And.Contain("\"Code\":\"User.NotFound\"");
        back.Should().Be(result);
    }

    [Fact]
    public void RoundTrip_Should_KeepAStructError_When_TheContextIsSourceGenerated()
    {
        Result<int, NotFoundError, RateLimited> result = new RateLimited("Rate.Limited");

        Result<int, NotFoundError, RateLimited> back = JsonSerializer.Deserialize(
            JsonSerializer.Serialize(result, TypedResultJsonContext.Default.ResultInt32NotFoundErrorRateLimited),
            TypedResultJsonContext.Default.ResultInt32NotFoundErrorRateLimited);

        back.Should().Be(result);
    }

    [Fact]
    public void Serialize_Should_ThrowNotSupportedException_When_TheContextLacksASlotType()
    {
        Result<int, NotFoundError> result = UserErrors.NotFound;

        Action act = () => JsonSerializer.Serialize(result, MissingSlotJsonContext.Default.ResultInt32NotFoundError);

        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void Factory_Should_CreateTheClosedConverter_When_TheTypeIsATypedResult()
    {
        TypedResultJsonConverterFactory factory = new();

        JsonConverter converter = factory.CreateConverter(typeof(Result<int, NotFoundError, ConflictError, RateLimited>), JsonSerializerOptions.Default);

        converter.Should().BeOfType<TypedResultJsonConverter<int, NotFoundError, ConflictError, RateLimited>>();
    }

    [Theory]
    [InlineData(typeof(Result<int, NotFoundError>), true)]
    [InlineData(typeof(Result<int, NotFoundError, ConflictError>), true)]
    [InlineData(typeof(Result<int>), false)]
    [InlineData(typeof(Result<,>), false)]
    [InlineData(typeof(int), false)]
    public void Factory_Should_ConvertOnlyTypedResults(Type type, bool expected)
    {
        bool canConvert = new TypedResultJsonConverterFactory().CanConvert(type);

        canConvert.Should().Be(expected);
    }

    internal static class OtherErrors
    {
        internal sealed record Locked : ConflictError
        {
            public Locked(string reason)
                : base("Other.Locked", "Locked elsewhere.") => Reason = reason;

            public string Reason { get; }
        }
    }

    [JsonSourceGenerationOptions(Converters = [typeof(TypedResultJsonConverter<int, NotFoundError, RateLimited>)])]
    [JsonSerializable(typeof(Result<int, NotFoundError, RateLimited>))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(NotFoundError))]
    [JsonSerializable(typeof(RateLimited))]
    internal sealed partial class TypedResultJsonContext : JsonSerializerContext;

    [JsonSourceGenerationOptions(Converters = [typeof(TypedResultJsonConverter<int, NotFoundError>)])]
    [JsonSerializable(typeof(Result<int, NotFoundError>))]
    [JsonSerializable(typeof(int))]
    internal sealed partial class MissingSlotJsonContext : JsonSerializerContext;
}
