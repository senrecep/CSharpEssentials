using CSharpEssentials.GcpSecretManager.Configuration;
using CSharpEssentials.GcpSecretManager.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace CSharpEssentials.Tests.GcpSecretManager;

public class ExtensionsTests
{
    [Fact]
    public void AddGcpSecretManager_WithManualProjects_ShouldAddSource()
    {
        using var configuration = new ConfigurationManager();

        configuration.AddGcpSecretManager(options => options.AddProject(new() { ProjectId = "test-project" }));

        configuration.Sources.Should().Contain(x => x is SecretManagerConfigurationSource);
    }

    [Fact]
    public void AddGcpSecretManager_WithNullOptions_ShouldLoadFromAppSettings()
    {
        using var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["GoogleSecretManager:Projects:0:ProjectId"] = "test-project"
        });

        configuration.AddGcpSecretManager();

        configuration.Sources.Should().Contain(x => x is SecretManagerConfigurationSource);
    }

    [Fact]
    public void AddGcpSecretManager_WithLoadFromAppSettings_ShouldLoadFromAppSettings()
    {
        using var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["GoogleSecretManager:Projects:0:ProjectId"] = "test-project"
        });

        configuration.AddGcpSecretManager(options => options.LoadFromAppSettings = true);

        configuration.Sources.Should().Contain(x => x is SecretManagerConfigurationSource);
    }

    [Fact]
    public void AddGcpSecretManager_WithNoProjectsAndNoAppSettings_ShouldThrowArgumentException()
    {
        using var configuration = new ConfigurationManager();

        Action action = () => configuration.AddGcpSecretManager(_ => { });

        action.Should().Throw<ArgumentException>()
            .WithMessage("*At least one project configuration is required*");
    }

    [Fact]
    public void AddGcpSecretManager_WithNullOptionsAndNoAppSettingsSection_ShouldThrowInvalidOperationException()
    {
        using var configuration = new ConfigurationManager();

        Action action = () => configuration.AddGcpSecretManager();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Configuration section 'GoogleSecretManager' not found*");
    }

    [Fact]
    public void AddGcpSecretManager_ShouldReturnSameConfigurationManager()
    {
        using var configuration = new ConfigurationManager();

        IConfigurationManager result = configuration.AddGcpSecretManager(options => options.AddProject(new() { ProjectId = "test-project" }));

        result.Should().BeSameAs(configuration);
    }

    [Fact]
    public void AddGcpSecretManager_WithMultipleProjects_ShouldAddSingleSource()
    {
        using var configuration = new ConfigurationManager();

        configuration.AddGcpSecretManager(options =>
        {
            options.AddProject(new() { ProjectId = "project1" });
            options.AddProject(new() { ProjectId = "project2" });
        });

        configuration.Sources.Should().Contain(x => x is SecretManagerConfigurationSource);
    }

    [Fact]
    public void AddGcpSecretManager_WithCustomSectionName_ShouldUseCustomSection()
    {
        using var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["CustomSection:Projects:0:ProjectId"] = "test-project"
        });

        configuration.AddGcpSecretManager(options =>
        {
            options.LoadFromAppSettings = true;
            options.ConfigurationSectionName = "CustomSection";
        });

        configuration.Sources.Should().Contain(x => x is SecretManagerConfigurationSource);
    }
}
