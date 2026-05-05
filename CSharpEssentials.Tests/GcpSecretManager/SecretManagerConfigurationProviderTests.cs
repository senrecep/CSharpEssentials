using System.Text.Json;
using CSharpEssentials.GcpSecretManager;
using CSharpEssentials.GcpSecretManager.Configuration;
using CSharpEssentials.GcpSecretManager.Models.Internal;
using FluentAssertions;
using Google.Api.Gax;
using Google.Api.Gax.Grpc;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.V1;
using Google.Protobuf;
using Moq;

namespace CSharpEssentials.Tests.GcpSecretManager;

public class SecretManagerConfigurationProviderTests
{
    private static readonly string[] TestArrayItems = { "a", "b", "c" };

    private static Secret CreateSecret(string projectId, string secretId)
    {
        return new Secret { SecretName = new SecretName(projectId, secretId) };
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

    private static SecretManagerConfigurationProvider CreateProvider(
        Mock<SecretManagerServiceClient> mockClient,
        List<ProjectSecretConfiguration> projectConfigs,
        ISecretManagerConfigurationLoader? loader = null,
        SecretManagerConfigurationOptions? options = null)
    {
        loader ??= new DefaultSecretManagerConfigurationLoader();
        options ??= new SecretManagerConfigurationOptions();

        var contexts = projectConfigs.Select(pc => new ProjectSecretLoadContext(
            mockClient.Object,
            new ProjectName(pc.ProjectId),
            pc)).ToList();

        return new SecretManagerConfigurationProvider(contexts, loader, options);
    }

    [Fact]
    public void Load_WithNoProjects_ShouldReturnEmptyData()
    {
        var mockClient = new Mock<SecretManagerServiceClient>();
        SecretManagerConfigurationProvider provider = CreateProvider(mockClient, []);

        provider.Load();

        provider.GetChildKeys(Enumerable.Empty<string>(), null).Should().BeEmpty();
    }

    [Fact]
    public void Load_WithSingleRawSecret_ShouldLoadRawValue()
    {
        var secrets = new List<Secret>
        {
            CreateSecret("project", "raw-secret")
        };
        var values = new Dictionary<string, string>
        {
            ["projects/project/secrets/raw-secret/versions/latest"] = "plain-text-value"
        };
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, values);
        var projectConfig = new ProjectSecretConfiguration
        {
            ProjectId = "project",
            RawSecretIds = new[] { "raw-secret" }
        };
        SecretManagerConfigurationProvider provider = CreateProvider(mockClient, [projectConfig]);

        provider.Load();

        provider.TryGet("raw-secret", out string? value).Should().BeTrue();
        value.Should().Be("plain-text-value");
    }

    [Fact]
    public void Load_WithJsonSecret_ShouldFlattenJson()
    {
        var secrets = new List<Secret>
        {
            CreateSecret("project", "app-settings")
        };
        var json = JsonSerializer.Serialize(new { Database = new { Host = "localhost", Port = 5432 } });
        var values = new Dictionary<string, string>
        {
            ["projects/project/secrets/app-settings/versions/latest"] = json
        };
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, values);
        var projectConfig = new ProjectSecretConfiguration { ProjectId = "project" };
        SecretManagerConfigurationProvider provider = CreateProvider(mockClient, [projectConfig]);

        provider.Load();

        provider.TryGet("app-settings:Database:Host", out string? host).Should().BeTrue();
        host.Should().Be("localhost");
        provider.TryGet("app-settings:Database:Port", out string? port).Should().BeTrue();
        port.Should().Be("5432");
    }

    [Fact]
    public void Load_WithInvalidJson_ShouldKeepRawValue()
    {
        var secrets = new List<Secret>
        {
            CreateSecret("project", "bad-json")
        };
        var values = new Dictionary<string, string>
        {
            ["projects/project/secrets/bad-json/versions/latest"] = "not-json-at-all"
        };
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, values);
        var projectConfig = new ProjectSecretConfiguration { ProjectId = "project" };
        SecretManagerConfigurationProvider provider = CreateProvider(mockClient, [projectConfig]);

        provider.Load();

        provider.TryGet("bad-json", out string? value).Should().BeTrue();
        value.Should().Be("not-json-at-all");
    }

    [Fact]
    public void Load_WithRawPrefix_ShouldNotFlattenJson()
    {
        var secrets = new List<Secret>
        {
            CreateSecret("project", "RAW_config")
        };
        var json = "{\"key\":\"value\"}";
        var values = new Dictionary<string, string>
        {
            ["projects/project/secrets/RAW_config/versions/latest"] = json
        };
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, values);
        var projectConfig = new ProjectSecretConfiguration
        {
            ProjectId = "project",
            RawSecretPrefixes = new[] { "RAW_" }
        };
        SecretManagerConfigurationProvider provider = CreateProvider(mockClient, [projectConfig]);

        provider.Load();

        provider.TryGet("RAW_config", out string? value).Should().BeTrue();
        value.Should().Be(json);
        provider.TryGet("RAW_config:key", out string? _).Should().BeFalse();
    }

    [Fact]
    public void Load_WithPrefixFilter_ShouldOnlyLoadMatchingSecrets()
    {
        var secrets = new List<Secret>
        {
            CreateSecret("project", "APP_setting1"),
            CreateSecret("project", "DB_setting1"),
            CreateSecret("project", "APP_setting2")
        };
        var values = new Dictionary<string, string>
        {
            ["projects/project/secrets/APP_setting1/versions/latest"] = "app1",
            ["projects/project/secrets/DB_setting1/versions/latest"] = "db1",
            ["projects/project/secrets/APP_setting2/versions/latest"] = "app2"
        };
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, values);
        var projectConfig = new ProjectSecretConfiguration
        {
            ProjectId = "project",
            PrefixFilters = new[] { "APP_" }
        };
        SecretManagerConfigurationProvider provider = CreateProvider(mockClient, [projectConfig]);

        provider.Load();

        provider.TryGet("APP_setting1", out _).Should().BeTrue();
        provider.TryGet("APP_setting2", out _).Should().BeTrue();
        provider.TryGet("DB_setting1", out _).Should().BeFalse();
    }

    [Fact]
    public void Load_WithSecretIdFilter_ShouldOnlyLoadMatchingSecrets()
    {
        var secrets = new List<Secret>
        {
            CreateSecret("project", "secret1"),
            CreateSecret("project", "secret2"),
            CreateSecret("project", "secret3")
        };
        var values = new Dictionary<string, string>
        {
            ["projects/project/secrets/secret1/versions/latest"] = "val1",
            ["projects/project/secrets/secret2/versions/latest"] = "val2",
            ["projects/project/secrets/secret3/versions/latest"] = "val3"
        };
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, values);
        var projectConfig = new ProjectSecretConfiguration
        {
            ProjectId = "project",
            SecretIds = new[] { "secret1", "secret3" }
        };
        SecretManagerConfigurationProvider provider = CreateProvider(mockClient, [projectConfig]);

        provider.Load();

        provider.TryGet("secret1", out _).Should().BeTrue();
        provider.TryGet("secret3", out _).Should().BeTrue();
        provider.TryGet("secret2", out _).Should().BeFalse();
    }

    [Fact]
    public void Load_WithMultipleProjects_ShouldLoadAll()
    {
        var secrets1 = new List<Secret> { CreateSecret("project1", "secret1") };
        var secrets2 = new List<Secret> { CreateSecret("project2", "secret2") };
        var values1 = new Dictionary<string, string>
        {
            ["projects/project1/secrets/secret1/versions/latest"] = "value1"
        };
        var values2 = new Dictionary<string, string>
        {
            ["projects/project2/secrets/secret2/versions/latest"] = "value2"
        };

        Mock<SecretManagerServiceClient> mockClient1 = CreateMockClient(secrets1, values1);
        Mock<SecretManagerServiceClient> mockClient2 = CreateMockClient(secrets2, values2);

        var loader = new DefaultSecretManagerConfigurationLoader();
        var options = new SecretManagerConfigurationOptions();
        var contexts = new List<ProjectSecretLoadContext>
        {
            new(mockClient1.Object, new ProjectName("project1"), new ProjectSecretConfiguration { ProjectId = "project1" }),
            new(mockClient2.Object, new ProjectName("project2"), new ProjectSecretConfiguration { ProjectId = "project2" })
        };

        var provider = new SecretManagerConfigurationProvider(contexts, loader, options);
        provider.Load();

        provider.TryGet("secret1", out _).Should().BeTrue();
        provider.TryGet("secret2", out _).Should().BeTrue();
    }

    [Fact]
    public void Load_WithArrayJson_ShouldFlattenWithIndices()
    {
        var secrets = new List<Secret>
        {
            CreateSecret("project", "array-secret")
        };
        var json = JsonSerializer.Serialize(new { Items = TestArrayItems });
        var values = new Dictionary<string, string>
        {
            ["projects/project/secrets/array-secret/versions/latest"] = json
        };
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, values);
        var projectConfig = new ProjectSecretConfiguration { ProjectId = "project" };
        SecretManagerConfigurationProvider provider = CreateProvider(mockClient, [projectConfig]);

        provider.Load();

        provider.TryGet("array-secret:Items:0", out string? item0).Should().BeTrue();
        item0.Should().Be("a");
        provider.TryGet("array-secret:Items:1", out string? item1).Should().BeTrue();
        item1.Should().Be("b");
        provider.TryGet("array-secret:Items:2", out string? item2).Should().BeTrue();
        item2.Should().Be("c");
    }

    [Fact]
    public void Load_WithNullJsonValue_ShouldStoreNull()
    {
        var secrets = new List<Secret>
        {
            CreateSecret("project", "null-secret")
        };
        var json = "{\"key\":null}";
        var values = new Dictionary<string, string>
        {
            ["projects/project/secrets/null-secret/versions/latest"] = json
        };
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, values);
        var projectConfig = new ProjectSecretConfiguration { ProjectId = "project" };
        SecretManagerConfigurationProvider provider = CreateProvider(mockClient, [projectConfig]);

        provider.Load();

        provider.TryGet("null-secret:key", out string? value).Should().BeTrue();
        value.Should().BeNull();
    }

    [Fact]
    public void Load_WithNestedObject_ShouldFlattenRecursively()
    {
        var secrets = new List<Secret>
        {
            CreateSecret("project", "nested-secret")
        };
        var json = JsonSerializer.Serialize(new { Level1 = new { Level2 = new { Level3 = "deep" } } });
        var values = new Dictionary<string, string>
        {
            ["projects/project/secrets/nested-secret/versions/latest"] = json
        };
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, values);
        var projectConfig = new ProjectSecretConfiguration { ProjectId = "project" };
        SecretManagerConfigurationProvider provider = CreateProvider(mockClient, [projectConfig]);

        provider.Load();

        provider.TryGet("nested-secret:Level1:Level2:Level3", out string? deep).Should().BeTrue();
        deep.Should().Be("deep");
    }

    [Fact]
    public void Load_ShouldAlsoStoreOriginalSecretKey()
    {
        var secrets = new List<Secret>
        {
            CreateSecret("project", "my-secret")
        };
        var json = "{\"a\":1}";
        var values = new Dictionary<string, string>
        {
            ["projects/project/secrets/my-secret/versions/latest"] = json
        };
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, values);
        var projectConfig = new ProjectSecretConfiguration { ProjectId = "project" };
        SecretManagerConfigurationProvider provider = CreateProvider(mockClient, [projectConfig]);

        provider.Load();

        provider.TryGet("my-secret", out string? original).Should().BeTrue();
        original.Should().Be(json);
        provider.TryGet("my-secret:a", out string? flat).Should().BeTrue();
        flat.Should().Be("1");
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
