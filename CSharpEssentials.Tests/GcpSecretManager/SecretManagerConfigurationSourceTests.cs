using CSharpEssentials.GcpSecretManager;
using CSharpEssentials.GcpSecretManager.Configuration;
using CSharpEssentials.GcpSecretManager.Infrastructure;
using FluentAssertions;
using Google.Api.Gax;
using Google.Api.Gax.Grpc;
using Google.Cloud.SecretManager.V1;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CSharpEssentials.Tests.GcpSecretManager;

public class SecretManagerConfigurationSourceTests
{
    private static Secret CreateSecret(string projectId, string secretId)
    {
        return new Secret { SecretName = new SecretName(projectId, secretId) };
    }

    [Fact]
    public void Build_WithNoProjects_ShouldReturnProviderWithEmptyData()
    {
        var options = new SecretManagerConfigurationOptions();
        options.AddProject(new ProjectSecretConfiguration { ProjectId = "project" });

        var mockHelper = new Mock<IServiceClientHelper>();
        var mockClient = new Mock<SecretManagerServiceClient>();
        var mockPaged = new Mock<PagedAsyncEnumerable<ListSecretsResponse, Secret>>();
        mockPaged.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => new FakeAsyncEnumerator([]));
        mockClient.Setup(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()))
            .Returns(mockPaged.Object);
        mockHelper.Setup(x => x.Create()).Returns(mockClient.Object);

        var source = new SecretManagerConfigurationSource(options, mockHelper.Object);
        IConfigurationProvider provider = source.Build(new ConfigurationBuilder());

        provider.Should().NotBeNull();
        provider.Load();
        provider.GetChildKeys(Enumerable.Empty<string>(), null).Should().BeEmpty();
    }

    [Fact]
    public void Build_WithNoLoader_ShouldUseDefaultLoader()
    {
        var options = new SecretManagerConfigurationOptions();
        options.AddProject(new ProjectSecretConfiguration { ProjectId = "project" });

        var mockHelper = new Mock<IServiceClientHelper>();
        var mockClient = new Mock<SecretManagerServiceClient>();
        var mockPaged = new Mock<PagedAsyncEnumerable<ListSecretsResponse, Secret>>();
        mockPaged.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => new FakeAsyncEnumerator([]));
        mockClient.Setup(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()))
            .Returns(mockPaged.Object);
        mockHelper.Setup(x => x.Create()).Returns(mockClient.Object);

        var source = new SecretManagerConfigurationSource(options, mockHelper.Object);
        IConfigurationProvider provider = source.Build(new ConfigurationBuilder());

        provider.Should().NotBeNull();
    }

    [Fact]
    public void Build_WithCustomLoader_ShouldUseCustomLoader()
    {
        var customLoader = new CustomLoader();
        var options = new SecretManagerConfigurationOptions
        {
            Loader = customLoader
        };
        options.AddProject(new ProjectSecretConfiguration { ProjectId = "project" });

        var mockHelper = new Mock<IServiceClientHelper>();
        var mockClient = new Mock<SecretManagerServiceClient>();
        var mockPaged = new Mock<PagedAsyncEnumerable<ListSecretsResponse, Secret>>();
        mockPaged.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => new FakeAsyncEnumerator([]));
        mockClient.Setup(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()))
            .Returns(mockPaged.Object);
        mockHelper.Setup(x => x.Create()).Returns(mockClient.Object);

        var source = new SecretManagerConfigurationSource(options, mockHelper.Object);
        IConfigurationProvider provider = source.Build(new ConfigurationBuilder());

        provider.Should().NotBeNull();
    }

    [Fact]
    public void Build_WithRegionAndNoCredentials_ShouldCreateWithRegion()
    {
        var options = new SecretManagerConfigurationOptions();
        options.AddProject(new ProjectSecretConfiguration
        {
            ProjectId = "project",
            Region = "us-central1"
        });

        var mockHelper = new Mock<IServiceClientHelper>();
        var mockClient = new Mock<SecretManagerServiceClient>();
        mockHelper.Setup(x => x.CreateWithRegion("us-central1")).Returns(mockClient.Object);

        var source = new SecretManagerConfigurationSource(options, mockHelper.Object);
        IConfigurationProvider provider = source.Build(new ConfigurationBuilder());

        mockHelper.Verify(x => x.CreateWithRegion("us-central1"), Times.Once);
        provider.Should().NotBeNull();
    }

    [Fact]
    public void Build_WithCredentialsAndNoRegion_ShouldCreateWithCredentials()
    {
        var options = new SecretManagerConfigurationOptions
        {
            CredentialsPath = "/path/to/creds.json"
        };
        options.AddProject(new ProjectSecretConfiguration { ProjectId = "project" });

        var mockHelper = new Mock<IServiceClientHelper>();
        var mockClient = new Mock<SecretManagerServiceClient>();
        mockHelper.Setup(x => x.Create("/path/to/creds.json")).Returns(mockClient.Object);

        var source = new SecretManagerConfigurationSource(options, mockHelper.Object);
        IConfigurationProvider provider = source.Build(new ConfigurationBuilder());

        mockHelper.Verify(x => x.Create("/path/to/creds.json"), Times.Once);
        provider.Should().NotBeNull();
    }

    [Fact]
    public void Build_WithRegionAndCredentials_ShouldCreateWithRegionAndCredentials()
    {
        var options = new SecretManagerConfigurationOptions
        {
            CredentialsPath = "/path/to/creds.json"
        };
        options.AddProject(new ProjectSecretConfiguration
        {
            ProjectId = "project",
            Region = "europe-west1"
        });

        var mockHelper = new Mock<IServiceClientHelper>();
        var mockClient = new Mock<SecretManagerServiceClient>();
        mockHelper.Setup(x => x.CreateWithRegion("/path/to/creds.json", "europe-west1")).Returns(mockClient.Object);

        var source = new SecretManagerConfigurationSource(options, mockHelper.Object);
        IConfigurationProvider provider = source.Build(new ConfigurationBuilder());

        mockHelper.Verify(x => x.CreateWithRegion("/path/to/creds.json", "europe-west1"), Times.Once);
        provider.Should().NotBeNull();
    }

    [Fact]
    public void Build_WithNoRegionAndNoCredentials_ShouldCreateDefaultClient()
    {
        var options = new SecretManagerConfigurationOptions();
        options.AddProject(new ProjectSecretConfiguration { ProjectId = "project" });

        var mockHelper = new Mock<IServiceClientHelper>();
        var mockClient = new Mock<SecretManagerServiceClient>();
        mockHelper.Setup(x => x.Create()).Returns(mockClient.Object);

        var source = new SecretManagerConfigurationSource(options, mockHelper.Object);
        IConfigurationProvider provider = source.Build(new ConfigurationBuilder());

        mockHelper.Verify(x => x.Create(), Times.Once);
        provider.Should().NotBeNull();
    }

    [Fact]
    public void Build_WithMultipleProjects_ShouldCreateClientForEach()
    {
        var options = new SecretManagerConfigurationOptions();
        options.AddProject(new ProjectSecretConfiguration { ProjectId = "project1" });
        options.AddProject(new ProjectSecretConfiguration { ProjectId = "project2", Region = "us-central1" });

        var mockHelper = new Mock<IServiceClientHelper>();
        var mockClient = new Mock<SecretManagerServiceClient>();
        mockHelper.Setup(x => x.Create()).Returns(mockClient.Object);
        mockHelper.Setup(x => x.CreateWithRegion("us-central1")).Returns(mockClient.Object);

        var source = new SecretManagerConfigurationSource(options, mockHelper.Object);
        IConfigurationProvider provider = source.Build(new ConfigurationBuilder());

        mockHelper.Verify(x => x.Create(), Times.Once);
        mockHelper.Verify(x => x.CreateWithRegion("us-central1"), Times.Once);
        provider.Should().NotBeNull();
    }

    [Fact]
    public void Build_ReturnedProvider_ShouldLoadSecrets()
    {
        var options = new SecretManagerConfigurationOptions();
        options.AddProject(new ProjectSecretConfiguration { ProjectId = "project" });

        var secrets = new List<Secret> { CreateSecret("project", "secret1") };
        var values = new Dictionary<string, string>
        {
            ["projects/project/secrets/secret1/versions/latest"] = "value1"
        };

        var mockHelper = new Mock<IServiceClientHelper>();
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, values);
        mockHelper.Setup(x => x.Create()).Returns(mockClient.Object);

        var source = new SecretManagerConfigurationSource(options, mockHelper.Object);
        IConfigurationProvider provider = source.Build(new ConfigurationBuilder());
        provider.Load();

        provider.TryGet("secret1", out string? loadedValue).Should().BeTrue();
        loadedValue.Should().Be("value1");
    }

    private static Mock<SecretManagerServiceClient> CreateMockClient(
        List<Secret> secrets,
        Dictionary<string, string> secretValues)
    {
        var mockClient = new Mock<SecretManagerServiceClient>();

        var mockPaged = new Mock<PagedAsyncEnumerable<ListSecretsResponse, Secret>>();
        mockPaged.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => new FakeAsyncEnumerator(secrets));
        mockClient.Setup(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()))
            .Returns(mockPaged.Object);

        foreach ((string path, string value) in secretValues)
        {
            mockClient.Setup(x => x.AccessSecretVersionAsync(
                    It.Is<AccessSecretVersionRequest>(r => r.Name == path),
                    It.IsAny<CallSettings>()))
                .ReturnsAsync(new AccessSecretVersionResponse
                {
                    Payload = new SecretPayload { Data = ByteString.CopyFromUtf8(value) }
                });
        }

        return mockClient;
    }

    private sealed class CustomLoader : ISecretManagerConfigurationLoader
    {
        public string GetKey(Secret secret) => secret.SecretName.SecretId;
        public string GetKey(string keyId) => keyId;
        public bool ShouldLoadSecret(Secret secret, ProjectSecretConfiguration projectConfig) => true;
    }

    private sealed class FakeAsyncEnumerator : IAsyncEnumerator<Secret>
    {
        private readonly List<Secret> _secrets;
        private int _index = -1;

        public FakeAsyncEnumerator(List<Secret> secrets) => _secrets = secrets;

        public Secret Current => _secrets[_index];

        public ValueTask<bool> MoveNextAsync()
        {
            _index++;
            return new ValueTask<bool>(_index < _secrets.Count);
        }

        public ValueTask DisposeAsync() => default;
    }
}
