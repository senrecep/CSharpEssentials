using CSharpEssentials.GcpSecretManager.Models.Internal;
using FluentAssertions;

namespace CSharpEssentials.Tests.GcpSecretManager;

public class SecretLoadResultTests
{
    [Fact]
    public void SecretLoadResult_ShouldStoreProperties()
    {
        var result = new SecretLoadResult("path/to/secret", "secret-value", "config-key");

        result.Path.Should().Be("path/to/secret");
        result.Value.Should().Be("secret-value");
        result.Key.Should().Be("config-key");
    }

    [Fact]
    public void SecretLoadResult_WithEmptyValue_ShouldStoreEmptyString()
    {
        var result = new SecretLoadResult("path", "", "key");

        result.Value.Should().BeEmpty();
    }

    [Fact]
    public void SecretLoadResult_EqualInstances_ShouldBeEqual()
    {
        var result1 = new SecretLoadResult("path", "value", "key");
        var result2 = new SecretLoadResult("path", "value", "key");

        result1.Should().Be(result2);
        result1.GetHashCode().Should().Be(result2.GetHashCode());
    }

    [Fact]
    public void SecretLoadResult_DifferentInstances_ShouldNotBeEqual()
    {
        var result1 = new SecretLoadResult("path1", "value", "key");
        var result2 = new SecretLoadResult("path2", "value", "key");

        result1.Should().NotBe(result2);
    }

    [Fact]
    public void SecretLoadResult_Deconstruct_ShouldReturnValues()
    {
        var result = new SecretLoadResult("path", "value", "key");

        (string? path, string? value, string? key) = result;

        path.Should().Be("path");
        value.Should().Be("value");
        key.Should().Be("key");
    }
}
