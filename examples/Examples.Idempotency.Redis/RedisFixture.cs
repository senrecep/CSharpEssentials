using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using StackExchange.Redis;

namespace Examples.Idempotency.Redis;

/// <summary>One Redis container and connection for the store tests; every test uses its own key namespace.</summary>
public sealed class RedisFixture : IAsyncLifetime
{
    private const int RedisPort = 6379;

    private readonly IContainer _container = new ContainerBuilder("redis:7-alpine")
        .WithPortBinding(RedisPort, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilCommandIsCompleted("redis-cli", "ping"))
        .Build();

    private ConnectionMultiplexer? _connection;

    /// <summary>Connection to the container, available once the fixture is initialized.</summary>
    public IConnectionMultiplexer Connection =>
        _connection ?? throw new InvalidOperationException("The Redis container has not started.");

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _connection = await ConnectionMultiplexer.ConnectAsync($"{_container.Hostname}:{_container.GetMappedPublicPort(RedisPort)}");
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
        await _container.DisposeAsync();
    }
}
