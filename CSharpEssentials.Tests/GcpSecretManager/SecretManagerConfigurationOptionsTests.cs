using CSharpEssentials.GcpSecretManager;
using CSharpEssentials.GcpSecretManager.Configuration;
using FluentAssertions;

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
}

