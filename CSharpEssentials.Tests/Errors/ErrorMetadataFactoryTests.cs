using CSharpEssentials.Errors;
using FluentAssertions;

namespace CSharpEssentials.Tests.Errors;

public class ErrorMetadataFactoryTests
{
    [Fact]
    public void DictionaryConstructor_ShouldCopyEntries()
    {
        Dictionary<string, object?> source = new() { ["a"] = 1, ["b"] = null };

        var metadata = new ErrorMetadata(source);
        source["c"] = 3;

        metadata.Should().HaveCount(2);
        metadata["a"].Should().Be(1);
        metadata["b"].Should().BeNull();
    }

    [Fact]
    public void KeyValueConstructors_ShouldCreateSingleEntry()
    {
        new ErrorMetadata("key", "value").Should().ContainSingle().Which.Should().Be(new KeyValuePair<string, object?>("key", "value"));
        new ErrorMetadata(new KeyValuePair<string, object?>("k", 5))["k"].Should().Be(5);
    }

    [Fact]
    public void CreateEmpty_ShouldReturnNewEmptyInstanceEachCall()
    {
        ErrorMetadata first = ErrorMetadata.CreateEmpty();

        first.Should().BeEmpty();
        first.Should().NotBeSameAs(ErrorMetadata.CreateEmpty());
    }

    [Fact]
    public void CreateWithStackTrace_ShouldContainCurrentStackTrace()
    {
        ErrorMetadata metadata = ErrorMetadata.CreateWithStackTrace();

        metadata.Should().ContainSingle();
        metadata["stackTrace"].Should().BeOfType<string>().Which.Should().Contain(nameof(CreateWithStackTrace_ShouldContainCurrentStackTrace));
    }

    [Fact]
    public void CreateWithException_ShouldStoreExceptionInstance()
    {
        var exception = new InvalidOperationException("boom");

        ErrorMetadata.CreateWithException(exception)["exception"].Should().BeSameAs(exception);
    }

    [Fact]
    public void CreateWithExceptionDetailed_ShouldStoreTypeMessageAndNestedInnerException()
    {
        var exception = new InvalidOperationException("outer", new ArgumentException("inner"));

        ErrorMetadata metadata = ErrorMetadata.CreateWithExceptionDetailed(exception);

        metadata["exceptionType"].Should().Be(nameof(InvalidOperationException));
        metadata["exceptionMessage"].Should().Be("outer");
        metadata["exceptionStackTrace"].Should().BeOfType<string>().Which.Should().NotBeEmpty();
        ErrorMetadata inner = metadata["innerException"].Should().BeOfType<ErrorMetadata>().Subject;
        inner["exceptionType"].Should().Be(nameof(ArgumentException));
        inner["exceptionMessage"].Should().Be("inner");
        inner.Should().NotContainKey("innerException");
    }

    [Fact]
    public void CreateWithExceptionDetailed_WithThrownException_ShouldUseItsStackTrace()
    {
        Exception exception;
        try
        {
            throw new InvalidOperationException("thrown");
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        ErrorMetadata.CreateWithExceptionDetailed(exception)["exceptionStackTrace"].Should().Be(exception.StackTrace);
    }

    [Fact]
    public void AddMetadata_ShouldAddNewKeyAndKeepExistingValue()
    {
        var metadata = new ErrorMetadata("key", "first");

        ErrorMetadata returned = metadata.AddMetadata("key", "second").AddMetadata("other", 2);

        returned.Should().BeSameAs(metadata);
        metadata["key"].Should().Be("first");
        metadata["other"].Should().Be(2);
    }

    [Fact]
    public void AddStackTrace_ShouldAddStackTraceOnce()
    {
        var metadata = new ErrorMetadata("stackTrace", "existing");
        ErrorMetadata empty = [];

        metadata.AddStackTrace().Should().BeSameAs(metadata);
        metadata["stackTrace"].Should().Be("existing");
        empty.AddStackTrace()["stackTrace"].Should().BeOfType<string>().Which.Should().NotBeEmpty();
    }

    [Fact]
    public void AddException_ShouldStoreExceptionWithoutOverwriting()
    {
        var first = new InvalidOperationException("first");
        ErrorMetadata metadata = [];

        metadata.AddException(first).AddException(new ArgumentException("second")).Should().BeSameAs(metadata);

        metadata.Should().ContainSingle();
        metadata["exception"].Should().BeSameAs(first);
    }

    [Fact]
    public void AddExceptionDetailed_ShouldMatchCreateWithExceptionDetailed()
    {
        var exception = new InvalidOperationException("outer", new ArgumentException("inner"));
        ErrorMetadata metadata = new("existing", 1);

        metadata.AddExceptionDetailed(exception).Should().BeSameAs(metadata);

        metadata.Keys.Should().BeEquivalentTo("existing", "exceptionType", "exceptionStackTrace", "exceptionMessage", "innerException");
        metadata["exceptionType"].Should().Be(nameof(InvalidOperationException));
        metadata["exceptionMessage"].Should().Be("outer");
        metadata["innerException"].Should().BeOfType<ErrorMetadata>().Which["exceptionMessage"].Should().Be("inner");
    }

    [Fact]
    public void AddExceptionDetailed_WithoutInnerException_ShouldNotAddInnerKey()
    {
        ErrorMetadata metadata = [];

        metadata.AddExceptionDetailed(new InvalidOperationException("only"));

        metadata.Should().HaveCount(3).And.NotContainKey("innerException");
    }

    [Fact]
    public void AddExceptionDetailed_ShouldKeepExistingExceptionKeys()
    {
        ErrorMetadata metadata = new("exceptionMessage", "kept");

        metadata.AddExceptionDetailed(new InvalidOperationException("ignored"));

        metadata["exceptionMessage"].Should().Be("kept");
    }

    [Fact]
    public void Combine_WithOverlappingKey_ShouldKeepOriginalValueAndReturnSameInstance()
    {
        ErrorMetadata metadata = new() { ["shared"] = "original", ["own"] = 1 };
        ErrorMetadata other = new() { ["shared"] = "other", ["extra"] = null };

        ErrorMetadata combined = metadata.Combine(other);

        combined.Should().BeSameAs(metadata);
        combined["shared"].Should().Be("original");
        combined.Should().ContainKey("extra").WhoseValue.Should().BeNull();
        other.Should().HaveCount(2);
    }

    [Fact]
    public void ToString_ShouldListEntriesInInsertionOrder()
    {
        ErrorMetadata metadata = new() { ["a"] = 1, ["b"] = null, ["c"] = 2.5 };

        metadata.ToString().Should().Be("[{ a: 1 }, { b:  }, { c: 2.5 }]");
    }

    [Fact]
    public void ToString_WithSingleEntry_ShouldHaveNoSeparator()
    {
        new ErrorMetadata("key", "value").ToString().Should().Be("[{ key: value }]");
    }

    [Fact]
    public void ToString_WhenEmpty_ShouldReturnEmptyBrackets()
    {
        ErrorMetadata.CreateEmpty().ToString().Should().Be("[]");
    }
}
