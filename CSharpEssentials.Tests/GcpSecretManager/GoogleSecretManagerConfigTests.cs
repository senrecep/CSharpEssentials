using CSharpEssentials.GcpSecretManager.Configuration;
using FluentAssertions;

namespace CSharpEssentials.Tests.GcpSecretManager;

public class GoogleSecretManagerConfigTests
{
    [Fact]
    public void GoogleSecretManagerConfig_ShouldHaveCorrectSectionName()
    {
        GoogleSecretManagerConfig.SectionName.Should().Be("GoogleSecretManager");
    }

    [Fact]
    public void GoogleSecretManagerConfig_ShouldInitializeWithEmptyProjects()
    {
        var config = new GoogleSecretManagerConfig();

        config.Projects.Should().NotBeNull();
        config.Projects.Should().BeEmpty();
    }

    [Fact]
    public void GoogleSecretManagerConfig_ShouldAllowAddingProjects()
    {
        var config = new GoogleSecretManagerConfig
        {
            Projects =
            [
                new() { ProjectId = "project1" },
                new() { ProjectId = "project2" }
            ]
        };

        config.Projects.Should().HaveCount(2);
        config.Projects[0].ProjectId.Should().Be("project1");
        config.Projects[1].ProjectId.Should().Be("project2");
    }
}
