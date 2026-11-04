using System.Globalization;
using CSharpEssentials.Errors;
using FluentAssertions;

namespace CSharpEssentials.Tests.Errors;

public class ErrorMetadataTests
{
    #region Creation

    [Fact]
    public void Constructor_ShouldCreateEmptyMetadata()
    {
        var metadata = new ErrorMetadata();

        metadata.Should().BeEmpty();
    }

    [Fact]
    public void Indexer_ShouldSetAndGetValue()
    {
        ErrorMetadata metadata = new()
        {
            ["key"] = "value"
        };

        metadata["key"].Should().Be("value");
    }

    #endregion

    #region CreateWithException

    [Fact]
    public void CreateWithException_ShouldContainExceptionInfo()
    {
        Exception exception = new InvalidOperationException("Test exception");

        var metadata = ErrorMetadata.CreateWithException(exception);

        ErrorMetadata nested = metadata["exception"].Should().BeOfType<ErrorMetadata>().Subject;
        nested["exceptionType"].Should().Be("InvalidOperationException");
        nested["exceptionMessage"].Should().Be("Test exception");
    }

    [Fact]
    public void AddException_ShouldStoreNestedExceptionInfo()
    {
        Exception exception = new InvalidOperationException("Test exception");

        var metadata = new ErrorMetadata().AddException(exception);

        ErrorMetadata nested = metadata["exception"].Should().BeOfType<ErrorMetadata>().Subject;
        nested["exceptionType"].Should().Be("InvalidOperationException");
        nested["exceptionMessage"].Should().Be("Test exception");
    }

    [Fact]
    public void CreateWithExceptionDetailed_ShouldContainDetailedInfo()
    {
        Exception exception = new InvalidOperationException("Test exception");

        var metadata = ErrorMetadata.CreateWithExceptionDetailed(exception);

        metadata.Should().ContainKey("exceptionType");
        metadata.Should().ContainKey("exceptionMessage");
        metadata.Should().ContainKey("exceptionStackTrace");
    }

    [Fact]
    public void CreateWithExceptionDetailed_WithInnerException_ShouldContainInfo()
    {
        Exception innerException = new ArgumentException("Inner");
        Exception exception = new InvalidOperationException("Outer", innerException);

        var metadata = ErrorMetadata.CreateWithExceptionDetailed(exception);

        metadata.Should().ContainKey("exceptionType");
        metadata.Should().ContainKey("innerException");
    }

    #endregion

    #region Combine

    [Fact]
    public void Combine_WithNull_ShouldReturnOriginal()
    {
        ErrorMetadata metadata = new() { ["key"] = "value" };

        ErrorMetadata combined = metadata.Combine(null);

        combined.Should().ContainKey("key");
        combined["key"].Should().Be("value");
    }

    [Fact]
    public void Combine_WithOtherMetadata_ShouldMerge()
    {
        ErrorMetadata metadata1 = new() { ["key1"] = "value1" };
        ErrorMetadata metadata2 = new() { ["key2"] = "value2" };

        ErrorMetadata combined = metadata1.Combine(metadata2);

        combined.Should().ContainKey("key1");
        combined.Should().ContainKey("key2");
    }

    #endregion

    #region Dictionary Operations

    [Fact]
    public void Add_ShouldAddKeyValue()
    {
        ErrorMetadata metadata = new()
        {
            { "key", "value" }
        };

        metadata.Should().ContainKey("key");
    }

    [Fact]
    public void ContainsKey_WithExistingKey_ShouldReturnTrue()
    {
        ErrorMetadata metadata = new() { ["key"] = "value" };

        metadata.ContainsKey("key").Should().BeTrue();
    }

    [Fact]
    public void ContainsKey_WithNonExistingKey_ShouldReturnFalse()
    {
        ErrorMetadata metadata = [];

        metadata.ContainsKey("key").Should().BeFalse();
    }

    [Fact]
    public void TryGetValue_WithExistingKey_ShouldReturnTrue()
    {
        ErrorMetadata metadata = new() { ["key"] = "value" };

        bool found = metadata.TryGetValue("key", out object? value);

        found.Should().BeTrue();
        value.Should().Be("value");
    }

    [Fact]
    public void Remove_ShouldRemoveKey()
    {
        ErrorMetadata metadata = new() { ["key"] = "value" };

        metadata.Remove("key");

        metadata.Should().NotContainKey("key");
    }

    [Fact]
    public void Clear_ShouldRemoveAllKeys()
    {
        ErrorMetadata metadata = new()
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };

        metadata.Clear();

        metadata.Should().BeEmpty();
    }

    #endregion

    #region Count and Keys

    [Fact]
    public void Count_ShouldReturnNumberOfItems()
    {
        ErrorMetadata metadata = new()
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };

        metadata.Count.Should().Be(2);
    }

    [Fact]
    public void Keys_ShouldReturnAllKeys()
    {
        ErrorMetadata metadata = new()
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };

        metadata.Keys.Should().Contain("key1");
        metadata.Keys.Should().Contain("key2");
    }

    [Fact]
    public void Values_ShouldReturnAllValues()
    {
        ErrorMetadata metadata = new()
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };

        metadata.Values.Should().Contain("value1");
        metadata.Values.Should().Contain("value2");
    }

    #endregion

    #region Formatting

    [Fact]
    public void ToString_UnderTurkishCulture_ShouldFormatValuesInvariantly()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
        try
        {
            var metadata = new ErrorMetadata { ["amount"] = 2.5 };

            metadata.ToString().Should().Be("[{ amount: 2.5 }]");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void ToString_WithSeveralEntries_ShouldListEachInInsertionOrder()
    {
        var metadata = new ErrorMetadata { ["a"] = 1, ["b"] = "two", ["c"] = null };

        metadata.ToString().Should().Be("[{ a: 1 }, { b: two }, { c:  }]");
    }

    [Fact]
    public void ToString_WhenEmpty_ShouldReturnEmptyBrackets()
    {
        new ErrorMetadata().ToString().Should().Be("[]");
    }

    #endregion
}
