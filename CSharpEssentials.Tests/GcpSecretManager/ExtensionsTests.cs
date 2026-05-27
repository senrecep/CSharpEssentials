using CSharpEssentials.GcpSecretManager.Configuration;
using CSharpEssentials.GcpSecretManager.Extensions;
using CSharpEssentials.GcpSecretManager.Infrastructure;
using FluentAssertions;
using Google.Api.Gax;
using Google.Api.Gax.Grpc;
using Google.Cloud.SecretManager.V1;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CSharpEssentials.Tests.GcpSecretManager;

public class ExtensionsTests
{
    [Fact]
    public void AddGcpSecretManager_WithManualProjects_ShouldAddSource()
    {
        using var configuration = new ConfigurationManager();
        Mock<IServiceClientHelper> clientHelper = CreateClientHelper();

        configuration.AddGcpSecretManager(options => options.AddProject(new() { ProjectId = "test-project" }), clientHelper.Object);

        configuration.Sources.Should().Contain(x => x is SecretManagerConfigurationSource);
    }

    [Fact]
    public void AddGcpSecretManager_WithNullOptions_ShouldLoadFromAppSettings()
    {
        using var configuration = new ConfigurationManager();
        Mock<IServiceClientHelper> clientHelper = CreateClientHelper();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["GoogleSecretManager:Projects:0:ProjectId"] = "test-project"
        });

        configuration.AddGcpSecretManager(null, clientHelper.Object);

        configuration.Sources.Should().Contain(x => x is SecretManagerConfigurationSource);
    }

    [Fact]
    public void AddGcpSecretManager_WithLoadFromAppSettings_ShouldLoadFromAppSettings()
    {
        using var configuration = new ConfigurationManager();
        Mock<IServiceClientHelper> clientHelper = CreateClientHelper();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["GoogleSecretManager:Projects:0:ProjectId"] = "test-project"
        });

        configuration.AddGcpSecretManager(options => options.LoadFromAppSettings = true, clientHelper.Object);

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
        Mock<IServiceClientHelper> clientHelper = CreateClientHelper();

        IConfigurationManager result = configuration.AddGcpSecretManager(options => options.AddProject(new() { ProjectId = "test-project" }), clientHelper.Object);

        result.Should().BeSameAs(configuration);
    }

    [Fact]
    public void AddGcpSecretManager_WithMultipleProjects_ShouldAddSingleSource()
    {
        using var configuration = new ConfigurationManager();
        Mock<IServiceClientHelper> clientHelper = CreateClientHelper();

        configuration.AddGcpSecretManager(options =>
        {
            options.AddProject(new() { ProjectId = "project1" });
            options.AddProject(new() { ProjectId = "project2" });
        }, clientHelper.Object);

        configuration.Sources.Should().Contain(x => x is SecretManagerConfigurationSource);
    }

    [Fact]
    public void AddGcpSecretManager_WithCustomSectionName_ShouldUseCustomSection()
    {
        using var configuration = new ConfigurationManager();
        Mock<IServiceClientHelper> clientHelper = CreateClientHelper();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["CustomSection:Projects:0:ProjectId"] = "test-project"
        });

        configuration.AddGcpSecretManager(options =>
        {
            options.LoadFromAppSettings = true;
            options.ConfigurationSectionName = "CustomSection";
        }, clientHelper.Object);

        configuration.Sources.Should().Contain(x => x is SecretManagerConfigurationSource);
    }

    private static Mock<IServiceClientHelper> CreateClientHelper()
    {
        var mockHelper = new Mock<IServiceClientHelper>();
        var mockClient = new Mock<SecretManagerServiceClient>();
        var mockPaged = new Mock<PagedAsyncEnumerable<ListSecretsResponse, Secret>>();

        mockPaged.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => new EmptySecretAsyncEnumerator());
        mockClient.Setup(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()))
            .Returns(mockPaged.Object);

        mockHelper.Setup(x => x.Create()).Returns(mockClient.Object);
        mockHelper.Setup(x => x.Create(It.IsAny<string>())).Returns(mockClient.Object);
        mockHelper.Setup(x => x.CreateWithRegion(It.IsAny<string?>())).Returns(mockClient.Object);
        mockHelper.Setup(x => x.CreateWithRegion(It.IsAny<string>(), It.IsAny<string?>())).Returns(mockClient.Object);

        return mockHelper;
    }

    private sealed class EmptySecretAsyncEnumerator : IAsyncEnumerator<Secret>
    {
        public Secret Current => throw new InvalidOperationException();

        public ValueTask<bool> MoveNextAsync() => new(false);

        public ValueTask DisposeAsync() => default;
    }
}
