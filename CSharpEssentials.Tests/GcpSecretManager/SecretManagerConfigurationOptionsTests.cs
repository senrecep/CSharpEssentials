using CSharpEssentials.GcpSecretManager;
using CSharpEssentials.GcpSecretManager.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace CSharpEssentials.Tests.GcpSecretManager;

public class SecretManagerConfigurationOptionsTests
{
    [Fact]
    public void SecretManagerConfigurationOptions_ShouldInitializeWithDefaults()
    {
        var options = new SecretManagerConfigurationOptions();

        options.Projects.Should().NotBeNull();
        options.Projects.Should().BeEmpty();
        options.BatchSize.Should().Be(10);
        options.PageSize.Should().Be(300);
        options.ConfigurationSectionName.Should().Be("GoogleSecretManager");
        options.CredentialsPath.Should().BeNull();
        options.Loader.Should().BeNull();
        options.LoadFromAppSettings.Should().BeFalse();
    }

    [Fact]
    public void AddProject_ShouldAddProject()
    {
        var options = new SecretManagerConfigurationOptions();
        var project = new ProjectSecretConfiguration
        {
            ProjectId = "test-project"
        };

        options.AddProject(project);

        options.Projects.Should().HaveCount(1);
        options.Projects[0].ProjectId.Should().Be("test-project");
    }

    [Fact]
    public void AddProject_WithNull_ShouldThrowArgumentNullException()
    {
        var options = new SecretManagerConfigurationOptions();

        Action action = () => options.AddProject(null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddProject_WithMultipleProjects_ShouldAddAll()
    {
        var options = new SecretManagerConfigurationOptions();
        var project1 = new ProjectSecretConfiguration { ProjectId = "project1" };
        var project2 = new ProjectSecretConfiguration { ProjectId = "project2" };

        options.AddProject(project1);
        options.AddProject(project2);

        options.Projects.Should().HaveCount(2);
    }

    [Fact]
    public void SecretManagerConfigurationOptions_ShouldAllowSettingCredentialsPath()
    {
        var options = new SecretManagerConfigurationOptions
        {
            CredentialsPath = "/path/to/credentials.json"
        };

        options.CredentialsPath.Should().Be("/path/to/credentials.json");
    }

    [Fact]
    public void SecretManagerConfigurationOptions_ShouldAllowSettingBatchSize()
    {
        var options = new SecretManagerConfigurationOptions
        {
            BatchSize = 20
        };

        options.BatchSize.Should().Be(20);
    }

    [Fact]
    public void SecretManagerConfigurationOptions_ShouldAllowSettingPageSize()
    {
        var options = new SecretManagerConfigurationOptions
        {
            PageSize = 500
        };

        options.PageSize.Should().Be(500);
    }

    [Fact]
    public void SecretManagerConfigurationOptions_ShouldAllowCustomLoader()
    {
        var loader = new CustomLoader();
        var options = new SecretManagerConfigurationOptions
        {
            Loader = loader
        };

        options.Loader.Should().Be(loader);
    }

    [Fact]
    public void SecretManagerConfigurationOptions_ShouldAllowCustomSectionName()
    {
        var options = new SecretManagerConfigurationOptions
        {
            ConfigurationSectionName = "CustomSection"
        };

        options.ConfigurationSectionName.Should().Be("CustomSection");
    }

    [Fact]
    public void LoadFromConfiguration_WhenLoadFromAppSettingsIsFalse_ShouldNotLoad()
    {
        var options = new SecretManagerConfigurationOptions
        {
            LoadFromAppSettings = false
        };
        IConfigurationRoot configuration = new ConfigurationBuilder().Build();

        options.LoadFromConfiguration(configuration);

        options.Projects.Should().BeEmpty();
    }

    [Fact]
    public void LoadFromConfiguration_WhenSectionMissing_ShouldThrowInvalidOperationException()
    {
        var options = new SecretManagerConfigurationOptions
        {
            LoadFromAppSettings = true
        };
        IConfigurationRoot configuration = new ConfigurationBuilder().Build();

        Action action = () => options.LoadFromConfiguration(configuration);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Configuration section 'GoogleSecretManager' not found in appsettings.json");
    }

    [Fact]
    public void LoadFromConfiguration_WhenProjectIdEmpty_ShouldThrowInvalidOperationException()
    {
        var options = new SecretManagerConfigurationOptions
        {
            LoadFromAppSettings = true
        };
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GoogleSecretManager:Projects:0:ProjectId"] = "",
                ["GoogleSecretManager:Projects:0:Region"] = "us-central1"
            })
            .Build();

        Action action = () => options.LoadFromConfiguration(configuration);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("ProjectId is required for each project configuration");
    }

    [Fact]
    public void LoadFromConfiguration_WhenProjectIdMissing_ShouldThrowInvalidOperationException()
    {
        var options = new SecretManagerConfigurationOptions
        {
            LoadFromAppSettings = true
        };
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GoogleSecretManager:Projects:0:Region"] = "us-central1"
            })
            .Build();

        Action action = () => options.LoadFromConfiguration(configuration);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("ProjectId is required for each project configuration");
    }

    [Fact]
    public void LoadFromConfiguration_WithValidSection_ShouldLoadProjects()
    {
        var options = new SecretManagerConfigurationOptions
        {
            LoadFromAppSettings = true
        };
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GoogleSecretManager:Projects:0:ProjectId"] = "project1",
                ["GoogleSecretManager:Projects:0:Region"] = "us-central1",
                ["GoogleSecretManager:Projects:1:ProjectId"] = "project2"
            })
            .Build();

        options.LoadFromConfiguration(configuration);

        options.Projects.Should().HaveCount(2);
        options.Projects[0].ProjectId.Should().Be("project1");
        options.Projects[0].Region.Should().Be("us-central1");
        options.Projects[1].ProjectId.Should().Be("project2");
    }

    [Fact]
    public void LoadFromConfiguration_WithCustomSectionName_ShouldUseCustomName()
    {
        var options = new SecretManagerConfigurationOptions
        {
            LoadFromAppSettings = true,
            ConfigurationSectionName = "CustomSecrets"
        };
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CustomSecrets:Projects:0:ProjectId"] = "project1"
            })
            .Build();

        options.LoadFromConfiguration(configuration);

        options.Projects.Should().HaveCount(1);
        options.Projects[0].ProjectId.Should().Be("project1");
    }

    private sealed class CustomLoader : ISecretManagerConfigurationLoader
    {
        public string GetKey(Google.Cloud.SecretManager.V1.Secret secret) => secret.SecretName.SecretId;
        public string GetKey(string keyId) => keyId;
        public bool ShouldLoadSecret(Google.Cloud.SecretManager.V1.Secret secret, ProjectSecretConfiguration projectConfig) => true;
    }
}
