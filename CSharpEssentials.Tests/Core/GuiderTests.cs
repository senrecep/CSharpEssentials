using FluentAssertions;
using static CSharpEssentials.Tests.TestData;
using CSharpEssentials.Core;

namespace CSharpEssentials.Tests.Core;

public class GuiderTests
{
    [Fact]
    public void NewGuid_ShouldReturnValidGuid()
    {
        Guid guid = Guider.NewGuid();

        guid.Should().NotBeEmpty();
        guid.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void NewGuid_ShouldReturnUniqueGuids()
    {
        Guid guid1 = Guider.NewGuid();
        Guid guid2 = Guider.NewGuid();

        guid1.Should().NotBe(guid2);
    }

    [Fact]
    public void ToStringFromGuid_ShouldReturnBase64String()
    {
        Guid guid = Guids.ValidGuid;
        string result = Guider.ToStringFromGuid(guid);

        result.Should().NotBeNullOrEmpty();
        result.Should().NotContain("=");
        result.Should().NotContain("/");
        result.Should().NotContain("+");
    }

    [Fact]
    public void ToGuidFromString_ShouldConvertBack()
    {
        Guid guid = Guids.ValidGuid;
        string str = Guider.ToStringFromGuid(guid);
        Guid result = Guider.ToGuidFromString(str);

        result.Should().Be(guid);
    }

    [Fact]
    public void ToGuidFromString_WithReadOnlySpan_ShouldWork()
    {
        Guid guid = Guids.ValidGuid;
        string str = Guider.ToStringFromGuid(guid);
        ReadOnlySpan<char> span = str;
        Guid result = Guider.ToGuidFromString(span);

        result.Should().Be(guid);
    }

    [Fact]
    public void ToStringFromGuid_And_ToGuidFromString_RoundTrip_ShouldWork()
    {
        var original = Guid.NewGuid();
        string str = original.ToStringFromGuid();
        Guid result = str.ToGuidFromString();

        result.Should().Be(original);
    }

    [Fact]
    public void ToStringFromGuid_WithEmptyGuid_ShouldWork()
    {
        string result = Guider.ToStringFromGuid(Guid.Empty);

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ToGuidFromString_And_ToStringFromGuid_Performance()
    {
        var guid = Guid.NewGuid();

        TestHelpers.MeasureExecutionTime(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                string str = guid.ToStringFromGuid();
                _ = str.ToGuidFromString();
            }
        }, out TimeSpan elapsed);

        elapsed.Should().BeLessThan(TimeSpan.FromSeconds(1));
    }
}

