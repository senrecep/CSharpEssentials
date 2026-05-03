using CSharpEssentials.GcpSecretManager;
using CSharpEssentials.GcpSecretManager.Models.Internal;
using FluentAssertions;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.V1;
using Moq;

namespace CSharpEssentials.Tests.GcpSecretManager;

public class ProjectSecretLoadContextTests
{
    [Fact]
    public void ProjectSecretLoadContext_ShouldStoreProperties()
    {
        var mockClient = new Mock<SecretManagerServiceClient>();
        var projectName = new ProjectName("test-project");
        var config = new ProjectSecretConfiguration { ProjectId = "test-project" };

        var context = new ProjectSecretLoadContext(mockClient.Object, projectName, config);

        context.Client.Should().Be(mockClient.Object);
        context.ProjectName.Should().Be(projectName);
        context.Config.Should().Be(config);
    }

    [Fact]
    public void ProjectSecretLoadContext_EqualInstances_ShouldBeEqual()
    {
        var mockClient = new Mock<SecretManagerServiceClient>();
        var projectName = new ProjectName("test-project");
        var config = new ProjectSecretConfiguration { ProjectId = "test-project" };

        var context1 = new ProjectSecretLoadContext(mockClient.Object, projectName, config);
        var context2 = new ProjectSecretLoadContext(mockClient.Object, projectName, config);

        context1.Should().Be(context2);
        context1.GetHashCode().Should().Be(context2.GetHashCode());
    }

    [Fact]
    public void ProjectSecretLoadContext_DifferentClient_ShouldNotBeEqual()
    {
        var mockClient1 = new Mock<SecretManagerServiceClient>();
        var mockClient2 = new Mock<SecretManagerServiceClient>();
        var projectName = new ProjectName("test-project");
        var config = new ProjectSecretConfiguration { ProjectId = "test-project" };

        var context1 = new ProjectSecretLoadContext(mockClient1.Object, projectName, config);
        var context2 = new ProjectSecretLoadContext(mockClient2.Object, projectName, config);

        context1.Should().NotBe(context2);
    }

    [Fact]
    public void ProjectSecretLoadContext_Deconstruct_ShouldReturnValues()
    {
        var mockClient = new Mock<SecretManagerServiceClient>();
        var projectName = new ProjectName("test-project");
        var config = new ProjectSecretConfiguration { ProjectId = "test-project" };

        var context = new ProjectSecretLoadContext(mockClient.Object, projectName, config);
        var (client, proj, cfg) = context;

        client.Should().Be(mockClient.Object);
        proj.Should().Be(projectName);
        cfg.Should().Be(config);
    }
}
