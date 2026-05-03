using CSharpEssentials.GcpSecretManager.Infrastructure;
using FluentAssertions;

namespace CSharpEssentials.Tests.GcpSecretManager;

public class SecretManagerPathsTests
{
    [Fact]
    public void BuildParentPath_WithoutRegion_ShouldReturnProjectPath()
    {
        string result = SecretManagerPaths.BuildParentPath("my-project", null);

        result.Should().Be("projects/my-project");
    }

    [Fact]
    public void BuildParentPath_WithRegion_ShouldReturnRegionalPath()
    {
        string result = SecretManagerPaths.BuildParentPath("my-project", "us-central1");

        result.Should().Be("projects/my-project/locations/us-central1");
    }

    [Fact]
    public void BuildParentPath_WithEmptyRegion_ShouldReturnProjectPath()
    {
        string result = SecretManagerPaths.BuildParentPath("my-project", "");

        result.Should().Be("projects/my-project");
    }

    [Fact]
    public void BuildSecretPath_WithoutRegion_ShouldReturnSecretPath()
    {
        string result = SecretManagerPaths.BuildSecretPath("my-project", null, "my-secret");

        result.Should().Be("projects/my-project/secrets/my-secret/versions/latest");
    }

    [Fact]
    public void BuildSecretPath_WithRegion_ShouldReturnRegionalSecretPath()
    {
        string result = SecretManagerPaths.BuildSecretPath("my-project", "europe-west1", "my-secret");

        result.Should().Be("projects/my-project/locations/europe-west1/secrets/my-secret/versions/latest");
    }

    [Fact]
    public void BuildSecretPath_WithEmptyRegion_ShouldReturnSecretPath()
    {
        string result = SecretManagerPaths.BuildSecretPath("my-project", "", "my-secret");

        result.Should().Be("projects/my-project/secrets/my-secret/versions/latest");
    }

    [Theory]
    [InlineData("proj", "reg", "sec", "projects/proj/locations/reg/secrets/sec/versions/latest")]
    [InlineData("proj", null, "sec", "projects/proj/secrets/sec/versions/latest")]
    [InlineData("proj", "", "sec", "projects/proj/secrets/sec/versions/latest")]
    public void BuildSecretPath_Theory_ShouldReturnExpectedPath(string projectId, string? region, string secretId, string expected)
    {
        string result = SecretManagerPaths.BuildSecretPath(projectId, region, secretId);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("proj", "reg", "projects/proj/locations/reg")]
    [InlineData("proj", null, "projects/proj")]
    [InlineData("proj", "", "projects/proj")]
    public void BuildParentPath_Theory_ShouldReturnExpectedPath(string projectId, string? region, string expected)
    {
        string result = SecretManagerPaths.BuildParentPath(projectId, region);
        result.Should().Be(expected);
    }
}
