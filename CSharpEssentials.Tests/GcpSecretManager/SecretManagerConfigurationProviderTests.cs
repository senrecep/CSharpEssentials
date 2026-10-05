using System.Collections.Concurrent;
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
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;

namespace CSharpEssentials.Tests.GcpSecretManager;

public class SecretManagerConfigurationProviderTests
{
    private static readonly string[] TestArrayItems = { "a", "b", "c" };
    private static readonly TimeSpan TestRetryDelay = TimeSpan.FromMilliseconds(1);

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

        return new SecretManagerConfigurationProvider(contexts, loader, options, TestRetryDelay);
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

        var provider = new SecretManagerConfigurationProvider(contexts, loader, options, TestRetryDelay);
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

    [Fact]
    public void Load_WhenListingFails_ShouldLogErrorToConfiguredLogger()
    {
        var mockClient = new Mock<SecretManagerServiceClient>();
        mockClient.Setup(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()))
            .Throws(new InvalidOperationException("boom"));
        var logger = new CapturingLogger();
        var options = new SecretManagerConfigurationOptions { LoggerFactory = new CapturingLoggerFactory(logger) };
        SecretManagerConfigurationProvider provider = CreateProvider(
            mockClient, [new ProjectSecretConfiguration { ProjectId = "project" }], options: options);

        provider.Load();

        logger.Entries.Should().Contain(e => e.Level == LogLevel.Error && e.Exception != null && e.Message.Contains("project"));
    }

    [Fact]
    public void Load_WithLoggerFactory_ShouldLogSecretLoadingAtDebug()
    {
        var secrets = new List<Secret> { CreateSecret("project", "raw-secret") };
        var values = new Dictionary<string, string>
        {
            ["projects/project/secrets/raw-secret/versions/latest"] = "v"
        };
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, values);
        var logger = new CapturingLogger();
        var options = new SecretManagerConfigurationOptions { LoggerFactory = new CapturingLoggerFactory(logger) };
        SecretManagerConfigurationProvider provider = CreateProvider(
            mockClient, [new ProjectSecretConfiguration { ProjectId = "project" }], options: options);

        provider.Load();

        logger.Entries.Count(e => e.Level == LogLevel.Debug).Should().Be(2);
    }

    [Fact]
    public void Load_WithoutLoggerFactory_ShouldNotWriteToConsole()
    {
        var mockClient = new Mock<SecretManagerServiceClient>();
        mockClient.Setup(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()))
            .Throws(new InvalidOperationException("boom"));
        SecretManagerConfigurationProvider provider = CreateProvider(
            mockClient, [new ProjectSecretConfiguration { ProjectId = "project" }]);
        TextWriter originalOut = Console.Out;
        TextWriter originalError = Console.Error;
        using var outWriter = new StringWriter();
        using var errorWriter = new StringWriter();
        try
        {
            Console.SetOut(outWriter);
            Console.SetError(errorWriter);

            provider.Load();
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }

        outWriter.ToString().Should().BeEmpty();
        errorWriter.ToString().Should().BeEmpty();
    }

    private static PagedAsyncEnumerable<ListSecretsResponse, Secret> CreatePagedSecrets(List<Secret> secrets)
    {
        var mockPaged = new Mock<PagedAsyncEnumerable<ListSecretsResponse, Secret>>();
        mockPaged.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => new FakeAsyncEnumerator(secrets));
        return mockPaged.Object;
    }

    [Theory]
    [InlineData(StatusCode.Unavailable)]
    [InlineData(StatusCode.ResourceExhausted)]
    public void Load_Should_Retry_Listing_When_Transient_RpcException_Is_Thrown(StatusCode statusCode)
    {
        var secrets = new List<Secret> { CreateSecret("project", "raw-secret") };
        var values = new Dictionary<string, string>
        {
            ["projects/project/secrets/raw-secret/versions/latest"] = "plain-text-value"
        };
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, values);
        mockClient.SetupSequence(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()))
            .Throws(new RpcException(new Status(statusCode, "transient")))
            .Throws(new RpcException(new Status(statusCode, "transient")))
            .Returns(CreatePagedSecrets(secrets));
        SecretManagerConfigurationProvider provider = CreateProvider(
            mockClient, [new ProjectSecretConfiguration { ProjectId = "project" }]);

        provider.Load();

        provider.TryGet("raw-secret", out string? value).Should().BeTrue();
        value.Should().Be("plain-text-value");
        mockClient.Verify(
            x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()),
            Times.Exactly(3));
    }

    [Fact]
    public void Load_Should_Log_Original_RpcException_Once_When_Listing_Retries_Are_Exhausted()
    {
        var mockClient = new Mock<SecretManagerServiceClient>();
        var exception = new RpcException(new Status(StatusCode.Unavailable, "down"));
        mockClient.Setup(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()))
            .Throws(exception);
        var logger = new CapturingLogger();
        var options = new SecretManagerConfigurationOptions { LoggerFactory = new CapturingLoggerFactory(logger) };
        SecretManagerConfigurationProvider provider = CreateProvider(
            mockClient, [new ProjectSecretConfiguration { ProjectId = "project" }], options: options);

        provider.Load();

        mockClient.Verify(
            x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()),
            Times.Exactly(4));
        logger.Entries.Where(e => e.Level == LogLevel.Error).Should().ContainSingle()
            .Which.Exception.Should().BeSameAs(exception);
    }

    [Fact]
    public void Load_Should_Not_Retry_Listing_When_PermissionDenied()
    {
        var mockClient = new Mock<SecretManagerServiceClient>();
        mockClient.Setup(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()))
            .Throws(new RpcException(new Status(StatusCode.PermissionDenied, "denied")));
        var logger = new CapturingLogger();
        var options = new SecretManagerConfigurationOptions { LoggerFactory = new CapturingLoggerFactory(logger) };
        SecretManagerConfigurationProvider provider = CreateProvider(
            mockClient, [new ProjectSecretConfiguration { ProjectId = "project" }], options: options);

        provider.Load();

        mockClient.Verify(
            x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()),
            Times.Once);
        logger.Entries.Should().Contain(e =>
            e.Level == LogLevel.Error && e.Exception is RpcException && e.Message.Contains("project"));
    }

    [Theory]
    [InlineData(StatusCode.Unavailable)]
    [InlineData(StatusCode.ResourceExhausted)]
    public void Load_Should_Retry_Secret_Access_When_Transient_RpcException_Is_Thrown(StatusCode statusCode)
    {
        const string path = "projects/project/secrets/raw-secret/versions/latest";
        var secrets = new List<Secret> { CreateSecret("project", "raw-secret") };
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, []);
        mockClient.SetupSequence(x => x.AccessSecretVersionAsync(
                It.Is<AccessSecretVersionRequest>(r => r.Name == path),
                It.IsAny<CallSettings>()))
            .ThrowsAsync(new RpcException(new Status(statusCode, "transient")))
            .ReturnsAsync(new AccessSecretVersionResponse
            {
                Payload = new SecretPayload { Data = ByteString.CopyFromUtf8("plain-text-value") }
            });
        SecretManagerConfigurationProvider provider = CreateProvider(
            mockClient, [new ProjectSecretConfiguration { ProjectId = "project" }]);

        provider.Load();

        provider.TryGet("raw-secret", out string? value).Should().BeTrue();
        value.Should().Be("plain-text-value");
        mockClient.Verify(
            x => x.AccessSecretVersionAsync(It.IsAny<AccessSecretVersionRequest>(), It.IsAny<CallSettings>()),
            Times.Exactly(2));
    }

    [Fact]
    public void Load_Should_Skip_Secret_Without_Retry_When_Access_Is_PermissionDenied()
    {
        const string deniedPath = "projects/project/secrets/denied-secret/versions/latest";
        var secrets = new List<Secret>
        {
            CreateSecret("project", "denied-secret"),
            CreateSecret("project", "raw-secret")
        };
        var values = new Dictionary<string, string>
        {
            ["projects/project/secrets/raw-secret/versions/latest"] = "plain-text-value"
        };
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, values);
        mockClient.Setup(x => x.AccessSecretVersionAsync(
                It.Is<AccessSecretVersionRequest>(r => r.Name == deniedPath),
                It.IsAny<CallSettings>()))
            .ThrowsAsync(new RpcException(new Status(StatusCode.PermissionDenied, "denied")));
        var logger = new CapturingLogger();
        var options = new SecretManagerConfigurationOptions { LoggerFactory = new CapturingLoggerFactory(logger) };
        SecretManagerConfigurationProvider provider = CreateProvider(
            mockClient, [new ProjectSecretConfiguration { ProjectId = "project" }], options: options);

        provider.Load();

        provider.TryGet("denied-secret", out _).Should().BeFalse();
        provider.TryGet("raw-secret", out _).Should().BeTrue();
        mockClient.Verify(
            x => x.AccessSecretVersionAsync(
                It.Is<AccessSecretVersionRequest>(r => r.Name == deniedPath),
                It.IsAny<CallSettings>()),
            Times.Once);
        logger.Entries.Should().Contain(e => e.Exception is RpcException && e.Message.Contains("denied-secret"));
    }

    [Fact]
    public void Load_Should_Load_All_Secrets_When_Batches_Run_Concurrently()
    {
        const int secretCount = 200;
        var secrets = Enumerable.Range(0, secretCount)
            .Select(i => CreateSecret("project", $"secret-{i}"))
            .ToList();
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, []);
        mockClient.Setup(x => x.AccessSecretVersionAsync(It.IsAny<AccessSecretVersionRequest>(), It.IsAny<CallSettings>()))
            .Returns(async (AccessSecretVersionRequest request, CallSettings _) =>
            {
                // Yield so each load completes on the thread pool and the loads in a batch really overlap.
                await Task.Yield();
                return new AccessSecretVersionResponse
                {
                    Payload = new SecretPayload
                    {
                        Data = ByteString.CopyFromUtf8(
                            $"{{\"Index\":{SecretVersionName.Parse(request.Name).SecretId["secret-".Length..]}}}")
                    }
                };
            });
        var options = new SecretManagerConfigurationOptions { BatchSize = 50 };

        for (int run = 0; run < 5; run++)
        {
            SecretManagerConfigurationProvider provider = CreateProvider(
                mockClient, [new ProjectSecretConfiguration { ProjectId = "project" }], options: options);

            provider.Load();

            for (int i = 0; i < secretCount; i++)
            {
                provider.TryGet($"secret-{i}:Index", out string? index).Should().BeTrue();
                index.Should().Be(i.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
        }
    }

    [Theory]
    [InlineData(new[] { "a__b", "a" }, "raw")]
    [InlineData(new[] { "a", "a__b" }, "json")]
    public void Load_Should_Keep_First_Value_By_List_Order_When_Keys_Collide(string[] secretIds, string expected)
    {
        var secrets = secretIds.Select(id => CreateSecret("project", id)).ToList();
        var values = new Dictionary<string, string>
        {
            ["projects/project/secrets/a__b/versions/latest"] = "raw",
            ["projects/project/secrets/a/versions/latest"] = "{\"b\":\"json\"}"
        };
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, values);
        var options = new SecretManagerConfigurationOptions { BatchSize = 10 };

        SecretManagerConfigurationProvider provider = CreateProvider(
            mockClient, [new ProjectSecretConfiguration { ProjectId = "project" }], options: options);
        provider.Load();

        provider.TryGet("a:b", out string? value).Should().BeTrue();
        value.Should().Be(expected);
    }

    [Fact]
    public void Load_Should_Not_Retry_Listing_When_Non_Rpc_Exception_Is_Thrown()
    {
        var mockClient = new Mock<SecretManagerServiceClient>();
        mockClient.Setup(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()))
            .Throws(new InvalidOperationException("boom"));
        SecretManagerConfigurationProvider provider = CreateProvider(
            mockClient, [new ProjectSecretConfiguration { ProjectId = "project" }]);

        provider.Load();

        mockClient.Verify(
            x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()),
            Times.Once);
    }

    [Fact]
    public void Load_Should_Retry_Listing_When_Transient_RpcException_Is_Thrown_During_Enumeration()
    {
        var secrets = new List<Secret> { CreateSecret("project", "raw-secret") };
        var values = new Dictionary<string, string>
        {
            ["projects/project/secrets/raw-secret/versions/latest"] = "plain-text-value"
        };
        Mock<SecretManagerServiceClient> mockClient = CreateMockClient(secrets, values);
        var failingEnumerator = new Mock<IAsyncEnumerator<Secret>>();
        failingEnumerator.Setup(x => x.MoveNextAsync())
            .Throws(new RpcException(new Status(StatusCode.Unavailable, "stream dropped")));
        var mockPaged = new Mock<PagedAsyncEnumerable<ListSecretsResponse, Secret>>();
        mockPaged.SetupSequence(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(failingEnumerator.Object)
            .Returns(() => new FakeAsyncEnumerator(secrets));
        mockClient.Setup(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()))
            .Returns(mockPaged.Object);
        SecretManagerConfigurationProvider provider = CreateProvider(
            mockClient, [new ProjectSecretConfiguration { ProjectId = "project" }]);

        provider.Load();

        provider.TryGet("raw-secret", out string? value).Should().BeTrue();
        value.Should().Be("plain-text-value");
        failingEnumerator.Verify(x => x.MoveNextAsync(), Times.Once);
    }

    private sealed class CapturingLoggerFactory(ILogger logger) : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider) { }
        public ILogger CreateLogger(string categoryName) => logger;
        public void Dispose() { }
    }

    private sealed class CapturingLogger : ILogger
    {
        public ConcurrentQueue<(LogLevel Level, Exception? Exception, string Message)> Entries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Entries.Enqueue((logLevel, exception, formatter(state, exception)));
    }
}
