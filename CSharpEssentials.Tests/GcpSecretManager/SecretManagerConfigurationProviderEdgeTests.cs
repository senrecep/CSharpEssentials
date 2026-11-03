using System.Collections.Concurrent;
using CSharpEssentials.GcpSecretManager;
using CSharpEssentials.GcpSecretManager.Configuration;
using CSharpEssentials.GcpSecretManager.Models.Internal;
using FluentAssertions;
using Google.Api.Gax;
using Google.Api.Gax.Grpc;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.V1;
using Microsoft.Extensions.Logging;
using Moq;

namespace CSharpEssentials.Tests.GcpSecretManager;

public sealed class SecretManagerConfigurationProviderEdgeTests
{
    [Fact]
    public async Task LoadAsync_WhenLoaderRejectsEverySecret_ShouldNotAccessAnySecretVersion()
    {
        var mockClient = new Mock<SecretManagerServiceClient>();
        var mockPaged = new Mock<PagedAsyncEnumerable<ListSecretsResponse, Secret>>();
        List<Secret> secrets = [new Secret { SecretName = new SecretName("project", "ignored") }];
        mockPaged.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => Enumerate(secrets).GetAsyncEnumerator());
        mockClient.Setup(x => x.ListSecretsAsync(It.IsAny<ListSecretsRequest>(), It.IsAny<CallSettings>()))
            .Returns(mockPaged.Object);
        var loader = new Mock<ISecretManagerConfigurationLoader>();
        loader.Setup(x => x.ShouldLoadSecret(It.IsAny<Secret>(), It.IsAny<ProjectSecretConfiguration>()))
            .Returns(false);
        var config = new ProjectSecretConfiguration { ProjectId = "project" };
        var provider = new SecretManagerConfigurationProvider(
            [new ProjectSecretLoadContext(mockClient.Object, new ProjectName("project"), config)],
            loader.Object,
            new SecretManagerConfigurationOptions());

        await provider.LoadAsync();

        provider.GetChildKeys([], null).Should().BeEmpty();
        loader.Verify(x => x.ShouldLoadSecret(secrets[0], config), Times.Once);
        mockClient.Verify(
            x => x.AccessSecretVersionAsync(It.IsAny<AccessSecretVersionRequest>(), It.IsAny<CallSettings>()),
            Times.Never);
    }

    [Fact]
    public async Task LoadAsync_WhenProjectContextIsIncomplete_ShouldLogCriticalErrorAndRethrow()
    {
        var logger = new CapturingLogger();
        var options = new SecretManagerConfigurationOptions { LoggerFactory = new CapturingLoggerFactory(logger) };
        var provider = new SecretManagerConfigurationProvider(
            [new ProjectSecretLoadContext(Mock.Of<SecretManagerServiceClient>(), null!, new ProjectSecretConfiguration { ProjectId = "project" })],
            new DefaultSecretManagerConfigurationLoader(),
            options);

        Func<Task> act = provider.LoadAsync;

        NullReferenceException thrown = (await act.Should().ThrowAsync<NullReferenceException>()).Which;
        logger.Entries.Should().ContainSingle(e =>
            e.Level == LogLevel.Error &&
            e.Exception == thrown &&
            e.Message == "Critical error during secret loading");
    }

    private static async IAsyncEnumerable<Secret> Enumerate(List<Secret> secrets)
    {
        foreach (Secret secret in secrets)
        {
            await Task.Yield();
            yield return secret;
        }
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
