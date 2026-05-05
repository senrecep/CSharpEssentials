using System.Text.Json;
using CSharpEssentials.Maybe;
using FluentAssertions;

namespace CSharpEssentials.Tests.Maybe;

public class MaybeTests
{
    [Fact]
    public void Some_ShouldCreateMaybeWithValue()
    {
        var maybe = Maybe<int>.From(42);

        maybe.HasValue.Should().BeTrue();
        maybe.HasNoValue.Should().BeFalse();
        maybe.Value.Should().Be(42);
    }

    [Fact]
    public void None_ShouldCreateMaybeWithoutValue()
    {
        Maybe<int> maybe = Maybe<int>.None;

        maybe.HasValue.Should().BeFalse();
        maybe.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void From_WithNull_ShouldReturnNone()
    {
        string? value = null;
        var maybe = Maybe<string>.From(value);

        maybe.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void From_WithValue_ShouldReturnSome()
    {
        var maybe = Maybe<int>.From(42);

        maybe.HasValue.Should().BeTrue();
        maybe.Value.Should().Be(42);
    }

    [Fact]
    public void From_WithFunction_ShouldReturnSome()
    {
        var maybe = Maybe<int>.From(() => 42);

        maybe.HasValue.Should().BeTrue();
        maybe.Value.Should().Be(42);
    }

    [Fact]
    public void From_WithNullFunction_ShouldReturnNone()
    {
        var maybe = Maybe<string>.From(() => (string?)null);

        maybe.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public async Task FromAsync_WithTask_ShouldReturnSome()
    {
        // Using string (reference type) for FromAsync since T? works naturally with reference types
        Task<Maybe<string>> maybeTask = Maybe<string>.FromAsync(Task.FromResult<string?>("hello"));
        Maybe<string> result = await maybeTask;

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public async Task FromAsync_WithNullTask_ShouldReturnNone()
    {
        Task<Maybe<string>> maybeTask = Maybe<string>.FromAsync(Task.FromResult<string?>(null));
        Maybe<string> result = await maybeTask;

        result.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void GetValueOrThrow_WithValue_ShouldReturnValue()
    {
        var maybe = Maybe<int>.From(42);
        int value = maybe.GetValueOrThrow();

        value.Should().Be(42);
    }

    [Fact]
    public void GetValueOrThrow_WithoutValue_ShouldThrow()
    {
        Maybe<int> maybe = Maybe<int>.None;

        Assert.Throws<InvalidOperationException>(() => maybe.GetValueOrThrow());
    }

    [Fact]
    public void GetValueOrThrow_WithCustomMessage_ShouldUseMessage()
    {
        Maybe<int> maybe = Maybe<int>.None;

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => maybe.GetValueOrThrow("Custom error"));
        exception.Message.Should().Be("Custom error");
    }

    [Fact]
    public void GetValueOrThrow_WithException_ShouldThrowException()
    {
        Maybe<int> maybe = Maybe<int>.None;
        var customException = new ArgumentException("Custom");

        Assert.Throws<ArgumentException>(() => maybe.GetValueOrThrow(customException));
    }

    [Fact]
    public void GetValueOrDefault_WithValue_ShouldReturnValue()
    {
        var maybe = Maybe<int>.From(42);
        int value = maybe.GetValueOrDefault(0);

        value.Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_WithoutValue_ShouldReturnDefault()
    {
        Maybe<int> maybe = Maybe<int>.None;
        int value = maybe.GetValueOrDefault(100);

        value.Should().Be(100);
    }

    [Fact]
    public void GetValueOrDefault_WithoutDefault_ShouldReturnDefaultOfType()
    {
        Maybe<int> maybe = Maybe<int>.None;
        int value = maybe.GetValueOrDefault();

        value.Should().Be(0);
    }

    [Fact]
    public void TryGetValue_WithValue_ShouldReturnTrue()
    {
        var maybe = Maybe<int>.From(42);
        bool success = maybe.TryGetValue(out int value);

        success.Should().BeTrue();
        value.Should().Be(42);
    }

    [Fact]
    public void TryGetValue_WithoutValue_ShouldReturnFalse()
    {
        Maybe<int> maybe = Maybe<int>.None;
        bool success = maybe.TryGetValue(out int value);

        success.Should().BeFalse();
        value.Should().Be(0);
    }

    [Fact]
    public void Deconstruct_ShouldWork()
    {
        var maybe = Maybe<int>.From(42);
        (bool hasValue, int value) = maybe;

        hasValue.Should().BeTrue();
        value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSome()
    {
        Maybe<int> maybe = 42;

        maybe.HasValue.Should().BeTrue();
        maybe.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromNull_ShouldCreateNone()
    {
        string? value = null;
        Maybe<string> maybe = value;

        maybe.HasNoValue.Should().BeTrue();
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        var maybe1 = Maybe<int>.From(42);
        var maybe2 = Maybe<int>.From(42);

        maybe1.Should().Be(maybe2);
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        var maybe1 = Maybe<int>.From(42);
        var maybe2 = Maybe<int>.From(43);

        maybe1.Should().NotBe(maybe2);
    }

    [Fact]
    public void Equality_WithNone_ShouldBeEqual()
    {
        Maybe<int> maybe1 = Maybe<int>.None;
        Maybe<int> maybe2 = Maybe<int>.None;

        maybe1.Should().Be(maybe2);
    }

    [Fact]
    public void JsonSerialization_ShouldWork()
    {
        var maybe = Maybe<int>.From(42);
        string json = JsonSerializer.Serialize(maybe);
        Maybe<int> deserialized = JsonSerializer.Deserialize<Maybe<int>>(json);

        deserialized.HasValue.Should().BeTrue();
        deserialized.Value.Should().Be(42);
    }

    [Fact]
    public void JsonSerialization_WithNone_ShouldWork()
    {
        Maybe<int> maybe = Maybe<int>.None;
        string json = JsonSerializer.Serialize(maybe);
        Maybe<int> deserialized = JsonSerializer.Deserialize<Maybe<int>>(json);

        deserialized.HasNoValue.Should().BeTrue();
    }
}

