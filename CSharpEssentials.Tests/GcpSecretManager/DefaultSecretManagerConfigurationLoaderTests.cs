using CSharpEssentials.GcpSecretManager;
using CSharpEssentials.GcpSecretManager.Configuration;
using FluentAssertions;
using Google.Cloud.SecretManager.V1;

namespace CSharpEssentials.Tests.GcpSecretManager;

public class DefaultSecretManagerConfigurationLoaderTests
{
    private readonly DefaultSecretManagerConfigurationLoader _loader = new();

    private static Secret CreateSecret(string projectId, string secretId)
    {
        return new Secret { SecretName = new SecretName(projectId, secretId) };
    }

    [Fact]
    public void GetKey_FromSecret_ShouldReturnSecretId()
    {
        Secret secret = CreateSecret("project", "my-secret");

        string key = _loader.GetKey(secret);

        key.Should().Be("my-secret");
    }

    [Fact]
    public void GetKey_FromString_ShouldReplaceDoubleUnderscoreWithColon()
    {
        string key = _loader.GetKey("Connection__String");

        key.Should().Be("Connection:String");
    }

    [Fact]
    public void GetKey_FromString_WithoutDoubleUnderscore_ShouldReturnAsIs()
    {
        string key = _loader.GetKey("SimpleKey");

        key.Should().Be("SimpleKey");
    }

    [Fact]
    public void GetKey_FromString_WithMultipleDoubleUnderscores_ShouldReplaceAll()
    {
        string key = _loader.GetKey("A__B__C");

        key.Should().Be("A:B:C");
    }

    [Fact]
    public void ShouldLoadSecret_WithNoFilters_ShouldReturnTrue()
    {
        Secret secret = CreateSecret("project", "any-secret");
        var config = new ProjectSecretConfiguration { ProjectId = "project" };

        bool result = _loader.ShouldLoadSecret(secret, config);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldLoadSecret_WithMatchingPrefix_ShouldReturnTrue()
    {
        Secret secret = CreateSecret("project", "APP_SETTING_1");
        var config = new ProjectSecretConfiguration
        {
            ProjectId = "project",
            PrefixFilters = new[] { "APP_" }
        };

        bool result = _loader.ShouldLoadSecret(secret, config);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldLoadSecret_WithNonMatchingPrefix_ShouldReturnFalse()
    {
        Secret secret = CreateSecret("project", "DB_SETTING_1");
        var config = new ProjectSecretConfiguration
        {
            ProjectId = "project",
            PrefixFilters = new[] { "APP_" }
        };

        bool result = _loader.ShouldLoadSecret(secret, config);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldLoadSecret_WithMatchingSecretId_ShouldReturnTrue()
    {
        Secret secret = CreateSecret("project", "specific-secret");
        var config = new ProjectSecretConfiguration
        {
            ProjectId = "project",
            SecretIds = new[] { "specific-secret" }
        };

        bool result = _loader.ShouldLoadSecret(secret, config);

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldLoadSecret_WithNonMatchingSecretId_ShouldReturnFalse()
    {
        Secret secret = CreateSecret("project", "other-secret");
        var config = new ProjectSecretConfiguration
        {
            ProjectId = "project",
            SecretIds = new[] { "specific-secret" }
        };

        bool result = _loader.ShouldLoadSecret(secret, config);

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldLoadSecret_WithPrefixAndSecretId_ShouldReturnTrueForEitherMatch()
    {
        Secret secret1 = CreateSecret("project", "APP_SETTING");
        Secret secret2 = CreateSecret("project", "specific-secret");
        Secret secret3 = CreateSecret("project", "other-secret");
        var config = new ProjectSecretConfiguration
        {
            ProjectId = "project",
            PrefixFilters = new[] { "APP_" },
            SecretIds = new[] { "specific-secret" }
        };

        _loader.ShouldLoadSecret(secret1, config).Should().BeTrue();
        _loader.ShouldLoadSecret(secret2, config).Should().BeTrue();
        _loader.ShouldLoadSecret(secret3, config).Should().BeFalse();
    }

    [Fact]
    public void ShouldLoadSecret_WithNullSecret_ShouldThrowArgumentNullException()
    {
        var config = new ProjectSecretConfiguration { ProjectId = "project" };

        Action action = () => _loader.ShouldLoadSecret(null!, config);

        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("secret");
    }

    [Fact]
    public void ShouldLoadSecret_WithNullConfig_ShouldThrowArgumentNullException()
    {
        Secret secret = CreateSecret("project", "secret");

        Action action = () => _loader.ShouldLoadSecret(secret, null!);

        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("projectConfig");
    }
}
