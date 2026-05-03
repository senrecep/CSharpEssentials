using CSharpEssentials.GcpSecretManager;
using FluentAssertions;

namespace CSharpEssentials.Tests.GcpSecretManager;

public class ProjectSecretConfigurationTests
{
    [Fact]
    public void ProjectSecretConfiguration_ShouldInitializeWithDefaults()
    {
        var config = new ProjectSecretConfiguration
        {
            ProjectId = "test-project"
        };

        config.ProjectId.Should().Be("test-project");
        config.Region.Should().BeNull();
        config.PrefixFilters.Should().NotBeNull();
        config.PrefixFilters.Should().BeEmpty();
        config.SecretIds.Should().NotBeNull();
        config.SecretIds.Should().BeEmpty();
        config.RawSecretIds.Should().NotBeNull();
        config.RawSecretIds.Should().BeEmpty();
        config.RawSecretPrefixes.Should().NotBeNull();
        config.RawSecretPrefixes.Should().BeEmpty();
    }

    [Fact]
    public void IsRawSecret_WithRawSecretId_ShouldReturnTrue()
    {
        var config = new ProjectSecretConfiguration
        {
            ProjectId = "test-project",
            RawSecretIds = new[] { "raw-secret-1" }
        };

        config.IsRawSecret("raw-secret-1").Should().BeTrue();
        config.IsRawSecret("other-secret").Should().BeFalse();
    }

    [Fact]
    public void IsRawSecret_WithRawSecretPrefix_ShouldReturnTrue()
    {
        var config = new ProjectSecretConfiguration
        {
            ProjectId = "test-project",
            RawSecretPrefixes = new[] { "RAW_" }
        };

        config.IsRawSecret("RAW_secret1").Should().BeTrue();
        config.IsRawSecret("RAW_secret2").Should().BeTrue();
        config.IsRawSecret("JSON_secret1").Should().BeFalse();
    }

    [Fact]
    public void IsRawSecret_WithBothRawSecretIdAndPrefix_ShouldReturnTrue()
    {
        var config = new ProjectSecretConfiguration
        {
            ProjectId = "test-project",
            RawSecretIds = new[] { "specific-raw" },
            RawSecretPrefixes = new[] { "RAW_" }
        };

        config.IsRawSecret("specific-raw").Should().BeTrue();
        config.IsRawSecret("RAW_any").Should().BeTrue();
        config.IsRawSecret("other-secret").Should().BeFalse();
    }

    [Fact]
    public void ProjectSecretConfiguration_ShouldSupportRegion()
    {
        var config = new ProjectSecretConfiguration
        {
            ProjectId = "test-project",
            Region = "us-central1"
        };

        config.Region.Should().Be("us-central1");
    }

    [Fact]
    public void IsRawSecret_WithEmptyLists_ShouldReturnFalse()
    {
        var config = new ProjectSecretConfiguration
        {
            ProjectId = "test-project"
        };

        config.IsRawSecret("any-secret").Should().BeFalse();
        config.IsRawSecret("").Should().BeFalse();
    }

    [Fact]
    public void IsRawSecret_WithMultiplePrefixes_ShouldMatchAny()
    {
        var config = new ProjectSecretConfiguration
        {
            ProjectId = "test-project",
            RawSecretPrefixes = new[] { "RAW_", "PLAIN_" }
        };

        config.IsRawSecret("RAW_secret").Should().BeTrue();
        config.IsRawSecret("PLAIN_secret").Should().BeTrue();
        config.IsRawSecret("JSON_secret").Should().BeFalse();
    }

    [Fact]
    public void IsRawSecret_WithMultipleSecretIds_ShouldMatchAny()
    {
        var config = new ProjectSecretConfiguration
        {
            ProjectId = "test-project",
            RawSecretIds = new[] { "id1", "id2", "id3" }
        };

        config.IsRawSecret("id1").Should().BeTrue();
        config.IsRawSecret("id2").Should().BeTrue();
        config.IsRawSecret("id4").Should().BeFalse();
    }
}
