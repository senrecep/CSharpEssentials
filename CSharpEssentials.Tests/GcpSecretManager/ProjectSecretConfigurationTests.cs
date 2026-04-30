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
        config.PrefixFilters.Should().NotBeNull();
        config.SecretIds.Should().NotBeNull();
        config.RawSecretIds.Should().NotBeNull();
        config.RawSecretPrefixes.Should().NotBeNull();
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
}

